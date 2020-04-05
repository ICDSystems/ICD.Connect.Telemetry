namespace ICD.Connect.Telemetry.Mappings
{
	public abstract class AbstractTelemetryMappingBase : ITelemetryMapping
	{
		public string TelemetrySetName { get; set; }
		public string TelemetryGetName { get; set; }

		public eTelemetryIoMask IoMask
		{
			get
			{
				eTelemetryIoMask output = eTelemetryIoMask.Na;

				if (!string.IsNullOrEmpty(TelemetrySetName))
					output |= eTelemetryIoMask.ServiceToProgram;

				if (!string.IsNullOrEmpty(TelemetryGetName))
					output |= eTelemetryIoMask.ProgramToService;

				return output;
			}
		}

		public override abstract int GetHashCode();
	}
}