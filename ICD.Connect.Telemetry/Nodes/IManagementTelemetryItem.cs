#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Nodes
{
	public interface IManagementTelemetryItem : ITelemetryItem
	{
		/// <summary>
		/// Gets the info for the method.
		/// </summary>
		[NotNull]
		MethodInfo MethodInfo { get; }

		/// <summary>
		/// Gets the info for the first method parameter.
		/// </summary>
		[CanBeNull]
		ParameterInfo ParameterInfo { get; }

		/// <summary>
		/// Calls the method with the given parameters.
		/// </summary>
		/// <param name="parameters"></param>
		void Invoke(params object[] parameters);
	}
}