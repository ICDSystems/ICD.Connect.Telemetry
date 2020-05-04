using ICD.Common.Properties;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Nodes.Collections;

namespace ICD.Connect.Telemetry.Service
{
	public interface ITelemetryService
	{
		[NotNull]
		ITelemetryCollection GetTelemetryForProvider(ITelemetryProvider provider);

		[CanBeNull]
		ITelemetryItem GetTelemetryForProvider(ITelemetryProvider provider, string name);
	}
}
