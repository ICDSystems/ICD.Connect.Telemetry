namespace ICD.Connect.Telemetry.Crestron.Assets
{
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
}