using System;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
	public sealed class TelemetryEventAttribute : AbstractTelemetryAttribute
	{
		public TelemetryEventAttribute(string name) 
			: base(name)
		{
		}
	}
}