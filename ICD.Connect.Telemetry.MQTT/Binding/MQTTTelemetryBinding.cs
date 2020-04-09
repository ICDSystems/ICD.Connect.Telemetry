using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.CrestronIO;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Json;
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
			if (SetTelemetry.ParameterCount == 0)
				SetTelemetry.Invoke();
		}

		public void UpdateLocalNodeValueFromService(string message)
		{
			if(SetTelemetry == null)
				return;

			Type paramType = SetTelemetry.ParameterTypes.First();
			if (paramType == typeof(string))
				SetTelemetry.Invoke(message);
			else if (paramType == typeof(bool))
			{
				bool parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(ushort))
			{
				ushort parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(short))
			{
				short parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(uint))
			{
				uint parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(long))
			{
				long parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(ulong))
			{
				ulong parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(float))
			{
				float parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(double))
			{
				double parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(byte))
			{
				byte parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
			else if (paramType == typeof(char))
			{
				char parsed;
				StringUtils.TryParse(message, out parsed);
				SetTelemetry.Invoke(parsed);
			}
		}

		protected override void UpdateAndSendValueToService()
		{
			if (GetTelemetry == null)
				return;

			JsonItemWrapper wrapper = new JsonItemWrapper(GetTelemetry.Value);
			string json = JsonConvert.SerializeObject(wrapper);
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