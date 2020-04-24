using ICD.Common.Properties;
using ICD.Connect.Telemetry.Nodes;

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
