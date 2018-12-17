using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Connect.Displays;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron;

namespace ICD.Connect.Telemetry.CrestronPro.SigMappings
{
	public static class IcdDisplayFusionSigs
	{
		public static IEnumerable<FusionSigMapping> Sigs {get { return IcdStandardFusionSigs.Sigs.Concat(s_Sigs); }}

		private static readonly IcdHashSet<FusionSigMapping> s_Sigs = new IcdHashSet<FusionSigMapping>
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