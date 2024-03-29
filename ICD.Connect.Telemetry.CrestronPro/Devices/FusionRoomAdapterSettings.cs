﻿using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Telemetry.CrestronPro.Devices
{
	/// <summary>
	/// Settings for the FusionRoomAdapter originator.
	/// </summary>
	[KrangSettings("FusionRoom", typeof(FusionRoomAdapter))]
	public sealed class FusionRoomAdapterSettings : AbstractDeviceSettings
	{
		private const string IPID_ELEMENT = "IPID";
		private const string ROOM_NAME_ELEMENT = "RoomName";
		private const string ROOM_ID_ELEMENT = "RoomId";

		private string m_RoomId;

		#region Properties

		[CrestronByteSettingsProperty]
		public byte? Ipid { get; set; }

		public string RoomName { get; set; }

		/// <summary>
		/// Gets/sets the room id. Returns a GUID if an id has not been set.
		/// </summary>
		public string RoomId
		{
			get
			{
				if (string.IsNullOrEmpty(m_RoomId))
					m_RoomId = Guid.NewGuid().ToString();
				return m_RoomId;
			}
			set { m_RoomId = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(IPID_ELEMENT, Ipid == null ? null : StringUtils.ToIpIdString(Ipid.Value));
			writer.WriteElementString(ROOM_NAME_ELEMENT, RoomName);
			writer.WriteElementString(ROOM_ID_ELEMENT, RoomId);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			byte ipid = XmlUtils.ReadChildElementContentAsByte(xml, IPID_ELEMENT);
			string roomName = XmlUtils.ReadChildElementContentAsString(xml, ROOM_NAME_ELEMENT);
			string roomId = XmlUtils.TryReadChildElementContentAsString(xml, ROOM_ID_ELEMENT);

			Ipid = ipid;
			RoomName = roomName;
			RoomId = roomId;
		}

		#endregion
	}
}
