using System;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Telemetry.MQTT
{
	public sealed class CoreTelemetrySettings
	{
		private const string ELEMENT_ENABLED = "Enabled";
		private const string ELEMENT_PATH_PREFIX = "PathPrefix";
		private const string ELEMENT_HOSTNAME = "Hostname";
		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_USERNAME = "Username";
		private const string ELEMENT_PASSWORD = "Password";
		private const string ELEMENT_SECURE = "Secure";

		#region Properties

		/// <summary>
		/// Gets/sets the enabled state of the core MQTT telemetry.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Gets/sets the path prefix for the core MQTT telemetry topics.
		/// </summary>
		public string PathPrefix { get; set; }

		/// <summary>
		/// Gets/sets the hostname.
		/// </summary>
		public string Hostname { get; set; }

		/// <summary>
		/// Gets/sets the network port.
		/// </summary>
		public ushort Port { get; set; }

		/// <summary>
		/// Gets/sets the username.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Gets/sets the password.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Gets/sets the secure mode.
		/// </summary>
		public bool Secure { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public CoreTelemetrySettings()
		{
			Port = MQTTUtils.DEFAULT_PORT;
		}

		#region Methods

		/// <summary>
		/// Updates this settings instance to match the given settings instance.
		/// </summary>
		/// <param name="settings"></param>
		public void Update(CoreTelemetrySettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			Enabled = settings.Enabled;
			PathPrefix = settings.PathPrefix;
			Hostname = settings.Hostname;
			Port = settings.Port;
			Username = settings.Username;
			Password = settings.Password;
			Secure = settings.Secure;
		}

		/// <summary>
		/// Resets settings to defaults.
		/// </summary>
		public void Clear()
		{
			Enabled = false;
			PathPrefix = null;
			Hostname = null;
			Port = MQTTUtils.DEFAULT_PORT;
			Username = null;
			Password = null;
			Secure = false;
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Writes the settings to XML.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		public void ToXml(IcdXmlTextWriter writer, string element)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			writer.WriteStartElement(element);
			{
				writer.WriteElementString(ELEMENT_ENABLED, IcdXmlConvert.ToString(Enabled));
				writer.WriteElementString(ELEMENT_PATH_PREFIX, PathPrefix);
				writer.WriteElementString(ELEMENT_HOSTNAME, Hostname);
				writer.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(Port));
				writer.WriteElementString(ELEMENT_USERNAME, Username);
				writer.WriteElementString(ELEMENT_PASSWORD, Password);
				writer.WriteElementString(ELEMENT_SECURE, IcdXmlConvert.ToString(Secure));
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Updates this settings instance from the given XML.
		/// </summary>
		/// <param name="xml"></param>
		public void ParseXml(string xml)
		{
			Enabled = XmlUtils.TryReadChildElementContentAsBoolean(xml, ELEMENT_ENABLED) ?? false;
			PathPrefix = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PATH_PREFIX);
			Hostname = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_HOSTNAME);
			Port = XmlUtils.TryReadChildElementContentAsUShort(xml, ELEMENT_PORT) ?? MQTTUtils.DEFAULT_PORT;
			Username = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_USERNAME);
			Password = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PASSWORD);
			Secure = XmlUtils.TryReadChildElementContentAsBoolean(xml, ELEMENT_SECURE) ?? false;
		}

		#endregion
	}
}
