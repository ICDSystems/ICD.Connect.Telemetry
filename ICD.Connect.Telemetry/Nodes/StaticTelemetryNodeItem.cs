using Crestron.SimplSharp.Reflection;

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