using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Routing.CrestronPro.ControlSystem;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Assets
{
	public static class IcdControlSystemFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			new IcdHashSet<AssetFusionSigMapping>
			{
				new AssetFusionSigMapping
				{
					TelemetryName = ControlSystemExternalTelemetryNames.PROGRAM_UPTIME,
					FusionSigName = "Program Uptime",
					SigType = eSigType.Serial,
					Sig = 58
				},
				new AssetFusionSigMapping
				{
					TelemetryName = ControlSystemExternalTelemetryNames.PROGRAMMER_NAME,
					FusionSigName = "Programmer Name",
					SigType = eSigType.Serial,
					Sig = 59
				},
				new AssetFusionSigMapping
				{
					TelemetryName = ControlSystemExternalTelemetryNames.SYSTEM_NAME,
					FusionSigName = "System Name",
					SigType = eSigType.Serial,
					Sig = 60
				},
				new AssetFusionSigMapping
				{
					TelemetryName = ControlSystemExternalTelemetryNames.PROGRAM_SOURCE_FILE,
					FusionSigName = "Program Source File",
					SigType = eSigType.Serial,
					Sig = 61
				},
				new AssetFusionSigMapping
				{
					TelemetryName = ControlSystemExternalTelemetryNames.PROGRAM_COMPLIE_DATE,
					FusionSigName = "Program Compile Date",
					SigType = eSigType.Serial,
					Sig = 62
				}
			};
	}
}
