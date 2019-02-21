using System.Collections.Generic;

namespace ICD.Connect.Telemetry
{
	public interface IExternalTelemetryProvider : ITelemetryProvider
	{
		void SetParent(ITelemetryProvider provider); 
	}
}