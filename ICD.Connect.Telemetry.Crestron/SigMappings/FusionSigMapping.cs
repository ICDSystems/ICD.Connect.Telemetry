using ICD.Common.Properties;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class FusionSigMapping : AbstractFusionSigMapping
	{
		public uint Sig { get; set; }

		/// <summary>
		/// Creates the telemetry binding for the given leaf.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="leaf"></param>
		/// <param name="assetId"></param>
		/// <param name="mappingUsage"></param>
		/// <returns></returns>
		[NotNull]
		public override FusionTelemetryBinding Bind([NotNull] IFusionRoom fusionRoom, [NotNull] TelemetryLeaf leaf,
		                                            uint assetId, [NotNull] RangeMappingUsageTracker mappingUsage)
		{
			return FusionTelemetryBinding.Bind(fusionRoom, leaf, this, assetId);
		}
	}
}