using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.CrestronPro.Devices;
using System.Collections.Generic;
using ICD.Connect.Telemetry.Nodes;
using eAssetType = ICD.Connect.Telemetry.Crestron.Assets.eAssetType;
using eSigType = ICD.Connect.Protocol.Sigs.eSigType;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using ICD.Connect.Telemetry.Crestron.Assets;

namespace ICD.Connect.Telemetry.CrestronPro.Assets
{
	public sealed class FusionStaticAssetAdapter : AbstractFusionAssetAdapter<FusionStaticAsset>, IFusionStaticAsset
	{
		private readonly FusionStaticAsset m_Asset;
		private readonly Dictionary<uint, bool> m_DigitalCache;
		private readonly Dictionary<uint, ushort> m_AnalogCache;
		private readonly Dictionary<uint, string> m_SerialCache;

		/// <summary>
		/// Gets the asset type.
		/// </summary>
		public override eAssetType AssetType { get { return eAssetType.StaticAsset; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fusionAsset"></param>
		public FusionStaticAssetAdapter(FusionStaticAsset fusionAsset)
			: base(fusionAsset)
		{
			m_Asset = fusionAsset;
			m_DigitalCache = new Dictionary<uint, bool>();
			m_AnalogCache = new Dictionary<uint, ushort>();
			m_SerialCache = new Dictionary<uint, string>();
		}

		public void SetOnlineState(bool connected)
		{
			m_Asset.Connected.InputSig.BoolValue = connected;
		}

		public void SetPoweredState(bool powered)
		{
			m_Asset.PowerOn.InputSig.BoolValue = powered;
		}

		public void UpdateDigitalSig(uint sig, bool newValue)
		{
			if(m_DigitalCache.ContainsKey(sig) && m_DigitalCache[sig] == newValue)
				return;

			m_Asset.FusionGenericAssetDigitalsAsset1.BooleanInput[sig + FusionRoomAdapter.SIG_OFFSET].BoolValue = newValue;
			m_DigitalCache[sig] = newValue;
		}

		public bool ReadDigitalSig(uint sig)
		{
			return m_Asset.FusionGenericAssetDigitalsAsset1.BooleanOutput[sig + FusionRoomAdapter.SIG_OFFSET].BoolValue;
		}

		public void UpdateAnalogSig(uint sig, ushort newValue)
		{
			if(m_AnalogCache.ContainsKey(sig) && m_AnalogCache[sig] == newValue)
				return;

			m_Asset.FusionGenericAssetAnalogsAsset2.UShortInput[sig + FusionRoomAdapter.SIG_OFFSET].UShortValue = newValue;
			m_AnalogCache[sig] = newValue;
		}

		public ushort ReadAnalogSig(uint sig)
		{
			return m_Asset.FusionGenericAssetAnalogsAsset2.UShortOutput[sig + FusionRoomAdapter.SIG_OFFSET].UShortValue;
		}

		public void UpdateSerialSig(uint sig, string newValue)
		{
			if(m_SerialCache.ContainsKey(sig) && m_SerialCache[sig] == newValue)
				return;

			m_Asset.FusionGenericAssetSerialsAsset3.StringInput[sig + FusionRoomAdapter.SIG_OFFSET].StringValue = newValue;
			m_SerialCache[sig] = newValue;
		}

		public string ReadSerialSig(uint sig)
		{
			return m_Asset.FusionGenericAssetSerialsAsset3.StringOutput[sig + FusionRoomAdapter.SIG_OFFSET].StringValue;
		}

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedBooleanSigs()
		{
			foreach (BooleanSigData sig in m_Asset.FusionGenericAssetDigitalsAsset1.UserDefinedBooleanSigDetails)
				yield return new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Digital, sig.Number, sig.Name, 0), sig.SigIoMask.ToIcd());
		}

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedUShortSigs()
		{
			foreach (UShortSigData sig in m_Asset.FusionGenericAssetAnalogsAsset2.UserDefinedUShortSigDetails)
				yield return new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Analog, sig.Number, sig.Name, 0), sig.SigIoMask.ToIcd());
		}

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedSerialSigs()
		{
			foreach (StringSigData sig in m_Asset.FusionGenericAssetSerialsAsset3.UserDefinedStringSigDetails)
				yield return new KeyValuePair<SigInfo, eTelemetryIoMask>(new SigInfo(eSigType.Serial, sig.Number, sig.Name, 0), sig.SigIoMask.ToIcd());
		}
	}
}
#endif