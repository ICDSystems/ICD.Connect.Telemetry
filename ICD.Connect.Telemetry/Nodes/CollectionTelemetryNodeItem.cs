using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
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
			return GetChildren().Cast<IConsoleNodeBase>();
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

		public override string ToString()
		{
			var x = new ReprBuilder(this);
			x.AppendProperty("Name", Name);
			x.AppendProperty("Parent", Parent);
			return x.ToString();
		}
	}
}