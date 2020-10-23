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
		TelemetryProviderNode LazyLoadCoreTelemetry();

		/// <summary>
		/// Disposes the root telemetry and all child telemetry nodes recursively.
		/// </summary>
		void DeinitializeCoreTelemetry();

		/// <summary>
		/// Returns previously loaded telemetry for the given provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="telemetryCollection"></param>
		/// <returns></returns>
		bool TryGetTelemetryForProvider([NotNull] ITelemetryProvider provider, out TelemetryProviderNode telemetryCollection);
	}
}
