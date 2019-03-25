#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
 {
	public sealed class StaticTelemetryNodeItem<T> : AbstractFeedbackTelemetryNodeItem<T>
	{
		public StaticTelemetryNodeItem(string name, T value, ITelemetryProvider parent, PropertyInfo propertyInfo)
			: base(name, parent, propertyInfo)
		{
			Value = value;
		}
	}
}