using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Settings;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdStandardFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = OriginatorTelemetryNames.ID,
				TelemetrySetName = "",
				FusionSigName = "Id",
				SigType = eSigType.Serial,
				Sig = 24951
			},
			new FusionSigMapping
			{
				TelemetryGetName = OriginatorTelemetryNames.NAME,
				TelemetrySetName = "",
				FusionSigName = "ComponentName",
				SigType = eSigType.Serial,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.ONLINE_STATE,
				TelemetrySetName = "",
				FusionSigName = "Connected",
				SigType = eSigType.Digital,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.POWER_STATE, 
				TelemetrySetName = DeviceTelemetryNames.POWER_ON,
				FusionSigName = "PowerOn",
				SigType = eSigType.Digital,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryGetName = "",
				TelemetrySetName = DeviceTelemetryNames.POWER_OFF,
				FusionSigName = "PowerOff",
				SigType = eSigType.Digital,
				Sig = 0
			},
		}; 
	}
}