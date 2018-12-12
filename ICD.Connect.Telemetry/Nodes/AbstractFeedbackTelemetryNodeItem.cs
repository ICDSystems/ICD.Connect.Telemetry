using System;
using System.Collections.Generic;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractFeedbackTelemetryNodeItem<T> : AbstractTelemetryNodeItemBase, IFeedbackTelemetryItem<T>
	{
		object IFeedbackTelemetryItem.Value { get { return Value; } }
		public T Value { get; protected set; }
		public Type ValueType { get { return typeof(T); } }

		protected AbstractFeedbackTelemetryNodeItem(string name) 
			: base(name)
		{

		}

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);
			addRow("Value Type", ValueType.ToString());
			addRow("Value", Value.ToString());
		}

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (var command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Get Value",
			                                "Prints the value for the telemetry item",
			                                () =>
			                                ServiceProvider.GetService<ILoggerService>()
			                                               .AddEntry(eSeverity.Informational, Value.ToString()));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}