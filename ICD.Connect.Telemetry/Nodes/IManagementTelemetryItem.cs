using System;

namespace ICD.Connect.Telemetry.Nodes
{
	public interface IManagementTelemetryItem : ITelemetryItem
	{
		int ParameterCount { get; }

		Type[] ParameterTypes { get; }

		void Invoke(params object[] parameters);
	}
}