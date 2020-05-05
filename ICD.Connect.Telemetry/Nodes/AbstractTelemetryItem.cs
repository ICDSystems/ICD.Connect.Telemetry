using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractTelemetryItem : ITelemetryItem, IDisposable
	{
		/// <summary>
		/// Gets the name of the telemetry item.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the provider that this telemetry item is attached to.
		/// </summary>
		public ITelemetryProvider Parent { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		protected AbstractTelemetryItem([NotNull] string name, [NotNull] ITelemetryProvider parent)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("Name must not be null or empty", "name");

			if (parent == null)
				throw new ArgumentNullException("parent");

			Name = name;
			Parent = parent;
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
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
			addRow("Parent", Parent);
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
	}
}