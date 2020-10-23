using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class TelemetryProviderNode : AbstractTelemetryNode, IEnumerable<ITelemetryNode>
	{
		private readonly IcdOrderedDictionary<string, ITelemetryNode> m_Children;
		private readonly SafeCriticalSection m_ChildrenSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="provider"></param>
		/// <param name="children"></param>
		public TelemetryProviderNode([NotNull] string name, [NotNull] ITelemetryProvider provider,
		                             [NotNull] IEnumerable<ITelemetryNode> children)
			: base(name, provider)
		{
			if (children == null)
				throw new ArgumentNullException("children");

			m_Children = new IcdOrderedDictionary<string, ITelemetryNode>();
			m_ChildrenSection = new SafeCriticalSection();

			m_Children.AddRange(children, c => c.Name);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			m_ChildrenSection.Execute(() => m_Children.Clear());
		}

		#region Methods

		/// <summary>
		/// Gets the child telemetry nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ITelemetryNode> GetChildren()
		{
			return m_ChildrenSection.Execute(() => m_Children.Values.ToArray());
		}

		#endregion

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
