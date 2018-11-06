using System;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public class MethodTelemetryAttribute : AbstractTelemetryAttribute
	{
		public MethodTelemetryAttribute(string name) 
			: base(name)
		{
		}
	}
}