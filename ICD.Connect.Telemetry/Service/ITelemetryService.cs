using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Service
{
	public interface ITelemetryService
	{
		void AddTelemetryProvider(ITelemetryProvider provider);
		void RemoveTelemetryProvider(ITelemetryProvider provider);

		IEnumerable<ITelemetryProvider> GetTelemetryProviders();
		ITelemetryCollection GetTelemetryForProvider(ITelemetryProvider provider);

		[CanBeNull]
		ITelemetryItem GetTelemetryForProvider(ITelemetryProvider provider, string name);
	}
}
