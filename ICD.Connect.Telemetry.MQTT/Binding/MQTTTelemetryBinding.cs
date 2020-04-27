using System;
using System.Linq;
using ICD.Common.Properties;
using ICD.Connect.Telemetry.Bindings;
using ICD.Connect.Telemetry.Nodes;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry.MQTT.Binding
{
	public sealed class MQTTTelemetryBinding : AbstractTelemetryBinding
	{
		[NotNull]
		private readonly CoreTelemetry m_Telemetry;

		[NotNull]
		private readonly string m_Path;

		public string Path { get { return m_Path; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getTelemetry"></param>
		/// <param name="setTelemetry"></param>
		/// <param name="path"></param>
		/// <param name="telemetry"></param>
		private MQTTTelemetryBinding(IFeedbackTelemetryItem getTelemetry, IManagementTelemetryItem setTelemetry,
		                             string path, CoreTelemetry telemetry)
			: base(getTelemetry, setTelemetry)
		{
			m_Path = path;
			m_Telemetry = telemetry;

			UpdateAndSendValueToService();
		}

		public override void UpdateLocalNodeValueFromService()
		{
			if (SetTelemetry == null || SetTelemetry.ParameterCount != 0)
				throw new NotSupportedException();

			SetTelemetry.Invoke();
		}

		public void UpdateLocalNodeValueFromService(string message)
		{
			if (SetTelemetry == null || SetTelemetry.ParameterCount != 1)
				throw new NotSupportedException();

			Type paramType = SetTelemetry.ParameterTypes.Single();
			object value = JsonConvert.DeserializeObject(message, paramType);

			SetTelemetry.Invoke(value);
		}

		protected override void UpdateAndSendValueToService()
		{
			if (GetTelemetry == null)
				return;

			m_Telemetry.Publish(Path, GetTelemetry.Value);
		}

		public static MQTTTelemetryBinding Bind([CanBeNull] IFeedbackTelemetryItem getTelemetry,
		                                        [CanBeNull] IManagementTelemetryItem setTelemetry,
		                                        [NotNull] string path,
		                                        [NotNull] CoreTelemetry telemetry)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			if (string.IsNullOrEmpty(path))
				throw new ArgumentException("Cannot create telemetry binding with a null or empty path.");

			if (getTelemetry == null && setTelemetry == null)
				throw new
					InvalidOperationException("Cannot create telemetry binding with neither a getter nor a setter.");

			return new MQTTTelemetryBinding(getTelemetry, setTelemetry, path, telemetry);
		}
	}
}