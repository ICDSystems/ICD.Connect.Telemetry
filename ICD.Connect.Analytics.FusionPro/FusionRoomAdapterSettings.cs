using System;
using ICD.Common.Attributes.Properties;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Analytics.FusionPro
{
	/// <summary>
	/// Settings for the FusionRoomAdapter originator.
	/// </summary>
	public sealed class FusionRoomAdapterSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "FusionRoom";

		private const string IPID_ELEMENT = "IPID";
		private const string ROOM_NAME_ELEMENT = "RoomName";
		private const string ROOM_ID_ELEMENT = "RoomId";
		private const string FUSION_SIGS_ELEMENT = "FusionSigsPath";

		private const string DEFAULT_FUSION_SIGS_PATH = "FusionSigs.xml";

		private string m_RoomId;
		private string m_FusionSigsPath;

		#region Properties

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(FusionRoomAdapter); } }

		[SettingsProperty(SettingsProperty.ePropertyType.Ipid)]
		public byte Ipid { get; set; }

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

		/// <summary>
		/// Gets/sets the fusion sigs path. Returns the default path if it has not been set.
		/// </summary>
		public string FusionSigsPath
		{
			get
			{
				if (string.IsNullOrEmpty(m_FusionSigsPath))
					m_FusionSigsPath = DEFAULT_FUSION_SIGS_PATH;
				return m_FusionSigsPath;
			}
			set { m_FusionSigsPath = value; }
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

			writer.WriteElementString(IPID_ELEMENT, StringUtils.ToIpIdString(Ipid));
			writer.WriteElementString(ROOM_NAME_ELEMENT, RoomName);
			writer.WriteElementString(ROOM_ID_ELEMENT, RoomId);
			writer.WriteElementString(FUSION_SIGS_ELEMENT, FusionSigsPath);
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static FusionRoomAdapterSettings FromXml(string xml)
		{
			byte ipid = XmlUtils.ReadChildElementContentAsByte(xml, IPID_ELEMENT);
			string roomName = XmlUtils.ReadChildElementContentAsString(xml, ROOM_NAME_ELEMENT);
			string roomId = XmlUtils.TryReadChildElementContentAsString(xml, ROOM_ID_ELEMENT);
			string fusionSigsPath = XmlUtils.TryReadChildElementContentAsString(xml, FUSION_SIGS_ELEMENT);

			FusionRoomAdapterSettings output = new FusionRoomAdapterSettings
			{
				Ipid = ipid,
				RoomName = roomName,
				RoomId = roomId,
				FusionSigsPath = fusionSigsPath
			};

			ParseXml(output, xml);
			return output;
		}

		#endregion
	}
}
