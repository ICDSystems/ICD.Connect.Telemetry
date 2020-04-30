using System;
using System.Collections.Generic;
using System.Text;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Assets.Mock
{
	public sealed class MockFusionOccupancySensorAsset : AbstractMockFusionAsset, IFusionOccupancySensorAsset
	{
		public bool IsEnabled { get; private set; }

		public bool IsOccupied { get; private set; }

		public ushort OccupancyTimeout { get; private set; }

		public MockFusionOccupancySensorAsset(uint number, string name, string type, string instanceId) : base(number, name, type, instanceId)
		{
		}

		public MockFusionOccupancySensorAsset(AssetInfo assetInfo) : this(assetInfo.Number, assetInfo.Name,
		                                                                  assetInfo.Type, assetInfo.InstanceId)
		{

		}

		public override eAssetType AssetType { get { return eAssetType.OccupancySensor; } }

		/// <summary>
		/// Sets the enabled state of the occupancy sensor.
		/// </summary>
		/// <param name="enabled"></param>
		public void EnableOccupancySensor(bool enabled)
		{
			IsEnabled = enabled;
		}

		/// <summary>
		/// Sets the occupied state of the room.
		/// </summary>
		/// <param name="occupied"></param>
		public void SetRoomOccupied(bool occupied)
		{
			IsOccupied = occupied;
		}

		/// <summary>
		/// Sets the timeout for the occupancy sensor.
		/// </summary>
		/// <param name="timeout"></param>
		public void SetOccupancySensorTimeout(ushort timeout)
		{
			OccupancyTimeout = timeout;
		}

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("IsEnabled", IsEnabled);
			addRow("IsOccupied", IsOccupied);
			addRow("OccupancyTimeout", OccupancyTimeout);
		}

		#endregion
	}
}
