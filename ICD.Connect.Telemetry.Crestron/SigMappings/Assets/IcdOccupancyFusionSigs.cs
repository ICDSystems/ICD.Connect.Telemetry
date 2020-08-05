using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;

namespace ICD.Connect.Telemetry.Crestron.SigMappings.Assets
{
	public static class IcdOccupancyFusionSigs
	{
		public static IEnumerable<AssetFusionSigMapping> AssetMappings { get { return s_AssetMappings; } }

		private static readonly IcdHashSet<AssetFusionSigMapping> s_AssetMappings =
			new IcdHashSet<AssetFusionSigMapping>
			{
				new AssetFusionSigMapping
				{
					TelemetryName = OccupancyTelemetryNames.OCCUPANCY_STATE,
					FusionSigName = "Occupied",
					Sig = 0,
					SigType = eSigType.Digital,
					FusionAssetTypes = new IcdHashSet<eAssetType> {eAssetType.OccupancySensor}
				}
			};
	}
}