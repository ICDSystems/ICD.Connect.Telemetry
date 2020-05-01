using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractUpdateableTelemetryNodeItem<T> : AbstractFeedbackTelemetryNodeItem<T>, IUpdatableTelemetryNodeItem
	{
		public event EventHandler OnValueChanged;

		private T m_CachedValue;

		[CanBeNull]
		public string SetTelemetryName { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="setTelemetry"></param>
		protected AbstractUpdateableTelemetryNodeItem(string name, ITelemetryProvider parent, PropertyInfo propertyInfo, string setTelemetry)
			: base(name, parent, propertyInfo)
		{
			SetTelemetryName = setTelemetry;

			Update();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public override void Dispose()
		{
			OnValueChanged = null;

			base.Dispose();
		}

		#region Methods

		public void Update()
		{
			T newValue = Value;

			if (EqualityComparer<T>.Default.Equals(m_CachedValue, newValue))
				return;

			m_CachedValue = newValue;

			OnValueChanged.Raise(this);
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
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