using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Attributes;

namespace ICD.Connect.Telemetry.Attributes
{
	[MeansImplicitUse]
	public abstract class AbstractTelemetryAttribute : AbstractIcdAttribute, ITelemetryAttribute
	{
		private readonly string m_Name;
		private string m_FormattedName;

		public string Name { get { return m_FormattedName = (m_FormattedName ?? FormatName(m_Name)); } }

		protected AbstractTelemetryAttribute(string name)
		{
			m_Name = name;
		}

		/// <summary>
		/// Capitalizes the first character of each word and removes all whitespace.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static string FormatName(string name)
		{
			if (name == null)
				return string.Empty;

			name = StringUtils.NiceName(name);
			name = StringUtils.ToTitleCase(name);
			return StringUtils.RemoveWhitespace(name);
		}
	}
}