namespace ICD.Connect.Telemetry.Attributes
{
	public interface ITelemetryAttribute
	{
		/// <summary>
		/// Gets the name for the attribute.
		/// </summary>
		string Name { get; }
	}
}