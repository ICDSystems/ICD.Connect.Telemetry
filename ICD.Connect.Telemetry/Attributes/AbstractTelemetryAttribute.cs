using ICD.Common.Properties;
using ICD.Common.Utils.Attributes;

namespace ICD.Connect.Telemetry.Attributes
{
	[MeansImplicitUse]
	public abstract class AbstractTelemetryAttribute : AbstractIcdAttribute, ITelemetryAttribute
	{
		private readonly string m_Name;

		/// <summary>
		/// Gets the name for the attribute.
		/// </summary>
		public string Name { get { return m_Name; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		protected AbstractTelemetryAttribute(string name)
		{
			m_Name = name;
		}
	}
}