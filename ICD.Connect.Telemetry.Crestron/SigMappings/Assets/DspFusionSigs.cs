using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Audio.Telemetry;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Assets
{
	public static class DspFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			new IcdHashSet<AssetFusionSigMapping>
			{
				new AssetFusionSigMapping
				{
					TelemetryName = DspTelemetryNames.CALL_ACTIVE,
					FusionSigName = "ATC Call Active",
					Sig = 301,
					SigType = eSigType.Digital
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DspTelemetryNames.ACTIVE_FAULT_STATE,
					FusionSigName = "Active Faults",
					Sig = 300,
					SigType = eSigType.Digital
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DspTelemetryNames.ACTIVE_FAULT_MESSAGE,
					FusionSigName = "Active Fault Message",
					Sig = 300, //todo: Verify Join Number is OK!
					SigType = eSigType.Serial
				}
			};
	}

	public static class VolumeDeviceControlFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			new IcdHashSet<AssetFusionSigMapping>
			{
				new AssetFusionSigMapping
				{
					TelemetryName = ControlTelemetryNames.NAME,
					FusionSigName = "Volume Control {0} Name",
					Sig = 10000,
					Range = 1000,
					SigType = eSigType.Serial,
					TelemetryProviderTypes = new IcdHashSet<Type> {typeof(IVolumeDeviceControl)}
				},
				new AssetFusionSigMapping
				{
					TelemetryName = VolumeTelemetryNames.VOLUME_CONTROL_PERCENT,
					FusionSigName = "Volume Control {0} Level",
					Sig = 10000,
					Range = 1000,
					SigType = eSigType.Analog,
					TelemetryProviderTypes = new IcdHashSet<Type> {typeof(IVolumeDeviceControl)}
				},
				new AssetFusionSigMapping
				{
					TelemetryName = VolumeTelemetryNames.VOLUME_CONTROL_IS_MUTED,
					FusionSigName = "Volume Control {0} Mute",
					Sig = 10000,
					Range = 1000,
					SigType = eSigType.Digital,
					TelemetryProviderTypes = new IcdHashSet<Type> {typeof(IVolumeDeviceControl)}
				}
			};
	}
}