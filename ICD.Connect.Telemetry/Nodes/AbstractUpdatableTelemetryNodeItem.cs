using System.Collections.Generic;
using ICD.Connect.API.Commands;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractUpdatableTelemetryNodeItem<T> : AbstractFeedbackTelemetryNodeItem<T>, IUpdatableTelemetryNodeItem
	{

		private readonly PropertyInfo m_PropertyInfo;
		private readonly object m_Parent;

		protected AbstractUpdatableTelemetryNodeItem(string name, object parent, PropertyInfo propertyInfo)
			: base(name)
		{
			m_PropertyInfo = propertyInfo;
			m_Parent = parent;
		}

		public void Update()
		{
			Value = (T)m_PropertyInfo.GetValue(m_Parent, null);
		}

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (var command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Update", "Invokes the update method manually", () => Update());
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		} 

		#endregion
	}
}