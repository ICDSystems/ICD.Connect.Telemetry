using System;
using ICD.Connect.Telemetry.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Attributes
{
	public interface IMethodTelemetryAttribute : ITelemetryAttribute
	{
		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="methodInfo"></param>
		/// <returns></returns>
		[NotNull]
		IManagementTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, MethodInfo methodInfo);
	}
}