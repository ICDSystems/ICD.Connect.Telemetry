using System;
using System.Collections.Generic;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Settings.Services;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Services
{
	public sealed class TelemetryService : AbstractService<ITelemetryService, TelemetryServiceSettings>, ITelemetryService
	{
		private readonly BiDictionary<ITelemetryProvider, TelemetryProviderNode> m_TelemetryProviders;
		private readonly SafeCriticalSection m_TelemetryProvidersSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public TelemetryService()
		{
			m_TelemetryProviders = new BiDictionary<ITelemetryProvider, TelemetryProviderNode>();
			m_TelemetryProvidersSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			DeinitializeCoreTelemetry();
		}

		/// <summary>
		/// Lazy-loads telemetry for the core and returns the root telemetry.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public TelemetryProviderNode LazyLoadCoreTelemetry()
		{
			return LazyLoadTelemetry("Core", Core);
		}

		/// <summary>
		/// Disposes the root telemetry and all child telemetry nodes recursively.
		/// </summary>
		public void DeinitializeCoreTelemetry()
		{
			TelemetryProviderNode root;
			if (TryGetTelemetryForProvider(Core, out root))
				DisposeTelemetry(root);
		}

		/// <summary>
		/// Returns previously loaded telemetry for the given provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="telemetryCollection"></param>
		/// <returns></returns>
		public bool TryGetTelemetryForProvider([NotNull] ITelemetryProvider provider, out TelemetryProviderNode telemetryCollection)
		{
			m_TelemetryProvidersSection.Enter();

			try
			{
				
				return m_TelemetryProviders.TryGetValue(provider, out telemetryCollection);
			}
			finally
			{
				m_TelemetryProvidersSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Lazy-loads telemetry nodes for the current collection contents.
		/// Disposes telemetry nodes that are no longer present in the collection.
		/// </summary>
		/// <param name="collection"></param>
		private IEnumerable<ITelemetryNode> UpdateCollectionTelemetry([NotNull] TelemetryCollection collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			// Determine the actual telemetry providers the collection needs to contain
			ITelemetryProvider[] actual = collection.Value.Cast<ITelemetryProvider>().ToArray();
			IcdHashSet<ITelemetryProvider> actualSet = actual.ToIcdHashSet();

			// Determine which telemetry providers were removed from the collection
			IEnumerable<ITelemetryProvider> removed =
				collection.Select(n => n.Provider)
				          .Where(e => !actualSet.Contains(e));

			// Determine the telemetry nodes that remain after removing providers
			Dictionary<string, TelemetryProviderNode> remaining =
				collection.Where(n => !actualSet.Contains(n.Provider))
				          .Cast<TelemetryProviderNode>()
				          .ToDictionary(n => n.Name);

			// Dispose the telemetry for removed providers
			foreach (ITelemetryProvider provider in removed)
				DisposeTelemetry(provider);

			// Lazy-load the remaining items     
			int index = 0;
			foreach (ITelemetryProvider provider in actual)
			{
				// Get the name for the node
				PropertyInfo idProperty = TelemetryCollectionIdentityAttribute.GetProperty(provider);
				string name =
					idProperty == null
						? string.Format("{0}{1}", collection.Name, index)
						: idProperty.GetValue(provider, null).ToString();
				index++;

				// Is there already a node for this?
				TelemetryProviderNode existingNode;
				if (remaining.TryGetValue(name, out existingNode))
				{
					if (provider == existingNode.Provider)
					{
						yield return existingNode;
						continue;
					}
					
					DisposeTelemetry(existingNode);
				}

				yield return LazyLoadTelemetry(name, provider);
			}
		}

		#endregion

		#region Disposal

		/// <summary>
		/// Disposes the telemetry nodes for the given provider recursively.
		/// </summary>
		/// <param name="provider"></param>
		private void DisposeTelemetry([NotNull] ITelemetryProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			TelemetryProviderNode collection;
			if (!TryGetTelemetryForProvider(provider, out collection))
				return;

			DisposeTelemetry(collection);
		}

		/// <summary>
		/// Disposes the given telemetry node recursively.
		/// </summary>
		/// <param name="node"></param>
		private void DisposeTelemetry([NotNull] ITelemetryNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			m_TelemetryProvidersSection.Enter();

			try
			{
				// Remove from the cache
				TelemetryProviderNode telemetryProviderNode = node as TelemetryProviderNode;
				if (telemetryProviderNode != null)
					m_TelemetryProviders.RemoveValue(telemetryProviderNode);

				// Copy the children
				ITelemetryNode[] children = node.GetChildren().ToArray();

				// Dispose the node
				node.Dispose();

				// Dispose the children
				foreach (ITelemetryNode child in children)
					DisposeTelemetry(child);
			}
			finally
			{
				m_TelemetryProvidersSection.Leave();
			}
		}

		#endregion

		#region Instantiation

		/// <summary>
		/// Lazy-loads telemetry recursively for a given provider. 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		[NotNull]
		private TelemetryProviderNode LazyLoadTelemetry([NotNull] string name, [NotNull] ITelemetryProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			m_TelemetryProvidersSection.Enter();

			try
			{
				return m_TelemetryProviders.GetOrAddNew(provider, () => InstantiateTelemetry(name, provider));
			}
			finally
			{
				m_TelemetryProvidersSection.Leave();
			}
		}

		/// <summary>
		/// Recursively creates the telemetry for the given provider.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		[NotNull]
		private TelemetryProviderNode InstantiateTelemetry([NotNull] string name, [NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			IEnumerable<ITelemetryNode> children = InstantiateTelemetryNodes(instance);
			return new TelemetryProviderNode(name, instance, children);
		}

		/// <summary>
		/// Recursively creates telemetry nodes for the given provider.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		[NotNull]
		private IEnumerable<ITelemetryNode> InstantiateTelemetryNodes([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			foreach (TelemetryLeaf leaf in InstantiateLeafTelemetry(instance))
				yield return leaf;

			foreach (TelemetryProviderNode node in InstantiateNodeTelemetry(instance))
				yield return node;

			foreach (TelemetryCollection collection in InstantiateCollectionTelemetry(instance))
				yield return collection;

			foreach (ITelemetryNode node in InstantiateExternalTelemetry(instance))
				yield return node;
		}

		/// <summary>
		/// Generates leaf telemetry nodes for the given provider.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		[NotNull]
		private IEnumerable<TelemetryLeaf> InstantiateLeafTelemetry([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			// There are a number of different leaf types to account for:
			// 1 - Property Only static telemetry
			// 2 - Method Only input telemetry
			// 3 - Event Only output telemetry
			// 4 - Event + Property output telemetry
			// 5 - Event + Property + Method input/output telemetry

			Dictionary<string, KeyValuePair<PropertyInfo, PropertyTelemetryAttribute>> properties =
				PropertyTelemetryAttribute.GetProperties(instance.GetType())
				                          .ToDictionary(kvp => kvp.Value.Name);

			Dictionary<string, KeyValuePair<MethodInfo, MethodTelemetryAttribute>> methods =
				MethodTelemetryAttribute.GetMethods(instance.GetType())
				                        .ToDictionary(kvp => kvp.Value.Name);

			Dictionary<string, KeyValuePair<EventInfo, EventTelemetryAttribute>> events =
				EventTelemetryAttribute.GetEvents(instance.GetType())
				                       .ToDictionary(kvp => kvp.Value.Name);

			// Property attributes point to events and methods, so start with those
			foreach (KeyValuePair<PropertyInfo, PropertyTelemetryAttribute> kvp in properties.Values)
			{
				KeyValuePair<MethodInfo, MethodTelemetryAttribute> methodAttributePair = default(KeyValuePair<MethodInfo, MethodTelemetryAttribute>);
				if (kvp.Value.MethodName != null && !methods.Remove(kvp.Value.MethodName, out methodAttributePair))
					Logger.Log(eSeverity.Error,
					           "Failed to find MethodInfo {0} for PropertyInfo {1} on TelemetryProvider {2}",
					           kvp.Value.MethodName, kvp.Value.Name, instance);

				KeyValuePair<EventInfo, EventTelemetryAttribute> eventAttributePair = default(KeyValuePair<EventInfo, EventTelemetryAttribute>);
				if (kvp.Value.EventName != null && !events.Remove(kvp.Value.EventName, out eventAttributePair))
					Logger.Log(eSeverity.Error,
					           "Failed to find EventInfo {0} for PropertyInfo {1} on TelemetryProvider {2}",
					           kvp.Value.EventName, kvp.Value.Name, instance);

				yield return new TelemetryLeaf(kvp.Value.Name, instance, kvp.Key, methodAttributePair.Key, eventAttributePair.Key);
			}

			// Loose Methods
			foreach (KeyValuePair<MethodInfo, MethodTelemetryAttribute> kvp in methods.Values)
				yield return new TelemetryLeaf(kvp.Value.Name, instance, null, kvp.Key, null);

			// Loose Events
			foreach (KeyValuePair<EventInfo, EventTelemetryAttribute> kvp in events.Values)
				yield return new TelemetryLeaf(kvp.Value.Name, instance, null, null, kvp.Key);
		}

		/// <summary>
		/// Generates the child telemetry nodes for the given provider.
		/// </summary>
		/// <param name="instance"></param>
		[NotNull]
		private IEnumerable<TelemetryProviderNode> InstantiateNodeTelemetry([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return
				NodeTelemetryAttribute
					.GetProperties(instance.GetType())
					.Select(kvp =>
					{
						try
						{
							return kvp.Value.InstantiateTelemetryNode(instance, kvp.Key, LazyLoadTelemetry);
						}
						catch (Exception e)
						{
							Logger.Log(eSeverity.Error, e.GetBaseException(), "Failed to instantiate node telemetry - {0} - {1}",
									   instance, kvp.Value.Name);
							return null;
						}
					})
					.Where(n => n != null);
		}

		/// <summary>
		/// Generates the collection telemetry nodes for the given provider.
		/// </summary>
		/// <param name="instance"></param>
		[NotNull]
		private IEnumerable<TelemetryCollection> InstantiateCollectionTelemetry([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return
				CollectionTelemetryAttribute
					.GetProperties(instance.GetType())
					.Select(kvp =>
					        {
						        try
						        {
							        return
								        new TelemetryCollection(kvp.Value.Name,
								                                instance,
								                                kvp.Key,
								                                UpdateCollectionTelemetry);
						        }
						        catch (Exception e)
						        {
							        Logger.Log(eSeverity.Error, e.GetBaseException(),
							                   "Failed to instantiate collection telemetry - {0} - {1}",
							                   instance, kvp.Value.Name);
							        return null;
						        }
					        })
					.Where(n => n != null);
		}

		/// <summary>
		/// Generates the external telemetry nodes for the given provider and adds them to the collection.
		/// </summary>
		/// <param name="instance"></param>
		private IEnumerable<ITelemetryNode> InstantiateExternalTelemetry([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return
				ExternalTelemetryAttribute
					.InstantiateExternalTelemetryProviders(instance)
					.SelectMany(e => InstantiateTelemetryNodes(e));
		}

		#endregion
	}
}
