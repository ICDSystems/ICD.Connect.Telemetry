using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Mappings;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public interface IFusionSigMapping : ITelemetryMapping
	{
		string FusionSigName { get; }

		eSigType SigType { get; }
		
		/// <summary>
		/// What provider types this binding can use
		/// If null, all types are allowed
		/// </summary>
		IEnumerable<Type> TelemetryProviderTypes { get; }

		[CanBeNull]
		FusionTelemetryBinding Bind(IFusionRoom fusionRoom, ITelemetryNode node, uint assetId, RangeMappingUsageTracker mappingUsage);
	}
}