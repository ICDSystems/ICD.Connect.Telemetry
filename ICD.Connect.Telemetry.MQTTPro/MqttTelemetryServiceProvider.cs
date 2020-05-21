using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.NetworkPro.EventArguments;
using ICD.Connect.Protocol.NetworkPro.Ports.Mqtt;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Cores;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Nodes.Collections;
using ICD.Connect.Telemetry.Services;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry.MQTTPro
{
	public sealed class MqttTelemetryServiceProvider :
		AbstractTelemetryServiceProvider<MqttTelemetryServiceProviderSettings>
	{
		public delegate void SubscriptionCallback(byte[] data);

		private const string PROGRAM_TO_SERVICE_PREFIX = "ProgramToService";
		private const string SERVICE_TO_PROGRAM_PREFIX = "ServiceToProgram";
		private const string SYSTEMS_PREFIX = "Systems";

		private readonly Dictionary<string, MqttTelemetryBinding> m_ProgramToServiceBindings;
		private readonly Dictionary<string, MqttTelemetryBinding> m_ServiceToProgramBindings;
		private readonly Dictionary<string, IcdHashSet<SubscriptionCallback>> m_SubscriptionCallbacks;
		private readonly SafeCriticalSection m_BindingsSection;

		private readonly ConnectionStateManager m_ConnectionStateManager;
		private readonly IcdMqttClient m_Client;

		private ICore m_Core;

		/// <summary>
		/// Gets the Core instance.
		/// </summary>
		public ICore Core { get { return m_Core ?? (m_Core = ServiceProvider.TryGetService<ICore>()); } }

		#region Properties

		/// <summary>
		/// Gets the telemetry service.
		/// </summary>
		private static ITelemetryService TelemetryService { get { return ServiceProvider.GetService<ITelemetryService>(); } }

		/// <summary>
		/// Returns true if the core is actively sending/receiving telemetry.
		/// </summary>
		public bool IsRunning { get { return m_ConnectionStateManager.Heartbeat.MonitoringActive; } }

		/// <summary>
		/// Gets/sets the path prefix for topics.
		/// </summary>
		public string PathPrefix { get; set; }

		/// <summary>
		/// Gets the MQTT client.
		/// </summary>
		public IMqttClient Port { get { return m_Client; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public MqttTelemetryServiceProvider()
		{
			m_ProgramToServiceBindings = new Dictionary<string, MqttTelemetryBinding>();
			m_ServiceToProgramBindings = new Dictionary<string, MqttTelemetryBinding>();
			m_SubscriptionCallbacks = new Dictionary<string, IcdHashSet<SubscriptionCallback>>();
			m_BindingsSection = new SafeCriticalSection();

			m_Client = new IcdMqttClient
			{
				Name = GetType().Name,
				DebugRx = eDebugMode.Ascii,
				DebugTx = eDebugMode.Ascii
			};
			Subscribe(m_Client);

			m_ConnectionStateManager = new ConnectionStateManager(this);
			m_ConnectionStateManager.SetPort(m_Client, false);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			Stop();
			Unsubscribe(m_Client);

			base.DisposeFinal(disposing);
		}

		#region Methods

		/// <summary>
		/// Generates telemetry for the system and starts sending data to the broker.
		/// </summary>
		public void Start()
		{
			Stop();

			GenerateBindingsForSystem();

			m_ConnectionStateManager.Start();
		}

		/// <summary>
		/// Disposes telemetry for the system and stops sending telemetry to the broker.
		/// </summary>
		public void Stop()
		{
			m_ConnectionStateManager.Stop();

			UnsubscribeAll();
			DisposeBindings();
		}

		/// <summary>
		/// Subscribes to telemetry feedback at the given topic.
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="callback"></param>
		public void Subscribe([NotNull] string topic, [NotNull] SubscriptionCallback callback)
		{
			if (string.IsNullOrEmpty(topic))
				throw new ArgumentException("Topic must not be null or empty");

			if (!topic.StartsWith(SERVICE_TO_PROGRAM_PREFIX))
				throw new ArgumentException("Subscribe topics must start with " + SERVICE_TO_PROGRAM_PREFIX);

			if (callback == null)
				throw new ArgumentNullException("callback");

			m_BindingsSection.Enter();

			try
			{
				// Add the callback to the cache - only subscribe to the topic if this is the first callback
				if (m_SubscriptionCallbacks.GetOrAddNew(topic).Add(callback) &&
				    m_SubscriptionCallbacks[topic].Count != 1)
					return;
			}
			finally
			{
				m_BindingsSection.Leave();
			}

			// Don't bother subscribing unless we're connected
			if (Port.IsConnected)
				m_Client.Subscribe(new[] {topic}, new[] {MqttUtils.QOS_LEVEL_EXACTLY_ONCE});
		}

		/// <summary>
		/// Unsubscribes from telemetry feedback at the given topic.
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="callback"></param>
		public void Unsubscribe([NotNull] string topic, [NotNull] SubscriptionCallback callback)
		{
			if (string.IsNullOrEmpty(topic))
				throw new ArgumentException("Topic must not be null or empty");

			if (!topic.StartsWith(SERVICE_TO_PROGRAM_PREFIX))
				throw new ArgumentException("Subscribe topics must start with " + SERVICE_TO_PROGRAM_PREFIX);

			if (callback == null)
				throw new ArgumentNullException("callback");

			m_BindingsSection.Enter();

			try
			{
				// Remove the callback from the cache - only unsubscribe from the topic if there are no more callbacks
				IcdHashSet<SubscriptionCallback> callbacks;
				if (!m_SubscriptionCallbacks.TryGetValue(topic, out callbacks) ||
				    !callbacks.Remove(callback) ||
				    callbacks.Count > 0)
					return;
			}
			finally
			{
				m_BindingsSection.Leave();
			}

			m_Client.Unsubscribe(new[] {topic});
		}

		/// <summary>
		/// Publishes telemetry data to the given topic.
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="message"></param>
		public void Publish([NotNull] string topic, [NotNull] PublishMessage message)
		{
			if (string.IsNullOrEmpty(topic))
				throw new ArgumentException("Topic must not be null or empty");

			if (!topic.StartsWith(PROGRAM_TO_SERVICE_PREFIX))
				throw new ArgumentException("Publish topics must start with " + PROGRAM_TO_SERVICE_PREFIX);

			if (message == null)
				throw new ArgumentNullException("message");

			string json = JsonConvert.SerializeObject(message);
			byte[] data = Encoding.UTF8.GetBytes(json);

			m_Client.Publish(topic, data);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Unsubscribes all of the known subscriptions.
		/// </summary>
		private void UnsubscribeAll()
		{
			string[] topics;

			m_BindingsSection.Enter();

			try
			{
				topics = m_SubscriptionCallbacks.Keys.ToArray();
				m_SubscriptionCallbacks.Clear();
			}
			finally
			{
				m_BindingsSection.Leave();
			}

			if (topics.Length > 0)
				m_Client.Unsubscribe(topics);
		}

		/// <summary>
		/// Disposes and clears all of the generated bindings.
		/// </summary>
		private void DisposeBindings()
		{
			m_BindingsSection.Enter();

			try
			{
				foreach (MqttTelemetryBinding binding in m_ProgramToServiceBindings.Values)
					binding.Dispose();
				m_ProgramToServiceBindings.Clear();

				foreach (MqttTelemetryBinding binding in m_ServiceToProgramBindings.Values)
					binding.Dispose();
				m_ServiceToProgramBindings.Clear();
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string BuildProgramToServiceTopic([NotNull] Stack<string> path)
		{
			return BuildProgramToServiceTopic(path.Reverse());
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string BuildProgramToServiceTopic([NotNull] params string[] path)
		{
			return BuildProgramToServiceTopic((IEnumerable<string>)path);
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string BuildProgramToServiceTopic([NotNull] IEnumerable<string> path)
		{
			IEnumerable<string> pathEnumerable =
				PROGRAM_TO_SERVICE_PREFIX.Yield()
				                         .Append(SYSTEMS_PREFIX)
				                         .Append(Core.Uuid.ToString())
				                         .Concat(path);

			if (!string.IsNullOrEmpty(PathPrefix))
				pathEnumerable = pathEnumerable.Prepend(PathPrefix);

			return MqttUtils.Join(pathEnumerable);
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string BuildServiceToProgramTopic([NotNull] Stack<string> path)
		{
			return BuildServiceToProgramTopic((IEnumerable<string>)path);
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string BuildServiceToProgramTopic([NotNull] IEnumerable<string> path)
		{
			IEnumerable<string> pathEnumerable =
				SERVICE_TO_PROGRAM_PREFIX.Yield()
				                         .Append(SYSTEMS_PREFIX)
				                         .Append(Core.Uuid.ToString())
				                         .Concat(path);

			if (!string.IsNullOrEmpty(PathPrefix))
				pathEnumerable = pathEnumerable.Prepend(PathPrefix);

			return MqttUtils.Join(pathEnumerable);
		}

		#endregion

		#region Binding

		/// <summary>
		/// Creates the bindings from the system telemetry to the client recursively.
		/// </summary>
		private void GenerateBindingsForSystem()
		{
			// Create the Last Will And Testament
			m_Client.Will.Topic = BuildProgramToServiceTopic("IsOnline");
			m_Client.Will.Message = JsonConvert.SerializeObject(false);

			// Create system bindings
			ITelemetryCollection systemTelemetry = TelemetryService.GetTelemetryForProvider(Core);
			GenerateTelemetryRecursive(systemTelemetry, new Stack<string>());
		}

		/// <summary>
		/// Creates the bindings for the given telemetry items recursively.
		/// </summary>
		/// <param name="telemetryItems"></param>
		/// <param name="path"></param>
		private void GenerateTelemetryRecursive([NotNull] IEnumerable<ITelemetryItem> telemetryItems,
		                                        [NotNull] Stack<string> path)
		{
			if (telemetryItems == null)
				throw new ArgumentNullException("telemetryItems");

			if (path == null)
				throw new ArgumentNullException("path");

			foreach (ITelemetryItem item in telemetryItems)
				GenerateTelemetryRecursive(item, path);
		}

		/// <summary>
		/// Creates the bindings for the given telemetry item recursively.
		/// </summary>
		/// <param name="telemetryItem"></param>
		/// <param name="path"></param>
		private void GenerateTelemetryRecursive([NotNull] ITelemetryItem telemetryItem, [NotNull] Stack<string> path)
		{
			if (telemetryItem == null)
				throw new ArgumentNullException("telemetryItem");

			if (path == null)
				throw new ArgumentNullException("path");

			path.Push(telemetryItem.Name);

			try
			{
				ITelemetryCollection collection = telemetryItem as ITelemetryCollection;
				if (collection != null)
					GenerateTelemetryRecursive(collection, path);

				PropertyTelemetryItem feedback = telemetryItem as PropertyTelemetryItem;
				if (feedback != null)
					CreateSystemToServiceBinding(feedback, path);

				MethodTelemetryItem method = telemetryItem as MethodTelemetryItem;
				if (method != null)
					CreateServiceToSystemBinding(method, path);
			}
			finally
			{
				path.Pop();
			}
		}

		/// <summary>
		/// Creates the binding and adds to the cache.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="path"></param>
		private void CreateSystemToServiceBinding([NotNull] PropertyTelemetryItem telemetry, [NotNull] Stack<string> path)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			if (path == null)
				throw new ArgumentNullException("path");

			string programToService = BuildProgramToServiceTopic(path);
			string serviceToProgram = BuildServiceToProgramTopic(path);
			
			m_BindingsSection.Enter();

			try
			{
				if (m_ProgramToServiceBindings.ContainsKey(programToService))
				{
					Logger.Log(eSeverity.Error, "Failed to add binding for duplicate topic {0}", programToService);
					return;
				}

				MqttTelemetryBinding binding = new MqttTelemetryBinding(telemetry, null, programToService, serviceToProgram, this);
				m_ProgramToServiceBindings.Add(programToService, binding);
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		/// <summary>
		/// Adds the binding to the cache and subscribes to the binding path.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="path"></param>
		private void CreateServiceToSystemBinding([NotNull] MethodTelemetryItem telemetry, [NotNull] Stack<string> path)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			if (path == null)
				throw new ArgumentNullException("path");

			string programToService = BuildProgramToServiceTopic(path);
			string serviceToProgram = BuildServiceToProgramTopic(path);

			m_BindingsSection.Enter();

			try
			{
				if (m_ServiceToProgramBindings.ContainsKey(serviceToProgram))
				{
					Logger.Log(eSeverity.Error, "Failed to add binding for duplicate topic {0}", serviceToProgram);
					return;
				}

				MqttTelemetryBinding binding = new MqttTelemetryBinding(null, telemetry, programToService, serviceToProgram, this);
				m_ServiceToProgramBindings.Add(serviceToProgram, binding);
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		#endregion

		#region Client Callbacks

		/// <summary>
		/// Subscribe to the client events.
		/// </summary>
		/// <param name="client"></param>
		private void Subscribe(IMqttClient client)
		{
			client.OnMessageReceived += ClientOnMessageReceived;
			client.OnConnectedStateChanged += ClientOnConnectedStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the client events.
		/// </summary>
		/// <param name="client"></param>
		private void Unsubscribe(IMqttClient client)
		{
			client.OnMessageReceived -= ClientOnMessageReceived;
			client.OnConnectedStateChanged -= ClientOnConnectedStateChanged;
		}

		/// <summary>
		/// Called when the client receives a message from the broker.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ClientOnMessageReceived(object sender, MqttMessageEventArgs args)
		{
			m_BindingsSection.Enter();

			try
			{
				IcdHashSet<SubscriptionCallback> callbacks;
				if (!m_SubscriptionCallbacks.TryGetValue(args.Topic, out callbacks))
					return;

				foreach (SubscriptionCallback callback in callbacks)
				{
					try
					{
						callback(args.Message);
					}
					catch (Exception e)
					{
						Logger.Log(eSeverity.Error, "Failed to handle MQTT message - {0}", e.Message);
					}
				}
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		/// <summary>
		/// Called when the client connects/disconnects.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ClientOnConnectedStateChanged(object sender, BoolEventArgs eventArgs)
		{
			// Resubscribe to everything on connection
			if (eventArgs.Data)
			{
				string[] topics = m_BindingsSection.Execute(() => m_SubscriptionCallbacks.Keys.ToArray());
				byte[] qosLevels = Enumerable.Repeat(MqttUtils.QOS_LEVEL_EXACTLY_ONCE, topics.Length).ToArray();

				m_Client.Subscribe(topics, qosLevels);
			}
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(MqttTelemetryServiceProviderSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			PathPrefix = settings.PathPrefix;

			m_Client.Hostname = settings.Hostname;
			m_Client.Port = settings.Port;
			m_Client.Username = settings.Username;
			m_Client.Password = settings.Password;
			m_Client.Secure = settings.Secure;

			// Client ID is the System ID
			m_Client.ClientId = Core.Uuid.ToString();

			if (settings.Enabled)
				Start();
			else
				Stop();
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Stop();

			PathPrefix = null;
			m_Client.ClearSettings();
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(MqttTelemetryServiceProviderSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Enabled = IsRunning;
			settings.PathPrefix = PathPrefix;
			settings.Hostname = Port.Hostname;
			settings.Port = Port.Port;
			settings.Username = Port.Username;
			settings.Password = Port.Password;
			settings.Secure = Port.Secure;
		}

		#endregion
	}
}
