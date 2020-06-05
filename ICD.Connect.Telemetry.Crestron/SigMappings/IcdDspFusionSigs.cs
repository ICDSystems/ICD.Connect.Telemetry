using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Audio.Telemetry;
using ICD.Connect.Audio.Controls.Volume;
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
				TelemetryName = DspTelemetryNames.CALL_ACTIVE,
				FusionSigName = "ATC Call Active",
				Sig = 301,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryName = DspTelemetryNames.ACTIVE_FAULT_STATE,
				FusionSigName = "Active Faults",
				Sig = 300,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryName = DspTelemetryNames.ACTIVE_FAULT_MESSAGE,
				FusionSigName = "Active Fault Message",
				Sig = 300, //todo: Verify Join Number is OK!
				SigType =  eSigType.Serial
			}
		};
	}

	public static class IcdVolumeDeviceControlFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMultiMapping
			{
				TelemetryName = ControlTelemetryNames.NAME,
				FusionSigName = "Volume Control {0} Name",
				FirstSig = 10000,
				LastSig = 10999,
				SigType = eSigType.Serial,
				TelemetryProviderTypes = new IcdHashSet<Type>{typeof(IVolumeDeviceControl)}
			},
			new FusionSigMultiMapping
			{
				TelemetryName = VolumeTelemetryNames.VOLUME_CONTROL_PERCENT,
				FusionSigName = "Volume Control {0} Level",
				FirstSig = 10000,
				LastSig = 10999,
				SigType = eSigType.Analog,
				TelemetryProviderTypes = new IcdHashSet<Type>{typeof(IVolumeDeviceControl)}
			},
			new FusionSigMultiMapping
			{
				TelemetryName = VolumeTelemetryNames.VOLUME_CONTROL_IS_MUTED,
				FusionSigName = "Volume Control {0} Mute",
				FirstSig = 10000,
				LastSig = 10999,
				SigType = eSigType.Digital,
				TelemetryProviderTypes = new IcdHashSet<Type>{typeof(IVolumeDeviceControl)}
			}
		};
	}
}