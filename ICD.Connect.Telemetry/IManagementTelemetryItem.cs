using System;

namespace ICD.Connect.Telemetry
{
	public interface IManagementTelemetryItem : ITelemetryItem
	{
		int ParameterCount { get; }

		Type[] ParameterTypes { get; }

		object Parent { get; }

		void Invoke(object[] parameters);
	}
}