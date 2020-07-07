namespace ICD.Connect.Telemetry.Providers
{
	public abstract class AbstractTelemetryProvider : ITelemetryProvider
	{
		/// <summary>
		/// Initializes the current telemetry state.
		/// </summary>
		public virtual void InitializeTelemetry()
		{
		}
	}
}
