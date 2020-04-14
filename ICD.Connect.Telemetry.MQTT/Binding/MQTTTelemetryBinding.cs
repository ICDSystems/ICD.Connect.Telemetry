using System;

using System.Globalization;
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

		private MQTTTelemetryBinding(ITelemetryItem getTelemetry, ITelemetryItem setTelemetry, string path, CoreTelemetry telemetry)
			:base(getTelemetry, setTelemetry)
		{
			m_Path = path;
			m_Telemetry = telemetry;

			UpdateAndSendValueToService();
		}

		public override void UpdateLocalNodeValueFromService()
		{
			if (SetTelemetry.ParameterCount != 0)
				throw new NotSupportedException();

			SetTelemetry.Invoke();
		}

		public void UpdateLocalNodeValueFromService(string message)
		{
			if(SetTelemetry == null)
				return;
			
			Type paramType = SetTelemetry.ParameterTypes.Single();
			object value = Convert.ChangeType(message, paramType, CultureInfo.InvariantCulture);
			SetTelemetry.Invoke(value);
		}

		protected override void UpdateAndSendValueToService()
		{
			if (GetTelemetry == null)
				return;

			string json = JsonConvert.SerializeObject(GetTelemetry.Value);
			m_Telemetry.UpdateValueForPath(Path, json);
		}

		public static MQTTTelemetryBinding Bind(ITelemetryItem getTelemetry, ITelemetryItem setTelemetry, string path,
		                                        CoreTelemetry telemetry)
		{
			if(telemetry == null)
				throw new InvalidOperationException("Cannot create telemetry binding with a null CoreTelemetry");

			if(string.IsNullOrEmpty(path))
				throw new InvalidOperationException("Cannot create telemetry binding with a null or empty path. ");

			if(!MQTTUtils.ValidatePath(path))
				throw new InvalidOperationException(string.Format("Cannot create telemetry binding with the invalid path {0}", path));

			if (getTelemetry == null && setTelemetry == null)
				throw new InvalidOperationException("Cannot create telemetry binding with neither a getter nor a setter.");

			return new MQTTTelemetryBinding(getTelemetry, setTelemetry, path, telemetry);
		}
	}
}