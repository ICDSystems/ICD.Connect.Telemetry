﻿using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Crestron;
using ICD.Connect.Telemetry.Crestron.Assets;
using ICD.Connect.Telemetry.CrestronPro.Assets;
#if SIMPLSHARP
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using ICD.Connect.Protocol.Sigs;
#endif
using System;
using ICD.Connect.Panels.Devices;
using ICD.Connect.Panels.SigCollections;
using ICD.Connect.Settings.Core;
using eAssetType = ICD.Connect.Telemetry.Crestron.Assets.eAssetType;
using eSigIoMask = ICD.Connect.Protocol.Sigs.eSigIoMask;
using eSigType = ICD.Connect.Protocol.Sigs.eSigType;

namespace ICD.Connect.Telemetry.CrestronPro
{
	/// <summary>
	/// Wrapper for the Crestron FusionRoom object.
	/// </summary>
	public sealed class FusionRoomAdapter : AbstractSigDevice<FusionRoomAdapterSettings>, IFusionRoom
	{
		public event EventHandler<FusionAssetSigUpdatedArgs> OnFusionAssetSigUpdated;

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

#if SIMPLSHARP
        private FusionRoom m_FusionRoom;
#endif
		private string m_FusionSigsPath;

#region Properties

		/// <summary>
		/// Collection of Boolean Inputs sent to the device.
		/// </summary>
		public override IDeviceBooleanInputCollection BooleanInput
		{
			get
			{
#if SIMPLSHARP
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
		private IDeviceBooleanOutputCollection BooleanOutput
		{
			get
			{
#if SIMPLSHARP
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
#if SIMPLSHARP
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
		private IDeviceUShortOutputCollection UShortOutput
		{
			get
			{
#if SIMPLSHARP
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
#if SIMPLSHARP
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
		private IDeviceStringOutputCollection StringOutput
		{
			get
			{
#if SIMPLSHARP
                return m_StringOutput ??
				       (m_StringOutput = new FusionStringOutputCollectionAdapter(m_FusionRoom.UserDefinedStringSigDetails));
#else
                throw new NotSupportedException();
#endif
            }
		}

		

		/// <summary>
		/// Collection of user configured assets received from the device.
		/// </summary>
		public IFusionAssetDataCollection UserConfigurableAssetDetails
		{
			get
			{
#if SIMPLSHARP
				return m_FusionAssets ??
					   (m_FusionAssets = new CustomFusionAssetDataCollectionAdapter(m_FusionRoom.UserConfigurableAssetDetails));
#else
                throw new NotSupportedException();
#endif
			}
		}

#endregion

#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

#if SIMPLSHARP
            SetFusionRoom(null);
#endif
		}

#if SIMPLSHARP
        /// <summary>
        /// Sets the wrapped fusion room.
        /// </summary>
        /// <param name="fusionRoom"></param>
        [PublicAPI]
		public void SetFusionRoom(FusionRoom fusionRoom)
		{
			Unsubscribe(m_FusionRoom);

			if (m_FusionRoom != null)
			{
				if (m_FusionRoom.Registered)
					m_FusionRoom.UnRegister();

				try
				{
					m_FusionRoom.Dispose();
				}
				catch (Exception)
				{
				}
			}

	        m_BooleanInput = null;
	        m_BooleanOutput = null;
	        m_UShortInput = null;
	        m_UShortOutput = null;
	        m_StringInput = null;
	        m_StringOutput = null;
	        m_FusionAssets = null;

			m_FusionRoom = fusionRoom;
			if (m_FusionRoom != null && !m_FusionRoom.Registered)
			{
				if (Name != null)
					m_FusionRoom.Description = Name;

				eDeviceRegistrationUnRegistrationResponse result = m_FusionRoom.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Log(eSeverity.Error, "Unable to register {0} - {1}", m_FusionRoom.GetType().Name, result);
			}

			Subscribe(m_FusionRoom);

			UpdateCachedOnlineStatus();
		}
#endif

		/// <summary>
		/// Adds the asset to the fusion room.
		/// </summary>
		/// <param name="assetType"></param>
		/// <param name="number"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="instanceId"></param>
		public void AddAsset(eAssetType assetType, uint number, string name, string type, string instanceId)
		{
#if SIMPLSHARP
			m_FusionRoom.AddAsset(assetType.FromIcd(), number, name, type, instanceId);
			m_FusionRoom.ReRegister();
			FusionRVI.GenerateFileForAllFusionDevices();
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
#if SIMPLSHARP
            m_FusionRoom.ErrorMessage.InputSig.StringValue = message;
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
#if SIMPLSHARP
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
#if SIMPLSHARP
			return m_FusionRoom.UserConfigurableAssetDetails
			                   .Select<CustomFusionAssetData, uint>(data => data.Number);
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
		public void AddSig(uint assetId, eSigType sigType, uint number, string name, eSigIoMask mask)
		{
#if SIMPLSHARP
			m_FusionRoom.AddSig(assetId, sigType.FromIcd(), number, name, mask.FromIcd());
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Loads sigs from the xml file at the given path.
		/// </summary>
		public void LoadSigs()
		{
#if SIMPLSHARP
            FusionRVI.GenerateFileForAllFusionDevices();
#else
            throw new NotImplementedException();
#endif
        }

#endregion

#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(FusionRoomAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

#if SIMPLSHARP
            settings.Ipid = m_FusionRoom == null ? (byte)0 : (byte)m_FusionRoom.ID;
			settings.RoomName = m_FusionRoom == null ? null : m_FusionRoom.ParameterRoomName;
			settings.RoomId = m_FusionRoom == null ? null : m_FusionRoom.ParameterInstanceID;
#else
            settings.Ipid = 0;
            settings.RoomName = null;
            settings.RoomId = null;
#endif
            settings.FusionSigsPath = m_FusionSigsPath;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if SIMPLSHARP
            SetFusionRoom(null);
#endif
			m_FusionSigsPath = null;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(FusionRoomAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
            FusionRoom fusionRoom = settings.Ipid == null 
									? null
									: new FusionRoom(settings.Ipid.Value,
													 ProgramInfo.ControlSystem,
													 settings.RoomName,
													 settings.RoomId);
			
			SetFusionRoom(fusionRoom);

			LoadSigs();
#else
            throw new NotImplementedException();
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
#if SIMPLSHARP
            return m_FusionRoom != null && m_FusionRoom.IsOnline;
#else
            return false;
#endif
        }

#if SIMPLSHARP
        /// <summary>
        /// Subscribes to the FusionRoom events.
        /// </summary>
        /// <param name="fusionRoom"></param>
        private void Subscribe(FusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				return;

			fusionRoom.FusionStateChange += FusionRoomOnFusionStateChange;
			fusionRoom.FusionAssetStateChange += FusionAssetStateChange;
			fusionRoom.OnlineStatusChange += FusionRoomOnlineStatusChange;
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
			fusionRoom.FusionAssetStateChange -= FusionAssetStateChange;
			fusionRoom.OnlineStatusChange -= FusionRoomOnlineStatusChange;
		}

		/// <summary>
		/// Called when a sig value changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void FusionRoomOnFusionStateChange(FusionBase device, FusionStateEventArgs args)
		{
			object detail = args.UserConfiguredSigDetail;

			BooleanSigData boolData = detail as BooleanSigData;
			if (boolData != null)
				RaiseOutputSigChangeCallback(boolData.Number, n => BooleanOutput[n]);

			StringSigData stringData = detail as StringSigData;
			if (stringData != null)
				RaiseOutputSigChangeCallback(stringData.Number, n => StringOutput[n]);

			UShortSigData ushortData = detail as UShortSigData;
			if (ushortData != null)
				RaiseOutputSigChangeCallback(ushortData.Number, n => UShortOutput[n]);
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

		private void FusionAssetStateChange(FusionBase device, FusionAssetStateEventArgs args)
		{
            //TODO: Parse out the userConfiguredSigDetail on the args to get the sig number

			uint sigNumber = 0;
			switch (args.EventId)
			{
				case FusionAssetEventId.StaticAssetAssetBoolAssetSigEventReceivedEventId:
					OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(args.UserConfigurableAssetDetailIndex,
																					  eSigType.Digital,
																					  sigNumber));
					break;
				case FusionAssetEventId.StaticAssetAssetUshortAssetSigEventReceivedEventId:
					OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(args.UserConfigurableAssetDetailIndex,
																					  eSigType.Analog,
																					  sigNumber));
					break;
				case FusionAssetEventId.StaticAssetAssetStringAssetSigEventReceivedEventId:
					OnFusionAssetSigUpdated.Raise(this, new FusionAssetSigUpdatedArgs(args.UserConfigurableAssetDetailIndex,
																					  eSigType.Serial,
																					  sigNumber));
					break;
				default:
					return;
			}
				
		}

		/// <summary>
		/// Called when the fusion room changes online status.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void FusionRoomOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

#endregion

		
	}
}
