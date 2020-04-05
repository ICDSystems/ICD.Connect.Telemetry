using System;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Utils;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	public sealed class UpdatablePropertyTelemetryAttribute : AbstractUpdatablePropertyTelemetryAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="setTelemetryName"></param>
		public UpdatablePropertyTelemetryAttribute(string name, string setTelemetryName)
			: base(name, setTelemetryName)
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

			return (IFeedbackTelemetryItem)ReflectionUtils.CreateInstance(type, new object[] { Name, instance, propertyInfo, SetTelemetryName });
		}
	}
}