namespace ICD.Connect.Telemetry.Providers
{
	public interface ITelemetryProvider
	{
		/// <summary>
		/// Initializes the current telemetry state.
		/// </summary>
		void InitializeTelemetry();
	}
}