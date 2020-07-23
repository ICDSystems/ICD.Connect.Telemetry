using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Telemetry.Services;

namespace ICD.Connect.Telemetry.MQTTPro
{
	[KrangSettings("MqttTelemetryServiceProvider", typeof(MqttTelemetryServiceProvider))]
	public sealed class MqttTelemetryServiceProviderSettings : AbstractTelemetryServiceProviderSettings
	{
		private const string ELEMENT_PATH_PREFIX = "PathPrefix";
		private const string ELEMENT_HOSTNAME = "Hostname";
		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_PROXY_HOSTNAME = "ProxyHostname";
		private const string ELEMENT_PROXY_PORT = "ProxyPort";
		private const string ELEMENT_USERNAME = "Username";
		private const string ELEMENT_PASSWORD = "Password";
		private const string ELEMENT_SECURE = "Secure";
		private const string ELEMENT_CA_CERT_PATH = "CaCertPath";

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
		/// Gets/sets the proxy hostname.
		/// </summary>
		public string ProxyHostname { get; set; }

		/// <summary>
		/// Gets/sets the proxy port.
		/// </summary>
		public ushort ProxyPort { get; set; }

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
		/// Gets/sets the path to the certificate-authority certificate.
		/// </summary>
		public string CaCertPath { get; set; }

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

			writer.WriteElementString(ELEMENT_PATH_PREFIX, PathPrefix);
			writer.WriteElementString(ELEMENT_HOSTNAME, Hostname);
			writer.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(ELEMENT_PROXY_HOSTNAME, ProxyHostname);
			writer.WriteElementString(ELEMENT_PROXY_PORT, IcdXmlConvert.ToString(ProxyPort));
			writer.WriteElementString(ELEMENT_USERNAME, Username);
			writer.WriteElementString(ELEMENT_PASSWORD, Password);
			writer.WriteElementString(ELEMENT_SECURE, IcdXmlConvert.ToString(Secure));
			writer.WriteElementString(ELEMENT_CA_CERT_PATH, CaCertPath);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			PathPrefix = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PATH_PREFIX);
			Hostname = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_HOSTNAME);
			Port = XmlUtils.TryReadChildElementContentAsUShort(xml, ELEMENT_PORT) ?? MqttUtils.DEFAULT_PORT;
			ProxyHostname = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PROXY_HOSTNAME);
			ProxyPort = XmlUtils.TryReadChildElementContentAsUShort(xml, ELEMENT_PROXY_PORT) ?? 0;
			Username = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_USERNAME);
			Password = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PASSWORD);
			Secure = XmlUtils.TryReadChildElementContentAsBoolean(xml, ELEMENT_SECURE) ?? false;
			CaCertPath = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_CA_CERT_PATH);
		}
	}
}
