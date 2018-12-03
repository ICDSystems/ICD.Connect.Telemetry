namespace ICD.Connect.Telemetry
{
	public interface IUpdatableTelemetryNodeItem : IFeedbackTelemetryItem
	{
		void Update();
	}
}