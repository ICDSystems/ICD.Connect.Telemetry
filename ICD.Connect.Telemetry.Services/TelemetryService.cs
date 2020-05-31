using System;
using System.Collections.Generic;
using System.Linq;
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

			foreach (PropertyTelemetryNode node in InstantiatePropertyTelemetry(instance))
				yield return node;

			foreach (MethodTelemetryNode node in InstantiateMethodTelemetry(instance))
				yield return node;

			foreach (TelemetryCollection node in InstantiateCollectionTelemetry(instance))
				yield return node;

			foreach (ITelemetryNode node in InstantiateExternalTelemetry(instance))
				yield return node;
		}

		/// <summary>
		/// Generates the property telemetry nodes for the given provider.
		/// </summary>
		/// <param name="instance"></param>
		[NotNull]
		private IEnumerable<PropertyTelemetryNode> InstantiatePropertyTelemetry([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return
				PropertyTelemetryAttribute
					.GetProperties(instance.GetType())
					.Select(kvp =>
					{
						try
						{
							return kvp.Value.InstantiateTelemetryItem(instance, kvp.Key);
						}
						catch (Exception e)
						{
							Logger.Log(eSeverity.Error, e.GetBaseException(),
							           "Failed to instantiate telemetry node - {0} - {1}",
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
		/// Generates the method telemetry nodes for the given provider.
		/// </summary>
		/// <param name="instance"></param>
		[NotNull]
		private IEnumerable<MethodTelemetryNode> InstantiateMethodTelemetry([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return
				MethodTelemetryAttribute
					.GetMethods(instance.GetType())
					.Select(kvp =>
					{
						try
						{
							return kvp.Value.InstantiateTelemetryItem(instance, kvp.Key);
						}
						catch (Exception e)
						{
							Logger.Log(eSeverity.Error, e.GetBaseException(), "Failed to instantiate method telemetry - {0} - {1}",
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
