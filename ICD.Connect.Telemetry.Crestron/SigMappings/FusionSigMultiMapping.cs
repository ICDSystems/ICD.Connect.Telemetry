using System;
using ICD.Common.Properties;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public sealed class FusionSigMultiMapping : AbstractFusionSigMapping
	{
		private ushort m_FirstSig;
		private ushort m_LastSig;

		public ushort FirstSig { get { return m_FirstSig; } set { m_FirstSig = value; } }

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

		/// <summary>
		/// Creates the telemetry binding for the given provider.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="leaf"></param>
		/// <param name="assetId"></param>
		/// <param name="mappingUsage"></param>
		/// <returns></returns>
		[NotNull]
		public override FusionTelemetryBinding Bind([NotNull] IFusionRoom room, [NotNull] TelemetryLeaf leaf,
		                                            uint assetId,
		                                            [NotNull] RangeMappingUsageTracker mappingUsage)
		{
			FusionSigMapping tempMapping = new FusionSigMapping
			{
				FusionSigName = string.Format(FusionSigName, mappingUsage.GetCurrentOffset(this) + 1),
				Sig = mappingUsage.GetNextSig(this),
				SigType = SigType,
				TelemetryName = TelemetryName,
				TelemetryProviderTypes = TelemetryProviderTypes
			};

			return FusionTelemetryBinding.Bind(room, leaf, tempMapping, assetId);
		}
	}
}
