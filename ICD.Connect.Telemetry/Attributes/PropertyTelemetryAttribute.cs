using System;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class PropertyTelemetryAttribute : AbstractTelemetryAttribute
	{
		public string EventName { get; private set; }

		public PropertyTelemetryAttribute(string name) 
			: base(name)
		{

		}

		public PropertyTelemetryAttribute(string name, string updateEventName) 
			: base(name)
		{
			EventName = updateEventName;
		}
	}
}