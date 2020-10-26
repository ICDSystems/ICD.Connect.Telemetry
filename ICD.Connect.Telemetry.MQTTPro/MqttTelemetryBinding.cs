using System;
using System.Linq;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Json;
using ICD.Connect.Telemetry.Bindings;
using ICD.Connect.Telemetry.Nodes;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry.MQTTPro
{
	public sealed class MqttTelemetryBinding : AbstractTelemetryBinding
	{
		private const string METADATA = "Metadata";

		[NotNull]
		private readonly MqttTelemetryServiceProvider m_TelemetryServiceProvider;

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

		/// <summary>
		/// Gets the program to service topic for the metadata.
		/// </summary>
		[NotNull]
		public string ProgramToServiceMetadataTopic { get { return MqttUtils.Join(ProgramToServiceTopic, METADATA); } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="programToServiceTopic"></param>
		/// <param name="serviceToProgramTopic"></param>
		/// <param name="telemetryServiceProvider"></param>
		public MqttTelemetryBinding([NotNull] TelemetryLeaf telemetry,
		                            [NotNull] string programToServiceTopic,
		                            [NotNull] string serviceToProgramTopic,
		                            [NotNull] MqttTelemetryServiceProvider telemetryServiceProvider)
			: base(telemetry)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			if (telemetryServiceProvider == null)
				throw new ArgumentNullException("telemetryServiceProvider");

			if (string.IsNullOrEmpty(programToServiceTopic))
				throw new ArgumentException("Cannot create telemetry binding with a null or empty topic.");

			if (string.IsNullOrEmpty(serviceToProgramTopic))
				throw new ArgumentException("Cannot create telemetry binding with a null or empty topic.");

			m_ProgramToServiceTopic = programToServiceTopic;
			m_ServiceToProgramTopic = serviceToProgramTopic;
			m_TelemetryServiceProvider = telemetryServiceProvider;
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public override void Dispose()
		{
			if (Telemetry.MethodInfo != null)
				UnsubscribeFromService();

			// Clear the retained messages
			m_TelemetryServiceProvider.Publish(ProgramToServiceTopic, null);
			m_TelemetryServiceProvider.Publish(MqttUtils.Join(ProgramToServiceTopic, METADATA), null);

			base.Dispose();
		}

		/// <summary>
		/// Instructs the telemetry provider to update its state and sends values to the service.
		/// </summary>
		public override void Initialize()
		{
			if (Telemetry.MethodInfo != null)
				SubscribeToService();

			PublishMetadata();

			base.Initialize();
		}

		#endregion

		#region Publish

		/// <summary>
		/// Sends the wrapped property value to the telemetry service.
		/// </summary>
		/// <param name="value"></param>
		protected override void SendValueToService(object value)
		{
			PublishMessage message = new PublishMessage {Data = value};

			m_TelemetryServiceProvider.Publish(ProgramToServiceTopic, message);
		}

		/// <summary>
		/// Generates metadata and publishes as telemetry data.
		/// </summary>
		private void PublishMetadata()
		{
			PublishMessage message =
				new PublishMessage
				{
					Data = TelemetryMetadata.FromTelemetry(Telemetry)
				};

			m_TelemetryServiceProvider.Publish(ProgramToServiceMetadataTopic, message);
		}

		#endregion

		#region Service Callbacks

		/// <summary>
		/// Subscribe to topic changes from the service.
		/// </summary>
		private void SubscribeToService()
		{
			m_TelemetryServiceProvider.Subscribe(ServiceToProgramTopic, HandleTopicMessage);
		}

		/// <summary>
		/// Unsubscribe from topic changes from the service.
		/// </summary>
		private void UnsubscribeFromService()
		{
			m_TelemetryServiceProvider.Unsubscribe(ServiceToProgramTopic, HandleTopicMessage);
		}

		/// <summary>
		/// Called when the binding gets a message from the MQTT broker.
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="data"></param>
		private void HandleTopicMessage(string topic, byte[] data)
		{
			if (topic == null)
				throw new ArgumentNullException("topic");

			if (topic != ServiceToProgramTopic)
				throw new InvalidOperationException("Unexpected topic");

			if (Telemetry.MethodInfo == null)
				throw new InvalidOperationException("Binding does not support feedback");

			string json = Encoding.UTF8.GetString(data, 0, data.Length);

			Type[] types =
				Telemetry.Parameters
				         .Select(p => p.ParameterType)
#if SIMPLSHARP
				         .Select(t => (Type)t)
#endif
				         .ToArray();

			// Trying to deserialize an array of different typed objects gets gross
			JsonSerializer serializer = JsonSerializer.Create(JsonUtils.CommonSettings);
			object[] values;

			using (IcdTextReader textReader = new IcdStringReader(json))
			{
				using (JsonReader reader = new JsonTextReader(textReader.WrappedTextReader))
				{
					int index = 0;
					values =
						serializer
							.DeserializeArray(reader, (s, r) => s.Deserialize(r, types[index++]))
							.ToArray();
				}
			}

			HandleValuesFromService(values);
		}

		#endregion
	}
}
