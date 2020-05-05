using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class MethodTelemetryItem : AbstractTelemetryItem
	{
		private readonly MethodInfo m_MethodInfo;
		private readonly ParameterInfo m_ParameterInfo;

		#region Properties

		/// <summary>
		/// Gets the info for the method.
		/// </summary>
		public MethodInfo MethodInfo { get { return m_MethodInfo; } }

		/// <summary>
		/// Gets the info for the first method parameter.
		/// </summary>
		public ParameterInfo ParameterInfo { get { return m_ParameterInfo; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="info"></param>
		public MethodTelemetryItem(string name, [NotNull] ITelemetryProvider parent, [NotNull] MethodInfo info)
			: base(name, parent)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			if (info == null)
				throw new ArgumentNullException("info");

			m_MethodInfo = info;
			m_ParameterInfo = info.GetParameters().SingleOrDefault();
		}

		public void Invoke(params object[] parameters)
		{
			try
			{
				m_MethodInfo.Invoke(Parent, parameters);
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException(string.Format("Exception invoking telemetry method Name:{0} Parent:{1}", Name, Parent),
				                            ex);
			}
		}

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Parameter Type", m_ParameterInfo == null ? null : m_ParameterInfo.ParameterType);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			if (ParameterInfo == null)
				yield return new ConsoleCommand("Invoke", "Invokes this parameterless command", () => Invoke());
			else
				yield return
					new GenericConsoleCommand<string>("Invoke", "Invokes this command with the given parameter", a => ConsoleParseCommand(a));
		}

		private void ConsoleParseCommand(string arg)
		{
			if (ParameterInfo == null)
				throw new InvalidOperationException();

			object parameter = AbstractConsoleCommand.Convert(arg, ParameterInfo.ParameterType);
			Invoke(parameter);
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
