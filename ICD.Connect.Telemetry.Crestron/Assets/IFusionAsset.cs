namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public interface IFusionAsset
	{
		/// <summary>
		/// Gets the name of the asset.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the user defined name of the asset type.
		/// </summary>
		string Type { get; }

		/// <summary>
		/// Gets the user defined instance id of the asset type.
		/// </summary>
		string InstanceId { get; }

		/// <summary>
		/// Gets the user defined number of the asset type.
		/// </summary>
		uint Number { get; }

		/// <summary>
		/// Gets the asset type.
		/// </summary>
		eAssetType AssetType { get; }
	}
}
