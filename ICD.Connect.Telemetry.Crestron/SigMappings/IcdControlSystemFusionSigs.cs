using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Routing.Telemetry;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdControlSystemFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_AssetMappings = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryName = ControlSystemExternalTelemetryNames.PROGRAM_UPTIME,
				FusionSigName = "Program Uptime",
				SigType = eSigType.Serial,
				Sig = 58
			},
			new FusionSigMapping
			{
				TelemetryName = ControlSystemExternalTelemetryNames.PROGRAMMER_NAME,
				FusionSigName = "Programmer Name",
				SigType = eSigType.Serial,
				Sig = 59
			},
			new FusionSigMapping
			{
				TelemetryName = ControlSystemExternalTelemetryNames.SYSTEM_NAME,
				FusionSigName = "System Name",
				SigType = eSigType.Serial,
				Sig = 60
			},
			new FusionSigMapping
			{
				TelemetryName = ControlSystemExternalTelemetryNames.PROGRAM_SOURCE_FILE,
				FusionSigName = "Program Source File",
				SigType = eSigType.Serial,
				Sig = 61
			},
			new FusionSigMapping
			{
				TelemetryName = ControlSystemExternalTelemetryNames.PROGRAM_COMPLIE_DATE,
				FusionSigName = "Program Compile Date",
				SigType = eSigType.Serial,
				Sig = 62
			}
		};
	}
}
