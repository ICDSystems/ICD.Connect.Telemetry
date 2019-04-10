using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public abstract class FusionSigMappingBase : IFusionSigMapping
	{
		public string TelemetrySetName { get; set; }
		public string TelemetryGetName { get; set; }
		public string FusionSigName { get; set; }
		public IEnumerable<Type> TargetAssetTypes { get; set; }

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
		public override abstract int GetHashCode();

		public FusionSigMappingBase()
		{
			TargetAssetTypes = new IcdHashSet<Type> {typeof(IFusionStaticAsset)};
		}
	}
}