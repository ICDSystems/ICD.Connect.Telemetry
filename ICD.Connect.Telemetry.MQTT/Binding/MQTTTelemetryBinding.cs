using System;
using System.Text;
using ICD.Common.Properties;
using ICD.Connect.Telemetry.Bindings;
using ICD.Connect.Telemetry.Nodes;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry.MQTT.Binding
{
	public sealed class MQTTTelemetryBinding : AbstractTelemetryBinding
	{
		private const string METADATA = "Metadata";

		[NotNull]
		private readonly CoreTelemetry m_CoreTelemetry;

		[NotNull]
		private readonly string m_ProgramToServiceTopic;

		[NotNull]
		private readonly string m_ServiceToProgramTopic;

		#region Properties

		/// <summary>
		/// Gets the program to service topic for the binding.
		/// </summary>
		[NotNull]
		public string ProgramToServiceTopic { get { return m_ProgramToServiceTopic; } }

		/// <summary>
		/// Gets the service to program topic for the binding.
		/// </summary>
		[NotNull]
		public string ServiceToProgramTopic { get { return m_ServiceToProgramTopic; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getTelemetry"></param>
		/// <param name="setTelemetry"></param>
		/// <param name="programToServiceTopic"></param>
		/// <param name="serviceToProgramTopic"></param>
		/// <param name="coreTelemetry"></param>
		public MQTTTelemetryBinding([CanBeNull] IFeedbackTelemetryItem getTelemetry,
		                            [CanBeNull] IManagementTelemetryItem setTelemetry,
		                            [NotNull] string programToServiceTopic,
		                            [NotNull] string serviceToProgramTopic,
		                            [NotNull] CoreTelemetry coreTelemetry)
			: base(getTelemetry, setTelemetry)
		{
			if (coreTelemetry == null)
				throw new ArgumentNullException("coreTelemetry");

			if (string.IsNullOrEmpty(programToServiceTopic))
				throw new ArgumentException("Cannot create telemetry binding with a null or empty topic.");

			if (string.IsNullOrEmpty(serviceToProgramTopic))
				throw new ArgumentException("Cannot create telemetry binding with a null or empty topic.");

			if (getTelemetry == null && setTelemetry == null)
				throw new
					InvalidOperationException("Cannot create telemetry binding with neither a getter nor a setter.");

			m_ProgramToServiceTopic = programToServiceTopic;
			m_ServiceToProgramTopic = serviceToProgramTopic;
			m_CoreTelemetry = coreTelemetry;

			SubscribeToService();

			PublishMetadata();
			UpdateAndSendValueToService();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			UnsubscribeFromService();
		}

		#endregion

		#region Publish

		protected override void UpdateAndSendValueToService()
		{
			if (GetTelemetry == null)
				return;

			object value = GetTelemetry.Value;
			string json = JsonConvert.SerializeObject(value);
			byte[] data = Encoding.UTF8.GetBytes(json);

			m_CoreTelemetry.Publish(ProgramToServiceTopic, data);
		}

		/// <summary>
		/// Generates metadata and publishes as telemetry data.
		/// </summary>
		private void PublishMetadata()
		{
			TelemetryMetadata metadata =
				GetTelemetry == null
					? TelemetryMetadata.FromManagementTelemetry(SetTelemetry)
					: TelemetryMetadata.FromFeedbackTelemetry(GetTelemetry);

			string json = JsonConvert.SerializeObject(metadata);
			byte[] data = Encoding.UTF8.GetBytes(json);

			string topic = MQTTUtils.Join(ProgramToServiceTopic, METADATA);

			m_CoreTelemetry.Publish(topic, data);
		}

		#endregion

		#region Core Telemetry Callbacks

		/// <summary>
		/// Subscribe to topic changes from the service.
		/// </summary>
		private void SubscribeToService()
		{
			m_CoreTelemetry.Subscribe(ServiceToProgramTopic, HandleTopicMessage);
		}

		/// <summary>
		/// Unsubscribe from topic changes from the service.
		/// </summary>
		private void UnsubscribeFromService()
		{
			m_CoreTelemetry.Unsubscribe(ServiceToProgramTopic, HandleTopicMessage);
		}

		/// <summary>
		/// Called when the binding gets a message from the MQTT broker.
		/// </summary>
		/// <param name="data"></param>
		private void HandleTopicMessage(byte[] data)
		{
			if (SetTelemetry == null)
				throw new InvalidOperationException();

			// Simple method call
			if (SetTelemetry.ParameterInfo == null)
			{
				UpdateLocalNodeValueFromService();
				return;
			}

			// Method call with parameter
			string json = Encoding.UTF8.GetString(data, 0, data.Length);
			object value = JsonConvert.DeserializeObject(json, SetTelemetry.ParameterInfo.ParameterType);

			UpdateLocalNodeValueFromService(value);
		}

		public override void UpdateLocalNodeValueFromService()
		{
			if (SetTelemetry == null || SetTelemetry.ParameterInfo != null)
				throw new NotSupportedException();

			SetTelemetry.Invoke();
		}

		public void UpdateLocalNodeValueFromService(object value)
		{
			if (SetTelemetry == null || SetTelemetry.ParameterInfo == null)
				throw new NotSupportedException();

			SetTelemetry.Invoke(value);
		}

		#endregion
	}
}