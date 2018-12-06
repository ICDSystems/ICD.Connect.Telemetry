using System.Collections.Generic;

namespace ICD.Connect.Telemetry.Service
{
	public interface ITelemetryService
	{
		void AddTelemetryProvider(ITelemetryProvider provider);
		void RemoveTelemetryProvider(ITelemetryProvider provider);

		IEnumerable<ITelemetryProvider> GetTelemetryProviders();
		ITelemetryCollection GetTelemetryForProvider(ITelemetryProvider provider);
	}
}