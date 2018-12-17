using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using ICD.Connect.Telemetry.Crestron;

namespace ICD.Connect.Telemetry.CrestronPro.TelemetryAssets
{
	public abstract class AbstractFusionAssetFactory : IFusionAssetFactory
	{
		public abstract IEnumerable<FusionSigMapping> Mappings { get; }
	}
}