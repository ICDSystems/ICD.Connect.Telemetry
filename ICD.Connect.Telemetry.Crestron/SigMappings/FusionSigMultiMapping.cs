using System;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class FusionSigMultiMapping : AbstractFusionSigMapping
	{
		private ushort m_FirstSig;
		private ushort m_LastSig;

		public ushort FirstSig
		{
			get { return m_FirstSig; }
			set
			{
				m_FirstSig = value;
			}
		}

		public ushort LastSig
		{
			get { return m_LastSig; }
			set
			{
				if (value < m_FirstSig)
					throw new ArgumentOutOfRangeException("value", "Last Sig in range must be greater than the first Sig in range.");

				m_LastSig = value;
			}
		}

		public string GetNameFromTemplate(ushort sigNumber)
		{
			if (sigNumber < m_FirstSig || sigNumber > m_LastSig)
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

		public override FusionTelemetryBinding Bind(IFusionRoom room, ITelemetryItem node, uint assetId,
		                                            RangeMappingUsageTracker mappingUsage)
		{
			FusionSigMapping tempMapping = new FusionSigMapping
			{
				FusionSigName = string.Format(FusionSigName, mappingUsage.GetCurrentOffset(this) + 1),
				Sig = mappingUsage.GetNextSig(this),
				SigType = SigType,
				TelemetryGetName = TelemetryGetName,
				TelemetrySetName = TelemetrySetName,
				TelemetryProviderTypes = TelemetryProviderTypes
			};

			return FusionTelemetryBinding.Bind(room, node.Parent, tempMapping, assetId);
		}
	}
}
