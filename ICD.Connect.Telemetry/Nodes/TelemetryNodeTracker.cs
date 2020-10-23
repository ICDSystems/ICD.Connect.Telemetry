using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Telemetry.Nodes
{
	/// <summary>
	/// Tracks the changes in the telemetry tree.
	/// </summary>
	public sealed class TelemetryNodeTracker
	{
		public delegate void NodePathCallback([NotNull] ITelemetryNode node, [NotNull] IEnumerable<ITelemetryNode> path);

		/// <summary>
		/// Raised when a node is added to the tree.
		/// </summary>
		public event NodePathCallback OnNodeAdded;

		/// <summary>
		/// Raised when a node is removed from the tree.
		/// </summary>
		public event NodePathCallback OnNodeRemoved;

		private readonly Dictionary<ITelemetryNode, IcdHashSet<ITelemetryNode>> m_ParentToChildren;
		private readonly Dictionary<ITelemetryNode, ITelemetryNode> m_ChildToParent; 
		private readonly SafeCriticalSection m_Section;
		
		[CanBeNull]
		private TelemetryProviderNode m_Root;

		/// <summary>
		/// Constructor.
		/// </summary>
		public TelemetryNodeTracker()
		{
			m_ParentToChildren = new Dictionary<ITelemetryNode, IcdHashSet<ITelemetryNode>>();
			m_ChildToParent = new Dictionary<ITelemetryNode, ITelemetryNode>();
			m_Section = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Clears the existing telemetry bindings and rebuilds bindings for the given root node.
		/// </summary>
		/// <param name="root"></param>
		public void SetRootNode([NotNull] TelemetryProviderNode root)
		{
			if (root == null)
				throw new ArgumentNullException("root");

			if (root == m_Root)
				return;

			Clear();

			m_Root = root;
			AddRootNodeRecursive(m_Root);
		}

		/// <summary>
		/// Clears the current root node and the telemetry bindings.
		/// </summary>
		public void Clear()
		{
			if (m_Root == null)
				return;

			RemoveRootNodeRecursive(m_Root);
			m_Root = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the path from the root to the given node inclusive.
		/// </summary>
		/// <param name="node"></param>
		[NotNull]
		private IEnumerable<ITelemetryNode> GetPath([NotNull] ITelemetryNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (m_Root == null)
				throw new InvalidOperationException("No root node configured");

			return m_Section.Execute(() => RecursionUtils.GetPath(node, m_Root, m_ChildToParent).Reverse().ToArray());
		}

		/// <summary>
		/// Disposes the bindings for the root node recursively.
		/// </summary>
		/// <param name="rootNode"></param>
		private void RemoveRootNodeRecursive([NotNull] TelemetryProviderNode rootNode)
		{
			if (rootNode == null)
				throw new ArgumentNullException("rootNode");

			RemoveNodeRecursive(rootNode, new ITelemetryNode[] { rootNode });
		}

		/// <summary>
		/// Disposes the bindings for the given node recursively.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="path">The path from the root node to the given node inclusive.</param>
		private void RemoveNodeRecursive([NotNull] ITelemetryNode node, IEnumerable<ITelemetryNode> path)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			IList<ITelemetryNode> pathList = path as IList<ITelemetryNode> ?? path.ToArray();

			// First remove the children
			IEnumerable<ITelemetryNode> cachedChildren =
				m_Section.Execute(() => m_ParentToChildren.GetDefault(node)) ?? Enumerable.Empty<ITelemetryNode>();

			foreach (ITelemetryNode child in cachedChildren.ToArray())
				RemoveNodeRecursive(child, pathList.Append(child));

			// Finally remove this node
			RemoveNode(node, pathList);
		}

		/// <summary>
		/// Disposes the bindings for the given node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="path"></param>
		private void RemoveNode([NotNull] ITelemetryNode node, [NotNull] IEnumerable<ITelemetryNode> path)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (path == null)
				throw new ArgumentNullException("path");

			IList<ITelemetryNode> pathList = path as IList<ITelemetryNode> ?? path.ToArray();

			m_Section.Enter();

			try
			{
				Unsubscribe(node);

				// Remove node and immediate children from cache
				IcdHashSet<ITelemetryNode> children;
				if (m_ParentToChildren.Remove(node, out children))
					children.ForEach(c => m_ChildToParent.Remove(c));
				m_ChildToParent.Remove(node);

				// Remove node from parent cache
				ITelemetryNode parent = pathList.Count > 1 ? pathList[pathList.Count - 2] : null;
				if (parent != null)
				{
					IcdHashSet<ITelemetryNode> parentChildren;
					if (m_ParentToChildren.TryGetValue(parent, out parentChildren))
						parentChildren.Remove(node);
				}
			}
			finally
			{
				m_Section.Leave();
			}

			NodePathCallback handler = OnNodeRemoved;
			if (handler != null)
				handler(node, pathList);
		}

		/// <summary>
		/// Updates the bindings for the root node recursively.
		/// </summary>
		/// <param name="rootNode"></param>
		private void AddRootNodeRecursive([NotNull] TelemetryProviderNode rootNode)
		{
			if (rootNode == null)
				throw new ArgumentNullException("rootNode");

			AddNodeRecursive(rootNode, new ITelemetryNode[] {rootNode});
		}

		/// <summary>
		/// Updates the bindings for the given node in the telemetry tree recursively.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="path">The path from the root node to the given node inclusive.</param>
		private void AddNodeRecursive([NotNull] ITelemetryNode node, [NotNull] IEnumerable<ITelemetryNode> path)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (path == null)
				throw new ArgumentNullException("path");

			IList<ITelemetryNode> pathToNodeList = path as IList<ITelemetryNode> ?? path.ToArray();

			// Adding telemetry breadth-first
			// High level items (system online, version, etc) are more important than low level details.
			RecursionUtils.BreadthFirstSearchPaths(node, n => n.GetChildren())
						  .ForEach(kvp => AddNode(kvp.Key, pathToNodeList.Concat(kvp.Value.Skip(1))));
		}

		/// <summary>
		/// Updates the bindings for the given node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="path">The path from the root node and given node inclusive.</param>
		private void AddNode([NotNull] ITelemetryNode node, [NotNull] IEnumerable<ITelemetryNode> path)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (path == null)
				throw new ArgumentNullException("path");

			IList<ITelemetryNode> pathList = path as IList<ITelemetryNode> ?? path.ToArray();

			m_Section.Enter();

			try
			{
				if (m_ParentToChildren.ContainsKey(node))
					return;

				IcdHashSet<ITelemetryNode> children = node.GetChildren().ToIcdHashSet();

				// Add node and immediate children to cache
				m_ParentToChildren.Add(node, children);
				children.ForEach(c => m_ChildToParent.Add(c, node));

				// Add node to parent cache
				ITelemetryNode parent = pathList.Count > 1 ? pathList[pathList.Count - 2] : null;
				if (parent != null)
				{
					m_ParentToChildren.GetOrAddNew(parent).Add(node);
					m_ChildToParent[node] = parent;
				}

				Subscribe(node);
			}
			finally
			{
				m_Section.Leave();
			}

			NodePathCallback handler = OnNodeAdded;
			if (handler != null)
				handler(node, pathList);
		}

		/// <summary>
		/// Adds/removes new child nodes for the given collection.
		/// </summary>
		/// <param name="collection"></param>
		private void UpdateCollection([NotNull] TelemetryCollection collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			m_Section.Enter();

			try
			{
				IcdHashSet<ITelemetryNode> expected;
				if (!m_ParentToChildren.TryGetValue(collection, out expected))
					throw new ArgumentException("Unexpected collection", "collection");

				IcdHashSet<ITelemetryNode> actual = collection.GetChildren().ToIcdHashSet();
				ITelemetryNode[] path = GetPath(collection).ToArray();

				// Remove nodes that no longer exist
				foreach (ITelemetryNode removed in expected.Where(e => !actual.Contains(e)).ToArray())
					RemoveNodeRecursive(removed, path.Append(removed));

				// Add new nodes
				foreach (ITelemetryNode added in actual.Where(a => !expected.Contains(a)).ToArray())
					AddNodeRecursive(added, path.Append(added));
			}
			finally
			{
				m_Section.Leave();
			}
		}

		#endregion

		#region Node Callbacks

		/// <summary>
		/// Subscribe to the telemetry node.
		/// </summary>
		/// <param name="node"></param>
		private void Subscribe([NotNull] ITelemetryNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			TelemetryCollection collection = node as TelemetryCollection;
			if (collection != null)
				collection.OnCollectionChanged += ObservableOnCollectionChanged;
		}

		/// <summary>
		/// Unsubscribe from the telemetry node.
		/// </summary>
		/// <param name="node"></param>
		private void Unsubscribe([NotNull] ITelemetryNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			TelemetryCollection collection = node as TelemetryCollection;
			if (collection != null)
				collection.OnCollectionChanged -= ObservableOnCollectionChanged;
		}

		/// <summary>
		/// Called when a telemetry collection contents change.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ObservableOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateCollection((TelemetryCollection)sender);
		}

		#endregion
	}
}
