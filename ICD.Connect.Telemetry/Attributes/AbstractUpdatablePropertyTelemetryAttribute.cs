namespace ICD.Connect.Telemetry.Attributes
{
	public abstract class AbstractUpdatablePropertyTelemetryAttribute : AbstractPropertyTelemetryAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		protected AbstractUpdatablePropertyTelemetryAttribute(string name)
			: base(name)
		{
		}
	}
}