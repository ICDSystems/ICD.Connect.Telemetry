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
			
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_INPUT_SYNC,
				TelemetrySetName = "",
				FusionSigName = "Input {0} Video Sync",
				FirstSig = 11001,
				LastSig = 11999,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Video Sync",
				FirstSig = 12001,
                LastSig = 12999,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE,
				TelemetrySetName = SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE_COMMAND,
				FusionSigName = "Output {0} Audio Mute",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = "",
				TelemetrySetName = SwitcherTelemetryNames.AUDIO_OUTPUT_UNMUTE_COMMAND,
				FusionSigName = "Output {0} Mute Audio Off",
				FirstSig = 22001,
                LastSig = 22999,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME,
				TelemetrySetName = SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME_COMMAND,
				FusionSigName = "Output {0} Audio Volume",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Analog
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.INPUT_ID,
				TelemetrySetName = "",
				FusionSigName = "Input {0} Id",
				FirstSig = 11001,
                LastSig = 11999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.INPUT_NAME,
				TelemetrySetName = "",
				FusionSigName = "Input {0} Name",
				FirstSig = 12001,
                LastSig = 12999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE,
				TelemetrySetName = "",
				FusionSigName = "Input {0} Video Sync Type",
				FirstSig = 13001,
                LastSig = 13999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION,
				TelemetrySetName = "",
				FusionSigName = "Input {0} Video Resolution",
				FirstSig = 14001,
                LastSig = 14999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.OUTPUT_ID,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Id",
				FirstSig = 15001,
                LastSig = 15999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.OUTPUT_NAME,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Name",
				FirstSig = 16001,
                LastSig = 16999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Video Sync Type",
				FirstSig = 17001,
                LastSig = 17999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Video Resolution",
				FirstSig = 18001,
                LastSig = 18999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Video Encoding",
				FirstSig = 19001,
                LastSig = 19999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.VIDEO_OUTPUT_SOURCE,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Video Source",
				FirstSig = 20001,
                LastSig = 20999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_OUTPUT_SOURCE,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Audio Source",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT,
				TelemetrySetName = "",
				FusionSigName = "Output {0} Audio Format",
				FirstSig = 22001,
                LastSig = 22999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = SwitcherTelemetryNames.USB_OUTPUT_ID,
				TelemetrySetName = "",
				FusionSigName = "Output {0} USB Id",
				FirstSig = 23001,
                LastSig = 23999,
				SigType = eSigType.Serial
			}
		};
	}
}