using ICD.Common.Logging.Activities;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Telemetry.Providers
{
	[ExternalTelemetry("Activities", typeof(ActivityExternalTelemetryProvider))]
	public interface IActivityTelemetryProvider : ITelemetryProvider
	{
		/// <summary>
		/// Gets the activities for this instance.
		/// </summary>
		IActivityContext Activities { get; }
	}
}
