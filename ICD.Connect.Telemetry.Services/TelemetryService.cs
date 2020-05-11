using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Settings.Services;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Nodes.Collections;

namespace ICD.Connect.Telemetry.Services
{
	public sealed class TelemetryService : AbstractService<ITelemetryService, TelemetryServiceSettings>, ITelemetryService
	{
		private readonly WeakKeyDictionary<ITelemetryProvider, ITelemetryCollection> m_TelemetryProviders;
		private readonly SafeCriticalSection m_TelemetryProvidersSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public TelemetryService()
		{
			m_TelemetryProviders = new WeakKeyDictionary<ITelemetryProvider, ITelemetryCollection>();
			m_TelemetryProvidersSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Attempts to lazy-load the telemetry nodes for a given provider. 
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		[NotNull]
		public ITelemetryCollection GetTelemetryForProvider(ITelemetryProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			m_TelemetryProvidersSection.Enter();

			try
			{
				return m_TelemetryProviders.GetOrAddNew(provider, () => TelemetryUtils.InstantiateTelemetry(provider));
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

			return GetTelemetryForProvider(provider).GetChildByName(name);
		}
	}
}
