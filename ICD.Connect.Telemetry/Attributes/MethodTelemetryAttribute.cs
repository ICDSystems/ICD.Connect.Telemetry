using System;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	public sealed class MethodTelemetryAttribute : AbstractMethodTelemetryAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		public MethodTelemetryAttribute(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="methodInfo"></param>
		/// <returns></returns>
		public override IManagementTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, MethodInfo methodInfo)
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();

			if (parameters.Length > 1)
				throw new NotSupportedException("Method Telemetry is unsupported for methods with 2 or more parameters");

			return new MethodTelemetryNodeItem(Name, instance, methodInfo);
		}
	}
}
