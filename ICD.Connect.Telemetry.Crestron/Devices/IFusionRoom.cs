using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Panels.Devices;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;

namespace ICD.Connect.Telemetry.Crestron.Devices
{
	public interface IFusionRoom : ISigDevice, IDevice
	{
		event EventHandler<FusionAssetSigUpdatedArgs> OnFusionAssetSigUpdated;
		event EventHandler<FusionAssetPowerStateUpdatedArgs> OnFusionAssetPowerStateUpdated;
		
		/// <summary>
		/// Gets the GUID for the Room ID.
		/// </summary>
		string RoomId { get; }

		/// <summary>
		/// Gets the user configurable assets.
		/// </summary>
		IFusionAssetDataCollection UserConfigurableAssetDetails { get; }

		/// <summary>
		/// Adds the asset to the fusion room.
		/// </summary>
		void AddAsset(AssetInfo asset);

		/// <summary>
		/// Adds the assets to the fusion room.
		/// </summary>
		/// <param name="assets"></param>
		void AddAssets(IEnumerable<AssetInfo> assets);

		void RebuildRvi();

		/// <summary>
		/// Sets the fusion error message.
		/// </summary>
		/// <param name="message"></param>
		[PublicAPI]
		void SetErrorMessage(string message);

		/// <summary>
		/// Removes the asset with the given id.
		/// </summary>
		/// <param name="assetId"></param>
		void RemoveAsset(uint assetId);

		/// <summary>
		/// Gets the configured asset ids.
		/// </summary>
		/// <returns></returns>
		IEnumerable<uint> GetAssetIds();

		/// <summary>
		/// Adds the sig for the given asset id.
		/// </summary>
		/// <param name="assetId"></param>
		/// <param name="sigType"></param>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="mask"></param>
		void AddSig(uint assetId, eSigType sigType, uint number, string name, eSigIoMask mask);
	}

	public sealed class FusionAssetSigUpdatedArgs : EventArgs
	{
		public eSigType SigType { get; private set; }
		public uint AssetId { get; private set; }
		public uint Sig { get; private set; }

		public FusionAssetSigUpdatedArgs(uint assetId, eSigType sigType, uint sigNumber)
		{
			AssetId = assetId;
			SigType = sigType;
			Sig = sigNumber;
		}

	}

	public sealed class FusionAssetPowerStateUpdatedArgs : EventArgs
	{
		public uint AssetId { get; private set; }
		public bool Powered { get; private set; }

		public FusionAssetPowerStateUpdatedArgs(uint assetId, bool powered)
		{
			AssetId = assetId;
			Powered = powered;
		}
	}
}
