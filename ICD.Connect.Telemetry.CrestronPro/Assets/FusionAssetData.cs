namespace ICD.Connect.Telemetry.CrestronPro.Assets
{
	public sealed class FusionAssetData
	{
		private readonly uint m_Number;
		private readonly eAssetType m_Type;
		private readonly IFusionAsset m_Asset;

		/// <summary>
		/// Get the asset.
		/// </summary>
		public IFusionAsset Asset { get { return m_Asset; } }

		/// <summary>
		/// Key for the asset.
		/// </summary>
		public uint Number { get { return m_Number; } }

		/// <summary>
		/// Type of asset.
		/// </summary>
		public eAssetType Type { get { return m_Type; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="type"></param>
		/// <param name="asset"></param>
		public FusionAssetData(uint number, eAssetType type, IFusionAsset asset)
		{
			m_Number = number;
			m_Type = type;
			m_Asset = asset;
		}
	}
}
