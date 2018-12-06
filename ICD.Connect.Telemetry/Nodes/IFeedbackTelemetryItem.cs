using System;

namespace ICD.Connect.Telemetry.Nodes
{
	public interface IFeedbackTelemetryItem : ITelemetryItem
	{
		Type ValueType { get; }
		object Value { get; }
	}

	public interface IFeedbackTelemetryItem<T> : IFeedbackTelemetryItem
	{
		new T Value { get; }
	}
}