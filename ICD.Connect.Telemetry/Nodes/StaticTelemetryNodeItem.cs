namespace ICD.Connect.Telemetry.Nodes
 {
	public sealed class StaticTelemetryNodeItem<T> : AbstractFeedbackTelemetryNodeItem<T>
	{
		public StaticTelemetryNodeItem(string name, T value)
			: base(name)
		{
			Value = value;
		}
	}
}