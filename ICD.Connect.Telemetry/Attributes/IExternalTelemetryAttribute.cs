using ICD.Common.Properties;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	public interface IExternalTelemetryAttribute : ITelemetryAttribute
	{
		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		[NotNull]
		IExternalTelemetryProvider InstantiateTelemetryItem(ITelemetryProvider instance);
	}
}