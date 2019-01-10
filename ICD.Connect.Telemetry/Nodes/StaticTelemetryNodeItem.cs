#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
 {
	public sealed class StaticTelemetryNodeItem<T> : AbstractFeedbackTelemetryNodeItem<T>
	{
		public StaticTelemetryNodeItem(string name, T value, PropertyInfo propertyInfo)
			: base(name, propertyInfo)
		{
			Value = value;
		}
	}
}