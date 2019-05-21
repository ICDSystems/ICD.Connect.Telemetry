using System;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
	public sealed class EventTelemetryAttribute : AbstractTelemetryAttribute, IEventTelemetryAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		public EventTelemetryAttribute(string name) 
			: base(name)
		{
		}
	}
}