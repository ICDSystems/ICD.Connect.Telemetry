using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class FusionSigMapping : AbstractFusionSigMapping
	{
		public uint Sig { get; set; }

		public override FusionTelemetryBinding Bind(IFusionRoom fusionRoom, ITelemetryNode node, uint assetId, RangeMappingUsageTracker mappingUsage)
		{
			return FusionTelemetryBinding.Bind(fusionRoom, node.Provider, this, assetId);
		}
	}
}