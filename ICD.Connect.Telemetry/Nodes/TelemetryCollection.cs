using System;
using System.Collections;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class TelemetryCollection : AbstractTelemetryNode, IEnumerable<ITelemetryNode>
	{
		private readonly IcdOrderedDictionary<string, ITelemetryNode> m_Children;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="provider"></param>
		/// <param name="children"></param>
		public TelemetryCollection([NotNull] string name, [NotNull] ITelemetryProvider provider,
		                           [NotNull] IEnumerable<ITelemetryNode> children)
			: base(name, provider)
		{
			if (children == null)
				throw new ArgumentNullException("children");

			m_Children = new IcdOrderedDictionary<string, ITelemetryNode>();
			m_Children.AddRange(children, c => c.Name);
		}

		[CanBeNull]
		public ITelemetryNode GetChildByName([NotNull] string name)
		{
			return m_Children.GetDefault(name);
		}

		#region ICollection

		public IEnumerator<ITelemetryNode> GetEnumerator()
		{
			return m_Children.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
