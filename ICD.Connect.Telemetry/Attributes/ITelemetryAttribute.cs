using ICD.Common.Utils.Attributes;

namespace ICD.Connect.Telemetry.Attributes
{
	public interface ITelemetryAttribute : IIcdAttribute
	{
		/// <summary>
		/// Gets the name for the attribute.
		/// </summary>
		string Name { get; }
	}
}