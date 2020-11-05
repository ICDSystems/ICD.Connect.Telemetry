using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Assets.Mock
{
	public interface IMockFusionAsset : IFusionAsset, IConsoleNode
	{
		FusionAssetData GetFusionAssetData();
	}
}
