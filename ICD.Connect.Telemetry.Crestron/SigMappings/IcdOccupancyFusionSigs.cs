using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Misc.Occupancy;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdOccupancyFusionSigs
	{
		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			new FusionSigMapping
			{
				TelemetryGetName = OccupancyTelemetryNames.OCCUPANCY_STATE,
				TelemetrySetName = "",
				FusionSigName = "Occupied",
				Sig = 0,
				SigType = eSigType.Digital,
				TargetAssetTypes = new IcdHashSet<Type>{typeof(IFusionOccupancySensorAsset)}
			}
		};
	}
}