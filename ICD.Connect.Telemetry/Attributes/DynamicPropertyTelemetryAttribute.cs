using System;
using ICD.Common.Utils;
using ICD.Connect.Telemetry.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Attributes
{
	public sealed class DynamicPropertyTelemetryAttribute : AbstractUpdateablePropertyTelemetryAttribute
	{
		private readonly string m_EventName;

		/// <summary>
		/// Gets the associated event name for this attribute.
		/// </summary>
		public string EventName { get { return m_EventName; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="setTelemetryName"></param>
		/// <param name="updateEventName"></param>
		public DynamicPropertyTelemetryAttribute(string name, string setTelemetryName, string updateEventName) 
			: base(name, setTelemetryName)
		{
			m_EventName = updateEventName;
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public override IFeedbackTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, PropertyInfo propertyInfo)
		{
			EventInfo eventInfo = TelemetryUtils.GetEventInfo(instance, EventName);
			Type type = typeof(DynamicTelemetryNodeItem<>).MakeGenericType(propertyInfo.PropertyType);

			return (IFeedbackTelemetryItem)ReflectionUtils.CreateInstance(type, new object[] {Name, instance, eventInfo, propertyInfo, SetTelemetryName});
		}
	}
}