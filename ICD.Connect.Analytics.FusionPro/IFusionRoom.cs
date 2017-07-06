using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Panels;

namespace ICD.Connect.Analytics.FusionPro
{
	public interface IFusionRoom : ISigDevice, IDevice
	{
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
