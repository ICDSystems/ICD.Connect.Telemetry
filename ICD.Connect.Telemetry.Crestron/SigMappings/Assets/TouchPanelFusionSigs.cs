using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Panels.Devices;
using ICD.Connect.Panels.Telemetry;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Assets
{
	public static class TouchPanelFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			new IcdHashSet<AssetFusionSigMapping>
			{
				new AssetFusionSigMapping
				{
					TelemetryName = CrestronPanelTelemetryNames.APP_MODE_PROPERTY,
					FusionSigName = "App Mode",
					Sig = 64,
					SigType = eSigType.Serial,
					TelemetryProviderTypes = new IcdHashSet<Type> { typeof(IPanelDevice) }
				},
				new AssetFusionSigMapping
				{
					TelemetryName = CrestronPanelTelemetryNames.DISPLAY_PROJECT_EVENT,
					FusionSigName = "Display Project",
					Sig = 63,
					SigType = eSigType.Serial,
					TelemetryProviderTypes = new IcdHashSet<Type> { typeof(IPanelDevice) }
				}
			};
	}
}