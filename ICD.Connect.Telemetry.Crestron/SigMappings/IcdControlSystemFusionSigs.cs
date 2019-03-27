#if SIMPLSHARP
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
				TelemetryGetName = ControlSystemExternalTelemetryNames.DHCP_STATUS,
				TelemetrySetName = "",
				FusionSigName = "DHCP On",
				SigType = eSigType.Digital,
				Sig = 54
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_MODEL,
				TelemetrySetName = "",
				FusionSigName = "Processor Model",
				SigType = eSigType.Serial,
				Sig = 50
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_FIRMWARE_VER,
				TelemetrySetName = "",
				FusionSigName = "Processor Firmware",
				SigType = eSigType.Serial,
				Sig = 51
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_FIRMWARE_DATE,
				TelemetrySetName = "",
				FusionSigName = "Processor FW Date",
				SigType = eSigType.Serial,
				Sig = 52
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_MAC_ADDRESS,
				TelemetrySetName = "",
				FusionSigName = "Processor MAC",
				SigType = eSigType.Serial,
				Sig = 53
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS,
				TelemetrySetName = "",
				FusionSigName = "Processor IP",
				SigType = eSigType.Serial,
				Sig = 54
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME,
				TelemetrySetName = "",
				FusionSigName = "Processor Hostname",
				SigType = eSigType.Serial,
				Sig = 55
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_SERIAL_NUMBER,
				TelemetrySetName = "",
				FusionSigName = "Processor Serial No",
				SigType = eSigType.Serial,
				Sig = 56
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_UPTIME,
				TelemetrySetName = "",
				FusionSigName = "Processor Uptime",
				SigType = eSigType.Serial,
				Sig = 57
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROGRAM_UPTIME,
				TelemetrySetName = "",
				FusionSigName = "Program Uptime",
				SigType = eSigType.Serial,
				Sig = 58
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROGRAMMER_NAME,
				TelemetrySetName = "",
				FusionSigName = "Programmer Name",
				SigType = eSigType.Serial,
				Sig = 59
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.SYSTEM_NAME,
				TelemetrySetName = "",
				FusionSigName = "System Name",
				SigType = eSigType.Serial,
				Sig = 60
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROGRAM_SOURCE_FILE,
				TelemetrySetName = "",
				FusionSigName = "Program Source File",
				SigType = eSigType.Serial,
				Sig = 61
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROGRAM_COMPLIE_DATE,
				TelemetrySetName = "",
				FusionSigName = "Program Compile Date",
				SigType = eSigType.Serial,
				Sig = 62
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS_SECONDARY,
				TelemetrySetName = "",
				FusionSigName = "Processor IP 2",
				SigType = eSigType.Serial,
				Sig = 63
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME_SECONDARY,
				TelemetrySetName = "",
				FusionSigName = "Processor Hostname Custom",
				SigType = eSigType.Serial,
				Sig = 64
			},
		};
	}
}
#endif