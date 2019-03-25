using ICD.Common.Properties;
using ICD.Common.Utils.Attributes;

namespace ICD.Connect.Telemetry.Attributes
{
	[MeansImplicitUse]
	public abstract class AbstractTelemetryAttribute : AbstractIcdAttribute, ITelemetryAttribute
	{
		private readonly string m_Name;

		public string Name { get { return m_Name; } }

		protected AbstractTelemetryAttribute(string name)
		{
			m_Name = name;
		}
	}
}