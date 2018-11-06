using System;

namespace ICD.Connect.Telemetry
{
	public interface ITelemetryItem
	{
		string Name { get; }
		Type ValueType { get; }
		object Value { get; }
	}

	public interface ITelemetryItem<T> : ITelemetryItem
	{
		new T Value { get; }
	}
}