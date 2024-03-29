﻿using System;
using System.Collections.Generic;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Timers;
using ICD.Connect.API.Commands;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings;
using ICD.Connect.Telemetry.Crestron.Utils;
using ICD.Connect.Telemetry.Nodes;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using Crestron.SimplSharpPro.CrestronThread;
using ICD.Connect.Telemetry.CrestronPro.Assets;
#endif
using ICD.Connect.Misc.CrestronPro;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Panels.Devices;
using ICD.Connect.Panels.SigCollections;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.Crestron.Devices;
using eAssetType = ICD.Connect.Telemetry.Crestron.Assets.eAssetType;
using eSigType = ICD.Connect.Protocol.Sigs.eSigType;

namespace ICD.Connect.Telemetry.CrestronPro.Devices
{
	/// <summary>
	/// Wrapper for the Crestron FusionRoom object.
	/// </summary>
	public sealed class FusionRoomAdapter : AbstractSigDevice<FusionRoomAdapterSettings>, IFusionRoom
	{
		public event EventHandler<FusionAssetSigUpdatedArgs> OnFusionAssetSigUpdated;
		public event EventHandler<FusionAssetPowerStateUpdatedArgs> OnFusionAssetPowerStateUpdated;
		public event EventHandler<BoolEventArgs> OnFusionSystemPowerChangeEvent;
		public event EventHandler<BoolEventArgs> OnFusionDisplayPowerChangeEvent;

		private const uint MIN_SIG = 1;
		private const uint MAX_SIG = 1001;
		public const uint SIG_OFFSET = 49;

		private IDeviceBooleanInputCollection m_BooleanInput;
		private IDeviceBooleanOutputCollection m_BooleanOutput;
		private IDeviceUShortInputCollection m_UShortInput;
		private IDeviceUShortOutputCollection m_UShortOutput;
		private IDeviceStringInputCollection m_StringInput;
		private IDeviceStringOutputCollection m_StringOutput;
		private IFusionAssetDataCollection m_FusionAssets;

#if !NETSTANDARD
		private FusionRoom m_FusionRoom;
		private Thread m_RviGenerationThread;
#endif

		#region Properties

		/// <summary>
		/// Collection of Boolean Inputs sent to the device.
		/// </summary>
		public override IDeviceBooleanInputCollection BooleanInput
		{
			get
			{
#if !NETSTANDARD
				return m_BooleanInput ??
				       (m_BooleanInput = new FusionBooleanInputCollectionAdapter(m_FusionRoom.UserDefinedBooleanSigDetails));
#else
                throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Collection of Boolean Outputs received from the device.
		/// </summary>
		public override IDeviceBooleanOutputCollection BooleanOutput
		{
			get
			{
#if !NETSTANDARD
				return m_BooleanOutput ??
				       (m_BooleanOutput = new FusionBooleanOutputCollectionAdapter(m_FusionRoom.UserDefinedBooleanSigDetails));
#else
                throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Collection of Integer Inputs sent to the device.
		/// </summary>
		public override IDeviceUShortInputCollection UShortInput
		{
			get
			{
#if !NETSTANDARD
				return m_UShortInput ??
				       (m_UShortInput = new FusionUShortInputCollectionAdapter(m_FusionRoom.UserDefinedUShortSigDetails));
#else
                throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Collection of Integer Outputs received from the device.
		/// </summary>
		public override IDeviceUShortOutputCollection UShortOutput
		{
			get
			{
#if !NETSTANDARD
				return m_UShortOutput ??
				       (m_UShortOutput = new FusionUShortOutputCollectionAdapter(m_FusionRoom.UserDefinedUShortSigDetails));
#else
                throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Collection of String Inputs sent to the device.
		/// </summary>
		public override IDeviceStringInputCollection StringInput
		{
			get
			{
#if !NETSTANDARD
				return m_StringInput ??
				       (m_StringInput = new FusionStringInputCollectionAdapter(m_FusionRoom.UserDefinedStringSigDetails));
#else
                throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Collection of String Outputs received from the device.
		/// </summary>
		public override IDeviceStringOutputCollection StringOutput
		{
			get
			{
#if !NETSTANDARD
				return m_StringOutput ??
				       (m_StringOutput = new FusionStringOutputCollectionAdapter(m_FusionRoom.UserDefinedStringSigDetails));
#else
                throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Gets the GUID for the Room ID.
		/// </summary>
		public string RoomId
		{
			get
			{
#if !NETSTANDARD
				return m_FusionRoom == null ? null : m_FusionRoom.ParameterInstanceID;
#else
                throw new NotSupportedException();
#endif
			}
		}

		public string RoomName { get; private set; }

		/// <summary>
		/// Collection of user configured assets received from the device.
		/// </summary>
		public IFusionAssetDataCollection UserConfigurableAssetDetails
		{
			get
			{
#if !NETSTANDARD
				return m_FusionAssets ??
				       (m_FusionAssets = new CustomFusionAssetDataCollectionAdapter(m_FusionRoom.UserConfigurableAssetDetails));
#else
                throw new NotSupportedException();
#endif
			}
		}


		/// <summary>
		/// Gets the IPID for the Fusion Room.
		/// </summary>
		public byte IpId
		{
			get
			{
#if !NETSTANDARD
				return m_FusionRoom == null ? (byte)0 : (byte)m_FusionRoom.ID;
#else
				return 0;
#endif
			}
		}

		#endregion

		#region Methods

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedBoolSigs()
		{
#if !NETSTANDARD
			foreach (BooleanSigData sig in m_FusionRoom.UserDefinedBooleanSigDetails)
			{
				var info = new SigInfo(eSigType.Digital, sig.Number, sig.Name, 0);
				yield return new KeyValuePair<SigInfo, eTelemetryIoMask>(info, sig.SigIoMask.ToIcd());
			}
#else
			throw new NotSupportedException();
#endif
		}

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedUShortSigs()
		{
#if !NETSTANDARD
			foreach (UShortSigData sig in m_FusionRoom.UserDefinedUShortSigDetails)
			{
				var info = new SigInfo(eSigType.Analog, sig.Number, sig.Name, 0);
				yield return new KeyValuePair<SigInfo, eTelemetryIoMask>(info, sig.SigIoMask.ToIcd());
			}
#else
			throw new NotSupportedException();
#endif
		}

		public IEnumerable<KeyValuePair<SigInfo, eTelemetryIoMask>> GetUserDefinedStringSigs()
		{
#if !NETSTANDARD
			foreach (StringSigData sig in m_FusionRoom.UserDefinedStringSigDetails)
			{
				var info = new SigInfo(eSigType.Serial, sig.Number, sig.Name, 0);
				yield return new KeyValuePair<SigInfo, eTelemetryIoMask>(info, sig.SigIoMask.ToIcd());
			}
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

#if !NETSTANDARD
			SetFusionRoom(null);

			Thread thread = m_RviGenerationThread;
			if (thread != null)
				thread.Abort();
#endif
		}

#if !NETSTANDARD
		/// <summary>
		/// Sets the wrapped fusion room.
		/// </summary>
		/// <param name="fusionRoom"></param>
		[PublicAPI]
		public void SetFusionRoom(FusionRoom fusionRoom)
		{
			RviUtils.UnregisterFusionRoom(this);

			Unsubscribe(m_FusionRoom);

			if (m_FusionRoom != null)
				GenericBaseUtils.TearDown(m_FusionRoom);

			m_BooleanInput = null;
			m_BooleanOutput = null;
			m_UShortInput = null;
			m_UShortOutput = null;
			m_StringInput = null;
			m_StringOutput = null;
			m_FusionAssets = null;

			m_FusionRoom = fusionRoom;

			eDeviceRegistrationUnRegistrationResponse result;
			if (m_FusionRoom != null && !GenericBaseUtils.SetUp(m_FusionRoom, this, out result))
				Logger.Log(eSeverity.Error, "Unable to register {0} - {1}", m_FusionRoom.GetType().Name, result);

			Subscribe(m_FusionRoom);

			if (m_FusionRoom != null)
				RviUtils.RegisterFusionRoom(this);

			UpdateCachedOnlineStatus();
		}
#endif

		/// <summary>
		/// Adds the asset to the fusion room.
		/// </summary>
		/// <param name="asset"></param>
		public void AddAsset(AssetInfo asset)
		{
#if !NETSTANDARD
			try
			{
				string name;

				// If it's not a static asset, append the asset type name
				if (asset.AssetType == eAssetType.StaticAsset)
					name = asset.Name;
				else
				{
					string suffix = StringUtils.NiceName(asset.AssetType);
					name = string.Format("{0} ({1})", asset.Name, suffix);
				}

				m_FusionRoom.AddAsset(asset.AssetType.FromIcd(),
				                      asset.Number,
				                      name,
				                      asset.Type,
				                      asset.InstanceId);

				if (asset.AssetType == eAssetType.StaticAsset)
				{
					FusionStaticAsset fusionStaticAsset = m_FusionRoom.UserConfigurableAssetDetails[asset.Number].Asset as FusionStaticAsset;
					if (fusionStaticAsset != null)
					{
						fusionStaticAsset.ParamMake.Value = asset.Make;
						fusionStaticAsset.ParamModel.Value = asset.Model;
					}
				}
		}
				//Throws an argument exception when a duplicate is added
			catch (Exception ex)
			{
				Logger.Log(eSeverity.Error, ex,
				           string.Format("Error adding Asset, Type:{0}, Number:{1}, Name:{2}, Id:{3}, DeviceType:{4}",
				                         asset.AssetType, asset.Number, asset.Name, asset.InstanceId, asset.Type));
			}
#else
			throw new NotSupportedException();
#endif
		}

		public void UpdateDigitalSig(uint sig, bool newValue)
		{
#if !NETSTANDARD
			// todo: add cache
			BooleanSigData data;
			if (m_FusionRoom.UserDefinedBooleanSigDetails.TryGetValue(sig, out data))
				data.InputSig.BoolValue = newValue;
			else
				throw new KeyNotFoundException(String.Format("Sig {0} not found in user defined bool sigs", sig));
#else
			throw new NotSupportedException();
#endif
		}

		public bool ReadDigitalSig(uint sig)
		{
#if !NETSTANDARD
			// todo: add cache
			BooleanSigData data;
			if (m_FusionRoom.UserDefinedBooleanSigDetails.TryGetValue(sig, out data))
				return data.OutputSig.BoolValue;

			throw new KeyNotFoundException(String.Format("Sig {0} not found in user defined bool sigs", sig));
#else
			throw new NotSupportedException();
#endif
		}

		public void UpdateAnalogSig(uint sig, ushort newValue)
		{
#if !NETSTANDARD
			// todo: add cache
			UShortSigData data;
			if (m_FusionRoom.UserDefinedUShortSigDetails.TryGetValue(sig, out data))
				data.InputSig.UShortValue = newValue;
			else
				throw new KeyNotFoundException(String.Format("Sig {0} not found in user defined ushort sigs", sig));
#else
			throw new NotSupportedException();
#endif
		}

		public ushort ReadAnalogSig(uint sig)
		{
#if !NETSTANDARD
			// todo: add cache
			UShortSigData data;
			if (m_FusionRoom.UserDefinedUShortSigDetails.TryGetValue(sig, out data))
				return data.OutputSig.UShortValue;

			throw new KeyNotFoundException(String.Format("Sig {0} not found in user defined ushort sigs", sig));
#else
			throw new NotSupportedException();
#endif
		}

		public void UpdateSerialSig(uint sig, string newValue)
		{
#if !NETSTANDARD
			// todo: add cache
			StringSigData data;
			if (m_FusionRoom.UserDefinedStringSigDetails.TryGetValue(sig, out data))
				data.InputSig.StringValue = newValue;
			else
				throw new KeyNotFoundException(String.Format("Sig {0} not found in user defined string sigs", sig));
#else
			throw new NotSupportedException();
#endif
		}

		public string ReadSerialSig(uint sig)
		{
#if !NETSTANDARD
			// todo: add cache
			StringSigData data;
			if (m_FusionRoom.UserDefinedStringSigDetails.TryGetValue(sig, out data))
				return data.OutputSig.StringValue;

			throw new KeyNotFoundException(String.Format("Sig {0} not found in user defined string sigs", sig));
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the fusion error message.
		/// </summary>
		/// <param name="message"></param>
		public void SetErrorMessage(string message)
		{
#if !NETSTANDARD
			m_FusionRoom.ErrorMessage.InputSig.StringValue = message;
#else
            throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sends the string as device usage.
		/// </summary>
		/// <param name="usage"></param>
		public void SendDeviceUsage(string usage)
		{
#if !NETSTANDARD
			m_FusionRoom.DeviceUsage.InputSig.StringValue = usage;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the display usage analog.
		/// </summary>
		/// <param name="usage"></param>
		public void SetDisplayUsage(ushort usage)
		{
#if !NETSTANDARD
			m_FusionRoom.DisplayUsage.InputSig.UShortValue = usage;
#else
			throw new NotSupportedException();
#endif
		}
	
		/// <summary>
		/// Sets the system power on/off state for the room
		/// </summary>
		/// <param name="powered"></param>
		public void SetSystemPower(bool powered)
		{
#if !NETSTANDARD
			m_FusionRoom.SystemPowerOn.InputSig.BoolValue = powered;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the display power on/off state for the room.
		/// </summary>
		/// <param name="powered"></param>
		public void SetDisplayPower(bool powered)
		{
#if !NETSTANDARD
			m_FusionRoom.DisplayPowerOn.InputSig.BoolValue = powered;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Removes the asset with the given id.
		/// </summary>
		/// <param name="assetId"></param>
		public void RemoveAsset(uint assetId)
		{
#if !NETSTANDARD
			m_FusionRoom.RemoveAsset(assetId);
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Gets the configured asset ids.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<uint> GetAssetIds()
		{
#if !NETSTANDARD
			return m_FusionRoom.UserConfigurableAssetDetails
			                   .Select<CustomFusionAssetData, uint>(data => data.Number);
#else
			throw new NotSupportedException();
#endif
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
#if !NETSTANDARD
			try
			{
				m_FusionRoom.AddSig(sigType.FromIcd(), number, name, mask.FromIcd());
			}
				//Throws an argument exception when a duplicate is added
			catch (Exception ex)
			{
				Logger.Log(eSeverity.Error, ex,
				           string.Format("Error adding Sig, Type:{0}, Number:{1}, Name:{2}, Mask:{3}",
				                         sigType, number, name, mask));
			}
#else
			throw new NotSupportedException();
#endif

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
#if !NETSTANDARD
			try
			{
				m_FusionRoom.AddSig(assetId, sigType.FromIcd(), number, name, mask.FromIcd());
			}
				//Throws an argument exception when a duplicate is added
			catch (Exception ex)
			{
				Logger.Log(eSeverity.Error, ex,
				           string.Format("Error adding Sig, AssetId:{0}, Type:{1}, Number:{2}, Name:{3}, Mask:{4}",
				                         assetId, sigType, number, name, mask));
			}
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Generates an RVI file for the fusion assets.
		/// </summary>
		public void RebuildRvi()
		{
#if !NETSTANDARD

			// If RVI generation is already running, Cancel
			if (m_RviGenerationThread != null)
			{
				Logger.Log(eSeverity.Warning, "RVI Generation already running, aborting");
				return;
			}

			m_RviGenerationThread = new Thread(RviGenerationThreadCallback, null, Thread.eThreadStartOptions.CreateSuspended)
			{
				Name = "RviGenerationThread",
				Priority = Thread.eThreadPriority.LowestPriority
			};
			m_RviGenerationThread.Start();
#else
			throw new NotSupportedException();
#endif
		}

#if !NETSTANDARD
		private object RviGenerationThreadCallback(object userspecific)
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
				Logger.Log(eSeverity.Error, e, "RVI Generation Exception");
			}
			finally
			{
				m_RviGenerationThread = null;
			}

			return null;
		}
#endif

#endregion

#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(FusionRoomAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

#if !NETSTANDARD
			settings.Ipid = m_FusionRoom == null ? (byte)0 : (byte)m_FusionRoom.ID;
			settings.RoomName = m_FusionRoom == null ? null : m_FusionRoom.ParameterRoomName;
			settings.RoomId = m_FusionRoom == null ? null : m_FusionRoom.ParameterInstanceID;
#else
            settings.Ipid = 0;
            settings.RoomName = null;
            settings.RoomId = null;
#endif
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if !NETSTANDARD
			SetFusionRoom(null);
#endif
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(FusionRoomAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

#if !NETSTANDARD

			RoomName = settings.RoomName;

			FusionRoom fusionRoom = settings.Ipid == null
				                        ? null
				                        : new FusionRoom(settings.Ipid.Value,
				                                         ProgramInfo.ControlSystem,
				                                         settings.RoomName,
				                                         settings.RoomId);

			SetFusionRoom(fusionRoom);
#else
            throw new NotSupportedException();
#endif
		}

		#endregion

#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if !NETSTANDARD
			return m_FusionRoom != null && m_FusionRoom.IsOnline;
#else
            return false;
#endif
		}

#if !NETSTANDARD

		/// <summary>
		/// Raises the OnFusionSystemPowerChangeEvent with the given system power state
		/// </summary>
		/// <param name="state"></param>
		private void RaiseSystemPowerChange(bool state)
		{
			OnFusionSystemPowerChangeEvent.Raise(this, new BoolEventArgs(state));
		}

		/// <summary>
		/// Raises the OnFusionDisplayPowerChangeEvent with the given display power state
		/// </summary>
		/// <param name="state"></param>
		private void RaiseDisplayPowerChange(bool state)
		{
			OnFusionDisplayPowerChangeEvent.Raise(this, new BoolEventArgs(state));
		}

		/// <summary>
		/// Gets the ISig from the collection, passing to the base class to be raised via delegate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="number"></param>
		/// <param name="getSig"></param>
		private void RaiseOutputSigChangeCallback<T>(uint number, Func<uint, T> getSig)
			where T : ISig
		{
			// Sometimes the FusionRoom will report changes that do not fit in the user
			// configured range. We can't access these sigs from the collections, so
			// ignore them for now.
			uint translated = number - SIG_OFFSET;
			if (translated < MIN_SIG || translated > MAX_SIG)
				return;

			T sig = getSig(number);
			SigInfo sigInfo = new SigInfo(sig);

			RaiseOutputSigChangeCallback(sigInfo);
		}

		private void RaiseSigUpdatedForDigitalSig(FusionAssetStateEventArgs args)
		{
			BooleanSigData data = args.UserConfiguredSigDetail as BooleanSigData;
			if (data == null)
				return;

			OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(args.UserConfigurableAssetDetailIndex,
			                                                                  eSigType.Digital,
			                                                                  data.Number - SIG_OFFSET));
		}

		private void RaiseSigUpdatedForAnalogSig(FusionAssetStateEventArgs args)
		{
			UShortSigData data = args.UserConfiguredSigDetail as UShortSigData;
			if (data == null)
				return;

			OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(args.UserConfigurableAssetDetailIndex,
			                                                                  eSigType.Analog,
			                                                                  data.Number - SIG_OFFSET));
		}

		private void RaiseSigUpdatedForSerialSig(FusionAssetStateEventArgs args)
		{
			StringSigData data = args.UserConfiguredSigDetail as StringSigData;
			if (data == null)
				return;

			OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(args.UserConfigurableAssetDetailIndex,
			                                                                  eSigType.Serial,
			                                                                  data.Number - SIG_OFFSET));
		}

		private void RaiseSigUpdatedForFixedNameSig(FusionAssetStateEventArgs args)
		{
			int data = args.EventId;
			switch (data)
			{
				case FusionAssetEventId.StaticAssetPowerOffReceivedEventId:
					OnFusionAssetPowerStateUpdated.Raise(this,
					                                     new FusionAssetPowerStateUpdatedArgs(args.UserConfigurableAssetDetailIndex,
					                                                                          false));
					return;
				case FusionAssetEventId.StaticAssetPowerOnReceivedEventId:
					OnFusionAssetPowerStateUpdated.Raise(this,
					                                     new FusionAssetPowerStateUpdatedArgs(args.UserConfigurableAssetDetailIndex,
					                                                                          true));
					return;
			}
		}

#endif

#endregion

#region FusionRoom Callbacks

#if !NETSTANDARD
		/// <summary>
		/// Subscribes to the FusionRoom events.
		/// </summary>
		/// <param name="fusionRoom"></param>
		private void Subscribe(FusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				return;

			fusionRoom.FusionStateChange += FusionRoomOnFusionStateChange;
			fusionRoom.FusionAssetStateChange += FusionRoomOnFusionAssetStateChange;
			fusionRoom.OnlineStatusChange += FusionRoomOnOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the FusionRoom events.
		/// </summary>
		/// <param name="fusionRoom"></param>
		private void Unsubscribe(FusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				return;

			fusionRoom.FusionStateChange -= FusionRoomOnFusionStateChange;
			fusionRoom.FusionAssetStateChange -= FusionRoomOnFusionAssetStateChange;
			fusionRoom.OnlineStatusChange -= FusionRoomOnOnlineStatusChange;
		}

		/// <summary>
		/// Called when a sig value changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void FusionRoomOnFusionStateChange(FusionBase device, FusionStateEventArgs args)
		{
			switch (args.EventId)
			{
				case FusionEventIds.UserConfiguredBoolSigChangeEventId:
					BooleanSigData boolData = args.UserConfiguredSigDetail as BooleanSigData;
					if (boolData != null)
						RaiseOutputSigChangeCallback(boolData.Number, n => BooleanOutput[n]);
					break;
				case FusionEventIds.UserConfiguredUShortSigChangeEventId:
					StringSigData stringData = args.UserConfiguredSigDetail as StringSigData;
					if (stringData != null)
						RaiseOutputSigChangeCallback(stringData.Number, n => StringOutput[n]);
					break;
				case FusionEventIds.UserConfiguredStringSigChangeEventId:
					UShortSigData ushortData = args.UserConfiguredSigDetail as UShortSigData;
					if (ushortData != null)
						RaiseOutputSigChangeCallback(ushortData.Number, n => UShortOutput[n]);
					break;
				case FusionEventIds.SystemPowerOffReceivedEventId:
					RaiseSystemPowerChange(false);
					break;
				case FusionEventIds.SystemPowerOnReceivedEventId:
					RaiseSystemPowerChange(true);
					break;
				case FusionEventIds.DisplayPowerOffReceivedEventId:
					RaiseDisplayPowerChange(false);
					break;
				case FusionEventIds.DisplayPowerOnReceivedEventId:
					RaiseDisplayPowerChange(true);
					break;
			}
		}

		private void FusionRoomOnFusionAssetStateChange(FusionBase device, FusionAssetStateEventArgs args)
		{
			switch (args.EventId)
			{
				case FusionAssetEventId.StaticAssetAssetBoolAssetSigEventReceivedEventId:
					RaiseSigUpdatedForDigitalSig(args);
					break;
				case FusionAssetEventId.StaticAssetAssetUshortAssetSigEventReceivedEventId:
					RaiseSigUpdatedForAnalogSig(args);
					break;
				case FusionAssetEventId.StaticAssetAssetStringAssetSigEventReceivedEventId:
					RaiseSigUpdatedForSerialSig(args);
					break;
				default:
					RaiseSigUpdatedForFixedNameSig(args);
					break;
			}
		}

		/// <summary>
		/// Called when the fusion room changes online status.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void FusionRoomOnOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

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

			yield return new ConsoleCommand("GenerateRVI", "Starts generation of the RVI", () => RebuildRvi());
#if !NETSTANDARD
			yield return new ConsoleCommand("CrestronRVI", "Generate the RVI with Crestron utils", () => FusionRVI.GenerateFileForAllFusionDevices());
#endif
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

#endregion
	}
}
