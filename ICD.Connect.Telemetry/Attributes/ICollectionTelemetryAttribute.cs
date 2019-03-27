#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Properties;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	public interface ICollectionTelemetryAttribute : ITelemetryAttribute
	{
		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		[NotNull]
		ICollectionTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, PropertyInfo propertyInfo);
	}
}