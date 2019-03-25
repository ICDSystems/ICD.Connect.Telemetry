using System;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class FusionSigRangeMapping : FusionSigMappingBase, IEquatable<FusionSigRangeMapping>
	{
		private ushort m_FirstSig;
		private ushort m_CurrentSig;
		private ushort m_LastSig;

		public ushort FirstSig
		{
			get
			{
				return m_FirstSig;
			}
			set
			{
				m_CurrentSig = value;
				m_FirstSig = value;
			}
		}

		public ushort LastSig
		{
			get
			{
				return m_LastSig;
			}
			set
			{
				if(value < m_FirstSig)
					throw new ArgumentOutOfRangeException("value", "Last Sig in range must be greater than the first Sig in range.");

				m_LastSig = value;
			}
		}

		public string GetNameFromTemplate(ushort sigNumber)
		{
			if(sigNumber < m_FirstSig || sigNumber > m_LastSig)
				throw new ArgumentOutOfRangeException("sigNumber", "sigNumber outside specified range.");

			int rangeSize = m_LastSig - m_FirstSig;
			int positionInRange = m_LastSig - sigNumber;

			if (rangeSize < 10)
				return string.Format(FusionSigName, positionInRange.ToString("D1"));
			if (rangeSize < 100)
				return string.Format(FusionSigName, positionInRange.ToString("D2"));
			if (rangeSize < 1000)
				return string.Format(FusionSigName, positionInRange.ToString("D3"));
			if (rangeSize < 10000)
				return string.Format(FusionSigName, positionInRange.ToString("D4"));

			return string.Format(FusionSigName, positionInRange.ToString("D5"));
		}

		public bool Equals(FusionSigRangeMapping other)
		{
			return other != null &&
				   TelemetrySetName == other.TelemetrySetName &&
				   TelemetryGetName == other.TelemetryGetName &&
				   FusionSigName == other.FusionSigName &&
				   FirstSig == other.FirstSig &&
				   LastSig == other.LastSig &&
				   SigType == other.SigType;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 23 + (TelemetrySetName == null ? 0 : TelemetrySetName.GetHashCode());
			hash = hash * 23 + (TelemetryGetName == null ? 0 : TelemetryGetName.GetHashCode());
			hash = hash * 23 + (FusionSigName == null ? 0 : FusionSigName.GetHashCode());
			hash = hash * 23 + FirstSig;
			hash = hash * 23 + LastSig;
			hash = hash * 23 + (int)SigType;
			return hash;
		}
	}
}