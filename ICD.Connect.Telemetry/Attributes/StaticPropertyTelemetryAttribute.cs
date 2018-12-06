using System;
using ICD.Connect.Telemetry.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Utils;

namespace ICD.Connect.Telemetry.Attributes
{
	public sealed class StaticPropertyTelemetryAttribute : AbstractPropertyTelemetryAttribute
	{
		public StaticPropertyTelemetryAttribute(string name) : base(name)
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
			Type type = typeof(StaticTelemetryNodeItem<>).MakeGenericType(propertyInfo.PropertyType);

			return (IFeedbackTelemetryItem)ReflectionUtils.CreateInstance(type, new[] { Name, propertyInfo.GetValue(instance, null)});
		}
	}
}