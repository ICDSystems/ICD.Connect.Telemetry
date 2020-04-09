using System;
using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.IoT.Ports;

namespace ICD.Connect.Telemetry.MQTT
{
	public sealed class CoreTelemetrySettings
	{
		private const string PATH_PREFIX_ELEMENT = "PathPrefix";
		private const string PORT_ELEMENT = "Port";
		
		public string PathPrefix { get; private set; }

		public IcdMqttClientSettings PortSettings { get; private set; }

		public CoreTelemetrySettings()
		{
			PortSettings = new IcdMqttClientSettings();
		}

		public void Update(CoreTelemetrySettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			PathPrefix = settings.PathPrefix;
			PortSettings = settings.PortSettings ?? new IcdMqttClientSettings();
		}

		public void Clear()
		{
			PathPrefix = string.Empty;
			PortSettings = new IcdMqttClientSettings();
		}

		public void ToXml(IcdXmlTextWriter writer, string element)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			writer.WriteStartElement(element);
			{
				writer.WriteElementString(PATH_PREFIX_ELEMENT, IcdXmlConvert.ToString(PathPrefix));

				PortSettings.ToXml(writer, PORT_ELEMENT);
			}
			writer.WriteEndElement();
		}

		public void ParseXml(string xml)
		{
			PathPrefix = XmlUtils.TryReadChildElementContentAsString(xml, PATH_PREFIX_ELEMENT);

			string child;
			if (XmlUtils.TryGetChildElementAsString(xml, PORT_ELEMENT, out child))
				PortSettings.ParseXml(child);
		}
	}
}