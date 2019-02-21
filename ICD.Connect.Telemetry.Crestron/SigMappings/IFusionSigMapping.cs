using ICD.Connect.Protocol.Sigs;

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
	}
}