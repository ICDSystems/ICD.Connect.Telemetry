using ICD.Common.Utils;

namespace ICD.Connect.Telemetry.Mappings
{
	public abstract class AbstractTelemetryMappingBase : ITelemetryMapping
	{
		public string TelemetryName { get; set; }

		public override string ToString()
		{
			return new ReprBuilder(this)
				.AppendProperty("TelemetryName", TelemetryName)
				.ToString();
		}
	}
}
