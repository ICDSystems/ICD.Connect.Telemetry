using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class PropertyTelemetryItem : AbstractTelemetryItem
	{
		/// <summary>
		/// Raised when the underlying property value changes.
		/// </summary>
		public event EventHandler OnValueChanged;

		private readonly PropertyInfo m_PropertyInfo;
		private readonly MethodInfo m_MethodInfo;
		private readonly EventInfo m_EventInfo;
		private readonly Delegate m_Delegate;

		private object m_CachedValue;

		#region Properties

		/// <summary>
		/// Helper to get the EventCallback method in this class.
		/// </summary>
		[NotNull]
		private MethodInfo CallbackMethodInfo
		{
			get
			{
				MethodInfo output = GetType()
#if SIMPLSHARP
					.GetCType()
#else
					.GetTypeInfo()
#endif
					.GetMethod("EventCallback", BindingFlags.NonPublic | BindingFlags.Instance);

				if (output == null)
					throw new InvalidProgramException();

				return output;
			}
		}

		/// <summary>
		/// Gets the value from the property.
		/// </summary>
		public object Value { get { return m_PropertyInfo.GetValue(Parent, null); } }

		/// <summary>
		/// Gets the property info for the property.
		/// </summary>
		public PropertyInfo PropertyInfo { get { return m_PropertyInfo; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="methodInfo"></param>
		/// <param name="eventInfo"></param>
		public PropertyTelemetryItem([NotNull] string name, [NotNull] ITelemetryProvider parent,
		                             [NotNull] PropertyInfo propertyInfo, [CanBeNull] MethodInfo methodInfo,
		                             [CanBeNull] EventInfo eventInfo)
			: base(name, parent)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (parent == null)
				throw new ArgumentNullException("parent");

			m_PropertyInfo = propertyInfo;
			m_MethodInfo = methodInfo;
			m_EventInfo = eventInfo;

			// Subscribe to the event.
			if (m_EventInfo != null)
				m_Delegate = ReflectionUtils.SubscribeEvent(parent, m_EventInfo, this, CallbackMethodInfo);

			Update();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public override void Dispose()
		{
			OnValueChanged = null;

			base.Dispose();

			ReflectionUtils.UnsubscribeEvent(Parent, m_EventInfo, m_Delegate);
		}

		#region Methods

		/// <summary>
		/// Updates the cached value and raises the changed event if the value has changed.
		/// </summary>
		public void Update()
		{
			object currentValue = Value;
			if (EqualityComparer<object>.Default.Equals(m_CachedValue, currentValue))
				return;

			m_CachedValue = currentValue;

			OnValueChanged.Raise(this);
		}

		#endregion

		#region Provider Callbacks

		/// <summary>
		/// Called when the provider event is raised.
		/// </summary>
		[UsedImplicitly]
		private void EventCallback(object sender, object args)
		{
			Update();
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Property Type", PropertyInfo.PropertyType);
			addRow("Property Value", Value);
		}

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
