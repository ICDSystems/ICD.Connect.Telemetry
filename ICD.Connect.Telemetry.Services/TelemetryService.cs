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
		private readonly WeakKeyDictionary<ITelemetryProvider, TelemetryCollection> m_TelemetryProviders;
		private readonly SafeCriticalSection m_TelemetryProvidersSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public TelemetryService()
		{
			m_TelemetryProviders = new WeakKeyDictionary<ITelemetryProvider, TelemetryCollection>();
			m_TelemetryProvidersSection = new SafeCriticalSection();
		}

#region Methods

		/// <summary>
		/// Lazy-loads telemetry for the core and returns the root telemetry.
		/// </summary>
		/// <returns></returns>
		public TelemetryCollection InitializeCoreTelemetry()
		{
			return LazyLoadTelemetry("Core", Core);
		}

		/// <summary>
		/// Returns previously loaded telemetry for the given provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		[NotNull]
		public TelemetryCollection GetTelemetryForProvider([NotNull] ITelemetryProvider provider)
		{
			return m_TelemetryProvidersSection.Execute(() => m_TelemetryProviders[provider]);
		}

#endregion

#region Private Methods

		/// <summary>
		/// Lazy-loads telemetry recursively for a given provider. 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		[NotNull]
		private TelemetryCollection LazyLoadTelemetry([NotNull] string name, [NotNull] ITelemetryProvider provider)
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
		private TelemetryCollection InstantiateTelemetry([NotNull] string name, [NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			IEnumerable<ITelemetryNode> children = InstantiateTelemetryNodes(instance);
			return new TelemetryCollection(name, instance, children);
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

			Dictionary<string, MethodInfo> methods =
				MethodTelemetryAttribute.GetMethods(instance.GetType())
				                        .ToDictionary(kvp => kvp.Value.Name, kvp => kvp.Key);

			Dictionary<string, EventInfo> events =
				EventTelemetryAttribute.GetEvents(instance.GetType())
				                       .ToDictionary(kvp => kvp.Value.Name, kvp => kvp.Key);

			// Property attributes point to events and methods, so start with those
			foreach (KeyValuePair<PropertyInfo, PropertyTelemetryAttribute> kvp in properties.Values)
			{
				MethodInfo methodInfo = default(MethodInfo);
				if (kvp.Value.MethodName != null && !methods.Remove(kvp.Value.MethodName, out methodInfo))
					Logger.Log(eSeverity.Error,
					           "Failed to find MethodInfo {0} for PropertyInfo {1} on TelemetryProvider {2}",
					           kvp.Value.MethodName, kvp.Value.Name, instance);

				EventInfo eventInfo = default(EventInfo);
				if (kvp.Value.EventName != null && !events.Remove(kvp.Value.EventName, out eventInfo))
					Logger.Log(eSeverity.Error,
					           "Failed to find EventInfo {0} for PropertyInfo {1} on TelemetryProvider {2}",
					           kvp.Value.EventName, kvp.Value.Name, instance);

				yield return new TelemetryLeaf(kvp.Value.Name, instance, kvp.Key, methodInfo, eventInfo);
			}

			// Loose Methods
			foreach (MethodInfo methodInfo in methods.Values)
				yield return new TelemetryLeaf(methodInfo.Name, instance, null, methodInfo, null);

			// Loose Events
			foreach (EventInfo eventInfo in events.Values)
				yield return new TelemetryLeaf(eventInfo.Name, instance, null, null, eventInfo);
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
							IEnumerable<ITelemetryNode> children =
								kvp.Value.InstantiateTelemetryNodes(instance, kvp.Key, LazyLoadTelemetry);
							return new TelemetryCollection(kvp.Value.Name, instance, children);
						}
						catch (Exception e)
						{
							Logger.Log(eSeverity.Error, e.GetBaseException(), "Failed to instantiate collection telemetry - {0} - {1}",
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
