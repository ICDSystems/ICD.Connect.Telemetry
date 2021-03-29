using System.Collections;
using System.Linq;
using ICD.Common.Utils.Collections;
#if SIMPLSHARP
using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using ICD.Common.Utils;
using ICD.Connect.Telemetry.Crestron.Assets;
using eAssetType = ICD.Connect.Telemetry.Crestron.Assets.eAssetType;

namespace ICD.Connect.Telemetry.CrestronPro.Assets
{
	public sealed class CustomFusionAssetDataCollectionAdapter : IFusionAssetDataCollection
	{
		private readonly CrestronCollection<CustomFusionAssetData> m_Collection;
		private readonly IcdSortedDictionary<uint, FusionAssetData> m_AdapterCache;
		private readonly Dictionary<eAssetType, List<uint>> m_TypeToAdapters; 
		private readonly SafeCriticalSection m_Section;

		/// <summary>
		/// Gets the fusion asset data with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public FusionAssetData this[uint id] { get { return LazyLoadAdapter(id); } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection"></param>
		public CustomFusionAssetDataCollectionAdapter(CrestronCollection<CustomFusionAssetData> collection)
		{
			m_Collection = collection;
			m_AdapterCache = new IcdSortedDictionary<uint, FusionAssetData>();
			m_TypeToAdapters = new Dictionary<eAssetType, List<uint>>();
			m_Section = new SafeCriticalSection();
		}

		/// <summary>
		/// Gets or builds the adapter for the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private FusionAssetData LazyLoadAdapter(uint id)
		{
			m_Section.Enter();

			try
			{
				if (!m_AdapterCache.ContainsKey(id))
				{
					CustomFusionAssetData assetData = m_Collection[id];
					FusionAssetBase asset = assetData.Asset;
					eAssetType assetType = assetData.Type.ToIcd();

					IFusionAsset adapter = BuildAdapterForAsset(assetData, asset);
					FusionAssetData data = new FusionAssetData(id, assetType, adapter);

					m_AdapterCache.Add(id, data);

					if (!m_TypeToAdapters.ContainsKey(assetType))
						m_TypeToAdapters.Add(assetType, new List<uint>());

					if (!m_TypeToAdapters[assetType].Contains(id))
						m_TypeToAdapters[assetType].Add(id);
				}

				return m_AdapterCache[id];
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Creates a new adapter given the crestron info.
		/// </summary>
		/// <param name="assetData"></param>
		/// <param name="asset"></param>
		/// <returns></returns>
		private IFusionAsset BuildAdapterForAsset(CustomFusionAssetData assetData, FusionAssetBase asset)
		{
			switch (assetData.Type)
			{
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.OccupancySensor:
					return new FusionOccupancySensorAdapter(asset as FusionOccupancySensor);

				case global::Crestron.SimplSharpPro.Fusion.eAssetType.RemoteOccupancySensor:
					return new FusionRemoteOccupancySensorAdapter(asset as FusionRemoteOccupancySensor);
				
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.StaticAsset:
					return new FusionStaticAssetAdapter(asset as FusionStaticAsset);

				case global::Crestron.SimplSharpPro.Fusion.eAssetType.NA:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.DemandResponse:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.DynamicAsset:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.EnergyLoad:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.EnergySupply:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.HvacZone:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.LightingLoad:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.LightingScenes:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.Logging:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.PhotocellSensor:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.RemoteRealTimePower:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.ShadeLoad:
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.ShadePresets:
					throw new NotSupportedException();

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public IEnumerator<FusionAssetData> GetEnumerator()
		{
			return m_Section.Execute(() => m_AdapterCache.Values.ToList().GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
#endif
