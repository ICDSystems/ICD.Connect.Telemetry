using System;
using System.Collections.Generic;
using System.Linq;
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

		private readonly ICore m_Core;

		private readonly Dictionary<string, MQTTTelemetryBinding> m_ProgramToServiceBindingsByPath;
		private readonly Dictionary<string, MQTTTelemetryBinding> m_ServiceToProgramBindingsByPath;
		private readonly SafeCriticalSection m_BindingsSection;

		private readonly ConnectionStateManager m_ConnectionStateManager;
		private readonly IcdMqttClient m_Client;

		#region Properties

		private static ITelemetryService TelemetryService { get { return ServiceProvider.GetService<ITelemetryService>(); } }

		/// <summary>
		/// Returns true if the core is actively sending/receiving telemetry.
		/// </summary>
		public bool IsRunning { get; private set; }

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
		public CoreTelemetry(ICore core)
		{
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

		public void Start()
		{
			GenerateTelemetryForSystem();

			m_ConnectionStateManager.Start();
		}

		public void Stop()
		{
			m_ConnectionStateManager.Stop();
		}

		public void SubscribeToPath(string path)
		{
			if (!m_Client.IsConnected)
				m_Client.Connect();

			m_Client.Subscribe(new[] { path }, new[] { MQTTUtils.QOS_LEVEL_EXACTLY_ONCE });
		}

		public void UpdateValueForPath(string path, string telemetryValue)
		{
			if (!m_Client.IsConnected)
				m_Client.Connect();

			m_Client.Publish(path, System.Text.Encoding.UTF8.GetBytes(telemetryValue));
		}

		#endregion

		#region Binding

		private void GenerateTelemetryForSystem()
		{
			ITelemetryCollection systemTelemetry = TelemetryService.GetTelemetryForProvider(m_Core);

			IEnumerable<string> prefix = MQTTUtils.Split(PathPrefix);
			Stack<string> workingPath = new Stack<string>(prefix);

			CreateBindingsForServiceToProgram(systemTelemetry, workingPath);
			CreateBindingsForProgramToService(systemTelemetry, workingPath);
		}

		private void CreateBindingsForProgramToService(IEnumerable<ITelemetryItem> systemTelemetry,
		                                               Stack<string> workingPath)
		{
			workingPath.Push(PROGRAM_TO_SERVICE_PREFIX);
			workingPath.Push(SYSTEMS_PREFIX);
			workingPath.Push(m_Core.Uuid.ToString());

			try
			{
				BindProgramToServiceTelemetryRecursive(systemTelemetry, workingPath);
			}
			finally
			{
				workingPath.Pop();
				workingPath.Pop();
				workingPath.Pop();
			}
		}

		private void CreateBindingsForServiceToProgram(IEnumerable<ITelemetryItem> systemTelemetry,
		                                               Stack<string> workingPath)
		{
			workingPath.Push(SERVICE_TO_PROGRAM_PREFIX);
			workingPath.Push(SYSTEMS_PREFIX);
			workingPath.Push(m_Core.Uuid.ToString());

			try
			{
				BindServiceToProgramTelemetryRecursive(systemTelemetry, workingPath);
			}
			finally
			{
				workingPath.Pop();
				workingPath.Pop();
				workingPath.Pop();
			}
		}

		private void BindProgramToServiceTelemetryRecursive([NotNull] IEnumerable<ITelemetryItem> telemetryItems, [NotNull] Stack<string> workingPath)
		{
			if (telemetryItems == null)
				throw new ArgumentNullException("telemetryItems");

			if (workingPath == null)
				throw new ArgumentNullException("workingPath");

			foreach (ITelemetryItem item in telemetryItems)
			{
				workingPath.Push(item.Name);

				try
				{
					ITelemetryCollection collection = item as ITelemetryCollection;
					if (collection != null)
					{
						BindProgramToServiceTelemetryRecursive(collection.GetChildren(), workingPath);
						continue;
					}

					IFeedbackTelemetryItem feedback = item as IFeedbackTelemetryItem;
					if (feedback != null)
						GenerateMetadataForItem(feedback, workingPath);

					string path = MQTTUtils.Join(workingPath.Reverse());
					MQTTTelemetryBinding binding = MQTTTelemetryBinding.Bind(item, null, path, this);
					AddProgramToServiceBinding(binding);
				}
				finally
				{
					workingPath.Pop();
				}
			}
		}

		private void GenerateMetadataForItem([NotNull] IFeedbackTelemetryItem item, [NotNull] Stack<string> workingPath)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (workingPath == null)
				throw new ArgumentNullException("workingPath");

			workingPath.Push("Metadata");

			try
			{
				TelemetryMetadata metadata = TelemetryMetadata.FromFeedbackTelemetry(item);

				string json = JsonConvert.SerializeObject(metadata);
				string path = MQTTUtils.Join(workingPath.Reverse());
				UpdateValueForPath(path, json);
			}
			finally
			{
				workingPath.Pop();
			}
		}

		private void BindServiceToProgramTelemetryRecursive(IEnumerable<ITelemetryItem> telemetryItems, Stack<string> workingPath)
		{
			foreach (ITelemetryItem item in telemetryItems)
			{
				workingPath.Push(item.Name);

				try
				{
					ITelemetryCollection collection = item as ITelemetryCollection;
					if (collection != null)
					{
						BindServiceToProgramTelemetryRecursive(collection.GetChildren(), workingPath);
						continue;
					}

					string path = MQTTUtils.Join(workingPath.Reverse());
					MQTTTelemetryBinding binding = MQTTTelemetryBinding.Bind(null, item, path, this);
					AddServiceToProgramBinding(binding);
				}
				finally
				{
					workingPath.Pop();
				}
			}
		}

		/// <summary>
		/// Adds the binding to the cache.
		/// </summary>
		/// <param name="binding"></param>
		private void AddProgramToServiceBinding([NotNull] MQTTTelemetryBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException("binding");

			m_BindingsSection.Enter();

			try
			{
				m_ProgramToServiceBindingsByPath.Add(binding.Path, binding);
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		/// <summary>
		/// Adds the binding to the cache and subscribes to the binding path.
		/// </summary>
		/// <param name="binding"></param>
		private void AddServiceToProgramBinding([NotNull] MQTTTelemetryBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException("binding");

			m_BindingsSection.Enter();

			try
			{
				m_ServiceToProgramBindingsByPath.Add(binding.Path, binding);
			}
			finally
			{
				m_BindingsSection.Leave();
			}

			SubscribeToPath(binding.Path);
		}

		private void ClientOnMessageReceived(object sender, MqttMessageEventArgs args)
		{
			MQTTTelemetryBinding binding;
			if(!TryGetServiceToProgramBinding(args.Topic, out binding))
				return;

			if (binding.SetTelemetry.ParameterCount == 0)
				binding.UpdateLocalNodeValueFromService();
			else
			{
				string convertedMessage = System.Text.Encoding.UTF8.GetString(args.Message, 0, args.Message.Length);
				binding.UpdateLocalNodeValueFromService(convertedMessage);
			}
		}

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

		#region Settings

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

		public void Clear()
		{
			Stop();

			m_Client.ClearSettings();
		}

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