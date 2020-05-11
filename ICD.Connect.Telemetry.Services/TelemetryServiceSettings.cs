using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Services;

namespace ICD.Connect.Telemetry.Services
{
	[KrangSettings("Telemetry", typeof(TelemetryService))]
	public sealed class TelemetryServiceSettings : AbstractServiceSettings, ITelemetryServiceSettings
	{
	}
}
