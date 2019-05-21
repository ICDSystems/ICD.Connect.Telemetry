using System;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public abstract class AbstractMethodTelemetryAttribute : AbstractTelemetryAttribute, IMethodTelemetryAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		protected AbstractMethodTelemetryAttribute(string name)
			: base(name)
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