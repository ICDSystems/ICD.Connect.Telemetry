using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Devices;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Assets.Mock
{
	public sealed class MockFusionStaticAsset : AbstractMockFusionAsset, IFusionStaticAsset
	{
		private const uint SIG_OFFSET = 49;

		public event EventHandler<FusionAssetSigUpdatedArgs> OnFusionAssetSigUpdated;
		public event EventHandler<FusionAssetPowerStateUpdatedArgs> OnFusionAssetPowerStateUpdated;

		private readonly Dictionary<uint, bool> m_InputSigsDigital;

		private readonly Dictionary<uint, ushort> m_InputSigsAnalog;

		private readonly Dictionary<uint, string> m_InputSigsSerials;

		private readonly Dictionary<uint, bool> m_OutputSigsDigital;
		private readonly Dictionary<uint, ushort> m_OutputSigsAnalog;
		private readonly Dictionary<uint, string> m_OutputSigsSerial;


		private readonly Dictionary<eSigType, Dictionary<uint, string>> m_SigNames;

		private readonly Dictionary<eSigType, Dictionary<uint, eTelemetryIoMask>> m_SigIoMasks;

		public override eAssetType AssetType { get { return eAssetType.StaticAsset; } }

		public bool PowerState { get; private set; }

		public bool OnlineState { get; private set; }

		public MockFusionStaticAsset(uint number, string name, string type, string instanceId): base(number, name, type, instanceId)
		{
			m_InputSigsDigital = new Dictionary<uint, bool>();
			m_InputSigsAnalog = new Dictionary<uint, ushort>();
			m_InputSigsSerials = new Dictionary<uint, string>();

			m_OutputSigsDigital = new Dictionary<uint, bool>();
			m_OutputSigsAnalog = new Dictionary<uint, ushort>();
			m_OutputSigsSerial = new Dictionary<uint, string>();

			m_SigNames = new Dictionary<eSigType, Dictionary<uint, string>>();
			m_SigIoMasks = new Dictionary<eSigType, Dictionary<uint, eTelemetryIoMask>>();
		}

		public MockFusionStaticAsset(AssetInfo info) : this(info.Number, info.Name, info.Type, info.InstanceId)
		{
		}

		/// <summary>
		/// Adds the sig for the asset
		/// </summary>
		/// <param name="sigType"></param>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="mask"></param>
		public void AddSig(eSigType sigType, uint number, string name, eTelemetryIoMask mask)
		{
			m_SigIoMasks.GetOrAddNew(sigType, () => new Dictionary<uint, eTelemetryIoMask>())[number] = mask;
			m_SigNames.GetOrAddNew(sigType, () => new Dictionary<uint, string>())[number] = name;
		}

		private object GetSigValue(eSigType type, uint number)
		{

			switch (type)
			{
				case eSigType.Serial:
					string outputString;
					m_InputSigsSerials.TryGetValue(number, out outputString);
					return outputString;
				case eSigType.Analog:
					ushort outputUshort;
					m_InputSigsAnalog.TryGetValue(number, out outputUshort);
					return outputUshort;
				case eSigType.Digital:
					bool outputBool;
					m_InputSigsDigital.TryGetValue(number, out outputBool);
					return outputBool;
				default:
					throw new ArgumentOutOfRangeException("type");
			}
		}

		/// <summary>
		/// Sends the serial data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="text"></param>
		public void SendInputSerial(uint number, string text)
		{
			Dictionary<uint, eTelemetryIoMask> masksForSigs;
			eTelemetryIoMask mask;
			if (m_SigIoMasks.TryGetValue(eSigType.Serial, out masksForSigs) && masksForSigs.TryGetValue(number, out mask) && mask.HasFlag(eTelemetryIoMask.ProgramToService))
				m_InputSigsSerials[number] = text;
			//else
				//Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, text);
		}

		public void UpdateSerialSig(uint sig, string newValue)
		{
			SendInputSerial(sig, newValue);
		}

		/// <summary>
		/// Sends the analog data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="value"></param>
		public void SendInputAnalog(uint number, ushort value)
		{
			Dictionary<uint, eTelemetryIoMask> masksForSigs;
			eTelemetryIoMask mask;
			if (m_SigIoMasks.TryGetValue(eSigType.Analog, out masksForSigs) && masksForSigs.TryGetValue(number, out mask) && mask.HasFlag(eTelemetryIoMask.ProgramToService))
				m_InputSigsAnalog[number] = value;
			//else
				//Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, value);
		}

		public void UpdateAnalogSig(uint sig, ushort newValue)
		{
			SendInputAnalog(sig, newValue);
		}

		/// <summary>
		/// Sends the digital data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="value"></param>
		public void SendInputDigital(uint number, bool value)
		{
			Dictionary<uint, eTelemetryIoMask> masksForSigs;
			eTelemetryIoMask mask;
			if (m_SigIoMasks.TryGetValue(eSigType.Digital, out masksForSigs) && masksForSigs.TryGetValue(number, out mask) && mask.HasFlag(eTelemetryIoMask.ProgramToService))
				m_InputSigsDigital[number] = value;
			//else
				//Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, value);
		}

		public void UpdateDigitalSig(uint sig, bool newValue)
		{
			SendInputDigital(sig, newValue);
		}

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Powered", PowerState);
			addRow("Online", OnlineState);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("PrintSigs", "Print the sigs for the asset", () => PrintSigs());
			yield return new GenericConsoleCommand<uint, bool>("SendOutputDigital",
			                                                   "Sends Digital Sig From Mock Fusion <number> <value>",
			                                                   (n, v) => SendOutputDigital(n, v));
			yield return new GenericConsoleCommand<uint, ushort>("SendOutputAnalog",
			                                                     "Sends Analog Sig From Mock Fusion <number> <value>",
			                                                     (n, v) => SendOutputAnalog(n, v));
			yield return new GenericConsoleCommand<uint, string>("SendOutputSerial",
			                                                     "Sends Serial Sig From Mock Fusion <number> <value>",
			                                                     (n, v) => SendOutputSerial(n, v));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{ return base.GetConsoleCommands(); }

		private string PrintSigs()
		{
			TableBuilder table = new TableBuilder("Type", "Sig Number", "SigName", "IoMask", "Value");

			foreach (var sigTypes in m_SigNames)
			{
				foreach (var sigNames in sigTypes.Value.OrderBy(kvp => kvp.Key))
				{
					table.AddRow(sigTypes.Key.ToString(), sigNames.Key, sigNames.Value, m_SigIoMasks[sigTypes.Key][sigNames.Key].ToString(), GetSigValue(sigTypes.Key, sigNames.Key));
				}
				table.AddSeparator();
			}

			return table.ToString();
		}

		private void SendOutputDigital(uint sig, bool value)
		{
			uint offsetSig = sig - SIG_OFFSET;
			m_OutputSigsDigital[sig] = value;
			OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(Number, eSigType.Digital, offsetSig));
		}

		private void SendOutputAnalog(uint sig, ushort value)
		{
			uint offsetSig = sig - SIG_OFFSET;
			m_OutputSigsAnalog[sig] = value;
			OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(Number, eSigType.Analog, offsetSig));
		}

		private void SendOutputSerial(uint sig, string value)
		{
			uint offsetSig = sig - SIG_OFFSET;
			m_OutputSigsSerial[sig] = value;
			OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(Number, eSigType.Serial, offsetSig));
		}

		private void SendPowerState(bool state)
		{
			PowerState = state;
			OnFusionAssetPowerStateUpdated.Raise(this, new FusionAssetPowerStateUpdatedArgs(Number, state));
		}

		#endregion

		public void SetPoweredState(bool powered)
		{
			PowerState = powered;
		}

		public void SetOnlineState(bool connected)
		{
			OnlineState = connected;
		}

		public bool ReadDigitalSig(uint sig)
		{
			bool value;
			if (m_OutputSigsDigital.TryGetValue(sig, out value))
				return value;
			return false;
		}

		public ushort ReadAnalogSig(uint sig)
		{
			ushort value;
			if (m_OutputSigsAnalog.TryGetValue(sig, out value))
				return value;
			return 0;
		}

		public string ReadSerialSig(uint sig)
		{
			string value;
			if (m_OutputSigsSerial.TryGetValue(sig, out value))
				return value;
			return string.Empty;
		}
	}
}
