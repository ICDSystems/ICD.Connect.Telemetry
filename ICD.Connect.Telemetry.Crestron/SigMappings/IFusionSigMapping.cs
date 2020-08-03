using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Mappings;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public interface IFusionSigMapping : ITelemetryMapping
	{
		string FusionSigName { get; }

		eSigType SigType { get; }
		
		/// <summary>
		/// What provider types this binding can use
		/// If null, all types are allowed
		/// </summary>
		IEnumerable<Type> TelemetryProviderTypes { get; }

		/// <summary>
		/// Whitelist for the fusion asset types this mapping is valid for.
		/// If null, treated as a static asset.
		/// </summary>
		IEnumerable<eAssetType> FusionAssetTypes { get; set; }

		/// <summary>
		/// Creates the telemetry binding for the given provider.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="leaf"></param>
		/// <param name="assetId"></param>
		/// <param name="mappingUsage"></param>
		/// <returns></returns>
		[NotNull]
		FusionTelemetryBinding Bind([NotNull] IFusionRoom room, [NotNull] TelemetryLeaf leaf, uint assetId,
		                            [NotNull] RangeMappingUsageTracker mappingUsage);

		/// <summary>
		/// Returns true if this mapping is valid for the given provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		bool ValidateProvider([NotNull] ITelemetryProvider provider);

		/// <summary>
		/// Returns true if this mapping is valid for the given asset type.
		/// </summary>
		/// <param name="assetType"></param>
		/// <returns></returns>
		bool ValidateAsset(eAssetType assetType);
	}
}