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

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (TelemetrySetName == null ? 0 : TelemetrySetName.GetHashCode());
				hash = hash * 23 + (TelemetryGetName == null ? 0 : TelemetryGetName.GetHashCode());
				hash = hash * 23 + (Path == null ? 0 : Path.GetHashCode());
				return hash;
			}
		}
	}
}