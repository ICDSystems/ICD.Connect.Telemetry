using System;
using Crestron.SimplSharp.Reflection;
using ICD.Common.Utils;
using ICD.Connect.Telemetry.Nodes;

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
		public override IFeedbackTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, PropertyInfo propertyInfo)
		{
			Type type = typeof(UpdatableTelemetryNodeItem<>).MakeGenericType(propertyInfo.PropertyType);

			return (IFeedbackTelemetryItem)ReflectionUtils.CreateInstance(type, new object[] { Name, instance, propertyInfo });
		}
	}
}