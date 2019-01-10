using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public interface IFusionRemoteOccupancySensorAsset : IFusionAsset
	{
		/// <summary>
		/// Sets the room occupied state.
		/// </summary>
		/// <param name="occupied"></param>
		void SetRoomOccupied(eOccupancyState occupied);

		/// <summary>
		/// Sets the room occupancy info.
		/// </summary>
		/// <param name="info"></param>
		void SetRoomOccupancyInfo(string info);
	}
}