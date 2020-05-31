using ICD.Common.Properties;
using ICD.Connect.Settings.Services;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Services
{
	public interface ITelemetryService : IService
	{
		/// <summary>
		/// Lazy-loads telemetry for the core and returns the root telemetry.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		TelemetryCollection InitializeCoreTelemetry();

		/// <summary>
		/// Lazy-loads telemetry for the core and returns telemetry for the given provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		[NotNull]
		TelemetryCollection GetTelemetryForProvider([NotNull] ITelemetryProvider provider);
	}
}
