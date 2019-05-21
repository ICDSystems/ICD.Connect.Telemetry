using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public interface IFusionAssetDataCollection
	{
		/// <summary>
		/// Gets the fusion asset data with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		FusionAssetData this[uint id] { get; }

		/// <summary>
		/// Gets the first asset data with the given asset type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[CanBeNull]
		FusionAssetData GetAssetData(eAssetType type);
	}
}
