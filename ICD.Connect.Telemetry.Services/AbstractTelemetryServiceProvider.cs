using ICD.Connect.Settings.Services;

namespace ICD.Connect.Telemetry.Services
{
	public abstract class AbstractTelemetryServiceProvider<TSettings> : AbstractServiceProvider<TSettings>, ITelemetryServiceProvider
		where TSettings : ITelemetryServiceProviderSettings, new()
	{
	}
}
