using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry
{
	public interface ITelemetryItem : IConsoleNode
	{
		string Name { get; }
	}
}