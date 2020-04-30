using System.Collections.Generic;

namespace ICD.Connect.Telemetry.Crestron.Assets.Mock
{
	public sealed class MockFusionAssetDataCollection : IFusionAssetDataCollection
	{
		private readonly Dictionary<uint, IMockFusionAsset> m_Assets;

		/// <summary>
		/// Gets the fusion asset data with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public FusionAssetData this[uint id] { get { return m_Assets[id].GetFusionAssetData(); } }

		public MockFusionAssetDataCollection(Dictionary<uint, IMockFusionAsset> assets)
		{
			m_Assets = assets;
		}
	}
}
