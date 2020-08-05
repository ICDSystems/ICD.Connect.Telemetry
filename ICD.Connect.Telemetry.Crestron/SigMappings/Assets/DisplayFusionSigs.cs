using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Displays;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Assets
{
	public static class DisplayFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			new IcdHashSet<AssetFusionSigMapping>
			{
				new AssetFusionSigMapping
				{
					TelemetryName = DisplayTelemetryNames.ACTIVE_INPUT_STATE,
					FusionSigName = "Input Selection",
					SigType = eSigType.Analog,
					Sig = 1701
				}
			};
	}
}
