using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Panels.Devices;
using ICD.Connect.Telemetry.CrestronPro.Assets;

namespace ICD.Connect.Telemetry
{
	public interface IFusionRoom : ISigDevice, IDevice
	{
		/// <summary>
		/// Gets the user configurable assets.
		/// </summary>
		IFusionAssetDataCollection UserConfigurableAssetDetails { get; }

		/// <summary>
		/// Loads sigs from the xml file at the given path.
		/// </summary>
		/// <param name="path"></param>
		[PublicAPI]
		void LoadSigsFromPath(string path);

		/// <summary>
		/// Adds the asset to the fusion room.
		/// </summary>
		/// <param name="assetType"></param>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="instanceId"></param>
		void AddAsset(eAssetType assetType, uint number, string name, string type, string instanceId);

		/// <summary>
		/// Sets the fusion error message.
		/// </summary>
		/// <param name="message"></param>
		[PublicAPI]
		void SetErrorMessage(string message);
	}
}
