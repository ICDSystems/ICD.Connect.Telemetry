using System.Collections.Generic;
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

		public void UpdateDigitalSig(uint sig, bool newValue)
		{
			if(m_DigitalCache.ContainsKey(sig) && m_DigitalCache[sig] == newValue)
				return;

			m_Asset.FusionGenericAssetDigitalsAsset1.BooleanInput[sig].BoolValue = newValue;
			m_DigitalCache[sig] = newValue;
		}

		public bool ReadDigitalSig(uint sig)
		{
			return m_Asset.FusionGenericAssetDigitalsAsset1.BooleanOutput[sig].BoolValue;
		}

		public void UpdateAnalogSig(uint sig, ushort newValue)
		{
			if(m_AnalogCache.ContainsKey(sig) && m_AnalogCache[sig] == newValue)
				return;

			m_Asset.FusionGenericAssetAnalogsAsset2.UShortInput[sig].UShortValue = newValue;
			m_AnalogCache[sig] = newValue;
		}

		public ushort ReadAnalogSig(uint sig)
		{
			return m_Asset.FusionGenericAssetAnalogsAsset2.UShortOutput[sig].UShortValue;
		}

		public void UpdateSerialSig(uint sig, string newValue)
		{
			if(m_SerialCache.ContainsKey(sig) && m_SerialCache[sig] == newValue)
				return;

			m_Asset.FusionGenericAssetSerialsAsset3.StringInput[sig].StringValue = newValue;
			m_SerialCache[sig] = newValue;
		}

		public string ReadSerialSig(uint sig)
		{
			return m_Asset.FusionGenericAssetSerialsAsset3.StringOutput[sig].StringValue;
		}
	}
}