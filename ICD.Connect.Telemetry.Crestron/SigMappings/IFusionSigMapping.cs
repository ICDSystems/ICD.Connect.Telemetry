using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public interface IFusionSigMapping
	{
		string TelemetrySetName { get; set; }
		string TelemetryGetName { get; set; }
		string FusionSigName { get; set; }
		eSigIoMask IoMask { get; }
		eSigType SigType { get; set; }
		int GetHashCode();
		
		/// <summary>
		/// What provider types this binding can use
		/// If null, all types are allowed
		/// </summary>
		IEnumerable<Type> TelemetryProviderTypes { get; }

		[CanBeNull]
		FusionTelemetryBinding Bind(IFusionRoom fusionRoom, ITelemetryItem node, uint assetId, RangeMappingUsageTracker mappingUsage);
	}
}