namespace ICD.Connect.Telemetry.Attributes
{
	public abstract class AbstractUpdateablePropertyTelemetryAttribute : AbstractPropertyTelemetryAttribute
	{
		private readonly string m_SetTelemetryName;

		public string SetTelemetryName{ get { return m_SetTelemetryName; }}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="setTelemetryName"></param>
		protected AbstractUpdateablePropertyTelemetryAttribute(string name, string setTelemetryName)
			: base(name)
		{
			m_SetTelemetryName = setTelemetryName;
		}
	}
}