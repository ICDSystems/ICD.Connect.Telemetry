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

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>();

		private static readonly IcdHashSet<IFusionSigMapping> s_InputOutputSigs = new IcdHashSet<IFusionSigMapping>
		{
			
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_INPUT_SYNC,
				FusionSigName = "Input {0} Video Sync",
				FirstSig = 11001,
				LastSig = 11999,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC,
				FusionSigName = "Output {0} Video Sync",
				FirstSig = 12001,
                LastSig = 12999,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE,
				FusionSigName = "Output {0} Audio Mute",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_UNMUTE_COMMAND,
				FusionSigName = "Output {0} Mute Audio Off",
				FirstSig = 22001,
                LastSig = 22999,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME,
				FusionSigName = "Output {0} Audio Volume",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Analog
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.INPUT_ID,
				FusionSigName = "Input {0} Id",
				FirstSig = 11001,
                LastSig = 11999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.INPUT_NAME,
				FusionSigName = "Input {0} Name",
				FirstSig = 12001,
                LastSig = 12999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE,
				FusionSigName = "Input {0} Video Sync Type",
				FirstSig = 13001,
                LastSig = 13999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION,
				FusionSigName = "Input {0} Video Resolution",
				FirstSig = 14001,
                LastSig = 14999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.OUTPUT_ID,
				FusionSigName = "Output {0} Id",
				FirstSig = 15001,
                LastSig = 15999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.OUTPUT_NAME,
				FusionSigName = "Output {0} Name",
				FirstSig = 16001,
                LastSig = 16999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE,
				FusionSigName = "Output {0} Video Sync Type",
				FirstSig = 17001,
                LastSig = 17999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION,
				FusionSigName = "Output {0} Video Resolution",
				FirstSig = 18001,
                LastSig = 18999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING,
				FusionSigName = "Output {0} Video Encoding",
				FirstSig = 19001,
                LastSig = 19999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_SOURCE,
				FusionSigName = "Output {0} Video Source",
				FirstSig = 20001,
                LastSig = 20999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_SOURCE,
				FusionSigName = "Output {0} Audio Source",
				FirstSig = 21001,
                LastSig = 21999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT,
				FusionSigName = "Output {0} Audio Format",
				FirstSig = 22001,
                LastSig = 22999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryName = SwitcherTelemetryNames.USB_OUTPUT_ID,
				FusionSigName = "Output {0} USB Id",
				FirstSig = 23001,
                LastSig = 23999,
				SigType = eSigType.Serial
			}
		};
	}
}