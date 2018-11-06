using System;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils;

namespace ICD.Connect.Telemetry
{
	public sealed class CallbackTelemetryNodeItem<T> : AbstractUpdatableTelemetryNodeItem<T>
	{
		private Delegate m_UpdateDelegate;

		private MethodInfo CallbackMethodInfo
		{
			get
			{
				return GetType()
#if SIMPLSHARP
					.GetCType()
#else
					.GetTypeInfo()
#endif
					.GetMethod("Update");
			}
		}

		public CallbackTelemetryNodeItem(string name, object parent, string eventName, Func<T> updateFunction)
			: base(name, updateFunction)
		{
			EventInfo eventInfo = GetEventInfo(parent, eventName);

			Delegate del = ReflectionUtils.SubscribeEvent(parent, eventInfo, this, CallbackMethodInfo);
			m_UpdateDelegate = del;
		}

		[NotNull]
		private EventInfo GetEventInfo(object parent, string eventName)
		{
			// TODO - Lookup event attribute by name on the given instance
			throw new NotImplementedException();
		}

		public override void Update()
		{
			throw new NotImplementedException();
		}
	}
}