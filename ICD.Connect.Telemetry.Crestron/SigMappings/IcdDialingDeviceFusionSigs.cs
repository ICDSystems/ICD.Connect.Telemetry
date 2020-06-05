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
				TelemetryName = DialingTelemetryNames.CALL_IN_PROGRESS,
				FusionSigName = "Call In Progress",
				Sig = 201,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryName = DialingTelemetryNames.END_CALL_COMMAND,
				FusionSigName = "End Call",
				Sig = 203,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryName = DialingTelemetryNames.SIP_ENABLED,
				FusionSigName = "Sip Enabled",
				Sig = 204,
				SigType = eSigType.Digital
			},
			new FusionSigMapping
			{
				TelemetryName = DialingTelemetryNames.CALL_TYPE,
				FusionSigName = "Call Type",
				Sig = 201,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryName = DialingTelemetryNames.CALL_NUMBER,
				FusionSigName = "Call Number",
				Sig = 202,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryName = DialingTelemetryNames.SIP_LOCAL_NAME,
				FusionSigName = "Sip Name",
				Sig = 206,
				SigType = eSigType.Serial
			},
			new FusionSigMapping
			{
				TelemetryName = DialingTelemetryNames.SIP_STATUS,
				FusionSigName = "Sip Registration Status",
				Sig = 210,
				SigType = eSigType.Serial
			}
		};
	}
}