using eAssetType = ICD.Connect.Telemetry.Crestron.Assets.eAssetType;
#if !NETSTANDARD
using Crestron.SimplSharpPro.Fusion;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Telemetry.CrestronPro.Assets
{
	public sealed class FusionRemoteOccupancySensorAdapter : AbstractFusionAssetAdapter<FusionRemoteOccupancySensor>, IFusionRemoteOccupancySensorAsset
	{
		/// <summary>
		/// Gets the asset type.
		/// </summary>
		public override eAssetType AssetType { get { return eAssetType.RemoteOccupancySensor; } }

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
		public void SetRoomOccupied(eOccupancyState occupied)
		{
			FusionAsset.RoomOccupied.InputSig.BoolValue = occupied == eOccupancyState.Occupied;
			FusionAsset.RoomUnoccupied.InputSig.BoolValue = occupied == eOccupancyState.Unoccupied;
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
