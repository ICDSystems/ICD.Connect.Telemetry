namespace ICD.Connect.Telemetry.Mappings
{
	public interface ITelemetryMapping
	{
		string TelemetrySetName { get; set; }
		string TelemetryGetName { get; set; }
		eTelemetryIoMask IoMask { get; }
	}
}
