using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Partitioning.Commercial.Rooms;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdCommercialRoomFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryName = CommercialRoomTelemetryNames.SYSTEM_POWER,
				FusionSigName = "System Power On",
				SigType = eSigType.Digital,
				Sig = 3
			},
			new FusionSigMapping
			{
				TelemetryName = CommercialRoomTelemetryNames.SYSTEM_SHUTDOWN_COMMAND,
				FusionSigName = "System Power",
				SigType = eSigType.Digital,
				Sig = 4
			},
			new FusionSigMapping
			{
				TelemetryName = CommercialRoomTelemetryNames.MUTE_PRIVACY,
				FusionSigName = "Mute Mics",
				SigType = eSigType.Digital,
				Sig = 56
			}
		};
	}
}