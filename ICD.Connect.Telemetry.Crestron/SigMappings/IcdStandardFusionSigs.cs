using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Settings;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdStandardFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_AssetMappings = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryName = OriginatorTelemetryNames.ID,
				FusionSigName = "Id",
				SigType = eSigType.Serial,
				Sig = 24951
			},
			new FusionSigMapping
			{
				TelemetryName = OriginatorTelemetryNames.NAME,
				FusionSigName = "ComponentName",
				SigType = eSigType.Serial,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.ONLINE_STATE,
				FusionSigName = "Connected",
				SigType = eSigType.Digital,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.POWER_STATE,
				FusionSigName = "PowerOn",
				SigType = eSigType.Digital,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.POWER_OFF,
				FusionSigName = "PowerOff",
				SigType = eSigType.Digital,
				Sig = 0
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_MODEL,
				FusionSigName = "Model",
				SigType = eSigType.Serial,
				Sig = 50
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_FIRMWARE_VERSION,
				FusionSigName = "Firmware",
				SigType = eSigType.Serial,
				Sig = 51
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_FIRMWARE_DATE,
				FusionSigName = "Firmware Date",
				SigType = eSigType.Serial,
				Sig = 52
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_SERIAL_NUMBER,
				FusionSigName = "Serial No",
				SigType = eSigType.Serial,
				Sig = 56
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_UPTIME,
				FusionSigName = "Uptime",
				SigType = eSigType.Serial,
				Sig = 57
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_MAC_ADDRESS,
				FusionSigName = "MAC Address",
				SigType = eSigType.Serial,
				Sig = 100
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_HOSTNAME,
				FusionSigName = "Hostname",
				SigType = eSigType.Serial,
				Sig = 101
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_IP_ADDRESS,
				FusionSigName = "IP Address",
				SigType = eSigType.Serial,
				Sig = 102
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_IP_SUBNET,
				FusionSigName = "IP Subnet",
				SigType = eSigType.Serial,
				Sig = 103
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_IP_GATEWAY,
				FusionSigName = "IP Gateway",
				SigType = eSigType.Serial,
				Sig = 104
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_DHCP_STATUS,
				FusionSigName = "IP DHCP Enabled",
				SigType = eSigType.Digital,
				Sig = 100
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_MAC_ADDRESS_SECONDARY,
				FusionSigName = "MAC Address 2",
				SigType = eSigType.Serial,
				Sig = 110
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY,
				FusionSigName = "Hostname 2",
				SigType = eSigType.Serial,
				Sig = 111
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY,
				FusionSigName = "IP Address 2",
				SigType = eSigType.Serial,
				Sig = 112
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_IP_SUBNET_SECONDARY,
				FusionSigName = "IP Subnet 2",
				SigType = eSigType.Serial,
				Sig = 113
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_IP_GATEWAY_SECONDARY,
				FusionSigName = "IP Gateway 2",
				SigType = eSigType.Serial,
				Sig = 114
			},
			new FusionSigMapping
			{
				TelemetryName = DeviceTelemetryNames.DEVICE_DHCP_STATUS_SECONDARY,
				FusionSigName = "IP DHCP Enabled 2",
				SigType = eSigType.Digital,
				Sig = 110
			}
		}; 
	}
}