using System;
using ICD.Connect.Telemetry.Crestron.Assets;
#if SIMPLSHARP

namespace ICD.Connect.Telemetry.CrestronPro.Assets
{
	public static class AssetTypeExtensions
	{
		/// <summary>
		/// Converts the crestron enum to an ICD enum.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static eAssetType ToIcd(this global::Crestron.SimplSharpPro.Fusion.eAssetType extends)
		{
			switch (extends)
			{
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.NA:
					return eAssetType.Na;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.DemandResponse:
					return eAssetType.DemandResponse;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.DynamicAsset:
					return eAssetType.DynamicAsset;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.EnergyLoad:
					return eAssetType.EnergyLoad;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.EnergySupply:
					return eAssetType.EnergySupply;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.HvacZone:
					return eAssetType.HvacZone;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.LightingLoad:
					return eAssetType.LightingLoad;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.LightingScenes:
					return eAssetType.LightingScenes;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.Logging:
					return eAssetType.Logging;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.OccupancySensor:
					return eAssetType.OccupancySensor;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.PhotocellSensor:
					return eAssetType.PhotocellSensor;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.RemoteOccupancySensor:
					return eAssetType.RemoteOccupancySensor;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.RemoteRealTimePower:
					return eAssetType.RemoteRealTimePower;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.ShadeLoad:
					return eAssetType.ShadeLoad;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.ShadePresets:
					return eAssetType.ShadePresets;
				case global::Crestron.SimplSharpPro.Fusion.eAssetType.StaticAsset:
					return eAssetType.StaticAsset;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}

		/// <summary>
		/// Converts the ICD enum to a crestron enum.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static global::Crestron.SimplSharpPro.Fusion.eAssetType FromIcd(this eAssetType extends)
		{
			switch (extends)
			{
				case eAssetType.Na:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.NA;
				case eAssetType.DemandResponse:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.DemandResponse;
				case eAssetType.DynamicAsset:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.DynamicAsset;
				case eAssetType.EnergyLoad:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.EnergyLoad;
				case eAssetType.EnergySupply:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.EnergySupply;
				case eAssetType.HvacZone:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.HvacZone;
				case eAssetType.LightingLoad:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.LightingLoad;
				case eAssetType.LightingScenes:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.LightingScenes;
				case eAssetType.Logging:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.Logging;
				case eAssetType.OccupancySensor:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.OccupancySensor;
				case eAssetType.PhotocellSensor:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.PhotocellSensor;
				case eAssetType.RemoteOccupancySensor:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.RemoteOccupancySensor;
				case eAssetType.RemoteRealTimePower:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.RemoteRealTimePower;
				case eAssetType.ShadeLoad:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.ShadeLoad;
				case eAssetType.ShadePresets:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.ShadePresets;
				case eAssetType.StaticAsset:
					return global::Crestron.SimplSharpPro.Fusion.eAssetType.StaticAsset;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}
	}
}
#endif
