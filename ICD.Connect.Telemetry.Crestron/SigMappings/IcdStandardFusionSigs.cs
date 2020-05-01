using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Settings;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdStandardFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = OriginatorTelemetryNames.ID,
				TelemetrySetName = string.Empty,
				FusionSigName = "Id",
				SigType = eSigType.Serial,
				Sig = 24951
			},
			new FusionSigMapping
			{
				TelemetryGetName = OriginatorTelemetryNames.NAME,
				TelemetrySetName = string.Empty,
				FusionSigName = "ComponentName",
				SigType = eSigType.Serial,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.ONLINE_STATE,
				TelemetrySetName = string.Empty,
				FusionSigName = "Connected",
				SigType = eSigType.Digital,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.POWER_STATE, 
				TelemetrySetName = DeviceTelemetryNames.POWER_ON,
				FusionSigName = "PowerOn",
				SigType = eSigType.Digital,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryGetName = string.Empty,
				TelemetrySetName = DeviceTelemetryNames.POWER_OFF,
				FusionSigName = "PowerOff",
				SigType = eSigType.Digital,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_MODEL,
				TelemetrySetName = string.Empty,
				FusionSigName = "Model",
				SigType = eSigType.Serial,
				Sig = 50
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_FIRMWARE_VERSION,
				TelemetrySetName = string.Empty,
				FusionSigName = "Firmware",
				SigType = eSigType.Serial,
				Sig = 51
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_FIRMWARE_DATE,
				TelemetrySetName = string.Empty,
				FusionSigName = "Firmware Date",
				SigType = eSigType.Serial,
				Sig = 52
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_SERIAL_NUMBER,
				TelemetrySetName = string.Empty,
				FusionSigName = "Serial No",
				SigType = eSigType.Serial,
				Sig = 56
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_UPTIME,
				TelemetrySetName = string.Empty,
				FusionSigName = "Uptime",
				SigType = eSigType.Serial,
				Sig = 57
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_MAC_ADDRESS,
				TelemetrySetName = string.Empty,
				FusionSigName = "MAC Address",
				SigType = eSigType.Serial,
				Sig = 100
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_HOSTNAME,
				TelemetrySetName = string.Empty,
				FusionSigName = "Hostname",
				SigType = eSigType.Serial,
				Sig = 101
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_IP_ADDRESS,
				TelemetrySetName = string.Empty,
				FusionSigName = "IP Address",
				SigType = eSigType.Serial,
				Sig = 102
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_IP_SUBNET,
				TelemetrySetName = string.Empty,
				FusionSigName = "IP Subnet",
				SigType = eSigType.Serial,
				Sig = 103
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_IP_GATEWAY,
				TelemetrySetName = string.Empty,
				FusionSigName = "IP Gateway",
				SigType = eSigType.Serial,
				Sig = 104
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_DHCP_STATUS,
				TelemetrySetName = string.Empty,
				FusionSigName = "IP DHCP Enabled",
				SigType = eSigType.Digital,
				Sig = 100
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_MAC_ADDRESS_SECONDARY,
				TelemetrySetName = string.Empty,
				FusionSigName = "MAC Address 2",
				SigType = eSigType.Serial,
				Sig = 110
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY,
				TelemetrySetName = string.Empty,
				FusionSigName = "Hostname 2",
				SigType = eSigType.Serial,
				Sig = 111
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY,
				TelemetrySetName = string.Empty,
				FusionSigName = "IP Address 2",
				SigType = eSigType.Serial,
				Sig = 112
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_IP_SUBNET_SECONDARY,
				TelemetrySetName = string.Empty,
				FusionSigName = "IP Subnet 2",
				SigType = eSigType.Serial,
				Sig = 113
			},
			new FusionSigMapping
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_IP_GATEWAY_SECONDARY,
				TelemetrySetName = string.Empty,
				FusionSigName = "IP Gateway 2",
				SigType = eSigType.Serial,
				Sig = 114
			},
			new FusionSigMapping()
			{
				TelemetryGetName = DeviceTelemetryNames.DEVICE_DHCP_STATUS_SECONDARY,
				TelemetrySetName = string.Empty,
				FusionSigName = "IP DHCP Enabled 2",
				SigType = eSigType.Digital,
				Sig = 110
			}
		}; 
	}
}