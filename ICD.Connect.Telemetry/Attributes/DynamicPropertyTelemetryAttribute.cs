using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Attributes
{
	public sealed class DynamicPropertyTelemetryAttribute : AbstractPropertyTelemetryAttribute
	{
		public string EventName { get; private set; }

		public DynamicPropertyTelemetryAttribute(string name, string updateEventName) 
			: base(name)
		{
			EventName = updateEventName;
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public override IFeedbackTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, PropertyInfo propertyInfo)
		{
			IcdHashSet<Type> types = instance.GetType().GetAllTypes().ToIcdHashSet();
#if SIMPLSHARP
			IEnumerable<CType> cTypes = types.Select(t => t.GetCType());
			IEnumerable<EventInfo> eventInfos = cTypes.SelectMany(c => c.GetEvents());
#else
			IEnumerable<EventInfo> eventInfos = types.SelectMany(t=> t.GetEvents());
#endif
			EventInfo eventInfo = eventInfos.FirstOrDefault(info => info.GetCustomAttributes<EventTelemetryAttribute>()
			                                                            .Any(attr => attr.Name == EventName));

			if (eventInfo == null)
				throw new InvalidOperationException(string.Format("Couldn't find event with name {0}", EventName));

			Type type = typeof(DynamicTelemetryNodeItem<>).MakeGenericType(propertyInfo.PropertyType);

			return (IFeedbackTelemetryItem)ReflectionUtils.CreateInstance(type, new object[] {Name, instance, eventInfo, propertyInfo});
		}
	}
}