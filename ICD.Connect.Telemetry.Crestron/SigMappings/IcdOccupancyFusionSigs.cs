using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public static class IcdOccupancyFusionSigs
	{
		private static readonly FusionSigMapping s_OccupancyState =
			new FusionSigMapping
			{
				TelemetryName = OccupancyTelemetryNames.OCCUPANCY_STATE,
				FusionSigName = "Occupied",
				Sig = 0,
				SigType = eSigType.Digital,
				FusionAssetTypes = new IcdHashSet<Type> {typeof(IFusionOccupancySensorAsset)}
			};

		private static readonly IcdHashSet<IFusionSigMapping> s_Sigs = new IcdHashSet<IFusionSigMapping>
		{
			s_OccupancyState
		};

		public static FusionSigMapping OccupancyState { get { return s_OccupancyState; } }

		public static IEnumerable<IFusionSigMapping> Sigs { get { return s_Sigs; } }
	}
}