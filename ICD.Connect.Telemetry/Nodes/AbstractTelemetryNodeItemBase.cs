using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractTelemetryNodeItemBase : ITelemetryItem, ITelemetryCollection, IDisposable
	{
		private readonly List<ITelemetryItem> m_Children;

		public string Name { get; private set; }

		protected AbstractTelemetryNodeItemBase(string name)
		{
			Name = name;

			m_Children = new List<ITelemetryItem>();
		}

		public void Dispose()
		{
			m_Children.Clear();
		}

		public virtual IEnumerable<ITelemetryItem> GetChildren()
		{
			return m_Children.ToList();
		}

		public virtual IEnumerable<TChildren> GetChildren<TChildren>()
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

		#region Console
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get; private set; }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get; private set; }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}