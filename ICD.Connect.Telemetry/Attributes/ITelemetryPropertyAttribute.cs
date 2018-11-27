using Crestron.SimplSharp.Reflection;
using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Attributes
{
	public interface ITelemetryPropertyAttribute : ITelemetryAttribute
	{
		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		[NotNull]
		ITelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, PropertyInfo propertyInfo);
	}
}