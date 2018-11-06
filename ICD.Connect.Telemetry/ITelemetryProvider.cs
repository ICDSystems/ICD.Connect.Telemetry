namespace ICD.Connect.Telemetry
{
	public interface ITelemetryProvider
	{
		ITelemetryCollection Telemetry { get; }
	}
}