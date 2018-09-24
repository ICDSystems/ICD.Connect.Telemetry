using ICD.Connect.Telemetry.Assets;
#if SIMPLSHARP
using System;

namespace ICD.Connect.Telemetry.CrestronPro.Assets
{
	public static class AssetTypeExtensions
	{
		/// <summary>
		/// Converts the crestron enum to an ICD enum.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static eAssetType ToIcd(this Crestron.SimplSharpPro.Fusion.eAssetType extends)
		{
			switch (extends)
			{
				case Crestron.SimplSharpPro.Fusion.eAssetType.NA:
					return eAssetType.Na;
				case Crestron.SimplSharpPro.Fusion.eAssetType.DemandResponse:
					return eAssetType.DemandResponse;
				case Crestron.SimplSharpPro.Fusion.eAssetType.DynamicAsset:
					return eAssetType.DynamicAsset;
				case Crestron.SimplSharpPro.Fusion.eAssetType.EnergyLoad:
					return eAssetType.EnergyLoad;
				case Crestron.SimplSharpPro.Fusion.eAssetType.EnergySupply:
					return eAssetType.EnergySupply;
				case Crestron.SimplSharpPro.Fusion.eAssetType.HvacZone:
					return eAssetType.HvacZone;
				case Crestron.SimplSharpPro.Fusion.eAssetType.LightingLoad:
					return eAssetType.LightingLoad;
				case Crestron.SimplSharpPro.Fusion.eAssetType.LightingScenes:
					return eAssetType.LightingScenes;
				case Crestron.SimplSharpPro.Fusion.eAssetType.Logging:
					return eAssetType.Logging;
				case Crestron.SimplSharpPro.Fusion.eAssetType.OccupancySensor:
					return eAssetType.OccupancySensor;
				case Crestron.SimplSharpPro.Fusion.eAssetType.PhotocellSensor:
					return eAssetType.PhotocellSensor;
				case Crestron.SimplSharpPro.Fusion.eAssetType.RemoteOccupancySensor:
					return eAssetType.RemoteOccupancySensor;
				case Crestron.SimplSharpPro.Fusion.eAssetType.RemoteRealTimePower:
					return eAssetType.RemoteRealTimePower;
				case Crestron.SimplSharpPro.Fusion.eAssetType.ShadeLoad:
					return eAssetType.ShadeLoad;
				case Crestron.SimplSharpPro.Fusion.eAssetType.ShadePresets:
					return eAssetType.ShadePresets;
				case Crestron.SimplSharpPro.Fusion.eAssetType.StaticAsset:
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
		public static Crestron.SimplSharpPro.Fusion.eAssetType FromIcd(this eAssetType extends)
		{
			switch (extends)
			{
				case eAssetType.Na:
					return Crestron.SimplSharpPro.Fusion.eAssetType.NA;
				case eAssetType.DemandResponse:
					return Crestron.SimplSharpPro.Fusion.eAssetType.DemandResponse;
				case eAssetType.DynamicAsset:
					return Crestron.SimplSharpPro.Fusion.eAssetType.DynamicAsset;
				case eAssetType.EnergyLoad:
					return Crestron.SimplSharpPro.Fusion.eAssetType.EnergyLoad;
				case eAssetType.EnergySupply:
					return Crestron.SimplSharpPro.Fusion.eAssetType.EnergySupply;
				case eAssetType.HvacZone:
					return Crestron.SimplSharpPro.Fusion.eAssetType.HvacZone;
				case eAssetType.LightingLoad:
					return Crestron.SimplSharpPro.Fusion.eAssetType.LightingLoad;
				case eAssetType.LightingScenes:
					return Crestron.SimplSharpPro.Fusion.eAssetType.LightingScenes;
				case eAssetType.Logging:
					return Crestron.SimplSharpPro.Fusion.eAssetType.Logging;
				case eAssetType.OccupancySensor:
					return Crestron.SimplSharpPro.Fusion.eAssetType.OccupancySensor;
				case eAssetType.PhotocellSensor:
					return Crestron.SimplSharpPro.Fusion.eAssetType.PhotocellSensor;
				case eAssetType.RemoteOccupancySensor:
					return Crestron.SimplSharpPro.Fusion.eAssetType.RemoteOccupancySensor;
				case eAssetType.RemoteRealTimePower:
					return Crestron.SimplSharpPro.Fusion.eAssetType.RemoteRealTimePower;
				case eAssetType.ShadeLoad:
					return Crestron.SimplSharpPro.Fusion.eAssetType.ShadeLoad;
				case eAssetType.ShadePresets:
					return Crestron.SimplSharpPro.Fusion.eAssetType.ShadePresets;
				case eAssetType.StaticAsset:
					return Crestron.SimplSharpPro.Fusion.eAssetType.StaticAsset;
				default:
					throw new ArgumentOutOfRangeException("extends");
			}
		}
	}
}
#endif
