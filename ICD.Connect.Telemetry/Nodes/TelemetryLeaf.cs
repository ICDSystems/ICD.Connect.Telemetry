using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Telemetry.Providers;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class TelemetryLeaf : AbstractTelemetryNode
	{
		/// <summary>
		/// Raises a new value from the telemetry to the consumer.
		/// </summary>
		public event EventHandler<GenericEventArgs<object>> OnValueRaised;

		[CanBeNull] private readonly PropertyInfo m_PropertyInfo;
		[CanBeNull] private readonly MethodInfo m_MethodInfo;
		[CanBeNull] private readonly EventInfo m_EventInfo;
		[CanBeNull] private readonly Delegate m_Delegate;

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
					throw new InvalidProgramException("Failed to find MethodInfo with name EventCallback");

				return output;
			}
		}

		/// <summary>
		/// Gets the value from the property.
		/// </summary>
		[CanBeNull]
		public object Value
		{
			get
			{
				if (m_PropertyInfo == null)
					throw new InvalidOperationException("Unable to get value for telemetry with no property");

				return m_PropertyInfo.GetValue(Provider, null);
			}
		}

		/// <summary>
		/// Gets the info for the method.
		/// </summary>
		[CanBeNull]
		public MethodInfo MethodInfo { get { return m_MethodInfo; } }

		/// <summary>
		/// Gets the property info for the property.
		/// </summary>
		[CanBeNull]
		public PropertyInfo PropertyInfo { get { return m_PropertyInfo; } }

		/// <summary>
		/// Gets the event info for the property.
		/// </summary>
		[CanBeNull]
		public EventInfo EventInfo { get { return m_EventInfo; } }

		/// <summary>
		/// Gets the info for the method parameters.
		/// </summary>
		[NotNull]
		public IEnumerable<ParameterInfo> Parameters
		{
			get
			{
				return m_MethodInfo == null
					       ? Enumerable.Empty<ParameterInfo>()
					       : m_MethodInfo.GetParameters();
			}
		}

		/// <summary>
		/// Gets the number of method parameters.
		/// </summary>
		public int ParameterCount { get { return m_MethodInfo == null ? 0 : m_MethodInfo.GetParameters().Length; } }

		/// <summary>
		/// Gets the IO mask for this instance.
		/// </summary>
		public eTelemetryIoMask IoMask
		{
			get
			{
				eTelemetryIoMask output = eTelemetryIoMask.Na;

				if (MethodInfo != null)
					output |= eTelemetryIoMask.ServiceToProgram;

				if (PropertyInfo != null || EventInfo != null)
					output |= eTelemetryIoMask.ProgramToService;

				return output;
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="provider"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="methodInfo"></param>
		/// <param name="eventInfo"></param>
		public TelemetryLeaf([NotNull] string name, [NotNull] ITelemetryProvider provider,
		                     [CanBeNull] PropertyInfo propertyInfo, [CanBeNull] MethodInfo methodInfo,
		                     [CanBeNull] EventInfo eventInfo)
			: base(name, provider)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (provider == null)
				throw new ArgumentNullException("Provider");

			if (propertyInfo == null && methodInfo == null && eventInfo == null)
				throw new ArgumentNullException("At least one member info must not be null");

			if (eventInfo != null && propertyInfo == null)
			{
				Type eventArgsType = eventInfo.GetEventArgsType();
				if (!eventArgsType.IsAssignableTo<IGenericEventArgs>())
					throw new NotSupportedException("Event-only telemetry must use GenericEventArgs");
			}

			m_PropertyInfo = propertyInfo;
			m_MethodInfo = methodInfo;
			m_EventInfo = eventInfo;

			// Subscribe to the event.
			if (m_EventInfo != null)
				m_Delegate = ReflectionUtils.SubscribeEvent(provider, m_EventInfo, this, CallbackMethodInfo);

			// Update to reflect the initial state of the property
			if (m_PropertyInfo != null)
				Update(Value);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public override void Dispose()
		{
			OnValueRaised = null;

			base.Dispose();

			ReflectionUtils.UnsubscribeEvent(Provider, m_EventInfo, m_Delegate);
		}

		#region Methods

		/// <summary>
		/// Invokes the telemetry method passing the given parameters.
		/// </summary>
		/// <param name="parameters"></param>
		public void Invoke(params object[] parameters)
		{
			if (m_MethodInfo == null)
				throw new InvalidOperationException("Telemetry has no method to invoke");

			try
			{
				m_MethodInfo.Invoke(Provider, parameters);
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException(string.Format("Exception invoking telemetry method Name:{0} Provider:{1}", Name, Provider),
				                            ex);
			}
		}

		/// <summary>
		/// Instructs the telemetry provider to initialize its state.
		/// </summary>
		public void Initialize()
		{
			Provider.InitializeTelemetry();
		}

		#endregion

		#region Provider Callbacks

		/// <summary>
		/// Called when the provider event is raised.
		/// </summary>
		[UsedImplicitly]
		[System.Reflection.Obfuscation(Exclude = true)]
		private void EventCallback(object sender, EventArgs args)
		{
			// Use the property to get the current value
			if (m_PropertyInfo != null)
			{
				Update(Value);
				return;
			}

			// Otherwise pull the value out of the event args
			IGenericEventArgs genericArgs = args as IGenericEventArgs;
			if (genericArgs == null)
				throw new InvalidOperationException("Unable to infer new value from event args");

			Update(genericArgs.Data);
		}

		/// <summary>
		/// Updates the cached value and raises the changed event if the value has changed.
		/// </summary>
		private void Update(object newValue)
		{
			if (EqualityComparer<object>.Default.Equals(m_CachedValue, newValue))
				return;

			m_CachedValue = newValue;

			OnValueRaised.Raise(this, new GenericEventArgs<object>(m_CachedValue));
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

			addRow("Event", m_EventInfo == null ? null : m_EventInfo.Name);
			addRow("Property", m_PropertyInfo == null ? null : m_PropertyInfo.Name);
			addRow("Method", m_MethodInfo == null ? null : m_MethodInfo.Name);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			if (PropertyInfo != null)
				yield return new ConsoleCommand("Update", "Invokes the update method manually", () => Update(Value));

			if (MethodInfo == null)
				yield break;

			string[] paramNames = Parameters.Select(p => p.Name).ToArray();

			string help =
				paramNames.Length == 0
					? "Invokes this parameterless command"
					: string.Format("Invoke <{0}>", string.Join(", ", paramNames));

			yield return new ParamsConsoleCommand("Invoke", help, p => ConsoleParseCommand(p));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		private void ConsoleParseCommand(params string[] args)
		{
			if (Parameters == null)
				throw new InvalidOperationException();

			Type[] types =
				Parameters.Select(p => p.ParameterType)
#if SIMPLSHARP
				          .Select(t => (Type)t)
#endif
				          .ToArray();

			object[] parameters =
				args.Zip(types)
				    .Select(kvp => AbstractConsoleCommand.Convert(kvp.Key, kvp.Value))
				    .ToArray();

			Invoke(parameters);
		}

		#endregion
	}
}
