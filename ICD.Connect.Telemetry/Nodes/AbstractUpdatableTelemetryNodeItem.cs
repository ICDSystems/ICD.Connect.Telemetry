﻿using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
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

		private readonly object m_Parent;
		
		protected AbstractUpdatableTelemetryNodeItem(string name, object parent, PropertyInfo propertyInfo)
			: base(name, propertyInfo)
		{
			m_Parent = parent;
		}

		public override void Dispose()
		{
			OnValueChanged = null;

			base.Dispose();
		}

		public event EventHandler OnValueChanged;

		public void Update()
		{
			T newValue = (T)PropertyInfo.GetValue(m_Parent, null);

			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (Value == null && newValue == null)
				return;

			if (newValue != null && newValue.Equals(Value))
				return;
			// ReSharper restore CompareNonConstrainedGenericWithNull

			Value = newValue;

			OnValueChanged.Raise(this);
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