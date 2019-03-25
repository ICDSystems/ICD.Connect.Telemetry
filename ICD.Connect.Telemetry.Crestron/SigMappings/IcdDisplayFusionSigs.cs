using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Displays;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdDisplayFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = DisplayTelemetryNames.ACTIVE_INPUT_STATE,
				TelemetrySetName = DisplayTelemetryNames.SET_ACTIVE_INPUT,
				FusionSigName = "Input Selection",
				SigType = eSigType.Analog,
				Sig = 1701
			}
		};
	}
}