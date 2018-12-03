using System;

namespace ICD.Connect.Telemetry
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