#if SIMPLSHARP
using Crestron.SimplSharpPro.Fusion;
using ICD.Connect.Analytics.Assets;

namespace ICD.Connect.Analytics.FusionPro.Assets
{
	public sealed class FusionRemoteOccupancySensorAdapter : AbstractFusionAssetAdapter<FusionRemoteOccupancySensor>, IFusionRemoteOccupancySensorAsset
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionAsset"></param>
		public FusionRemoteOccupancySensorAdapter(FusionRemoteOccupancySensor fusionAsset)
			: base(fusionAsset)
		{
		}

		/// <summary>
		/// Sets the room occupied state.
		/// </summary>
		/// <param name="occupied"></param>
		public void SetRoomOccupied(bool occupied)
		{
			FusionAsset.RoomOccupied.InputSig.BoolValue = occupied;
			FusionAsset.RoomUnoccupied.InputSig.BoolValue = !occupied;
		}

		/// <summary>
		/// Sets the room occupancy info.
		/// </summary>
		/// <param name="info"></param>
		public void SetRoomOccupancyInfo(string info)
		{
			FusionAsset.RoomOccupancyInfo.InputSig.StringValue = info;
		}
	}
}
#endif
