using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public interface ITelemetryItem : IConsoleNode
	{
		/// <summary>
		/// Gets the name of the telemetry item.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the provider that this telemetry item is attached to.
		/// </summary>
		ITelemetryProvider Parent { get; }
	}
}