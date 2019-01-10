using System;

namespace ICD.Connect.Telemetry.Nodes
{
	public interface IUpdatableTelemetryNodeItem : IFeedbackTelemetryItem
	{
		event EventHandler OnValueChanged; 

		void Update();
	}
}