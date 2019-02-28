using System.Collections.Generic;
using System.Linq;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{

	public sealed class CollectionTelemetryNodeItem : AbstractTelemetryCollection, ICollectionTelemetryItem
	{
		public CollectionTelemetryNodeItem(string name, IEnumerable<ITelemetryItem> childNodes)
		{
			Name = name;

			foreach (ITelemetryItem item in childNodes)
			{
				Add(item);
			}
		}

		public string Name { get; private set; }

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
	}
}