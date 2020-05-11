using ICD.Common.Properties;
using ICD.Connect.Settings.Services;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Nodes.Collections;

namespace ICD.Connect.Telemetry.Services
{
	public interface ITelemetryService : IService
	{
		[NotNull]
		ITelemetryCollection GetTelemetryForProvider(ITelemetryProvider provider);

		[CanBeNull]
		ITelemetryItem GetTelemetryForProvider(ITelemetryProvider provider, string name);
	}
}
