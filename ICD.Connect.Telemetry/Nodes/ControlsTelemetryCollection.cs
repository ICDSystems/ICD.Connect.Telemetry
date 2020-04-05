using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Nodes
{
	[UsedImplicitly]
	public sealed class ControlsTelemetryCollection : AbstractCollectionTelemetryWithParent
	{
		public const string PATH_STRING = "Controls";

		public ControlsTelemetryCollection(ITelemetryProvider parent) : base (parent)
		{
			
		}
	}
}