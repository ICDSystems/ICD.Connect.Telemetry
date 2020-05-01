using System;
using ICD.Connect.Telemetry.Mappings;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.MQTT.Mappings
{
	public class MqttTelemetryMapping : AbstractTelemetryMappingBase
	{
		public string Path { get; private set; }

		public ITelemetryItem Item { get; private set; }

		public MqttTelemetryMapping(ITelemetryItem item, string path)
		{
			if(item.Parent == null)
				throw new InvalidOperationException("Cannot generate mqtt mapping with null parent on telemetry item");

			Item = item;
			Path = path;
		}
	}
}