using ICD.Connect.API.Nodes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Nodes
{
	public interface ITelemetryNode : IConsoleNode
	{
		/// <summary>
		/// Gets the name of the telemetry node.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the provider that this telemetry item is attached to.
		/// </summary>
		ITelemetryProvider Provider { get; }
	}
}