using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractTelemetryNode : ITelemetryNode, IDisposable
	{
		/// <summary>
		/// Gets the name of the telemetry node.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the provider that this telemetry node is attached to.
		/// </summary>
		[NotNull]
		public ITelemetryProvider Provider { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="provider"></param>
		protected AbstractTelemetryNode([NotNull] string name, [NotNull] ITelemetryProvider provider)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("Name must not be null or empty", "name");

			if (provider == null)
				throw new ArgumentNullException("provider");

			Name = name;
			Provider = provider;
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
				.AppendProperty("Provider", Provider)
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
		public string ConsoleHelp { get { return "Telemetry Node"; } }

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
			addRow("Provider", Provider);
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