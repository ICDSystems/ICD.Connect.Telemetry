using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class RangeMappingUsageTracker
	{
		private readonly Dictionary<FusionSigMultiMapping, ushort> m_Ranges = new Dictionary<FusionSigMultiMapping, ushort>();

		public ushort GetNextSig(FusionSigMultiMapping mapping)
		{
			ushort offset;
			if (!m_Ranges.TryGetValue(mapping, out offset))
			{
				AddMappingType(mapping);
			}

			if (offset + mapping.FirstSig > mapping.LastSig)
				throw new IndexOutOfRangeException("Sigs have exceeded maximum range allowed by mapping.");

			m_Ranges[mapping] = (ushort)(offset + 1);
			return (ushort)(mapping.FirstSig + offset);
		}

		public ushort GetCurrentOffset(FusionSigMultiMapping mapping)
		{
			return m_Ranges.GetDefault(mapping);
		}

		private void AddMappingType(FusionSigMultiMapping mapping)
		{
			if (m_Ranges.ContainsKey(mapping))
				return;

			m_Ranges.Add(mapping, 0);
		}
	}
}