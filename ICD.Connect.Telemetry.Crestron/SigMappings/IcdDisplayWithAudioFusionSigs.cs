using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Displays;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdDisplayWithAudioFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = DisplayTelemetryNames.MUTE_STATE,
				TelemetrySetName = DisplayTelemetryNames.MUTE_ON,
				FusionSigName = "Audio Output 1 Mute",
                SigType = eSigType.Digital,
				Sig = 3001
			},
			new FusionSigMapping
			{
				TelemetryGetName = "",
				TelemetrySetName = DisplayTelemetryNames.MUTE_OFF,
				FusionSigName = "Audio Output 1 Mute Off",
				SigType = eSigType.Digital,
				Sig = 3501
			},
			new FusionSigMapping
			{
				TelemetryGetName = DisplayTelemetryNames.VOLUME_PERCENT,
				TelemetrySetName = DisplayTelemetryNames.SET_VOLUME,
				FusionSigName = "Audio Output 1 Volume",
				SigType = eSigType.Analog,
				Sig = 3001
			}

		};
	}
}