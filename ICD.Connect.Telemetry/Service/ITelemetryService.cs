using System.Collections.Generic;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Service
{
	public interface ITelemetryService
	{
		void AddTelemetryProvider(ITelemetryProvider provider);
		void RemoveTelemetryProvider(ITelemetryProvider provider);

		IEnumerable<ITelemetryProvider> GetTelemetryProviders();
		ITelemetryCollection GetTelemetryForProvider(ITelemetryProvider provider);
		ITelemetryItem GetTelemetryForProvider(ITelemetryProvider provider, string name);
	}
}