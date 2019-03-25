using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public interface ITelemetryItem : IConsoleNode
	{
		string Name { get; }
		ITelemetryProvider Parent { get; }
	}
}