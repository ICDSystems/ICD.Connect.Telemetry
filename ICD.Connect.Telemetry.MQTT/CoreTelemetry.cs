using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.IoT.EventArguments;
using ICD.Connect.Protocol.IoT.Ports;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Cores;
using ICD.Connect.Telemetry.MQTT.Binding;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Service;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry.MQTT
{
	public sealed class CoreTelemetry
	{
		private const string PROGRAM_TO_SERVICE_PREFIX = "ProgramToService";
		private const string SERVICE_TO_PROGRAM_PREFIX = "ServiceToProgram";
		private const string SYSTEMS_PREFIX = "Systems";

		[NotNull]
		private readonly ICore m_Core;

		private readonly Dictionary<string, MQTTTelemetryBinding> m_ProgramToServiceBindingsByPath;
		private readonly Dictionary<string, MQTTTelemetryBinding> m_ServiceToProgramBindingsByPath;
		private readonly SafeCriticalSection m_BindingsSection;

		private readonly ConnectionStateManager m_ConnectionStateManager;
		private readonly IcdMqttClient m_Client;

		#region Properties

		/// <summary>
		/// Gets the encoding for telemetry data.
		/// </summary>
		private static Encoding Encoding { get { return Encoding.UTF8; } }

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
		/// <param name="core"></param>
		public CoreTelemetry([NotNull] ICore core)
		{
			if (core == null)
				throw new ArgumentNullException("core");

			m_Core = core;
			m_ProgramToServiceBindingsByPath = new Dictionary<string, MQTTTelemetryBinding>();
			m_ServiceToProgramBindingsByPath = new Dictionary<string, MQTTTelemetryBinding>();
			m_BindingsSection = new SafeCriticalSection();

			m_Client = new IcdMqttClient
			{
				Name = GetType().Name,
				DebugRx = eDebugMode.Ascii,
				DebugTx = eDebugMode.Ascii
			};
			m_Client.OnMessageReceived += ClientOnMessageReceived;

			m_ConnectionStateManager = new ConnectionStateManager(this);
			m_ConnectionStateManager.SetPort(m_Client, false);
		}

		#region Methods

		/// <summary>
		/// Generates telemetry for the system and starts sending data to the broker.
		/// </summary>
		public void Start()
		{
			GenerateTelemetryForSystem();

			m_ConnectionStateManager.Start();
		}

		/// <summary>
		/// Stops sending telemetry to the broker.
		/// </summary>
		public void Stop()
		{
			m_ConnectionStateManager.Stop();
		}

		/// <summary>
		/// Subscribes to telemetry feedback at the given path.
		/// </summary>
		/// <param name="path"></param>
		public void Subscribe(string path)
		{
			if (!m_Client.IsConnected)
				m_Client.Connect();

			m_Client.Subscribe(new[] { path }, new[] { MQTTUtils.QOS_LEVEL_EXACTLY_ONCE });
		}

		/// <summary>
		/// Publishes telemetry data to the given path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="data"></param>
		public void Publish(string path, string data)
		{
			if (!m_Client.IsConnected)
				m_Client.Connect();

			m_Client.Publish(path, Encoding.GetBytes(data));
		}

		/// <summary>
		/// Publishes telemetry data to the given path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="data"></param>
		public void Publish(string path, object data)
		{
			string json = JsonConvert.SerializeObject(data);

			Publish(path, json);
		}

		/// <summary>
		/// Publishes the telemetry data to the given path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="data"></param>
		public void Publish([NotNull] Stack<string> path, [CanBeNull] object data)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			string pathString = MQTTUtils.Join(path.Reverse());
			Publish(pathString, data);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Generates metadata for the given telemetry item and publishes as telemetry data.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="workingPath"></param>
		private void PublishMetadata([NotNull] IFeedbackTelemetryItem item, [NotNull] Stack<string> workingPath)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (workingPath == null)
				throw new ArgumentNullException("workingPath");

			workingPath.Push("Metadata");

			try
			{
				TelemetryMetadata metadata = TelemetryMetadata.FromFeedbackTelemetry(item);
				Publish(workingPath, metadata);
			}
			finally
			{
				workingPath.Pop();
			}
		}

		// TODO - This method belongs in the binding
		private bool TryGetServiceToProgramBinding(string topic, out MQTTTelemetryBinding binding)
		{
			m_BindingsSection.Enter();

			try
			{
				return m_ServiceToProgramBindingsByPath.TryGetValue(topic, out binding);
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		#endregion

		#region Binding

		/// <summary>
		/// Creates the bindings from the system telemetry to the client recursively.
		/// </summary>
		private void GenerateTelemetryForSystem()
		{
			ITelemetryCollection systemTelemetry = TelemetryService.GetTelemetryForProvider(m_Core);

			IEnumerable<string> prefix = MQTTUtils.Split(PathPrefix);
			Stack<string> workingPath = new Stack<string>(prefix);

			CreateBindingsForServiceToProgram(systemTelemetry, workingPath);
			CreateBindingsForProgramToService(systemTelemetry, workingPath);
		}

		/// <summary>
		/// Creates outbound, program to service bindings recursively.
		/// </summary>
		/// <param name="telemetryItems"></param>
		/// <param name="path"></param>
		private void CreateBindingsForProgramToService(IEnumerable<ITelemetryItem> telemetryItems,
		                                               Stack<string> path)
		{
			path.Push(PROGRAM_TO_SERVICE_PREFIX);
			path.Push(SYSTEMS_PREFIX);
			path.Push(m_Core.Uuid.ToString());

			try
			{
				BindProgramToServiceTelemetryRecursive(telemetryItems, path);
			}
			finally
			{
				path.Pop();
				path.Pop();
				path.Pop();
			}
		}

		/// <summary>
		/// Creates inbound, service to program bindings recursively.
		/// </summary>
		/// <param name="telemetryItems"></param>
		/// <param name="path"></param>
		private void CreateBindingsForServiceToProgram(IEnumerable<ITelemetryItem> telemetryItems,
		                                               Stack<string> path)
		{
			path.Push(SERVICE_TO_PROGRAM_PREFIX);
			path.Push(SYSTEMS_PREFIX);
			path.Push(m_Core.Uuid.ToString());

			try
			{
				BindServiceToProgramTelemetryRecursive(telemetryItems, path);
			}
			finally
			{
				path.Pop();
				path.Pop();
				path.Pop();
			}
		}

		/// <summary>
		/// Creates outbound, program to service bindings recursively.
		/// </summary>
		/// <param name="telemetryItems"></param>
		/// <param name="path"></param>
		private void BindProgramToServiceTelemetryRecursive([NotNull] IEnumerable<ITelemetryItem> telemetryItems, [NotNull] Stack<string> path)
		{
			if (telemetryItems == null)
				throw new ArgumentNullException("telemetryItems");

			if (path == null)
				throw new ArgumentNullException("path");

			foreach (ITelemetryItem item in telemetryItems)
			{
				path.Push(item.Name);

				try
				{
					ITelemetryCollection collection = item as ITelemetryCollection;
					if (collection != null)
					{
						BindProgramToServiceTelemetryRecursive(collection, path);
						continue;
					}

					IFeedbackTelemetryItem feedback = item as IFeedbackTelemetryItem;
					if (feedback == null)
						continue;

					// TODO - This belongs in the binding
					PublishMetadata(feedback, path);
					CreateProgramToServiceBinding(feedback, path);
				}
				finally
				{
					path.Pop();
				}
			}
		}

		/// <summary>
		/// Creates inbound, service to program bindings recursively.
		/// </summary>
		/// <param name="telemetryItems"></param>
		/// <param name="path"></param>
		private void BindServiceToProgramTelemetryRecursive(IEnumerable<ITelemetryItem> telemetryItems, Stack<string> path)
		{
			if (telemetryItems == null)
				throw new ArgumentNullException("telemetryItems");

			if (path == null)
				throw new ArgumentNullException("path");

			foreach (ITelemetryItem item in telemetryItems)
			{
				path.Push(item.Name);

				try
				{
					ITelemetryCollection collection = item as ITelemetryCollection;
					if (collection != null)
					{
						BindServiceToProgramTelemetryRecursive(collection, path);
						continue;
					}

					IManagementTelemetryItem managementItem = item as IManagementTelemetryItem;
					if (managementItem != null)
						CreateServiceToProgramBinding(managementItem, path);
				}
				finally
				{
					path.Pop();
				}
			}
		}

		/// <summary>
		/// Creates the binding and adds to the cache.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="path"></param>
		private void CreateProgramToServiceBinding([NotNull] IFeedbackTelemetryItem telemetry, [NotNull] Stack<string> path)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			if (path == null)
				throw new ArgumentNullException("path");

			string pathString = MQTTUtils.Join(path.Reverse());
			MQTTTelemetryBinding binding = MQTTTelemetryBinding.Bind(telemetry, null, pathString, this);

			m_BindingsSection.Execute(() => m_ProgramToServiceBindingsByPath[binding.Path] = binding);
		}

		/// <summary>
		/// Adds the binding to the cache and subscribes to the binding path.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="path"></param>
		private void CreateServiceToProgramBinding([NotNull] IManagementTelemetryItem telemetry, [NotNull] Stack<string> path)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			if (path == null)
				throw new ArgumentNullException("path");

			string pathString = MQTTUtils.Join(path.Reverse());
			MQTTTelemetryBinding binding = MQTTTelemetryBinding.Bind(null, telemetry, pathString, this);

			m_BindingsSection.Execute(() => m_ServiceToProgramBindingsByPath[binding.Path] = binding);

			// TODO - This belongs in the binding.
			Subscribe(binding.Path);
		}

		#endregion

		#region Client Callbacks

		/// <summary>
		/// Called when the client receives a message from the broker.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ClientOnMessageReceived(object sender, MqttMessageEventArgs args)
		{
			// TODO - This all belongs in the bindings

			MQTTTelemetryBinding binding;
			if (!TryGetServiceToProgramBinding(args.Topic, out binding))
				return;

			if (binding.SetTelemetry == null)
				throw new InvalidOperationException();

			if (binding.SetTelemetry.ParameterCount == 0)
				binding.UpdateLocalNodeValueFromService();
			else
			{
				string convertedMessage = Encoding.GetString(args.Message, 0, args.Message.Length);
				binding.UpdateLocalNodeValueFromService(convertedMessage);
			}
		}

		#endregion

		#region Settings

		/// <summary>
		/// Applies the core telemetry settings.
		/// </summary>
		/// <param name="settings"></param>
		public void ApplySettings(CoreTelemetrySettings settings)
		{
			PathPrefix = settings.PathPrefix;

			m_Client.Hostname = settings.Hostname;
			m_Client.Port = settings.Port;
			m_Client.Username = settings.Username;
			m_Client.Password = settings.Password;
			m_Client.Secure = settings.Secure;
			m_Client.ClientId = m_Core.Uuid.ToString();

			if (settings.Enabled)
				Start();
			else
				Stop();
		}

		/// <summary>
		/// Clears the telemetry settings.
		/// </summary>
		public void Clear()
		{
			Stop();

			PathPrefix = null;
			m_Client.ClearSettings();
		}

		/// <summary>
		/// Returns a copy of the core telemetry settings.
		/// </summary>
		/// <returns></returns>
		public CoreTelemetrySettings CopySettings()
		{
			return new CoreTelemetrySettings
			{
				Enabled = IsRunning,
				PathPrefix = PathPrefix,
				Hostname = Port.Hostname,
				Port = Port.Port,
				Username = Port.Username,
				Password = Port.Password,
				Secure = Port.Secure
			};
		}

		#endregion
	}
}