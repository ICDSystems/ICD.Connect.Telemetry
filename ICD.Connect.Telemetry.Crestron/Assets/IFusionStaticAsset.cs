namespace ICD.Connect.Telemetry.Crestron.Assets
{
	public interface IFusionStaticAsset : IFusionAsset
	{
		void UpdateDigitalSig(uint sig, bool newValue);

		bool ReadDigitalSig(uint sig);

		void UpdateAnalogSig(uint sig, ushort newValue);

		ushort ReadAnalogSig(uint sig);

		void UpdateSerialSig(uint sig, string newValue);

		string ReadSerialSig(uint sig);
	}
}