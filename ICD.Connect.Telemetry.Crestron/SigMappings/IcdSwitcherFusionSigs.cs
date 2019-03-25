using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Routing.Telemetry;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdSwitcherFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }
		public static IEnumerable<IFusionSigMapping> InputOutputSigs { get { return s_InputOutputSigs; } } 

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_BREAKAWAY_ENABLED,
				TelemetrySetName = "",
				FusionSigName = "Audio Breakaway Enabled",
				Sig = 221,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.USB_BREAKAWAY_ENABLED,
				TelemetrySetName = "",
				FusionSigName = "USB Breakaway Enabled",
				Sig = 222,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.IP_ADDRESS,
				TelemetrySetName = "",
				FusionSigName = "IP Address",
				Sig = 119,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.HOSTNAME,
				TelemetrySetName = "",
				FusionSigName = "Hostname",
				Sig = 120,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.SUBNET_MASK,
				TelemetrySetName = "",
				FusionSigName = "Subnet Mask",
				Sig = 121,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.MAC_ADDRESS,
				TelemetrySetName = "",
				FusionSigName = "MAC Address",
				Sig = 122,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.DEFAULT_ROUTER,
				TelemetrySetName = "",
				FusionSigName = "Default Router",
				Sig = 123,
				SigType = eSigType.Serial
			}
		};

		private static readonly IcdHashSet<IFusionSigMapping> s_InputOutputSigs = new IcdHashSet<IFusionSigMapping>
		{
			
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_INPUT_SYNC,
				TelemetrySetName = "",
				FusionSigName = "Video Input {0} Sync",
				FirstSig = 11001,
				LastSig = 11999,
				SigType = eSigType.Digital
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC,
				TelemetrySetName = "",
				FusionSigName = "Video Output {0} Sync",
				FirstSig = 12001,
                LastSig = 12999,
				SigType = eSigType.Digital
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE,
				TelemetrySetName = SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE_COMMAND,
				FusionSigName = "Audio Output {0} Mute",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Digital
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = "",
				TelemetrySetName = SwitcherTelemetryNames.AUDIO_OUTPUT_UNMUTE_COMMAND,
				FusionSigName = "Audio Output {0} Mute Off",
				FirstSig = 22001,
                LastSig = 22999,
				SigType = eSigType.Digital
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME,
				TelemetrySetName = SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME_COMMAND,
				FusionSigName = "Audio Output {0} Volume",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Analog
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.INPUT_ID,
				TelemetrySetName = "",
				FusionSigName = "Input {0} Id",
				FirstSig = 11001,
                LastSig = 11999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.INPUT_NAME,
				TelemetrySetName = "",
				FusionSigName = "Input {0} Name",
				FirstSig = 12001,
                LastSig = 12999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE,
				TelemetrySetName = "",
				FusionSigName = "Video Input {0} Sync Type",
				FirstSig = 13001,
                LastSig = 13999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION,
				TelemetrySetName = "",
				FusionSigName = "Video Input {0} Resolution",
				FirstSig = 14001,
                LastSig = 14999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.OUTPUT_ID,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Id",
				FirstSig = 15001,
                LastSig = 15999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.OUTPUT_NAME,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Name",
				FirstSig = 16001,
                LastSig = 16999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE,
				TelemetrySetName = "",
				FusionSigName = "Video Output {0} Sync Type",
				FirstSig = 17001,
                LastSig = 17999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION,
				TelemetrySetName = "",
				FusionSigName = "Video Output {0} Resolution",
				FirstSig = 18001,
                LastSig = 18999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING,
				TelemetrySetName = "",
				FusionSigName = "Video Output {0} Encoding",
				FirstSig = 19001,
                LastSig = 19999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_SOURCE,
				TelemetrySetName = "",
				FusionSigName = "Video Output {0} Name",
				FirstSig = 20001,
                LastSig = 20999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_OUTPUT_SOURCE,
				TelemetrySetName = "",
				FusionSigName = "Audio Output {0} Name",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT,
				TelemetrySetName = "",
				FusionSigName = "Audio Output {0} Format",
				FirstSig = 22001,
                LastSig = 22999,
				SigType = eSigType.Serial
			},
			new FusionSigRangeMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.USB_OUTPUT_ID,
				TelemetrySetName = "",
				FusionSigName = "USB Output {0} Id",
				FirstSig = 31001,
                LastSig = 31999,
				SigType = eSigType.Serial
			}
		};
	}
}