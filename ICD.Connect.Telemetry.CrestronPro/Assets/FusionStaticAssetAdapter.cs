﻿using ICD.Common.Utils;
using ICD.Connect.Telemetry.CrestronPro.Devices;
using System.Collections.Generic;
#if SIMPLSHARP
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

			IcdConsole.PrintLine(eConsoleColor.Green, "Asset {2}: Digital Sig {0}, set to {1}", sig, newValue, Name);

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

			IcdConsole.PrintLine(eConsoleColor.Green, "Asset {2}: Analog Sig {0}, set to {1}", sig, newValue, Name);

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

			IcdConsole.PrintLine(eConsoleColor.Green, "Asset {2}: Serial Sig {0}, set to {1}", sig, newValue, Name);

			m_Asset.FusionGenericAssetSerialsAsset3.StringInput[sig + FusionRoomAdapter.SIG_OFFSET].StringValue = newValue;
			m_SerialCache[sig] = newValue;
		}

		public string ReadSerialSig(uint sig)
		{
			return m_Asset.FusionGenericAssetSerialsAsset3.StringOutput[sig + FusionRoomAdapter.SIG_OFFSET].StringValue;
		}
	}
}
#endif