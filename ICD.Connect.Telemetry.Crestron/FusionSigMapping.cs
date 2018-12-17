using System;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron
{
	public class FusionSigMapping : IEquatable<FusionSigMapping>
	{
		public string TelemetrySetName { get; set; }
		public string TelemetryGetName { get; set; }
		public string FusionSigName { get; set; }
		public uint Sig { get; set; }

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

		public bool Equals(FusionSigMapping other)
		{
			return other != null &&
			       TelemetrySetName == other.TelemetrySetName &&
			       TelemetryGetName == other.TelemetryGetName &&
			       FusionSigName == other.FusionSigName &&
			       Sig == other.Sig &&
			       SigType == other.SigType;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (TelemetrySetName == null ? 0 : TelemetrySetName.GetHashCode());
				hash = hash * 23 + (TelemetryGetName == null ? 0 : TelemetryGetName.GetHashCode());
				hash = hash * 23 + (FusionSigName == null ? 0 : FusionSigName.GetHashCode());
				hash = hash * 23 + (int)Sig;
				hash = hash * 23 + (int)SigType;
				return hash;
			}
		}
	}
}