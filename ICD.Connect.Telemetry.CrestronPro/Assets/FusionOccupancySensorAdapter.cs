using ICD.Connect.Telemetry.Assets;
#if SIMPLSHARP
using Crestron.SimplSharpPro.Fusion;

namespace ICD.Connect.Telemetry.CrestronPro.Assets
{
	public sealed class FusionOccupancySensorAdapter : AbstractFusionAssetAdapter<FusionOccupancySensor>,
	                                                   IFusionOccupancySensorAsset
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionAsset"></param>
		public FusionOccupancySensorAdapter(FusionOccupancySensor fusionAsset)
			: base(fusionAsset)
		{
		}

		/// <summary>
		/// Sets the enabled state of the occupancy sensor.
		/// </summary>
		/// <param name="enabled"></param>
		public void EnableOccupancySensor(bool enabled)
		{
			FusionAsset.EnableOccupancySensor.InputSig.BoolValue = enabled;
			FusionAsset.DisableOccupancySensor.InputSig.BoolValue = !enabled;
		}

		/// <summary>
		/// Sets the occupied state of the room.
		/// </summary>
		/// <param name="occupied"></param>
		public void SetRoomOccupied(bool occupied)
		{
			FusionAsset.RoomOccupied.InputSig.BoolValue = occupied;
		}

		/// <summary>
		/// Sets the timeout for the occupancy sensor.
		/// </summary>
		/// <param name="timeout"></param>
		public void SetOccupancySensorTimeout(ushort timeout)
		{
			FusionAsset.OccupancySensorTimeout.InputSig.UShortValue = timeout;
		}
	}
}
#endif
