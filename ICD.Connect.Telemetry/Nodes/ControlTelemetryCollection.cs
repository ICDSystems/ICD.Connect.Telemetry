using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Nodes
{
	[UsedImplicitly]
	public sealed class ControlTelemetryCollection : AbstractCollectionTelemetryWithParent
	{
		public ControlTelemetryCollection(ITelemetryProvider parent) : base(parent)
		{
		}
	}
}