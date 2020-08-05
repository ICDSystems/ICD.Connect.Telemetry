using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Mappings;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public interface IFusionSigMapping : ITelemetryMapping
	{
		uint Sig { get; set; }

		ushort Range { get; set; }

		string FusionSigName { get; }

		eSigType SigType { get; }
		
		/// <summary>
		/// What provider types this binding can use
		/// If null, all types are allowed
		/// </summary>
		IEnumerable<Type> TelemetryProviderTypes { get; }

		/// <summary>
		/// Returns true if this mapping is valid for the given provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		bool ValidateProvider([NotNull] ITelemetryProvider provider);
	}
}
