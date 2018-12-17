using System.Collections.Generic;
using ICD.Connect.Telemetry.Crestron;

namespace ICD.Connect.Telemetry.CrestronPro.TelemetryAssets
{
	public interface IFusionAssetFactory
	{
		IEnumerable<FusionSigMapping> Mappings { get; } 
	}
}