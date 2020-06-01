using ICD.Common.Logging.Activities;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

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

	public sealed class ActivityExternalTelemetryProvider : AbstractExternalTelemetryProvider<IActivityTelemetryProvider>
	{
	}
}
