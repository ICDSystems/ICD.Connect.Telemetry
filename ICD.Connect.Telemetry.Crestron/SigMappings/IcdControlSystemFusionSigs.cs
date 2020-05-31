using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Routing.CrestronPro.ControlSystem;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdControlSystemFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROGRAM_UPTIME,
				TelemetrySetName = string.Empty,
				FusionSigName = "Program Uptime",
				SigType = eSigType.Serial,
				Sig = 58
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROGRAMMER_NAME,
				TelemetrySetName = string.Empty,
				FusionSigName = "Programmer Name",
				SigType = eSigType.Serial,
				Sig = 59
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.SYSTEM_NAME,
				TelemetrySetName = string.Empty,
				FusionSigName = "System Name",
				SigType = eSigType.Serial,
				Sig = 60
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROGRAM_SOURCE_FILE,
				TelemetrySetName = string.Empty,
				FusionSigName = "Program Source File",
				SigType = eSigType.Serial,
				Sig = 61
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROGRAM_COMPLIE_DATE,
				TelemetrySetName = string.Empty,
				FusionSigName = "Program Compile Date",
				SigType = eSigType.Serial,
				Sig = 62
			}
		};
	}
}
