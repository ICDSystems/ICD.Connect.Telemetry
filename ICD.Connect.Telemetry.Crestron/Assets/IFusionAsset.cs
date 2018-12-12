using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public interface IFusionAsset
	{
		/// <summary>
		/// Gets the name of the asset.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the user defined name of the asset type.
		/// </summary>
		string Type { get; }

		/// <summary>
		/// Gets the user defined instance id of the asset type.
		/// </summary>
		string InstanceId { get; }

		/// <summary>
		/// Gets the user defined number of the asset type.
		/// </summary>
		uint Number { get; }
	}

	public interface IFusionOccupancySensorAsset : IFusionAsset
	{
		/// <summary>
		/// Sets the enabled state of the occupancy sensor.
		/// </summary>
		/// <param name="enabled"></param>
		void EnableOccupancySensor(bool enabled);

		/// <summary>
		/// Sets the occupied state of the room.
		/// </summary>
		/// <param name="occupied"></param>
		void SetRoomOccupied(bool occupied);

		/// <summary>
		/// Sets the timeout for the occupancy sensor.
		/// </summary>
		/// <param name="timeout"></param>
		void SetOccupancySensorTimeout(ushort timeout);
	}

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
