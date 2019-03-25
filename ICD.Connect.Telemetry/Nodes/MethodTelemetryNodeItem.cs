using System;
using System.Collections.Generic;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class MethodTelemetryNodeItem : AbstractTelemetryNodeItemBase, IManagementTelemetryItem
	{
		private readonly MethodInfo m_MethodInfo;

		public int ParameterCount { get { return ParameterTypes.Length; } }
		public Type[] ParameterTypes { get; private set; }

		public MethodTelemetryNodeItem(string name, ITelemetryProvider parent ,MethodInfo info) : base(name, parent)
		{
			m_MethodInfo = info;

			ParameterTypes = m_MethodInfo.GetParameters().Select(p => (Type)p.ParameterType).ToArray();
		}

		
		public void Invoke(params object[] parameters)
		{
			m_MethodInfo.Invoke(Parent, parameters);
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Parameter Count", ParameterCount);

			if(ParameterCount == 0)
				return;

			addRow("Parameter Types", "");
			var index = 1;
			foreach (var type in ParameterTypes)
			{
				addRow(string.Format("Parameter {0}", index), type.ToString());
				index ++;
			}
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (var command in GetBaseConsoleCommands())
				yield return command;

			if(ParameterCount == 0)
				yield return new ConsoleCommand("Invoke", "Invokes this parameterless command", ()=> Invoke(new object[0]));
			else
				yield return new ParamsConsoleCommand("Invoke", "Invokes this command with the given parameters", a => ConsoleParseCommand(a));
		}

		private void ConsoleParseCommand(IEnumerable<string> args)
		{
			List<object> parameters = new List<object>();

			var index = 0;
			foreach (var arg in args)
			{
				parameters.Add(AbstractConsoleCommand.Convert(arg, ParameterTypes[index]));
				index++;
			}

			Invoke(parameters.ToArray());
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		} 
	}
}