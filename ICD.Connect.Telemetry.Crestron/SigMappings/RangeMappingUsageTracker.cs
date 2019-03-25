using System;
using System.Collections.Generic;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class RangeMappingUsageTracker
	{
		private readonly Dictionary<FusionSigRangeMapping, ushort> m_Ranges = new Dictionary<FusionSigRangeMapping, ushort>();

		public ushort GetNextSig(FusionSigRangeMapping mapping)
		{
			ushort currentSig;
			if (!m_Ranges.TryGetValue(mapping, out currentSig))
			{
				AddMappingType(mapping);
				currentSig = mapping.FirstSig;
			}

			if (currentSig > mapping.LastSig)
				throw new IndexOutOfRangeException("Sigs have exceeded maximum range allowed by mapping.");

			m_Ranges[mapping] = (ushort)(currentSig + 1);
			return currentSig;
		}

		private void AddMappingType(FusionSigRangeMapping mapping)
		{
			if (m_Ranges.ContainsKey(mapping))
				return;

			m_Ranges.Add(mapping, mapping.FirstSig);
		}
	}
}