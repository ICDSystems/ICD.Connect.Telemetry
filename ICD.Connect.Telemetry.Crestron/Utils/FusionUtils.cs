using System;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Connect.Telemetry.Crestron.Utils
{
	public static class FusionUtils
	{
		/// <summary>
		/// Return the text format to send to Fusion
		/// </summary>
		/// <returns>Text format for fusion, including timestamp, severity, and message</returns>
		[PublicAPI]
		public static string GetFusionLogText(LogItem logItem)
		{
			return GetFusionLogText(logItem.Timestamp, logItem.Severity, logItem.Message);
		}

		/// <summary>
		/// Return the text format to send to Fusion
		/// </summary>
		/// <returns>Text format for fusion, including timestamp, severity, and message</returns>
		[PublicAPI]
		public static string GetFusionLogText(DateTime timestamp, eSeverity severity, string message)
		{
			StringBuilder s = new StringBuilder();
			{
				s.Append(timestamp.ToString("yyyyMMddHHmmss"));
				s.Append("||");
				s.Append((int)severity);
				s.Append("||");
				s.Append(message);
			}
			return s.ToString();
		}
	}
}
