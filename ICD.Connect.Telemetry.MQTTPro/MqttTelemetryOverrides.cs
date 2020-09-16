using ICD.Common.Utils.Xml;

namespace ICD.Connect.Telemetry.MQTTPro
{
	public sealed class MqttTelemetryOverrides
	{
		private const string ELEMENT_HOSTNAME = "Hostname";
		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_PROXY_HOSTNAME = "ProxyHostname";
		private const string ELEMENT_PROXY_PORT = "ProxyPort";
		private const string ELEMENT_DISABLE = "Disable";

		/// <summary>
		/// Gets/sets the hostname.
		/// </summary>
		public string Hostname { get; set; }

		/// <summary>
		/// Gets/sets the network port.
		/// </summary>
		public ushort? Port { get; set; }

		/// <summary>
		/// Gets/sets the proxy hostname.
		/// </summary>
		public string ProxyHostname { get; set; }

		/// <summary>
		/// Gets/sets the proxy port.
		/// </summary>
		public ushort? ProxyPort { get; set; }

		/// <summary>
		/// Gets/sets the disabled state.
		/// </summary>
		public bool? Disable { get; set; }

		/// <summary>
		/// Parses the MqttTelemetryOverrides XML element.
		/// </summary>
		/// <param name="xml"></param>
		public void ParseXml(string xml)
		{
			Hostname = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_HOSTNAME);
			Port = XmlUtils.TryReadChildElementContentAsUShort(xml, ELEMENT_PORT);
			ProxyHostname = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PROXY_HOSTNAME);
			ProxyPort = XmlUtils.TryReadChildElementContentAsUShort(xml, ELEMENT_PROXY_PORT);
			Disable = XmlUtils.TryReadChildElementContentAsBoolean(xml, ELEMENT_DISABLE);
		}

		/// <summary>
		/// Clears the configured properties.
		/// </summary>
		public void Clear()
		{
			Hostname = null;
			Port = null;
			ProxyHostname = null;
			ProxyPort = null;
			Disable = null;
		}
	}
}
