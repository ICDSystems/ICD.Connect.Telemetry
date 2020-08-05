using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class MappingUsageTracker
	{
		private readonly Dictionary<IFusionSigMapping, ushort> m_Ranges =
			new Dictionary<IFusionSigMapping, ushort>();

		public ushort GetNextSig([NotNull] IFusionSigMapping mapping)
		{
			if (mapping == null)
				throw new ArgumentNullException("mapping");

			ushort offset = GetCurrentOffset(mapping);
			if (offset >= mapping.Range)
				throw new IndexOutOfRangeException("Sigs have exceeded maximum range allowed by mapping.");

			m_Ranges[mapping] = (ushort)(offset + 1);
			return (ushort)(mapping.Sig + offset);
		}

		public ushort GetCurrentOffset([NotNull] IFusionSigMapping mapping)
		{
			if (mapping == null)
				throw new ArgumentNullException("mapping");

			return m_Ranges.GetDefault(mapping);
		}
	}
}
