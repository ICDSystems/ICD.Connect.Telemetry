using System;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public abstract class FusionSigMappingBase : IFusionSigMapping
	{
		public string TelemetrySetName { get; set; }
		public string TelemetryGetName { get; set; }
		public string FusionSigName { get; set; }

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
	}
}