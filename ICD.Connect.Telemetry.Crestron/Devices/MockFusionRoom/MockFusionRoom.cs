using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Panels;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Assets.Mock;
using ICD.Connect.Telemetry.Crestron.SigMappings;

namespace ICD.Connect.Telemetry.Crestron.Devices.MockFusionRoom
{
	public sealed class MockFusionRoom : AbstractDevice<MockFusionRoomSettings>, IFusionRoom
	{
		#region Fields

		private readonly Dictionary<uint, bool> m_InputSigsDigital;

		private readonly Dictionary<uint, ushort> m_InputSigsAnalog;

		private readonly Dictionary<uint, string> m_InputSigsSerials;


		private readonly Dictionary<eSigType, Dictionary<uint, string>> m_SigNames;

		private readonly Dictionary<eSigType, Dictionary<uint, eSigIoMask>> m_SigIoMasks;

		private readonly SigCallbackManager m_SigCallbackManager;

		private readonly Dictionary<uint, IMockFusionAsset> m_Assets;

		private readonly MockFusionAssetDataCollection m_AssetDataCollection;

		private string m_ErrorMessage;

		private string m_DeviceUsage;

		private bool m_SystemPower;

		private bool m_DisplayPower;

		#endregion

		#region Events

		/// <summary>
		/// Raised when the user interacts with the panel.
		/// </summary>
		public event EventHandler<SigInfoEventArgs> OnAnyOutput;

		public event EventHandler<FusionAssetSigUpdatedArgs> OnFusionAssetSigUpdated;
		public event EventHandler<FusionAssetPowerStateUpdatedArgs> OnFusionAssetPowerStateUpdated;
		public event EventHandler<BoolEventArgs> OnFusionSystemPowerChangeEvent;
		public event EventHandler<BoolEventArgs> OnFusionDisplayPowerChangeEvent;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the time that the user last interacted with the panel.
		/// </summary>
		public DateTime? LastOutput { get; private set; }

		/// <summary>
		/// Gets the GUID for the Room ID.
		/// </summary>
		public string RoomId { get; private set; }

		/// <summary>
		/// Gets the user configurable assets.
		/// </summary>
		public IFusionAssetDataCollection UserConfigurableAssetDetails { get { return m_AssetDataCollection; } }

		#endregion

		public MockFusionRoom()
		{
			RoomId = Guid.NewGuid().ToString();

			m_InputSigsDigital = new Dictionary<uint, bool>();
			m_InputSigsAnalog  = new Dictionary<uint, ushort>();
			m_InputSigsSerials = new Dictionary<uint, string>();

			m_SigNames = new Dictionary<eSigType, Dictionary<uint, string>>();
			m_SigIoMasks = new Dictionary<eSigType, Dictionary<uint, eSigIoMask>>();
			m_Assets = new Dictionary<uint, IMockFusionAsset>();

			m_SigCallbackManager = new SigCallbackManager();

			m_AssetDataCollection = new MockFusionAssetDataCollection(m_Assets);

		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		/// <summary>
		/// Clears the assigned input sig values.
		/// </summary>
		public void Clear()
		{
			m_InputSigsDigital.Clear();
			m_InputSigsAnalog.Clear();
			m_InputSigsSerials.Clear();
		}

		/// <summary>
		/// Registers the callback for output sig change events.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="type"></param>
		/// <param name="callback"></param>
		public void RegisterOutputSigChangeCallback(uint number, eSigType type, Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			m_SigCallbackManager.RegisterSigChangeCallback(number, type, callback);
		}

		/// <summary>
		/// Unregisters the callback for output sig change events.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="type"></param>
		/// <param name="callback"></param>
		public void UnregisterOutputSigChangeCallback(uint number, eSigType type, Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			m_SigCallbackManager.UnregisterSigChangeCallback(number, type, callback);
		}

		/// <summary>
		/// Sends the serial data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="text"></param>
		public void SendInputSerial(uint number, string text)
		{
			Dictionary<uint, eSigIoMask> masksForSigs;
			eSigIoMask mask;
			if (m_SigIoMasks.TryGetValue(eSigType.Serial, out masksForSigs) && masksForSigs.TryGetValue(number, out mask) && mask.HasFlag(eSigIoMask.ProgramToFusion))
				m_InputSigsSerials[number] = text;
			else
				Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, text);
		}

		/// <summary>
		/// Sends the analog data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="value"></param>
		public void SendInputAnalog(uint number, ushort value)
		{
			Dictionary<uint, eSigIoMask> masksForSigs;
			eSigIoMask mask;
			if (m_SigIoMasks.TryGetValue(eSigType.Analog, out masksForSigs) && masksForSigs.TryGetValue(number, out mask) && mask.HasFlag(eSigIoMask.ProgramToFusion))
				m_InputSigsAnalog[number] = value;
			else
				Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, value);
		}

		/// <summary>
		/// Sends the digital data to the panel.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="value"></param>
		public void SendInputDigital(uint number, bool value)
		{
			Dictionary<uint, eSigIoMask> masksForSigs;
			eSigIoMask mask;
			if (m_SigIoMasks.TryGetValue(eSigType.Digital, out masksForSigs) && masksForSigs.TryGetValue(number, out mask) && mask.HasFlag(eSigIoMask.ProgramToFusion))
				m_InputSigsDigital[number] = value;
			else
				Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, value);
		}

		/// <summary>
		/// Adds the asset to the fusion room.
		/// </summary>
		public void AddAsset(AssetInfo assetInfo)
		{
			IMockFusionAsset asset;
			switch (assetInfo.AssetType)
			{
				case eAssetType.StaticAsset:
					asset = new MockFusionStaticAsset(assetInfo);
					break;
				case eAssetType.OccupancySensor:
					asset = new MockFusionOccupancySensorAsset(assetInfo);
					break;
				default:
					asset = null;
					break;
			}

			Subscribe(asset);

			if (asset != null)
				m_Assets.Add(asset.Number, asset);
		}

		/// <summary>
		/// Adds the assets to the fusion room.
		/// </summary>
		/// <param name="assets"></param>
		public void AddAssets(IEnumerable<AssetInfo> assets)
		{
			foreach(AssetInfo asset in assets)
				AddAsset(asset);
		}

		public void UpdateDigitalSig(uint sig, bool newValue)
		{
			SendInputDigital(sig, newValue);
		}

		public void UpdateAnalogSig(uint sig, ushort newValue)
		{
			SendInputAnalog(sig, newValue);
		}

		public void UpdateSerialSig(uint sig, string newValue)
		{
			SendInputSerial(sig, newValue);
		}

		/// <summary>
		/// Sets the fusion error message.
		/// </summary>
		/// <param name="message"></param>
		public void SetErrorMessage(string message)
		{
			m_ErrorMessage = message;
		}

		/// <summary>
		/// Sends the string as device usage.
		/// </summary>
		/// <param name="usage"></param>
		public void SendDeviceUsage(string usage)
		{
			m_DeviceUsage = usage;
		}

		/// <summary>
		/// Sets the system power on/off state for the room.
		/// </summary>
		/// <param name="powered"></param>
		public void SetSystemPower(bool powered)
		{
			m_SystemPower = powered;
		}

		/// <summary>
		/// Sets the display power on/off state for the room.
		/// </summary>
		/// <param name="powered"></param>
		public void SetDisplayPower(bool powered)
		{
			m_DisplayPower = powered;
		}

		/// <summary>
		/// Removes the asset with the given id.
		/// </summary>
		/// <param name="assetId"></param>
		public void RemoveAsset(uint assetId)
		{
			m_Assets.Remove(assetId);
		}

		/// <summary>
		/// Gets the configured asset ids.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<uint> GetAssetIds()
		{
			return m_Assets.Keys;
		}

		/// <summary>
		/// Adds the sig for the room itself
		/// </summary>
		/// <param name="sigType"></param>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="mask"></param>
		public void AddSig(eSigType sigType, uint number, string name, eSigIoMask mask)
		{
			m_SigIoMasks.GetOrAddNew(sigType, () => new Dictionary<uint, eSigIoMask>())[number] = mask;
			m_SigNames.GetOrAddNew(sigType, () => new Dictionary<uint, string>())[number] = name;
		}

		/// <summary>
		/// Adds the sig for the given asset id.
		/// </summary>
		/// <param name="assetId"></param>
		/// <param name="sigType"></param>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="mask"></param>
		public void AddSig(uint assetId, eSigType sigType, uint number, string name, eSigIoMask mask)
		{
			IMockFusionAsset asset;
			if (!m_Assets.TryGetValue(assetId, out asset))
				return;

			var staticAsset = asset as MockFusionStaticAsset;
			if (staticAsset != null)
				staticAsset.AddSig(sigType,number,name,mask);
		}

		/// <summary>
		/// Generates an RVI file for the fusion assets.
		/// </summary>
		public void RebuildRvi()
		{
			//No RVI for MockFusion
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

		#region Asset Callbacks

		private void Subscribe(IMockFusionAsset asset)
		{
			var staticAsset = asset as MockFusionStaticAsset;
			if (staticAsset != null)
			{
				staticAsset.OnFusionAssetSigUpdated += StaticAssetOnOnFusionAssetSigUpdated;
				staticAsset.OnFusionAssetPowerStateUpdated += StaticAssetOnOnFusionAssetPowerStateUpdated;
			}
		}

		private void StaticAssetOnOnFusionAssetSigUpdated(object sender, FusionAssetSigUpdatedArgs args)
		{
			OnFusionAssetSigUpdated.Raise(this, args);
		}

		private void StaticAssetOnOnFusionAssetPowerStateUpdated(object sender, FusionAssetPowerStateUpdatedArgs args)
		{
			OnFusionAssetPowerStateUpdated.Raise(this, args);
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("PrintSigs", "Print all the registered sigs and their values", () => PrintSigs());
			yield return new ConsoleCommand("PrintAssets", "Prints assets", () => PrintAssets());
			yield return new ConsoleCommand("PrintAllMappings", "Print all avaliable telemetry mappings", () => PrintAllMappings());
			yield return new GenericConsoleCommand<uint, bool>("SendOutputDigital",
			                                                   "Sends Digital Sig From Mock Fusion <number> <value>",
			                                                   (n, v) => SendOutputDigital(n, v));
			yield return new GenericConsoleCommand<uint, ushort>("SendOutputAnalog",
			                                                   "Sends Analog Sig From Mock Fusion <number> <value>",
			                                                   (n, v) => SendOutputAnalog(n, v));
			yield return new GenericConsoleCommand<uint, string>("SendOutputSerial",
			                                                   "Sends Serial Sig From Mock Fusion <number> <value>",
			                                                   (n, v) => SendOutputSerial(n, v));
			yield return new GenericConsoleCommand<bool>("FusionActionSystemPower", "FusionActionSystemPower [true|false]", (b) => FusionActionSystemPower(b));
			yield return new GenericConsoleCommand<bool>("FusionActionDisplayPower",
			                                             "FusionActionDisplayPower [true|false]",
			                                             (b) => FusionActionDisplayPower(b));


		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase consoleNode in GetBaseConsoleNodes())
				yield return consoleNode;

			yield return ConsoleNodeGroup.KeyNodeMap("Assets", "Assets for the room", m_Assets.Values, a => a.Number);
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		#endregion

		#region Console Methods

		public string PrintSigs()
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

		public string PrintAssets()
		{
			TableBuilder table = new TableBuilder("Number", "Name", "AssetType", "Type", "InstanceId");

			foreach (IMockFusionAsset asset in m_Assets.Values.OrderBy(a => a.Number))
				table.AddRow(asset.Number, asset.Name, asset.AssetType, asset.Type, asset.InstanceId);

			return table.ToString();
		}

		private void SendOutputDigital(uint number, bool value)
		{
			SigInfo sigInfo = new SigInfo(number, "", 0, value);
			m_SigCallbackManager.RaiseSigChangeCallback(sigInfo);
		}

		private void SendOutputAnalog(uint number, ushort value)
		{
			SigInfo sigInfo = new SigInfo(number, "", 0, value);
			m_SigCallbackManager.RaiseSigChangeCallback(sigInfo);
		}

		private void SendOutputSerial(uint number, string value)
		{
			SigInfo sigInfo = new SigInfo(number, "", 0, value);
			m_SigCallbackManager.RaiseSigChangeCallback(sigInfo);
		}

		public void FusionActionSystemPower(bool state)
		{
			OnFusionSystemPowerChangeEvent.Raise(this, new BoolEventArgs(state));
		}

		public void FusionActionDisplayPower(bool state)
		{
			OnFusionDisplayPowerChangeEvent.Raise(this, new BoolEventArgs(state) );
		}

		public string PrintAllMappings()
		{
			var table = new TableBuilder("MappingType", "SigType", "Start Sig", "End Sig", "Mapping Name", "Get Name", "Set Name");
			foreach (var mapping in FusionTelemetryMunger.GetMappings())
			{
				var singleMapping = mapping as FusionSigMapping;
				var multiMapping = mapping as FusionSigMultiMapping;
				if (singleMapping != null)
					table.AddRow("Single", mapping.SigType, singleMapping.Sig, "", mapping.FusionSigName, mapping.TelemetryGetName, mapping.TelemetrySetName);
				else if (multiMapping != null)
					table.AddRow("Multi", mapping.SigType, multiMapping.FirstSig, multiMapping.LastSig, mapping.FusionSigName, mapping.TelemetryGetName, mapping.TelemetrySetName);
			}

			return table.ToString();
		}

		#endregion
	}
}
