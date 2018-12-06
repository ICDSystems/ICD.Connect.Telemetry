using System;

namespace ICD.Connect.Telemetry
{
	public interface ITelemetryProvider
	{
		event EventHandler OnRequestTelemetryRebuild;

		ITelemetryCollection Telemetry { get; }
	}
}