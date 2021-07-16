using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Panels.Devices;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Settings.Originators;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Devices
{
	public interface IFusionRoom : ISigDevice
	{
		event EventHandler<FusionAssetSigUpdatedArgs> OnFusionAssetSigUpdated;
		event EventHandler<FusionAssetPowerStateUpdatedArgs> OnFusionAssetPowerStateUpdated;
		event EventHandler<BoolEventArgs> OnFusionSystemPowerChangeEvent;
		event EventHandler<BoolEventArgs> OnFusionDisplayPowerChangeEvent;

		/// <summary>
		/// Gets the GUID for the Room ID.
		/// </summary>
		string RoomId { get; }

		/// <summary>
		/// Gets the room name for the Fusion Room
		/// </summary>
		string RoomName { get; }

		/// <summary>
		/// IPID of the fusion instance
		/// </summary>
		byte IpId { get; }

		/// <summary>
		/// Gets the user configurable assets.
		/// </summary>
		IFusionAssetDataCollection UserConfigurableAssetDetails { get; }

		/// <summary>
		/// Adds the asset to the fusion room.
		/// </summary>
		void AddAsset(AssetInfo asset);

		void UpdateDigitalSig(uint sig, bool newValue);

		bool ReadDigitalSig(uint sig);

		void UpdateAnalogSig(uint sig, ushort newValue);

		ushort ReadAnalogSig(uint sig);

		void UpdateSerialSig(uint sig, string newValue);

		string ReadSerialSig(uint sig);

		/// <summary>
		/// Sets the fusion error message.
		/// </summary>
		/// <param name="message"></param>
		[PublicAPI]
		void SetErrorMessage(string message);

		/// <summary>
		/// Sends the string as device usage.
		/// </summary>
		/// <param name="usage"></param>
		[PublicAPI]
		void SendDeviceUsage(string usage);

		/// <summary>
		/// Sets the display usage analog.
		/// </summary>
		/// <param name="usage"></param>
		[PublicAPI]
		void SetDisplayUsage(ushort usage);

		/// <summary>
		/// Sets the system power on/off state for the room.
		/// </summary>
		/// <param name="powered"></param>
		[PublicAPI]
		void SetSystemPower(bool powered);

		/// <summary>
		/// Sets the display power on/off state for the room.
		/// </summary>
		/// <param name="powered"></param>
		[PublicAPI]
		void SetDisplayPower(bool powered);

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
		/// Adds the sig for the room itself
		/// </summary>
		/// <param name="sigType"></param>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="mask"></param>
		void AddSig(eSigType sigType, uint number, string name, eTelemetryIoMask mask);

		/// <summary>
		/// Adds the sig for the given asset id.
		/// </summary>
		/// <param name="assetId"></param>
		/// <param name="sigType"></param>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="mask"></param>
		void AddSig(uint assetId, eSigType sigType, uint number, string name, eTelemetryIoMask mask);

		IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedBoolSigs();

		IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedUShortSigs();
              
		IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedStringSigs();

		/// <summary>
		/// Generates an RVI file for the fusion assets.
		/// </summary>
		void RebuildRvi();
	}

	public static class FusionRoomExtensions
	{
		/// <summary>
		/// Returns the next unused asset id for the fusion room.
		/// </summary>
		/// <returns></returns>
		public static uint GetNextAssetId([NotNull] this IFusionRoom extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			uint next = extends.GetAssetIds().MaxOrDefault() + 1;
			return Math.Max(next, 4); // Start at 4
		}

		/// <summary>
		/// Generates a deterministic asset instance id for the given device.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="device"></param>
		/// <param name="staticAsset"></param>
		/// <returns></returns>
		public static string GetInstanceId([NotNull] this IFusionRoom extends, [NotNull] IOriginator device, eAssetType staticAsset)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (device == null)
				throw new ArgumentNullException("device");

			return GuidUtils.Combine(device.Uuid, staticAsset.GetAssetTypeGuid()).ToString();
		}
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
