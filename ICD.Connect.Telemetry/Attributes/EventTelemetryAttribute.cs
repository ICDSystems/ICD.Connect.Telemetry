using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Comparers;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
	public sealed class EventTelemetryAttribute : AbstractTelemetryAttribute
	{
		private const BindingFlags BINDING_FLAGS =
			BindingFlags.Instance | // Non-static classes
			BindingFlags.Static | // Static classes
			BindingFlags.Public | // Public members only
			BindingFlags.DeclaredOnly; // No inherited members

		private static readonly Dictionary<Type, Dictionary<EventInfo, EventTelemetryAttribute>> s_TypeToEventInfo;
		private static readonly SafeCriticalSection s_CacheSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		static EventTelemetryAttribute()
		{
			s_TypeToEventInfo = new Dictionary<Type, Dictionary<EventInfo, EventTelemetryAttribute>>();
			s_CacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		public EventTelemetryAttribute(string name) 
			: base(name)
		{
		}

		/// <summary>
		/// Gets the events on the given type decorated for telemetry.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<EventInfo, EventTelemetryAttribute>> GetEvents([NotNull] Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				Dictionary<EventInfo, EventTelemetryAttribute> eventInfos;

				if (!s_TypeToEventInfo.TryGetValue(type, out eventInfos))
				{
					eventInfos = new Dictionary<EventInfo, EventTelemetryAttribute>(TelemetryEventInfoEqualityComparer.Instance);
					s_TypeToEventInfo.Add(type, eventInfos);

					// First get the inherited events
					IEnumerable<KeyValuePair<EventInfo, EventTelemetryAttribute>> inheritedEvents =
						type.GetAllTypes()
							.Except(type)
							.SelectMany(t => GetEvents(t));

					// Then get the events for this type
					IEnumerable<KeyValuePair<EventInfo, EventTelemetryAttribute>> typeEvents =
#if SIMPLSHARP
 ((CType)type)
#else
						type.GetTypeInfo()
#endif
.GetEvents(BINDING_FLAGS)
							.Select(e => new KeyValuePair<EventInfo, EventTelemetryAttribute>(e, GetTelemetryAttribute(e)))
							.Where(kvp => kvp.Value != null);

					// Then insert everything - Type events win
					foreach (KeyValuePair<EventInfo, EventTelemetryAttribute> kvp in inheritedEvents.Concat(typeEvents))
						eventInfos[kvp.Key] = kvp.Value;
				}

				return eventInfos;
			}
			finally
			{
				s_CacheSection.Leave();
			}
		}

		[CanBeNull]
		private static EventTelemetryAttribute GetTelemetryAttribute([NotNull] EventInfo eventInfo)
		{
			if (eventInfo == null)
				throw new ArgumentException("eventInfo");

			// ReSharper disable InvokeAsExtensionMethod
			return ReflectionExtensions.GetCustomAttributes<EventTelemetryAttribute>(eventInfo, true).FirstOrDefault();
			// ReSharper restore InvokeAsExtensionMethod
		}
	}
}
