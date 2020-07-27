using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdRoomFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryName = RoomTelemetryNames.MUTE_ROOM,
				FusionSigName = "Mute Room",
				SigType = eSigType.Digital,
				Sig = 55
			},
			new FusionSigMapping
			{
				TelemetryName = RoomTelemetryNames.ROOM_VOLUME,
				FusionSigName = "Room Volume",
				SigType = eSigType.Analog,
				Sig = 60
			}
		};
	}
}