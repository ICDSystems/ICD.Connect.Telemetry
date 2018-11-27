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
	public sealed class DynamicTelemetryNodeItem<T> : AbstractUpdatableTelemetryNodeItem<T>
	{
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

		public DynamicTelemetryNodeItem(string name, object parent, EventInfo eventInfo, PropertyInfo propertyInfo)
			: base(name, parent, propertyInfo)
		{
			if(propertyInfo.PropertyType != typeof(T))
				throw new InvalidOperationException(string.Format("Cannot instantiate a telemetry node item of type {0} for property with type {1}", typeof(T), propertyInfo.PropertyType));

			IcdConsole.PrintLine(eConsoleColor.Magenta, 
				"Parent Type: " + parent.GetType() + IcdEnvironment.NewLine +
				"--Event Info--" + IcdEnvironment.NewLine +
				"Declaring Type: " + eventInfo.DeclaringType + IcdEnvironment.NewLine +
				"Event Handler Type: " + eventInfo.EventHandlerType + IcdEnvironment.NewLine +
				"--This--" + IcdEnvironment.NewLine +
				"Typeof This: " + GetType() + IcdEnvironment.NewLine +
				"--CallbackMethodInfo--" + IcdEnvironment.NewLine +
				"Name: " + CallbackMethodInfo.Name +IcdEnvironment.NewLine +
				"Declaring Type: " + CallbackMethodInfo.DeclaringType
				);

			ReflectionUtils.SubscribeEvent(parent, eventInfo, this, CallbackMethodInfo);
		}

		[UsedImplicitly]
		private void EventCallback(object sender, object args)
		{
			Update();
		}
	}
}