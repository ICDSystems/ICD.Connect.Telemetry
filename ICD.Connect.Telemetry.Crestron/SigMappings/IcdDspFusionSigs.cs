using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Audio;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdDspFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = DspTelemetryNames.FIRMWARE_VERSION,
				TelemetrySetName = string.Empty,
				FusionSigName = "Firmware Version",
				Sig = 300,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryGetName = DspTelemetryNames.CALL_ACTIVE,
				TelemetrySetName = string.Empty,
				FusionSigName = "ATC Call Active",
				Sig = 301,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryGetName = DspTelemetryNames.ACTIVE_FAULTS,
				TelemetrySetName = string.Empty,
				FusionSigName = "Active Faults",
				Sig = 300,
				SigType = eSigType.Digital
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = ControlTelemetryNames.NAME,
				TelemetrySetName = string.Empty,
				FusionSigName = "Volume Control {0} Name",
				FirstSig = 10000,
				LastSig = 10999,
				SigType = eSigType.Serial
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = VolumeTelemetryNames.VOLUME_CONTROL_PERCENT,
				TelemetrySetName = VolumeTelemetryNames.VOLUME_CONTROL_PERCENT_COMMAND,
				FusionSigName = "Volume Control {0} Level",
				FirstSig = 10000,
				LastSig = 10999,
				SigType = eSigType.Analog
			},
			new FusionSigMultiMapping
			{
				TelemetryGetName = VolumeTelemetryNames.VOLUME_CONTROL_MUTE,
				TelemetrySetName = VolumeTelemetryNames.VOLUME_CONTROL_MUTE_COMMAND,
				FusionSigName = "Volume Control {0} Mute",
				FirstSig = 10000,
				LastSig = 10999,
				SigType = eSigType.Digital
			}
		};
	}
}