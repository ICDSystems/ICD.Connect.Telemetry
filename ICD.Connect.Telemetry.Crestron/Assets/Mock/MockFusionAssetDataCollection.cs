using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;

namespace ICD.Connect.Telemetry.Crestron.Assets.Mock
{
	public sealed class MockFusionAssetDataCollection : IFusionAssetDataCollection
	{
		private readonly IcdOrderedDictionary<uint, IMockFusionAsset> m_Assets;

		/// <summary>
		/// Gets the fusion asset data with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public FusionAssetData this[uint id] { get { return m_Assets[id].GetFusionAssetData(); } }
        
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="assets"></param>
		public MockFusionAssetDataCollection(IDictionary<uint, IMockFusionAsset> assets)
		{
			m_Assets = new IcdOrderedDictionary<uint, IMockFusionAsset>(assets);
		}

		public IEnumerator<FusionAssetData> GetEnumerator()
		{
			return m_Assets.Values
			               .Select(a => a.GetFusionAssetData())
			               .ToList()
			               .GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
