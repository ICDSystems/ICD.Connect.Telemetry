﻿using System;
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

			// Special case
			if (mapping.Sig == 0)
				return 0;

			ushort offset = GetCurrentOffset(mapping);
			if (offset >= mapping.Range)
			{
				string message = string.Format("Offset {0} exceeds maximum range allowed by {1}", offset, mapping);
				throw new IndexOutOfRangeException(message);
			}

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
