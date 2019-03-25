using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractTelemetryNodeItemBase : ITelemetryItem, IDisposable
	{
		public string Name { get; private set; }

		public ITelemetryProvider Parent { get; private set; }

		protected AbstractTelemetryNodeItemBase(string name, ITelemetryProvider parent)
		{
			Name = name;
			Parent = parent;
		}

		public virtual void Dispose()
		{
			
		}

		#region Console
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return Name; }}

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return "Telemetry Item"; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield break;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public virtual void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Name", Name);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleCommand> GetConsoleCommands()
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