﻿#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using System;
using ICD.Common.Properties;
using ICD.Common.Utils;

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class DynamicTelemetryNodeItem<T> : AbstractUpdatableTelemetryNodeItem<T>
	{
		private readonly Delegate m_Delegate;
		private readonly EventInfo m_EventInfo;

		[NotNull]
		private MethodInfo CallbackMethodInfo
		{
			get
			{
				MethodInfo output = GetType()
#if SIMPLSHARP
					.GetCType()
#else
					.GetTypeInfo()
#endif
					.GetMethod("EventCallback", BindingFlags.NonPublic | BindingFlags.Instance);

				if (output == null)
					throw new InvalidProgramException();

				return output;
			}
		}

		public DynamicTelemetryNodeItem(string name, ITelemetryProvider parent, EventInfo eventInfo, PropertyInfo propertyInfo)
			: base(name, parent, propertyInfo)
		{
			if(propertyInfo.PropertyType != typeof(T))
				throw new InvalidOperationException(string.Format("Cannot instantiate a telemetry node item of type {0} for property with type {1}", typeof(T), propertyInfo.PropertyType));

			m_EventInfo = eventInfo;
			m_Delegate = ReflectionUtils.SubscribeEvent(parent, m_EventInfo, this, CallbackMethodInfo);

			Update();
		}

		public override void Dispose()
		{
			ReflectionUtils.UnsubscribeEvent(Parent, m_EventInfo, m_Delegate);

			base.Dispose();
		}

		[UsedImplicitly]
		private void EventCallback(object sender, object args)
		{
			Update();
		}
	}
}