using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Partitioning.Commercial.Rooms;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Rooms
{
	public static class CommercialRoomFusionSigs
	{
		public static IEnumerable<RoomFusionSigMapping> RoomMappings { get { return s_RoomMappings; } }

		private static readonly IcdHashSet<RoomFusionSigMapping> s_RoomMappings =
			new IcdHashSet<RoomFusionSigMapping>
			{
				new RoomFusionSigMapping
				{
					TelemetryName = CommercialRoomTelemetryNames.IS_AWAKE,
					FusionSigName = "System Power On",
					SigType = eSigType.Digital,
					Sig = 3,
					SendReservedSig = (r, o) => r.SetSystemPower((bool)o)
				},
				new RoomFusionSigMapping
				{
					TelemetryName = CommercialRoomTelemetryNames.SLEEP_COMMAND,
					FusionSigName = "System Power",
					SigType = eSigType.Digital,
					Sig = 4,
					SendReservedSig = (r, o) => r.SetSystemPower((bool)o)
				},
				new RoomFusionSigMapping
				{
					TelemetryName = CommercialRoomTelemetryNames.MUTE_PRIVACY,
					FusionSigName = "Mute Mics",
					SigType = eSigType.Digital,
					Sig = 56
				}
			};
	}
}