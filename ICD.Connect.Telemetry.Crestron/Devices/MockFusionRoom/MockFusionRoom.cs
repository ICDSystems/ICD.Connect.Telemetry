using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Timers;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Panels;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Settings;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Assets.Mock;
using ICD.Connect.Telemetry.Crestron.SigMappings;
using ICD.Connect.Telemetry.Crestron.Utils;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Crestron.Devices.MockFusionRoom
{
	public sealed class MockFusionRoom : AbstractDevice<MockFusionRoomSettings>, IFusionRoom
	{
		#region Fields

		private readonly Dictionary<uint, bool> m_InputSigsDigital;

		private readonly Dictionary<uint, ushort> m_InputSigsAnalog;

		private readonly Dictionary<uint, string> m_InputSigsSerials;

		private readonly Dictionary<eSigType, Dictionary<uint, string>> m_SigNames;

		private readonly Dictionary<eSigType, Dictionary<uint, eTelemetryIoMask>> m_SigIoMasks;

		private readonly SigCallbackManager m_SigCallbackManager;

		private readonly Dictionary<uint, IMockFusionAsset> m_Assets;

		private string m_ErrorMessage;

		private string m_DeviceUsage;

		private bool m_SystemPower;

		private bool m_DisplayPower;

		private ushort m_DisplayUsage;

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
		/// Gets the room name for the Fusion Room
		/// </summary>
		public string RoomName { get; private set; }

		/// <summary>
		/// IPID of the fusion instance
		/// This isn't a thing for Mock Fusion, but is required for RVI Generation
		/// So use a dummy value
		/// </summary>
		public byte IpId { get; private set; }

		/// <summary>
		/// Gets the user configurable assets.
		/// </summary>
		public IFusionAssetDataCollection UserConfigurableAssetDetails { get { return new MockFusionAssetDataCollection(m_Assets); } }

		#endregion

		public MockFusionRoom()
		{
			RoomId = Guid.NewGuid().ToString();

			m_InputSigsDigital = new Dictionary<uint, bool>();
			m_InputSigsAnalog  = new Dictionary<uint, ushort>();
			m_InputSigsSerials = new Dictionary<uint, string>();

			m_SigNames = new Dictionary<eSigType, Dictionary<uint, string>>();
			m_SigIoMasks = new Dictionary<eSigType, Dictionary<uint, eTelemetryIoMask>>();

			m_SigCallbackManager = new SigCallbackManager();

			m_Assets = new Dictionary<uint, IMockFusionAsset>();

		}

		#region Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		/// <summary>
		/// Gets the created input sigs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<SigInfo> GetInputSigInfo()
		{
			foreach (var kvp in m_InputSigsDigital)
				yield return new SigInfo(kvp.Key, 0, kvp.Value);

			foreach (var kvp in m_InputSigsAnalog)
				yield return new SigInfo(kvp.Key, 0, kvp.Value);

			foreach (var kvp in m_InputSigsSerials)
				yield return new SigInfo(kvp.Key, 0, kvp.Value);
		}

		/// <summary>
		/// Gets the created output sigs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<SigInfo> GetOutputSigInfo()
		{
			return m_SigCallbackManager.GetOutputSigs();
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
			Dictionary<uint, eTelemetryIoMask> masksForSigs;
			eTelemetryIoMask mask;
			if (m_SigIoMasks.TryGetValue(eSigType.Serial, out masksForSigs) && masksForSigs.TryGetValue(number, out mask) && mask.HasFlag(eTelemetryIoMask.ProgramToService))
				m_InputSigsSerials[number] = text;
			else
				Logger.Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, text);
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
			else
				Logger.Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, value);
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
			else
				Logger.Log(eSeverity.Error, "Couldn't send sig {0} with value {1}, Sig Not Added", number, value);
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

		public void UpdateDigitalSig(uint sig, bool newValue)
		{
			SendInputDigital(sig, newValue);
		}

		public bool ReadDigitalSig(uint sig)
		{
			throw new NotImplementedException();
		}

		public void UpdateAnalogSig(uint sig, ushort newValue)
		{
			SendInputAnalog(sig, newValue);
		}

		public ushort ReadAnalogSig(uint sig)
		{
			throw new NotImplementedException();
		}

		public void UpdateSerialSig(uint sig, string newValue)
		{
			SendInputSerial(sig, newValue);
		}

		public string ReadSerialSig(uint sig)
		{
			throw new NotImplementedException();
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
		/// Sets the display usage analog.
		/// </summary>
		/// <param name="usage"></param>
		public void SetDisplayUsage(ushort usage)
		{
			m_DisplayUsage = usage;
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
		public void AddSig(eSigType sigType, uint number, string name, eTelemetryIoMask mask)
		{
			m_SigIoMasks.GetOrAddNew(sigType, () => new Dictionary<uint, eTelemetryIoMask>())[number] = mask;
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
		public void AddSig(uint assetId, eSigType sigType, uint number, string name, eTelemetryIoMask mask)
		{
			IMockFusionAsset asset;
			if (!m_Assets.TryGetValue(assetId, out asset))
				return;

			var staticAsset = asset as MockFusionStaticAsset;
			if (staticAsset != null)
				staticAsset.AddSig(sigType,number,name,mask);
		}

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedBoolSigs()
		{
			return GetUserDefinedSigs(eSigType.Digital);
		}

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedUShortSigs()
		{
			return GetUserDefinedSigs(eSigType.Analog);
		}

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedStringSigs()
		{
			return GetUserDefinedSigs(eSigType.Serial);
		}

		/// <summary>
		/// Generates an RVI file for the fusion assets.
		/// </summary>
		public void RebuildRvi()
		{
			Logger.Log(eSeverity.Debug, "RVI Generation Starting");
			IcdStopwatch stopwatch = new IcdStopwatch();

			try
			{
				stopwatch.Start();
				RviUtils.GenerateFileForAllFusionDevices();
				stopwatch.Stop();
				Logger.Log(eSeverity.Debug, "RVI Generation took {0}ms", stopwatch.ElapsedMilliseconds);
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, "RVI Generation Exception {0}", e.Message);
			}
		}

		#endregion

		#region Private Methods

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedSigs(eSigType sigType)
		{
			Dictionary<uint, string> sigNameDictionary;
			if (!m_SigNames.TryGetValue(sigType, out sigNameDictionary))
				yield break;

			foreach (KeyValuePair<uint, string> sig in sigNameDictionary)
			{
				SigInfo sigInfo = new SigInfo(sigType, sig.Key, sig.Value, 0);
				yield return new KeyValuePair<SigInfo, eTelemetryIoMask>(sigInfo, GetSigMask(sigType, sig.Key));
			}
		}

		private eTelemetryIoMask GetSigMask(eSigType type, uint number)
		{
			Dictionary<uint, eTelemetryIoMask> sigMaskDictionary;
			if (!m_SigIoMasks.TryGetValue(type, out sigMaskDictionary))
				return eTelemetryIoMask.Na;

			eTelemetryIoMask telemetryIoMask;
			if (!sigMaskDictionary.TryGetValue(number, out telemetryIoMask))
				return eTelemetryIoMask.Na;

			return telemetryIoMask;
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

		#endregion

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

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(MockFusionRoomSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			RoomName = settings.RoomName;
			IpId = settings.Ipid ?? 0xF0;
			RoomId = settings.RoomId;

			RviUtils.RegisterFusionRoom(this);
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			RviUtils.UnregisterFusionRoom(this);
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
			yield return new ConsoleCommand("PrintRoomMappings", "Print the room telemetry mappings", () => PrintRoomMappings());
			yield return new ConsoleCommand("PrintAssetMappings", "Print the asset telemetry mappings", () => PrintAssetMappings());
			yield return new GenericConsoleCommand<uint, bool>("SendOutputDigital",
			                                                   "Sends Digital Sig From Mock Fusion <number> <value>",
			                                                   (n, v) => SendOutputDigital(n, v));
			yield return new GenericConsoleCommand<uint, ushort>("SendOutputAnalog",
			                                                   "Sends Analog Sig From Mock Fusion <number> <value>",
			                                                   (n, v) => SendOutputAnalog(n, v));
			yield return new GenericConsoleCommand<uint, string>("SendOutputSerial",
			                                                   "Sends Serial Sig From Mock Fusion <number> <value>",
			                                                   (n, v) => SendOutputSerial(n, v));
			yield return new GenericConsoleCommand<bool>("FusionActionSystemPower", "FusionActionSystemPower [true|false]", b => FusionActionSystemPower(b));
			yield return new GenericConsoleCommand<bool>("FusionActionDisplayPower",
			                                             "FusionActionDisplayPower [true|false]",
			                                             b => FusionActionDisplayPower(b));


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

		private void FusionActionSystemPower(bool state)
		{
			OnFusionSystemPowerChangeEvent.Raise(this, new BoolEventArgs(state));
		}

		private void FusionActionDisplayPower(bool state)
		{
			OnFusionDisplayPowerChangeEvent.Raise(this, new BoolEventArgs(state) );
		}

		private static string PrintRoomMappings()
		{
			TableBuilder table = new TableBuilder("SigType", "Start Sig", "End Sig", "Mapping Name", "Telemetry Name");

			foreach (RoomFusionSigMapping mapping in FusionTelemetryMunger.GetRoomMappings())
				table.AddRow(mapping.SigType,
							 mapping.Sig,
							 mapping.Sig + (mapping.Range - 1),
							 mapping.FusionSigName,
							 mapping.TelemetryName);

			return table.ToString();
		}

		private static string PrintAssetMappings()
		{
			TableBuilder table = new TableBuilder("SigType", "Start Sig", "End Sig", "Mapping Name", "Telemetry Name");

			foreach (AssetFusionSigMapping mapping in FusionTelemetryMunger.GetAssetMappings())
				table.AddRow(mapping.SigType,
				             mapping.Sig,
				             mapping.Sig + (mapping.Range - 1),
				             mapping.FusionSigName,
				             mapping.TelemetryName);

			return table.ToString();
		}

		#endregion
	}
}
