using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Collections;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class AssetFusionSigMapping : AbstractFusionSigMapping
	{
		/// <summary>
		/// Whitelist for the fusion asset types this mapping is valid for.
		/// </summary>
		public IEnumerable<eAssetType> FusionAssetTypes { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssetFusionSigMapping()
		{
			FusionAssetTypes = new IcdHashSet<eAssetType> { eAssetType.StaticAsset };
		}

		/// <summary>
		/// Creates the telemetry binding for the given provider.
		/// </summary>
		/// <param name="fusionRoom"></param>
		/// <param name="leaf"></param>
		/// <param name="assetId"></param>
		/// <param name="mappingUsage"></param>
		/// <returns></returns>
		[NotNull]
		public FusionTelemetryBinding Bind([NotNull] IFusionRoom fusionRoom,
		                                            [NotNull] TelemetryLeaf leaf,
		                                            uint assetId,
		                                            [NotNull] MappingUsageTracker mappingUsage)
		{
			string name = string.Format(FusionSigName, mappingUsage.GetCurrentOffset(this) + 1);
			ushort sig = mappingUsage.GetNextSig(this);

			AssetFusionSigMapping tempMapping = new AssetFusionSigMapping
			{
				FusionSigName = string.Format(FusionSigName, mappingUsage.GetCurrentOffset(this) + 1),
				Sig = mappingUsage.GetNextSig(this),
				SigType = SigType,
				TelemetryName = TelemetryName,
				TelemetryProviderTypes = TelemetryProviderTypes
			};

			return FusionTelemetryBinding.Bind(fusionRoom, leaf, tempMapping, assetId);
		}

		/// <summary>
		/// Returns true if this mapping is valid for the given asset type.
		/// </summary>
		/// <param name="assetType"></param>
		/// <returns></returns>
		public bool ValidateAsset(eAssetType assetType)
		{
			if (FusionAssetTypes != null && !FusionAssetTypes.Contains(assetType))
				return false;

			return assetType == eAssetType.StaticAsset;
		}
	}
}
