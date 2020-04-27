using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public abstract class AbstractFusionSigMapping : IFusionSigMapping
	{
		public string TelemetrySetName { get; set; }
		public string TelemetryGetName { get; set; }
		public string FusionSigName { get; set; }

		/// <summary>
		/// Whitelist for the fusion asset types this mapping is valid for.
		/// </summary>
		public IEnumerable<Type> FusionAssetTypes { get; set; }

		/// <summary>
		/// Whitelist for the telemetry provider types this mapping is valid for.
		/// </summary>
		public IEnumerable<Type> TelemetryProviderTypes { get; set; }

		public eSigIoMask IoMask
		{
			get
			{
				eSigIoMask output = eSigIoMask.Na;

				if (!string.IsNullOrEmpty(TelemetrySetName))
					output |= eSigIoMask.FusionToProgram;

				if (!string.IsNullOrEmpty(TelemetryGetName))
					output |= eSigIoMask.ProgramToFusion;

				return output;
			}
		}

		public eSigType SigType { get; set; }

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