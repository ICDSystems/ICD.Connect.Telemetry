using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Mappings;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public abstract class AbstractFusionSigMapping : AbstractTelemetryMappingBase, IFusionSigMapping
	{
		public string FusionSigName { get; set; }
		public eSigType SigType { get; set; }
		public IEnumerable<Type> TargetAssetTypes { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractFusionSigMapping()
		{
			TargetAssetTypes = new IcdHashSet<Type> {typeof(IFusionStaticAsset)};
		}
	}
}