using System;

namespace ICD.Connect.Telemetry
{
	public interface ITelemetryItem
	{
		string Name { get; }
		Type ValueType { get; }
	}

	public interface ITelemetryItem<T> : ITelemetryItem
	{
		T Value { get; }
	}
}