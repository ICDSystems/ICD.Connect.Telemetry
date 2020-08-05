using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Devices.Telemetry;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Settings;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Assets
{
	public static class IcdStandardFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			new IcdHashSet<AssetFusionSigMapping>
			{
				new AssetFusionSigMapping
				{
					TelemetryName = OriginatorTelemetryNames.ID,
					FusionSigName = "Id",
					SigType = eSigType.Serial,
					Sig = 24951
				},
				new AssetFusionSigMapping
				{
					TelemetryName = OriginatorTelemetryNames.NAME,
					FusionSigName = "ComponentName",
					SigType = eSigType.Serial,
					Sig = 0
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.ONLINE_STATE,
					FusionSigName = "Connected",
					SigType = eSigType.Digital,
					Sig = 0
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.POWER_STATE,
					FusionSigName = "PowerOn",
					SigType = eSigType.Digital,
					Sig = 0
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.POWER_OFF,
					FusionSigName = "PowerOff",
					SigType = eSigType.Digital,
					Sig = 0
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_MODEL,
					FusionSigName = "Model",
					SigType = eSigType.Serial,
					Sig = 50
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_FIRMWARE_VERSION,
					FusionSigName = "Firmware",
					SigType = eSigType.Serial,
					Sig = 51
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_FIRMWARE_DATE,
					FusionSigName = "Firmware Date",
					SigType = eSigType.Serial,
					Sig = 52
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_SERIAL_NUMBER,
					FusionSigName = "Serial No",
					SigType = eSigType.Serial,
					Sig = 56
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_UPTIME,
					FusionSigName = "Uptime",
					SigType = eSigType.Serial,
					Sig = 57
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_MAC_ADDRESS,
					FusionSigName = "MAC Address",
					SigType = eSigType.Serial,
					Sig = 100
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_HOSTNAME,
					FusionSigName = "Hostname",
					SigType = eSigType.Serial,
					Sig = 101
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_IP_ADDRESS,
					FusionSigName = "IP Address",
					SigType = eSigType.Serial,
					Sig = 102
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_IP_SUBNET,
					FusionSigName = "IP Subnet",
					SigType = eSigType.Serial,
					Sig = 103
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_IP_GATEWAY,
					FusionSigName = "IP Gateway",
					SigType = eSigType.Serial,
					Sig = 104
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_DHCP_STATUS,
					FusionSigName = "IP DHCP Enabled",
					SigType = eSigType.Digital,
					Sig = 100
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_MAC_ADDRESS_SECONDARY,
					FusionSigName = "MAC Address 2",
					SigType = eSigType.Serial,
					Sig = 110
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY,
					FusionSigName = "Hostname 2",
					SigType = eSigType.Serial,
					Sig = 111
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY,
					FusionSigName = "IP Address 2",
					SigType = eSigType.Serial,
					Sig = 112
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_IP_SUBNET_SECONDARY,
					FusionSigName = "IP Subnet 2",
					SigType = eSigType.Serial,
					Sig = 113
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_IP_GATEWAY_SECONDARY,
					FusionSigName = "IP Gateway 2",
					SigType = eSigType.Serial,
					Sig = 114
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DeviceTelemetryNames.DEVICE_DHCP_STATUS_SECONDARY,
					FusionSigName = "IP DHCP Enabled 2",
					SigType = eSigType.Digital,
					Sig = 110
				}
			};
	}
}