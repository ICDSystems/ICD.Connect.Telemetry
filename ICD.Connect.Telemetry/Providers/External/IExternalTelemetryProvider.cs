using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Providers.External
{
	public interface IExternalTelemetryProvider : ITelemetryProvider
	{
		/// <summary>
		/// Gets the parent telemetry provider that this provider extends.
		/// </summary>
		[CanBeNull]
		ITelemetryProvider Parent { get; }

		/// <summary>
		/// Sets the parent telemetry provider that this instance extends.
		/// </summary>
		/// <param name="provider"></param>
		void SetParent([CanBeNull] ITelemetryProvider provider);
	}
}
