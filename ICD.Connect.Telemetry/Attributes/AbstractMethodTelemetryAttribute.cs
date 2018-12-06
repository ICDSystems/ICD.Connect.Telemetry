using System;
using Crestron.SimplSharp.Reflection;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public abstract class AbstractMethodTelemetryAttribute : AbstractTelemetryAttribute, IMethodTelemetryAttribute
	{
		protected AbstractMethodTelemetryAttribute(string name) : base(name)
		{
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="methodInfo"></param>
		/// <returns></returns>
		public abstract IManagementTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, MethodInfo methodInfo);
	}
}