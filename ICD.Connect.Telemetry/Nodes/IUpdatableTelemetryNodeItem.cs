namespace ICD.Connect.Telemetry.Nodes
{
	public interface IUpdatableTelemetryNodeItem : IFeedbackTelemetryItem
	{
		void Update();
	}
}