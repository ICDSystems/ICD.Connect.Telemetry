using System;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
	public sealed class EventTelemetryAttribute : AbstractTelemetryAttribute, IEventTelemetryAttribute
	{
		public EventTelemetryAttribute(string name) 
			: base(name)
		{
		}
	}
}