using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Telemetry.Service
{
	public sealed class TelemetryService: ITelemetryService
	{
		private readonly Dictionary<ITelemetryProvider, ITelemetryCollection> m_Telemetries; 

		public TelemetryService()
		{
			m_Telemetries = new Dictionary<ITelemetryProvider, ITelemetryCollection>();
		}

		/// <summary>
		/// Adds a telemetry provider to be tracked. When accessed, its nodes will be lazy-loaded.
		/// </summary>
		/// <param name="provider"></param>
		public void AddTelemetryProvider(ITelemetryProvider provider)
		{
			if (m_Telemetries.ContainsKey(provider))
				throw new ArgumentException("Already contains telemetry provider");

			m_Telemetries.Add(provider, null);
			provider.OnRequestTelemetryRebuild += ProviderOnRequestTelemetryRebuild;
		}

		/// <summary>
		/// Removes a telemetry provider and its associated nodes.
		/// </summary>
		/// <param name="provider"></param>
		public void RemoveTelemetryProvider(ITelemetryProvider provider)
		{
			provider.OnRequestTelemetryRebuild -= ProviderOnRequestTelemetryRebuild;
			m_Telemetries.Remove(provider);
		}

		/// <summary>
		/// Returns all active telemetry providers.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ITelemetryProvider> GetTelemetryProviders()
		{
			return m_Telemetries.Keys.ToArray(m_Telemetries.Count);
		}

		/// <summary>
		/// Attempts to lazy-load the telemetry nodes for a given provider. 
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		[NotNull]
		public ITelemetryCollection GetTelemetryForProvider(ITelemetryProvider provider)
		{
			if (m_Telemetries.ContainsKey(provider))
				return m_Telemetries[provider] ?? (m_Telemetries[provider] = TelemetryUtils.InstantiateTelemetry(provider));
			
			throw new KeyNotFoundException("Provider not added to the telemetry service.");
		}

		private void ProviderOnRequestTelemetryRebuild(object sender, EventArgs eventArgs)
		{
			var provider = sender as ITelemetryProvider;
			if (provider == null || !m_Telemetries.ContainsKey(provider))
				return;

			// If the telemetry needs a rebuild, invalidate the cached nodes.
			m_Telemetries[provider] = null;
		}
	}
}