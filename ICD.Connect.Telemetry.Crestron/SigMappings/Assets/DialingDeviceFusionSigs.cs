using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Conferencing.Controls.Dialing;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Assets
{
	public static class DialingDeviceFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			new IcdHashSet<AssetFusionSigMapping>
			{
				new AssetFusionSigMapping
				{
					TelemetryName = DialingTelemetryNames.CALL_IN_PROGRESS,
					FusionSigName = "Call In Progress",
					Sig = 201,
					SigType = eSigType.Digital
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DialingTelemetryNames.END_CALL_COMMAND,
					FusionSigName = "End Call",
					Sig = 203,
					SigType = eSigType.Digital
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DialingTelemetryNames.SIP_ENABLED,
					FusionSigName = "Sip Enabled",
					Sig = 204,
					SigType = eSigType.Digital
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DialingTelemetryNames.CALL_TYPE,
					FusionSigName = "Call Type",
					Sig = 201,
					SigType = eSigType.Serial
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DialingTelemetryNames.CALL_NUMBER,
					FusionSigName = "Call Number",
					Sig = 202,
					SigType = eSigType.Serial
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DialingTelemetryNames.SIP_LOCAL_NAME,
					FusionSigName = "Sip Name",
					Sig = 206,
					SigType = eSigType.Serial
				},
				new AssetFusionSigMapping
				{
					TelemetryName = DialingTelemetryNames.SIP_STATUS,
					FusionSigName = "Sip Registration Status",
					Sig = 210,
					SigType = eSigType.Serial
				}
			};
	}
}