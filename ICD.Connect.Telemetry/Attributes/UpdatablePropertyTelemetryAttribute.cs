using System;
using Crestron.SimplSharp.Reflection;
using ICD.Common.Utils;

namespace ICD.Connect.Telemetry.Attributes
{
	public sealed class UpdatablePropertyTelemetryAttribute : AbstractUpdatablePropertyTelemetryAttribute
	{
		public UpdatablePropertyTelemetryAttribute(string name) : base(name)
		{
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public override ITelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, PropertyInfo propertyInfo)
		{
			Type type = typeof(UpdatableTelemetryNodeItem<>).MakeGenericType(propertyInfo.PropertyType);

			return (ITelemetryItem)ReflectionUtils.CreateInstance(type, new object[] { Name, instance, propertyInfo });
		}
	}
}