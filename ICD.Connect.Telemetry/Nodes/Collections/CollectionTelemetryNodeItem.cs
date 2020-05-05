using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes.Collections
{
	public sealed class CollectionTelemetryNodeItem : AbstractTelemetryCollection, ICollectionTelemetryItem
	{
		public string Name { get; private set; }

		public ITelemetryProvider Parent { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="childNodes"></param>
		public CollectionTelemetryNodeItem(string name, ITelemetryProvider parent, IEnumerable<ITelemetryItem> childNodes)
		{
			Name = name;
			Parent = parent;

			foreach (ITelemetryItem item in childNodes)
				Add(item);
		}

		public override string ToString()
		{
			return new ReprBuilder(this)
				.AppendProperty("Name", Name)
				.AppendProperty("Parent", Parent)
				.ToString();
		}

		#region Console

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return "Telemetry Item"; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield break;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Name", Name);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield break;
		}

		#endregion
	}
}
