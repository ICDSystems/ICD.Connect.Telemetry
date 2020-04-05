using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Protocol.IoT.EventArguments;
using ICD.Connect.Protocol.IoT.Ports;
using ICD.Connect.Settings.Cores;
using ICD.Connect.Settings.Originators;
using ICD.Connect.Telemetry.MQTT.Binding;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Service;

namespace ICD.Connect.Telemetry.MQTT
{
	public sealed class CoreTelemetry
	{
		private const string PROGRAM_TO_SERVICE_PREFIX = "ProgramToService";
		private const string SERVICE_TO_PROGRAM_PREFIX = "ServiceToProgram";
		private const string SYSTEMS_PREFIX = "Systems";

		private static ITelemetryService TelemetryService{ get { return ServiceProvider.GetService<ITelemetryService>(); }}
		
		private readonly ICore m_Core;
		private readonly Dictionary<string, MQTTTelemetryBinding> m_ProgramToServiceBindingsByPath;
		private readonly Dictionary<string, MQTTTelemetryBinding> m_ServiceToProgramBindingsByPath;
		private readonly SafeCriticalSection m_BindingsSection;
		private readonly int m_SystemId;
		private readonly IcdMqttClient m_Client;

		public string PathPrefix { get; set; }

		public IcdMqttClient Port { get { return m_Client; }}
		
		public CoreTelemetry(ICore core)
		{
			m_Core = core;
			m_ProgramToServiceBindingsByPath = new Dictionary<string, MQTTTelemetryBinding>();
			m_ServiceToProgramBindingsByPath = new Dictionary<string, MQTTTelemetryBinding>();
			m_BindingsSection = new SafeCriticalSection();
			m_SystemId = core.Id;

			m_Client = new IcdMqttClient();
			m_Client.OnMessageReceived += ClientOnMessageReceived;
		}

		#region Binding

		public void GenerateTelemetryForSystem()
		{
			TelemetryCollection systemTelemetry = new TelemetryCollection();

			foreach (ITelemetryItem systemLevelTelemetry in TelemetryService.GetTelemetryForProvider(m_Core))
				systemTelemetry.Add(systemLevelTelemetry);

			CreateBindingsForSystem(systemTelemetry);
		}
			
		private void CreateBindingsForSystem(ITelemetryCollection systemTelemetry)
		{
			Stack<string> workingPath = new Stack<string>();
			string[] prefixItems = MQTTUtils.Split(PathPrefix);

			if (prefixItems.Length != 0)
				foreach (string prefixItem in prefixItems)
					workingPath.Push(prefixItem);

			CreateBindingsForServiceToProgram(systemTelemetry, workingPath);
			CreateBindingsForProgramToService(systemTelemetry, workingPath);
		}

		private void CreateBindingsForProgramToService(IEnumerable<ITelemetryItem> systemTelemetry, Stack<string> workingPath)
		{
			workingPath.Push(PROGRAM_TO_SERVICE_PREFIX);
			workingPath.Push(SYSTEMS_PREFIX);
			workingPath.Push(m_SystemId.ToString());
			BindProgramToServiceTelemetryRecursive(systemTelemetry, workingPath);
			workingPath.Pop();
			workingPath.Pop();
			workingPath.Pop();
		}

		private void CreateBindingsForServiceToProgram(IEnumerable<ITelemetryItem> systemTelemetry, Stack<string> workingPath)
		{
			workingPath.Push(SERVICE_TO_PROGRAM_PREFIX);
			workingPath.Push(SYSTEMS_PREFIX);
			workingPath.Push(m_SystemId.ToString());
			BindServiceToProgramTelemetryRecursive(systemTelemetry, workingPath);
			workingPath.Pop();
			workingPath.Pop();
			workingPath.Pop();
		}

		private void BindProgramToServiceTelemetryRecursive(IEnumerable<ITelemetryItem> telemetryItems, Stack<string> workingPath)
		{
			foreach (ITelemetryItem item in telemetryItems)
			{
				ITelemetryCollection collection = item as ITelemetryCollection;
				if (collection != null)
				{
					workingPath.Push(item.Name);
					BindProgramToServiceTelemetryRecursive(collection.GetChildren(), workingPath);
				}
				else
				{
					workingPath.Push(item.Name);
					string path = MQTTUtils.Join(workingPath.Reverse().ToArray());
					MQTTTelemetryBinding binding = MQTTTelemetryBinding.Bind(item, null, path, this);
					AddProgramToServiceBinding(binding);
				}
				workingPath.Pop();
			}
		}

		private void BindServiceToProgramTelemetryRecursive(IEnumerable<ITelemetryItem> telemetryItems, Stack<string> workingPath)
		{
			foreach (ITelemetryItem item in telemetryItems)
			{
				ITelemetryCollection collection = item as ITelemetryCollection;
				if (collection != null)
				{
					workingPath.Push(item.Name);
					BindServiceToProgramTelemetryRecursive(collection.GetChildren(), workingPath);
				}
				else
				{
					workingPath.Push(item.Name);
					string path = MQTTUtils.Join(workingPath.Reverse().ToArray());
					MQTTTelemetryBinding binding = MQTTTelemetryBinding.Bind(null, item, path, this);
					AddServiceToProgramBinding(binding);
				}
				workingPath.Pop();
			}
		}

		private void AddProgramToServiceBinding(MQTTTelemetryBinding binding)
		{
			m_BindingsSection.Enter();
			try
			{
				m_ProgramToServiceBindingsByPath[binding.Path] = binding;
			}
			finally
			{
				m_BindingsSection.Leave();
			}
		}

		private void AddServiceToProgramBinding(MQTTTelemetryBinding binding)
		{
			m_BindingsSection.Enter();
			try
			{
				m_ServiceToProgramBindingsByPath[binding.Path] = binding;
			}
			finally
			{
				m_BindingsSection.Leave();
			}
			if (!m_Client.IsConnected)
				m_Client.Connect();

			m_Client.Subscribe(new[] {binding.Path}, new[] {MQTTUtils.QOS_LEVEL_EXACTLY_ONCE});
		}

		#endregion

		public void UpdateValueForPath(string path, string telemetryValue)
		{
			if (!m_Client.IsConnected)
				m_Client.Connect();

			m_Client.Publish(path, System.Text.Encoding.UTF8.GetBytes(telemetryValue));
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
	}
}