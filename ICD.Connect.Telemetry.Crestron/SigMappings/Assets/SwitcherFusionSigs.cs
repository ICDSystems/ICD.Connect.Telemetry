using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Routing.Telemetry;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Assets
{
	public static class SwitcherFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }
		public static IEnumerable<AssetFusionSigMapping> InputOutputAssetMappings { get { return s_InputOutputAssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings = new IcdHashSet<AssetFusionSigMapping>();

		private static readonly IcdHashSet<AssetFusionSigMapping> s_InputOutputAssetMappings = new IcdHashSet<AssetFusionSigMapping>
		{
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_INPUT_SYNC,
				FusionSigName = "Input {0} Video Sync",
				Sig = 11001,
				Range = 999,
				SigType = eSigType.Digital
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC,
				FusionSigName = "Output {0} Video Sync",
				Sig = 12001,
				Range = 999,
				SigType = eSigType.Digital
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE,
				FusionSigName = "Output {0} Audio Mute",
				Sig = 21001,
				Range = 999,
				SigType = eSigType.Digital
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_UNMUTE_COMMAND,
				FusionSigName = "Output {0} Mute Audio Off",
				Sig = 22001,
				Range = 999,
				SigType = eSigType.Digital
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME,
				FusionSigName = "Output {0} Audio Volume",
				Sig = 21001,
				Range = 999,
				SigType = eSigType.Analog
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.INPUT_ID,
				FusionSigName = "Input {0} Id",
				Sig = 11001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.INPUT_NAME,
				FusionSigName = "Input {0} Name",
				Sig = 12001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE,
				FusionSigName = "Input {0} Video Sync Type",
				Sig = 13001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION,
				FusionSigName = "Input {0} Video Resolution",
				Sig = 14001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.OUTPUT_ID,
				FusionSigName = "Output {0} Id",
				Sig = 15001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.OUTPUT_NAME,
				FusionSigName = "Output {0} Name",
				Sig = 16001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE,
				FusionSigName = "Output {0} Video Sync Type",
				Sig = 17001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION,
				FusionSigName = "Output {0} Video Resolution",
				Sig = 18001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING,
				FusionSigName = "Output {0} Video Encoding",
				Sig = 19001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.VIDEO_OUTPUT_SOURCE,
				FusionSigName = "Output {0} Video Source",
				Sig = 20001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_SOURCE,
				FusionSigName = "Output {0} Audio Source",
				Sig = 21001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT,
				FusionSigName = "Output {0} Audio Format",
				Sig = 22001,
				Range = 999,
				SigType = eSigType.Serial
			},
			new AssetFusionSigMapping
			{
				TelemetryName = SwitcherTelemetryNames.USB_OUTPUT_ID,
				FusionSigName = "Output {0} USB Id",
				Sig = 23001,
				Range = 999,
				SigType = eSigType.Serial
			}
		};
	}
}