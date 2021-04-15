using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Json;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Network.EventArguments;
using ICD.Connect.Protocol.Network.Ports.Mqtt;
using ICD.Connect.Protocol.NetworkPro.Ports.Mqtt;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Originators;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Services;
using Newtonsoft.Json;
#if SIMPLSHARP
using Crestron.SimplSharpPro.CrestronThread;
#else
using System.Threading;
#endif

namespace ICD.Connect.Telemetry.MQTTPro
{
	public sealed class MqttTelemetryServiceProvider :
		AbstractTelemetryServiceProvider<MqttTelemetryServiceProviderSettings>
	{
		public delegate void SubscriptionCallback(string topic, byte[] data);

		private readonly Dictionary<string, MqttTelemetryBinding> m_Bindings;
		private readonly Dictionary<string, IcdHashSet<SubscriptionCallback>> m_SubscriptionCallbacks;
		private readonly Dictionary<string, IcdHashSet<SubscriptionCallback>> m_WildcardSubscriptionCallbacks;
		private readonly IcdOrderedDictionary<string, PublishMessageInfo> m_PublishBuffer;
		private readonly TelemetryNodeTracker m_NodeTracker;
		private readonly SafeCriticalSection m_BindingsSection;
		private readonly SafeCriticalSection m_BufferSection;
		private readonly SafeCriticalSection m_ProcessSection;

		private readonly ConnectionStateManager m_ConnectionStateManager;
		private readonly IcdMqttClient m_Client;

		// External config workaround until DAV can fully configure MQTT
		private readonly MqttTelemetryOverrides m_Overrides;

		private bool m_CoreSettingsApplied;
		private Thread m_StartHandle;

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
		/// Gets/sets the hostname.
		/// </summary>
		public string Hostname { get; set; }

		/// <summary>
		/// Gets/sets the network port.
		/// </summary>
		public ushort Port { get; set; }

		/// <summary>
		/// Gets/sets the proxy hostname.
		/// </summary>
		public string ProxyHostname { get; set; }

		/// <summary>
		/// Gets/sets the proxy port.
		/// </summary>
		public ushort ProxyPort { get; set; }

		/// <summary>
		/// Gets/sets the username.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Gets/sets the password.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Gets/sets the secure mode.
		/// </summary>
		public bool Secure { get; set; }

		/// <summary>
		/// Gets/sets the path to the certificate-authority certificate.
		/// </summary>
		public string CaCertPath { get; set; }

		/// <summary>
		/// Gets/sets the path to the overrides config.
		/// </summary>
		public string ConfigPath { get; set; }

		/// <summary>
		/// Gets the system online topic.
		/// </summary>
		private string SystemOnlineTopic
		{
			get { return TopicUtils.GetProgramToServiceTopic(ClientId, PathPrefix, "IsOnline"); }
		}

		/// <summary>
		/// Gets the system online metadata topic.
		/// </summary>
		private string SystemOnlineMetadataTopic
		{
			get { return TopicUtils.GetProgramToServiceTopic(ClientId, PathPrefix, "IsOnline", "Metadata"); }
		}

		/// <summary>
		/// Returns a new system online message.
		/// </summary>
		private PublishMessage SystemOnlineMessage
		{
			get
			{
				return
					new PublishMessage
					{
						Data = true,
						Date = DateTime.MinValue
					};
			}
		}

		/// <summary>
		/// Returns a new system offline message.
		/// </summary>
		private PublishMessage SystemOfflineMessage
		{
			get
			{
				return
					new PublishMessage
					{
						Data = false,
						Date = DateTime.MinValue
					};
			}
		}

		/// <summary>
		/// Returns the metadata message for the system online topic.
		/// </summary>
		private PublishMessage SystemOnlineMetadataMessage
		{
			get
			{
				return
					new PublishMessage
					{
						Data = new TelemetryMetadata
						{
							IoMask = eTelemetryIoMask.ProgramToService,
							PublishMetadata = new TelemetryPublishMetadata
							{
								Property = new TelemetryMemberMetadata
								{
									Type = typeof(bool).GetMinimalName()
								}
							}
						},
						Date = DateTime.MinValue
					};
			}
		}

		/// <summary>
		/// Gets the client ID.
		/// </summary>
		public string ClientId { get { return Core.Uuid.ToString(); } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public MqttTelemetryServiceProvider()
		{
			m_Bindings = new Dictionary<string, MqttTelemetryBinding>();
			m_SubscriptionCallbacks = new Dictionary<string, IcdHashSet<SubscriptionCallback>>();
			m_WildcardSubscriptionCallbacks = new Dictionary<string, IcdHashSet<SubscriptionCallback>>();
			m_PublishBuffer = new IcdOrderedDictionary<string, PublishMessageInfo>();
			m_BindingsSection = new SafeCriticalSection();
			m_BufferSection = new SafeCriticalSection();
			m_ProcessSection = new SafeCriticalSection();
			m_Overrides = new MqttTelemetryOverrides();

			m_Client = new IcdMqttClient
			{
				Name = GetType().Name
			};
			Subscribe(m_Client);

			m_NodeTracker = new TelemetryNodeTracker();
			Subscribe(m_NodeTracker);

			m_ConnectionStateManager = new ConnectionStateManager(this);
			m_ConnectionStateManager.SetPort(m_Client, false);

			IcdEnvironment.OnProgramInitializationComplete += IcdEnvironmentOnProgramInitializationComplete;
			Core.OnLifecycleStateChanged += CoreOnLifecycleStateChanged;
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			Stop();

			Unsubscribe(m_Client);
			Unsubscribe(m_NodeTracker);

			m_Client.Dispose();
			m_NodeTracker.Clear();

			base.DisposeFinal(disposing);
		}

		#region Methods

		/// <summary>
		/// Generates telemetry for the system and starts sending data to the broker.
		/// </summary>
		public void Start()
		{
			Stop();

			m_Client.Hostname = m_Overrides.Hostname ?? Hostname;
			m_Client.Port = m_Overrides.Port ?? Port;
			m_Client.ProxyHostname = m_Overrides.ProxyHostname ?? ProxyHostname;
			m_Client.ProxyPort = m_Overrides.ProxyPort ?? ProxyPort;
			m_Client.Username = Username;
			m_Client.Password = Password;
			m_Client.Secure = Secure;
			m_Client.CaCertPath = CaCertPath;

			// Client ID is the System ID
			m_Client.ClientId = Core.Uuid.ToString();

			GenerateLastWillAndTestament();
			m_ConnectionStateManager.Start();

			// Build the telemetry asynchronously - it's heavy on startup
#if SIMPLSHARP
			m_StartHandle =
				new Thread(o =>
				           {
					           GenerateBindingsForSystem();
					           m_StartHandle = null;
					           return null;
				           }, null, Thread.eThreadStartOptions.CreateSuspended)
				{Priority = Thread.eThreadPriority.LowestPriority};
			m_StartHandle.Start();
#else
			m_StartHandle =
				new Thread(o =>
					{
						GenerateBindingsForSystem();
						m_StartHandle = null;
					})
					{Priority = ThreadPriority.Lowest};
			m_StartHandle.Start();
#endif
		}

		/// <summary>
		/// Disposes telemetry for the system and stops sending telemetry to the broker.
		/// </summary>
		public void Stop()
		{
#if SIMPLSHARP
			if (m_StartHandle != null)
				m_StartHandle.Abort();
#endif

			m_ConnectionStateManager.Stop();

			if (m_Client.IsConnected)
				m_Client.Publish(m_Client.Will.Topic, m_Client.Will.MessageBytes, m_Client.Will.QosLevel, m_Client.Will.Retain);

			m_Client.Disconnect();

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

			if (callback == null)
				throw new ArgumentNullException("callback");

			m_BindingsSection.Enter();

			try
			{
				IDictionary<string, IcdHashSet<SubscriptionCallback>> map =
					topic.EndsWith("/#")
						? m_WildcardSubscriptionCallbacks
						: m_SubscriptionCallbacks;

				// Add the callback to the cache - only subscribe to the topic if this is the first callback
				if (map.GetOrAddNew(topic).Add(callback) &&
				    map[topic].Count != 1)
					return;
			}
			finally
			{
				m_BindingsSection.Leave();
			}

			// Don't bother subscribing unless we're connected
			if (m_Client.IsConnected)
				m_Client.Subscribe(topic, MqttUtils.QOS_LEVEL_AT_MOST_ONCE);
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

			if (!topic.StartsWith(TopicUtils.SERVICE_TO_PROGRAM_PREFIX))
				throw new ArgumentException("Subscribe topics must start with " + TopicUtils.SERVICE_TO_PROGRAM_PREFIX);

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

			// Don't bother unsubscribing unless we're connected
			if (m_Client.IsConnected)
				m_Client.Unsubscribe(new[] {topic});
		}

		/// <summary>
		/// Publishes telemetry data to the given topic.
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="message">Clears the retained message if null</param>
		public void Publish([NotNull] string topic, [CanBeNull] PublishMessage message)
		{
			if (string.IsNullOrEmpty(topic))
				throw new ArgumentException("Topic must not be null or empty");

			if (!topic.StartsWith(TopicUtils.PROGRAM_TO_SERVICE_PREFIX))
				throw new ArgumentException("Publish topics must start with " + TopicUtils.PROGRAM_TO_SERVICE_PREFIX);

			// Enqueue the message onto the buffer
			EnqueueBuffer(topic, message);

			// Create a worker thread
			ThreadingUtils.SafeInvoke(ProcessPublishBuffer);
		}

		/// <summary>
		/// Works through the publish buffer.
		/// </summary>
		private void ProcessPublishBuffer()
		{
			if (!m_ProcessSection.TryEnter())
				return;

			try
			{
				PublishMessageInfo item = default(PublishMessageInfo);
				while (m_Client.IsConnected && m_BufferSection.Execute(() => DequeueBuffer(out item)))
				{
					if (item.PublishMessage == null)
						m_Client.Clear(item.Topic);
					else
					{
						string json = JsonConvert.SerializeObject(item.PublishMessage, Formatting.None, JsonUtils.CommonSettings);
						byte[] data = Encoding.UTF8.GetBytes(json);
						m_Client.Publish(item.Topic, data, MqttUtils.QOS_LEVEL_AT_LEAST_ONCE, true);
					}
				}
			}
			finally
			{
				m_ProcessSection.Leave();
			}
		}

		/// <summary>
		/// Stores the message to be sent to the broker.
		/// Overwrites the existing message at the given topic.
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="message"></param>
		private void EnqueueBuffer(string topic, PublishMessage message)
		{
			m_BufferSection.Execute(() => m_PublishBuffer[topic] = new PublishMessageInfo(topic, message));
		}

		/// <summary>
		/// Gets and removes the next item from the buffer to be sent to the broker.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		private bool DequeueBuffer(out PublishMessageInfo output)
		{
			output = default(PublishMessageInfo);

			m_BufferSection.Enter();

			try
			{
				if (m_PublishBuffer.Count == 0)
					return false;

				KeyValuePair<string, PublishMessageInfo> kvp = m_PublishBuffer.Get(0);
				output = kvp.Value;

				m_PublishBuffer.Remove(kvp.Key);

				return true;
			}
			finally
			{
				m_BufferSection.Leave();
			}
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

			if (topics.Length > 0 && m_Client.IsConnected)
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
				foreach (MqttTelemetryBinding binding in m_Bindings.Values)
					binding.Dispose();
				m_Bindings.Clear();
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		/// <summary>
		/// Sets the LWT properties on the MQTT client.
		/// </summary>
		private void GenerateLastWillAndTestament()
		{
			m_Client.Will.Topic = SystemOnlineTopic;
			m_Client.Will.Message = JsonConvert.SerializeObject(SystemOfflineMessage, Formatting.None, JsonUtils.CommonSettings);
			m_Client.Will.Retain = true;
			m_Client.Will.QosLevel = MqttUtils.QOS_LEVEL_EXACTLY_ONCE;
			m_Client.Will.Flag = true;
		}

		/// <summary>
		/// Creates the bindings from the system telemetry to the client recursively.
		/// </summary>
		private void GenerateBindingsForSystem()
		{
			// Start building telemetry bindings
			TelemetryProviderNode systemTelemetry = TelemetryService.LazyLoadCoreTelemetry();
			m_NodeTracker.SetRootNode(systemTelemetry);
		}

		/// <summary>
		/// Creates the binding and adds to the cache.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="path"></param>
		private void CreateBinding([NotNull] TelemetryLeaf telemetry, [NotNull] IEnumerable<ITelemetryNode> path)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			if (path == null)
				throw new ArgumentNullException("path");

			// Skip the root node
			ITelemetryNode[] pathArray = path.Skip(1).ToArray();

			string programToService = TopicUtils.GetProgramToServiceTopic(ClientId, PathPrefix, pathArray);
			string serviceToProgram = TopicUtils.GetServiceToProgramTopic(ClientId, PathPrefix, pathArray);

			MqttTelemetryBinding binding;

			m_BindingsSection.Enter();

			try
			{
				if (m_Bindings.ContainsKey(programToService))
				{
					Logger.Log(eSeverity.Error, "Failed to add binding for duplicate topic {0}", programToService);
					return;
				}

				if (m_Bindings.ContainsKey(serviceToProgram))
				{
					Logger.Log(eSeverity.Error, "Failed to add binding for duplicate topic {0}", serviceToProgram);
					return;
				}

				binding = new MqttTelemetryBinding(telemetry, programToService, serviceToProgram, this);
				m_Bindings.Add(programToService, binding);
				m_Bindings.Add(serviceToProgram, binding);
				m_Bindings.Add(binding.ProgramToServiceMetadataTopic, binding);
			}
			finally
			{
				m_BindingsSection.Leave();
			}

			binding.Initialize();
		}

		/// <summary>
		/// Destroys the binding and removes from the cache.
		/// </summary>
		/// <param name="path"></param>
		private void DestroyBinding([NotNull] IEnumerable<ITelemetryNode> path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			// Skip the root node
			ITelemetryNode[] pathArray = path.Skip(1).ToArray();

			string programToService = TopicUtils.GetProgramToServiceTopic(ClientId, PathPrefix, pathArray);

			m_BindingsSection.Enter();

			try
			{
				MqttTelemetryBinding binding;
				if (!m_Bindings.TryGetValue(programToService, out binding))
					return;

				m_Bindings.Remove(binding.ProgramToServiceTopic);
				m_Bindings.Remove(binding.ServiceToProgramTopic);
				m_Bindings.Remove(binding.ProgramToServiceMetadataTopic);

				binding.Dispose();
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		#endregion

		#region Node Tracker Callbacks

		/// <summary>
		/// Subscribe to the node tracker events.
		/// </summary>
		/// <param name="tracker"></param>
		private void Subscribe(TelemetryNodeTracker tracker)
		{
			tracker.OnNodeAdded += TrackerOnNodeAdded;
			tracker.OnNodeRemoved += TrackerOnNodeRemoved;
		}

		/// <summary>
		/// Unsubscribe from the node tracker events.
		/// </summary>
		/// <param name="tracker"></param>
		private void Unsubscribe(TelemetryNodeTracker tracker)
		{
			tracker.OnNodeAdded -= TrackerOnNodeAdded;
			tracker.OnNodeRemoved -= TrackerOnNodeRemoved;
		}

		/// <summary>
		/// Called when a node is added to the telemetry tree.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="path"></param>
		private void TrackerOnNodeAdded(ITelemetryNode node, IEnumerable<ITelemetryNode> path)
		{
			TelemetryLeaf leaf = node as TelemetryLeaf;
			if (leaf != null)
				CreateBinding(leaf, path);
		}

		/// <summary>
		/// Called when a node is removed from the telemetry tree.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="path"></param>
		private void TrackerOnNodeRemoved(ITelemetryNode node, IEnumerable<ITelemetryNode> path)
		{
			TelemetryLeaf leaf = node as TelemetryLeaf;
			if (leaf != null)
				DestroyBinding(path);
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
			IcdHashSet<SubscriptionCallback> callbacks = new IcdHashSet<SubscriptionCallback>();

			m_BindingsSection.Enter();

			try
			{
				// Get the exact matches
				IcdHashSet<SubscriptionCallback> subscriptionCallbacks;
				if (m_SubscriptionCallbacks.TryGetValue(args.Topic, out subscriptionCallbacks))
					callbacks.AddRange(subscriptionCallbacks);

				// Get wildcard matches
				IEnumerable<SubscriptionCallback> wildcardCallbacks =
					m_WildcardSubscriptionCallbacks.Where(kvp => TopicUtils.MatchesWildcard(kvp.Key, args.Topic))
					                               .SelectMany(kvp => kvp.Value);
				callbacks.AddRange(wildcardCallbacks);
			}
			finally
			{
				m_BindingsSection.Leave();
			}

			foreach (SubscriptionCallback callback in callbacks)
			{
				try
				{
					callback(args.Topic, args.Message);
				}
				catch (Exception e)
				{
					Logger.Log(eSeverity.Error, "Failed to handle MQTT message - {0}", e.Message);
				}
			}
		}

		/// <summary>
		/// Called when the client connects/disconnects.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ClientOnConnectedStateChanged(object sender, BoolEventArgs eventArgs)
		{
			if (!eventArgs.Data)
				return;

			// Send the online message
			Publish(SystemOnlineTopic, SystemOnlineMessage);
			Publish(SystemOnlineMetadataTopic, SystemOnlineMetadataMessage);

			// Resubscribe to everything on connection
			Dictionary<string, byte> topics;

			m_BindingsSection.Enter();

			try
			{
				topics =
					m_SubscriptionCallbacks.Concat(m_WildcardSubscriptionCallbacks)
					                       .ToDictionary(kvp => kvp.Key,
					                                     kvp => MqttUtils.QOS_LEVEL_AT_MOST_ONCE);
			}
			finally
			{
				m_BindingsSection.Leave();
			}

			m_Client.Subscribe(topics);

			// Begin working through the publish buffer
			ThreadingUtils.SafeInvoke(ProcessPublishBuffer);
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
			m_CoreSettingsApplied = false;

			base.ApplySettingsFinal(settings, factory);

			PathPrefix = settings.PathPrefix;
			Hostname = settings.Hostname;
			Port = settings.Port;
			ProxyHostname = settings.ProxyHostname;
			ProxyPort = settings.ProxyPort;
			Username = settings.Username;
			Password = settings.Password;
			Secure = settings.Secure;
			CaCertPath = settings.CaCertPath;

			ConfigPath =
				string.IsNullOrEmpty(settings.ConfigPath)
					? null
					: PathUtils.GetDefaultConfigPath("Telemetry", settings.ConfigPath);

			try
			{
				if (ConfigPath == null)
				{
					m_Overrides.Clear();
				}
				else if (IcdFile.Exists(ConfigPath))
				{
					string xml = IcdFile.ReadToEnd(ConfigPath, new UTF8Encoding(false));
					xml = EncodingUtils.StripUtf8Bom(xml);
					m_Overrides.ParseXml(xml);
				}
				else
				{
					Logger.Log(eSeverity.Error, "Failed to load MQTT telemetry overrides {0} - File does not exist", ConfigPath);
					m_Overrides.Clear();
				}
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, e, "Failed to load MQTT telemetry overrides {0} - {1}", ConfigPath, e.Message);
				m_Overrides.Clear();
			}

			Disable = m_Overrides.Disable ?? Disable;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			PathPrefix = null;
			Hostname = null;
			Port = MqttUtils.DEFAULT_PORT;
			ProxyHostname = null;
			ProxyPort = 0;
			Username = null;
			Password = null;
			Secure = false;
			CaCertPath = null;
			ConfigPath = null;

			// Hack - Assume DAV disabled this instance if we're using overrides
			Disable = m_Overrides.Disable == null ? Disable : m_Overrides.Disable.Value;

			m_Overrides.Clear();
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(MqttTelemetryServiceProviderSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.PathPrefix = PathPrefix;
			settings.Hostname = Hostname;
			settings.Port = Port;
			settings.ProxyHostname = ProxyHostname;
			settings.ProxyPort = ProxyPort;
			settings.Username = Username;
			settings.Password = Password;
			settings.Secure = Secure;
			settings.CaCertPath = CaCertPath;
			settings.ConfigPath = ConfigPath;

			// Hack - Assume DAV disabled this instance if we're using overrides
			settings.Disable = m_Overrides.Disable == null ? Disable : m_Overrides.Disable.Value;
		}

		/// <summary>
		/// Called when the Core lifecycle state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void CoreOnLifecycleStateChanged(object sender, LifecycleStateEventArgs eventArgs)
		{
			switch (eventArgs.Data)
			{
				case eLifecycleState.Started:
					m_CoreSettingsApplied = true;
					StartConnectionStateManagerIfReady();
					break;
			}
		}

		/// <summary>
		/// Called when the application initialization state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void IcdEnvironmentOnProgramInitializationComplete(object sender, EventArgs eventArgs)
		{
			StartConnectionStateManagerIfReady();
		}

		/// <summary>
		/// Called when the application finishes initializing or the core finishes loading.
		/// Starts the connection state manager if both have started.
		/// </summary>
		private void StartConnectionStateManagerIfReady()
		{
			if (Disable)
				return;

			if (m_CoreSettingsApplied && IcdEnvironment.ProgramIsInitialized)
				Start();
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

			addRow("IsRunning", IsRunning);
			addRow("PathPrefix", PathPrefix);
			addRow("Hostname", Hostname);
			addRow("Port", Port);
			addRow("ProxyHostname", ProxyHostname);
			addRow("ProxyPort", ProxyPort);
			addRow("Username", Username);
			addRow("Password", Password);
			addRow("Secure", Secure);
			addRow("CaCertPath", CaCertPath);
			addRow("ConfigPath", ConfigPath);
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			yield return m_Client;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		#endregion
	}
}
