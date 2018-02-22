using ICD.Common.Properties;
using ICD.Connect.Analytics.Assets;
using ICD.Connect.Devices;
using ICD.Connect.Panels;

namespace ICD.Connect.Analytics
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
		/// Sets the fusion error message.
		/// </summary>
		/// <param name="message"></param>
		[PublicAPI]
		void SetErrorMessage(string message);
	}
}
