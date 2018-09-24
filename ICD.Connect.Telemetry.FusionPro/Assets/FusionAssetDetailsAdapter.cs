#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using ICD.Common.Utils;
using ICD.Connect.Analytics.Assets;
using eAssetType = Crestron.SimplSharpPro.Fusion.eAssetType;

namespace ICD.Connect.Analytics.FusionPro.Assets
{
	public sealed class FusionAssetDetailsAdapter : IFusionAssetDataCollection
	{
		private readonly CrestronCollection<CustomFusionAssetData> m_Collection;
		private readonly Dictionary<uint, FusionAssetData> m_AdapterCache;
		private readonly Dictionary<Analytics.Assets.eAssetType, List<uint>> m_TypeToAdapters; 
		private readonly SafeCriticalSection m_Section;

		/// <summary>
		/// Gets the fusion asset data with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public FusionAssetData this[uint id] { get { return LazyLoadAdapter(id); } }

		/// <summary>
		/// Gets the first asset data with the given asset type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public FusionAssetData GetAssetData(Analytics.Assets.eAssetType type)
		{
			m_Section.Enter();

			try
			{
				if (!m_TypeToAdapters.ContainsKey(type))
					return null;

				return m_TypeToAdapters[type].Select(id => LazyLoadAdapter(id))
				                             .FirstOrDefault();
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection"></param>
		public FusionAssetDetailsAdapter(CrestronCollection<CustomFusionAssetData> collection)
		{
			m_Collection = collection;
			m_AdapterCache = new Dictionary<uint, FusionAssetData>();
			m_TypeToAdapters = new Dictionary<Analytics.Assets.eAssetType, List<uint>>();
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
					Analytics.Assets.eAssetType assetType = assetData.Type.ToIcd();

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
				case eAssetType.OccupancySensor:
					return new FusionOccupancySensorAdapter(asset as FusionOccupancySensor);

				case eAssetType.RemoteOccupancySensor:
					return new FusionRemoteOccupancySensorAdapter(asset as FusionRemoteOccupancySensor);

				case eAssetType.NA:
				case eAssetType.DemandResponse:
				case eAssetType.DynamicAsset:
				case eAssetType.EnergyLoad:
				case eAssetType.EnergySupply:
				case eAssetType.HvacZone:
				case eAssetType.LightingLoad:
				case eAssetType.LightingScenes:
				case eAssetType.Logging:
				case eAssetType.PhotocellSensor:
				case eAssetType.RemoteRealTimePower:
				case eAssetType.ShadeLoad:
				case eAssetType.ShadePresets:
				case eAssetType.StaticAsset:
					throw new NotImplementedException();

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
#endif
