using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Mappings;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public abstract class AbstractFusionSigMapping : AbstractTelemetryMappingBase, IFusionSigMapping
	{
		public string FusionSigName { get; set; }

		/// <summary>
		/// Whitelist for the fusion asset types this mapping is valid for.
		/// </summary>
		public IEnumerable<eAssetType> FusionAssetTypes { get; set; }

		/// <summary>
		/// Whitelist for the telemetry provider types this mapping is valid for.
		/// </summary>
		public IEnumerable<Type> TelemetryProviderTypes { get; set; }

		public eSigType SigType { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractFusionSigMapping()
		{
			FusionAssetTypes = new IcdHashSet<eAssetType> {eAssetType.StaticAsset};
		}

		/// <summary>
		/// Creates the telemetry binding for the given leaf.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="leaf"></param>
		/// <param name="assetId"></param>
		/// <param name="mappingUsage"></param>
		/// <returns></returns>
		[NotNull]
		public abstract FusionTelemetryBinding Bind([NotNull] IFusionRoom room, [NotNull] TelemetryLeaf leaf,
		                                            uint assetId, [NotNull] RangeMappingUsageTracker mappingUsage);

		/// <summary>
		/// Returns true if this mapping is valid for the given provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		public bool ValidateProvider([NotNull] ITelemetryProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			if (TelemetryProviderTypes != null && !provider.GetType().GetAllTypes().Any(t => TelemetryProviderTypes.Contains(t)))
				return false;

			return true;
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