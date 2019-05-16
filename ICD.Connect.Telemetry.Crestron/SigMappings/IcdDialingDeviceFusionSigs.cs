using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Conferencing.Controls.Dialing;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdDialingDeviceFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = DialingTelemetryNames.CALL_IN_PROGRESS,
				TelemetrySetName = string.Empty,
				FusionSigName = "Call In Progress",
				Sig = 201,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryGetName = string.Empty,
				TelemetrySetName = DialingTelemetryNames.END_CALL_COMMAND,
				FusionSigName = "End Call",
				Sig = 203,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryGetName = DialingTelemetryNames.SIP_ENABLED,
				TelemetrySetName = string.Empty,
				FusionSigName = "Sip Enabled",
				Sig = 204,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryGetName = DialingTelemetryNames.CALL_TYPE,
				TelemetrySetName = string.Empty,
				FusionSigName = "Call Type",
				Sig = 201,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryGetName = DialingTelemetryNames.CALL_NUMBER,
				TelemetrySetName = string.Empty,
				FusionSigName = "Call Number",
				Sig = 202,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryGetName = DialingTelemetryNames.SIP_LOCAL_NAME,
				TelemetrySetName = string.Empty,
				FusionSigName = "Sip Name",
				Sig = 206,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryGetName = DialingTelemetryNames.SIP_STATUS,
				TelemetrySetName = string.Empty,
				FusionSigName = "Sip Registration Status",
				Sig = 210,
				SigType = eSigType.Serial
			}
		};
	}
}