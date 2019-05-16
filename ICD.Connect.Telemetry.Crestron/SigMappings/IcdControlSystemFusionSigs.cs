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
				TelemetrySetName = string.Empty,
				FusionSigName = "DHCP On",
				SigType = eSigType.Digital,
				Sig = 54
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_MODEL,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor Model",
				SigType = eSigType.Serial,
				Sig = 50
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_FIRMWARE_VER,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor Firmware",
				SigType = eSigType.Serial,
				Sig = 51
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_FIRMWARE_DATE,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor FW Date",
				SigType = eSigType.Serial,
				Sig = 52
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_MAC_ADDRESS,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor MAC",
				SigType = eSigType.Serial,
				Sig = 53
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor IP",
				SigType = eSigType.Serial,
				Sig = 54
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor Hostname",
				SigType = eSigType.Serial,
				Sig = 55
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_SERIAL_NUMBER,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor Serial No",
				SigType = eSigType.Serial,
				Sig = 56
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_UPTIME,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor Uptime",
				SigType = eSigType.Serial,
				Sig = 57
			},
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
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS_SECONDARY,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor IP 2",
				SigType = eSigType.Serial,
				Sig = 63
			},
			new FusionSigMapping
			{
				TelemetryGetName = ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME_SECONDARY,
				TelemetrySetName = string.Empty,
				FusionSigName = "Processor Hostname Custom",
				SigType = eSigType.Serial,
				Sig = 64
			},
		};
	}
}
#endif