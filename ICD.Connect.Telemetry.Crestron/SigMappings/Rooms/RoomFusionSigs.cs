using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Partitioning.Rooms;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Rooms
{
	public static class RoomFusionSigs
	{
		public static IEnumerable<RoomFusionSigMapping> RoomMappings { get { return s_RoomMappings; } }

		private static readonly IcdHashSet<RoomFusionSigMapping> s_RoomMappings =
			new IcdHashSet<RoomFusionSigMapping>
			{
				new RoomFusionSigMapping
				{
					TelemetryName = RoomTelemetryNames.MUTED,
					FusionSigName = "Room Muted",
					SigType = eSigType.Digital,
					Sig = 55
				},
				new RoomFusionSigMapping
				{
					TelemetryName = RoomTelemetryNames.VOLUME_PERCENT,
					FusionSigName = "Room Volume",
					SigType = eSigType.Analog,
					Sig = 60
				}
			};
	}
}