using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Mappings;

namespace ICD.Connect.Telemetry.Crestron.SigMappings
{
	public interface IFusionSigMapping : ITelemetryMapping
	{
		string FusionSigName { get; set; }
		eSigType SigType { get; set; }
		int GetHashCode();
	}
}