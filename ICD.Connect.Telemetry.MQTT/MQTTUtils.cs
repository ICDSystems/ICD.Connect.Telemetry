using System.Linq;
using System.Text.RegularExpressions;

namespace ICD.Connect.Telemetry.MQTT
{
	public static class MQTTUtils
	{
		public const byte QOS_LEVEL_AT_MOST_ONCE = 0x00;
		public const byte QOS_LEVEL_AT_LEAST_ONCE = 0x01;
		public const byte QOS_LEVEL_EXACTLY_ONCE = 0x02;

		private const char SEPERATOR = '/';

		private const string VALIDATION_REGEX = @"^([\w\d])+(\/([\w\d]+))*$";

		private static readonly Regex s_ValidPathRegex = new Regex(VALIDATION_REGEX);

		public static string Join(string[] items)
		{
			if (items.Length == 0)
				return string.Empty;
			return string.Join(SEPERATOR.ToString(), items).Replace(" ", ""); //remove spaces for more consistent paths
		}

		public static string[] Split(string path)
		{
			if (string.IsNullOrEmpty(path))
				return new string[0];

			return path.Split(SEPERATOR);
		}

		public static bool ValidatePath(string path)
		{
			return s_ValidPathRegex.IsMatch(path);
		}
	}
}