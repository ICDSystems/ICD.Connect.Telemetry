using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Nodes;
using ICD.Connect.Telemetry.Providers;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class TelemetryCollection : AbstractTelemetryNode, IEnumerable<ITelemetryNode>, INotifyCollectionChanged
	{
		/// <summary>
		/// Raised when the contents of the collection change, or the order changes.
		/// </summary>
		public event EventHandler OnCollectionChanged;

		private readonly Func<TelemetryCollection, IEnumerable<ITelemetryNode>> m_GetTelemetryNodes;
		private readonly IcdSortedDictionary<string, ITelemetryNode> m_TelemetryNodes; 
		private readonly SafeCriticalSection m_TelemetryNodesSection;

		[NotNull]
		private readonly PropertyInfo m_PropertyInfo;

		#region Properties

		/// <summary>
		/// Gets the property info for the property.
		/// </summary>
		[NotNull]
		public PropertyInfo PropertyInfo { get { return m_PropertyInfo; } }

		/// <summary>
		/// Gets the value from the property.
		/// </summary>
		public IEnumerable Value { get { return (IEnumerable)m_PropertyInfo.GetValue(Provider, null); } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="provider"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="getTelemetryNodes"></param>
		public TelemetryCollection([NotNull] string name, [NotNull] ITelemetryProvider provider,
		                           [NotNull] PropertyInfo propertyInfo,
		                           [NotNull] Func<TelemetryCollection, IEnumerable<ITelemetryNode>> getTelemetryNodes)
			: base(name, provider)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException("propertyInfo");

			if (getTelemetryNodes == null)
				throw new ArgumentNullException("getTelemetryNodes");

			if (!typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
				throw new InvalidProgramException(string.Format("{0}.{1} is not an IEnumerable", provider.GetType().Name,
				                                                propertyInfo.Name));

			m_PropertyInfo = propertyInfo;
			m_GetTelemetryNodes = getTelemetryNodes;
			m_TelemetryNodes = new IcdSortedDictionary<string, ITelemetryNode>();
			m_TelemetryNodesSection = new SafeCriticalSection();

			Subscribe(Value);

			UpdateTelemetryNodes();
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		public override void Dispose()
		{
			OnCollectionChanged = null;
			Unsubscribe(Value);

			base.Dispose();

			m_TelemetryNodesSection.Execute(() => m_TelemetryNodes.Clear());
		}

		/// <summary>
		/// Gets the child telemetry nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ITelemetryNode> GetChildren()
		{
			return m_TelemetryNodesSection.Execute(() => m_TelemetryNodes.Values.ToArray());
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Updates the collection of telemetry nodes to match the wrapped collection.
		/// </summary>
		private void UpdateTelemetryNodes()
		{
			IcdHashSet<ITelemetryNode> currentNodes = m_GetTelemetryNodes(this).ToIcdHashSet();

			m_TelemetryNodesSection.Enter();

			try
			{
				if (currentNodes.ScrambledEquals(m_TelemetryNodes.Values))
					return;

				m_TelemetryNodes.Clear();

				foreach (ITelemetryNode node in currentNodes)
				{
					if (m_TelemetryNodes.ContainsKey(node.Name))
						throw new InvalidOperationException(string.Format(
							                                    "{0} {1} already contains a telemetry node with name {2}",
							                                    GetType().Name, Name, node.Name));
					m_TelemetryNodes.Add(node.Name, node);
				}
			}
			finally
			{
				m_TelemetryNodesSection.Leave();
			}

			OnCollectionChanged.Raise(this);
		}

		#endregion

		#region Collection Callbacks

		/// <summary>
		/// Subscribe to the underlying collection events.
		/// </summary>
		/// <param name="collection"></param>
		private void Subscribe(IEnumerable collection)
		{
			if (collection == null)
				return;

			INotifyCollectionChanged observable = collection as INotifyCollectionChanged;
			if (observable != null)
				observable.OnCollectionChanged += ObservableOnCollectionChanged;
		}

		/// <summary>
		/// Unsubscribe from the underlying collection events.
		/// </summary>
		/// <param name="collection"></param>
		private void Unsubscribe(IEnumerable collection)
		{
			if (collection == null)
				return;

			INotifyCollectionChanged observable = collection as INotifyCollectionChanged;
			if (observable != null)
				observable.OnCollectionChanged -= ObservableOnCollectionChanged;
		}

		/// <summary>
		/// Called when the underlying collection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ObservableOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateTelemetryNodes();
		}

		#endregion

		#region IEnumerable

		public IEnumerator<ITelemetryNode> GetEnumerator()
		{
			return GetChildren().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Property", m_PropertyInfo.Name);
		}

		#endregion
	}
}
