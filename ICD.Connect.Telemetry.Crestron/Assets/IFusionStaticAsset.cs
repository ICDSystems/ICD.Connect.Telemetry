using System.Collections.Generic;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public interface IFusionStaticAsset : IFusionAsset
	{
		string Make { get; set; }

		string Model { get; set; }

		void SetPoweredState(bool powered);

		void SetOnlineState(bool connected);

		void UpdateDigitalSig(uint sig, bool newValue);

		bool ReadDigitalSig(uint sig);

		void UpdateAnalogSig(uint sig, ushort newValue);

		ushort ReadAnalogSig(uint sig);

		void UpdateSerialSig(uint sig, string newValue);

		string ReadSerialSig(uint sig);

		IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedBooleanSigs();

		IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedUShortSigs();
		
		IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedSerialSigs();
	}
}