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
			ushort offset = GetCurrentOffset(mapping);

			if (offset + mapping.FirstSig > mapping.LastSig)
				throw new IndexOutOfRangeException("Sigs have exceeded maximum range allowed by mapping.");

			m_Ranges[mapping] = (ushort)(offset + 1);
			return (ushort)(mapping.FirstSig + offset);
		}

		public ushort GetCurrentOffset(FusionSigMultiMapping mapping)
		{
			return m_Ranges.GetDefault(mapping);
		}
	}
}