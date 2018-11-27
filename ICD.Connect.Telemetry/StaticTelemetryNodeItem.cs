namespace ICD.Connect.Telemetry
 {
	public sealed class StaticTelemetryNodeItem<T> : AbstractTelemetryNodeItem<T>
	{
		public StaticTelemetryNodeItem(string name, T value)
			: base(name)
		{
			Value = value;
		}
	}
}