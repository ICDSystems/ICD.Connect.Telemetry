using System;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class FusionSigMapping : AbstractFusionSigMapping
	{
		public uint Sig { get; set; }

		public override FusionTelemetryBinding Bind(IFusionRoom fusionRoom, ITelemetryItem node, uint assetId, RangeMappingUsageTracker mappingUsage)
		{
			return FusionTelemetryBinding.Bind(fusionRoom, node.Parent, this, assetId);
		}
	}
}