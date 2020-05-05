using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Attributes;

namespace ICD.Connect.Telemetry.Attributes
{
	[MeansImplicitUse]
	public abstract class AbstractTelemetryAttribute : AbstractIcdAttribute, ITelemetryAttribute
	{
		[NotNull]
		private readonly string m_Name;

		/// <summary>
		/// Gets the name for the attribute.
		/// </summary>
		[NotNull]
		public string Name { get { return m_Name; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		protected AbstractTelemetryAttribute([NotNull] string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("Name must not be null or empty");

			m_Name = name;
		}
	}
}