using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Mappings;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public abstract class AbstractFusionSigMapping : AbstractTelemetryMappingBase, IFusionSigMapping
	{
		public string FusionSigName { get; set; }

		/// <summary>
		/// Whitelist for the fusion asset types this mapping is valid for.
		/// </summary>
		public IEnumerable<Type> FusionAssetTypes { get; set; }

		/// <summary>
		/// Whitelist for the telemetry provider types this mapping is valid for.
		/// </summary>
		public IEnumerable<Type> TelemetryProviderTypes { get; set; }

		public eSigType SigType { get; set; }
		public IEnumerable<Type> TargetAssetTypes { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractFusionSigMapping()
		{
			FusionAssetTypes = new IcdHashSet<Type> {typeof(IFusionStaticAsset)};
		}

		public abstract FusionTelemetryBinding Bind(IFusionRoom room, ITelemetryItem node, uint assetId, RangeMappingUsageTracker mappingUsage);
	}
}