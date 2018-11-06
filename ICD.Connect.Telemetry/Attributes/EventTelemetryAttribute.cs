using System;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
	public class EventTelemetryAttribute : AbstractTelemetryAttribute
	{
		public EventTelemetryAttribute(string name) 
			: base(name)
		{
		}
	}
}