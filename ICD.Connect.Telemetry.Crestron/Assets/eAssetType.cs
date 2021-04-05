using System;

namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public enum eAssetType
	{
		Na,
		DemandResponse,
		DynamicAsset,
		EnergyLoad,
		EnergySupply,
		HvacZone,
		LightingLoad,
		LightingScenes,
		Logging,
		OccupancySensor,
		PhotocellSensor,
		RemoteOccupancySensor,
		RemoteRealTimePower,
		ShadeLoad,
		ShadePresets,
		StaticAsset,
	}

	public static class AssetTypeExtensions
	{
		public static Guid GetAssetTypeGuid(this eAssetType extends)
		{
			switch (extends)
			{
				case eAssetType.Na:
					return new Guid("A0BFAA6E-DB6A-47A7-8EFB-265342CAAA0F");
				case eAssetType.DemandResponse:
					return new Guid("C52CE5FD-BE2F-4D17-BD36-336D02625493");
				case eAssetType.DynamicAsset:
					return new Guid("62E4DF19-A8F2-470F-9876-F36384F207BF");
				case eAssetType.EnergyLoad:
					return new Guid("75C9BB35-4EA4-408C-ADB3-DEA52993980D");
				case eAssetType.EnergySupply:
					return new Guid("6430E395-2DC0-4B03-BC98-20A25B49D905");
				case eAssetType.HvacZone:
					return new Guid("DBEC5676-74CE-4D28-BE59-B4DF547CF267");
				case eAssetType.LightingLoad:
					return new Guid("9CC8F51A-8C4A-4634-9FB8-5574BEA7A9CD");
				case eAssetType.LightingScenes:
					return new Guid("DB6A8E90-2DA9-48A1-A292-FF8CA89934CA");
				case eAssetType.Logging:
					return new Guid("8142DD42-B381-4748-9C3B-F0CB0B48E282");
				case eAssetType.OccupancySensor:
					return new Guid("E99A8298-ED39-45CF-B26E-DEA498834E30");
				case eAssetType.PhotocellSensor:
					return new Guid("25C17C08-A1F8-49CA-8575-4D79AD70C1AE");
				case eAssetType.RemoteOccupancySensor:
					return new Guid("FF3E0433-8547-45CF-BA2D-B09DED036053");
				case eAssetType.RemoteRealTimePower:
					return new Guid("F1CE17B9-38F7-4921-9F12-9A150C2DA922");
				case eAssetType.ShadeLoad:
					return new Guid("F3097B77-80FA-41A3-8D20-0CE5D13C4F0F");
				case eAssetType.ShadePresets:
					return new Guid("0F9FFBD1-E9D8-4ED9-A52B-7F195BF7A23C");
				case eAssetType.StaticAsset:
					return new Guid("5516D674-A71F-46E6-84A1-709521D14EDF");
				default:
					throw new ArgumentOutOfRangeException("extends");

			}
		}
	}
}
