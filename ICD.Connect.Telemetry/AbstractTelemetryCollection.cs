using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry
{
	public abstract class AbstractTelemetryCollection : ITelemetryCollection
	{
		private readonly List<ITelemetryItem> m_Children;

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractTelemetryCollection()
		{
			m_Children = new List<ITelemetryItem>();
		}

		public IEnumerable<ITelemetryItem> GetChildren()
		{
			return m_Children.ToList();
		}

		public IEnumerable<TChildren> GetChildren<TChildren>()
			where TChildren : ITelemetryItem
		{
			return m_Children.OfType<TChildren>().ToList();
		}

		public virtual ITelemetryItem GetChildByName(string name)
		{
			return m_Children.FirstOrDefault(child => child.Name == name);
		}

		#region ICollection

		public bool IsReadOnly { get { return false; } }
		public int Count { get { return m_Children.Count; } }

		public virtual void Add(ITelemetryItem item)
		{
			if (!m_Children.Contains(item))
				m_Children.Add(item);
		}

		public virtual bool Remove(ITelemetryItem item)
		{
			return m_Children.Remove(item);
		}

		public virtual void Clear()
		{
			m_Children.Clear();
		}

		public virtual bool Contains(ITelemetryItem item)
		{
			return m_Children.Contains(item);
		}

		public virtual void CopyTo(ITelemetryItem[] array, int index)
		{
			m_Children.CopyTo(array, index);
		}

		public virtual IEnumerator<ITelemetryItem> GetEnumerator()
		{
			return m_Children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}