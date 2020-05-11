using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Telemetry.Services;

namespace ICD.Connect.Telemetry.MQTTPro
{
	[KrangSettings("MqttTelemetryServiceProvider", typeof(MqttTelemetryServiceProvider))]
	public sealed class MqttTelemetryServiceProviderSettings : AbstractTelemetryServiceProviderSettings
	{
		private const string ELEMENT_ENABLED = "Enabled";
		private const string ELEMENT_PATH_PREFIX = "PathPrefix";
		private const string ELEMENT_HOSTNAME = "Hostname";
		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_USERNAME = "Username";
		private const string ELEMENT_PASSWORD = "Password";
		private const string ELEMENT_SECURE = "Secure";

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

		/// <summary>
		/// Constructor.
		/// </summary>
		public MqttTelemetryServiceProviderSettings()
		{
			Port = MqttUtils.DEFAULT_PORT;
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_ENABLED, IcdXmlConvert.ToString(Enabled));
			writer.WriteElementString(ELEMENT_PATH_PREFIX, PathPrefix);
			writer.WriteElementString(ELEMENT_HOSTNAME, Hostname);
			writer.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(ELEMENT_USERNAME, Username);
			writer.WriteElementString(ELEMENT_PASSWORD, Password);
			writer.WriteElementString(ELEMENT_SECURE, IcdXmlConvert.ToString(Secure));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Enabled = XmlUtils.TryReadChildElementContentAsBoolean(xml, ELEMENT_ENABLED) ?? false;
			PathPrefix = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PATH_PREFIX);
			Hostname = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_HOSTNAME);
			Port = XmlUtils.TryReadChildElementContentAsUShort(xml, ELEMENT_PORT) ?? MqttUtils.DEFAULT_PORT;
			Username = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_USERNAME);
			Password = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PASSWORD);
			Secure = XmlUtils.TryReadChildElementContentAsBoolean(xml, ELEMENT_SECURE) ?? false;
		}
	}
}
