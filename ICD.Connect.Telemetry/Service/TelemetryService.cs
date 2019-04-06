using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Service
{
	public sealed class TelemetryService: ITelemetryService
	{
		private readonly Dictionary<ITelemetryProvider, ITelemetryCollection> m_TelemetryProviders;
		private readonly SafeCriticalSection m_TelemetryProvidersSection;

		public TelemetryService()
		{
			m_TelemetryProviders = new Dictionary<ITelemetryProvider, ITelemetryCollection>();
			m_TelemetryProvidersSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Adds a telemetry provider to be tracked. When accessed, its nodes will be lazy-loaded.
		/// </summary>
		/// <param name="provider"></param>
		public void AddTelemetryProvider(ITelemetryProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			m_TelemetryProvidersSection.Enter();

			try
			{
				if (m_TelemetryProviders.ContainsKey(provider))
					throw new ArgumentException("Already contains telemetry provider");

				m_TelemetryProviders.Add(provider, null);
				provider.OnRequestTelemetryRebuild += ProviderOnRequestTelemetryRebuild;
			}
			finally
			{
				m_TelemetryProvidersSection.Leave();
			}
		}

		/// <summary>
		/// Removes a telemetry provider and its associated nodes.
		/// </summary>
		/// <param name="provider"></param>
		public void RemoveTelemetryProvider(ITelemetryProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			m_TelemetryProvidersSection.Enter();

			try
			{
				if (m_TelemetryProviders.Remove(provider))
					provider.OnRequestTelemetryRebuild -= ProviderOnRequestTelemetryRebuild;
			}
			finally
			{
				m_TelemetryProvidersSection.Leave();
			}
		}

		/// <summary>
		/// Returns all active telemetry providers.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ITelemetryProvider> GetTelemetryProviders()
		{
			return m_TelemetryProvidersSection.Execute(() => m_TelemetryProviders.Keys.ToArray(m_TelemetryProviders.Count));
		}

		/// <summary>
		/// Attempts to lazy-load the telemetry nodes for a given provider. 
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		[CanBeNull]
		public ITelemetryCollection GetTelemetryForProvider(ITelemetryProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			m_TelemetryProvidersSection.Enter();

			try
			{
				ITelemetryCollection collection;
				if (!m_TelemetryProviders.TryGetValue(provider, out collection))
				{
					AddTelemetryProvider(provider);

					collection = TelemetryUtils.InstantiateTelemetry(provider);
					m_TelemetryProviders[provider] = collection;
				}

				return collection;
			}
			finally
			{
				m_TelemetryProvidersSection.Leave();
			}
		}

		/// <summary>
		/// Attempts to lazy-load the telemetry nodes for a given provider. 
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[CanBeNull]
		public ITelemetryItem GetTelemetryForProvider(ITelemetryProvider provider, string name)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			ITelemetryCollection collection = GetTelemetryForProvider(provider);
			return collection == null ? null : collection.GetChildByName(name);
		}

		private void ProviderOnRequestTelemetryRebuild(object sender, EventArgs eventArgs)
		{
			ITelemetryProvider provider = sender as ITelemetryProvider;
			if (provider == null)
				return;

			m_TelemetryProvidersSection.Enter();

			try
			{
				// If the telemetry needs a rebuild, invalidate the cached nodes.
				if (m_TelemetryProviders.ContainsKey(provider))
					m_TelemetryProviders[provider] = null;
			}
			finally
			{
				m_TelemetryProvidersSection.Leave();
			}
		}
	}
}