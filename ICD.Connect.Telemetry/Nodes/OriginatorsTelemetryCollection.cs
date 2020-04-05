using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Nodes
{
	[UsedImplicitly]
	public sealed class OriginatorsTelemetryCollection : AbstractCollectionTelemetryWithParent
	{
		public const string PATH_STRING = "Originators";

		public OriginatorsTelemetryCollection(ITelemetryProvider parent) : base(parent)
		{
			
		}
	}
}