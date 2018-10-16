#if SIMPLSHARP
using Crestron.SimplSharpPro.Fusion;

namespace ICD.Connect.Telemetry.CrestronPro.Assets
{
	public abstract class AbstractFusionAssetAdapter<TAsset> : IFusionAsset
		where TAsset : FusionAssetBase
	{
		private readonly TAsset m_FusionAsset;

		/// <summary>
		/// Gets the wrapped fusion asset.
		/// </summary>
		public TAsset FusionAsset { get { return m_FusionAsset; } }

		/// <summary>
		/// Gets the name of the asset.
		/// </summary>
		public string Name { get { return m_FusionAsset.ParamAssetName; } }

		/// <summary>
		/// Gets the user defined name of the asset type.
		/// </summary>
		public string Type { get { return m_FusionAsset.ParamAssetType; } }

		/// <summary>
		/// Gets the user defined instance id of the asset type.
		/// </summary>
		public string InstanceId { get { return m_FusionAsset.ParamInstanceID; } }

		/// <summary>
		/// Gets the user defined number of the asset type.
		/// </summary>
		public uint Number { get { return m_FusionAsset.ParamAssetNumber; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionAsset"></param>
		protected AbstractFusionAssetAdapter(TAsset fusionAsset)
		{
			m_FusionAsset = fusionAsset;
		}
	}
}
#endif
