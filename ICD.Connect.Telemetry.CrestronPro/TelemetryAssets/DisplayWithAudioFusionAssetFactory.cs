using System.Collections.Generic;
using ICD.Connect.Telemetry.Crestron;
using ICD.Connect.Telemetry.CrestronPro.SigMappings;

namespace ICD.Connect.Telemetry.CrestronPro.TelemetryAssets
{
	public sealed class DisplayWithAudioFusionAssetFactory : AbstractDisplayFusionAssetFactory
	{
		public override IEnumerable<FusionSigMapping> Mappings { get { return IcdDisplayWithAudioFusionSigs.Sigs; } }
	}
}