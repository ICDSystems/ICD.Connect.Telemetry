using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Panels;
using ICD.Connect.Panels.SigCollections;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Analytics.FusionPro
{
	/// <summary>
	/// Wrapper for the Crestron FusionRoom object.
	/// </summary>
	public sealed class FusionRoomAdapter : AbstractSigDevice<FusionRoomAdapterSettings>, IFusionRoom
	{
		private const uint MIN_SIG = 1;
		private const uint MAX_SIG = 1001;
		public const uint SIG_OFFSET = 49;

		private IDeviceBooleanInputCollection m_BooleanInput;
		private IDeviceBooleanOutputCollection m_BooleanOutput;
		private IDeviceUShortInputCollection m_UShortInput;
		private IDeviceUShortOutputCollection m_UShortOutput;
		private IDeviceStringInputCollection m_StringInput;
		private IDeviceStringOutputCollection m_StringOutput;

		private FusionRoom m_FusionRoom;
		private string m_FusionSigsPath;

		#region Properties

		/// <summary>
		/// Collection of Boolean Inputs sent to the device.
		/// </summary>
		protected override IDeviceBooleanInputCollection BooleanInput
		{
			get
			{
				return m_BooleanInput ??
				       (m_BooleanInput = new FusionBooleanInputCollectionAdapter(m_FusionRoom.UserDefinedBooleanSigDetails));
			}
		}

		/// <summary>
		/// Collection of Boolean Outputs received from the device.
		/// </summary>
		private IDeviceBooleanOutputCollection BooleanOutput
		{
			get
			{
				return m_BooleanOutput ??
				       (m_BooleanOutput = new FusionBooleanOutputCollectionAdapter(m_FusionRoom.UserDefinedBooleanSigDetails));
			}
		}

		/// <summary>
		/// Collection of Integer Inputs sent to the device.
		/// </summary>
		protected override IDeviceUShortInputCollection UShortInput
		{
			get
			{
				return m_UShortInput ??
				       (m_UShortInput = new FusionUShortInputCollectionAdapter(m_FusionRoom.UserDefinedUShortSigDetails));
			}
		}

		/// <summary>
		/// Collection of Integer Outputs received from the device.
		/// </summary>
		private IDeviceUShortOutputCollection UShortOutput
		{
			get
			{
				return m_UShortOutput ??
				       (m_UShortOutput = new FusionUShortOutputCollectionAdapter(m_FusionRoom.UserDefinedUShortSigDetails));
			}
		}

		/// <summary>
		/// Collection of String Inputs sent to the device.
		/// </summary>
		protected override IDeviceStringInputCollection StringInput
		{
			get
			{
				return m_StringInput ??
				       (m_StringInput = new FusionStringInputCollectionAdapter(m_FusionRoom.UserDefinedStringSigDetails));
			}
		}

		/// <summary>
		/// Collection of String Outputs received from the device.
		/// </summary>
		private IDeviceStringOutputCollection StringOutput
		{
			get
			{
				return m_StringOutput ??
				       (m_StringOutput = new FusionStringOutputCollectionAdapter(m_FusionRoom.UserDefinedStringSigDetails));
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

			SetFusionRoom(null);
		}

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

			m_FusionRoom = fusionRoom;
			if (m_FusionRoom != null && !m_FusionRoom.Registered)
			{
				eDeviceRegistrationUnRegistrationResponse result = m_FusionRoom.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					ErrorLog.Error("Unable to register {0} - {1}", m_FusionRoom.GetType().Name, result);
			}

			Subscribe(m_FusionRoom);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Sets the fusion error message.
		/// </summary>
		/// <param name="message"></param>
		public void SetErrorMessage(string message)
		{
			m_FusionRoom.ErrorMessage.InputSig.StringValue = message;
		}

		/// <summary>
		/// Loads sigs from the xml file at the given path.
		/// </summary>
		/// <param name="path"></param>
		public void LoadSigsFromPath(string path)
		{
			m_FusionRoom.RemoveAllSigs();

			m_FusionSigsPath = PathUtils.GetDefaultConfigPath(path);
			if (string.IsNullOrEmpty(m_FusionSigsPath) || !File.Exists(m_FusionSigsPath))
			{
				IcdErrorLog.Error("Unable to find {0}", m_FusionSigsPath);
				return;
			}

			string xml = File.ReadToEnd(m_FusionSigsPath, Encoding.UTF8);

			foreach (FusionXmlSig sig in FusionXmlSig.SigsFromXml(xml))
			{
				// S#Pro Fusion is the only place where joins start at 1 instead of 50.
				// The FusionRoom class adds 49 to each number.
				int number = (int)sig.Number - (int)SIG_OFFSET;
				if (number < MIN_SIG || number > MAX_SIG)
				{
					ErrorLog.Error("Skipping FusionRoom sig {0} - joins start at 50.", sig);
					continue;
				}

				m_FusionRoom.AddSig(sig.CrestronSigType, (uint)number, sig.Name, sig.CrestronSigMask);
			}
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

			settings.Ipid = m_FusionRoom == null ? (byte)0 : (byte)m_FusionRoom.ID;
			settings.RoomName = m_FusionRoom == null ? null : m_FusionRoom.ParameterRoomName;
			settings.RoomId = m_FusionRoom == null ? null : m_FusionRoom.ParameterInstanceID;
			settings.FusionSigsPath = m_FusionSigsPath;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetFusionRoom(null);
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

			FusionRoom fusionRoom = new FusionRoom(settings.Ipid, ProgramInfo.ControlSystem, settings.RoomName, settings.RoomId);

			SetFusionRoom(fusionRoom);
			LoadSigsFromPath(settings.FusionSigsPath);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_FusionRoom != null && m_FusionRoom.IsOnline;
		}

		/// <summary>
		/// Subscribes to the FusionRoom events.
		/// </summary>
		/// <param name="fusionRoom"></param>
		private void Subscribe(FusionRoom fusionRoom)
		{
			if (fusionRoom == null)
				return;

			fusionRoom.FusionStateChange += FusionRoomOnFusionStateChange;
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
			RaiseOutputSigChangeCallback(sig);
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

		#endregion
	}
}
