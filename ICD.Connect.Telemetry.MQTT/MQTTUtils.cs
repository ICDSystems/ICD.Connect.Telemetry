﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.MQTT
{
	public static class MQTTUtils
	{
		public const byte QOS_LEVEL_AT_MOST_ONCE = 0x00;
		public const byte QOS_LEVEL_AT_LEAST_ONCE = 0x01;
		public const byte QOS_LEVEL_EXACTLY_ONCE = 0x02;

		private const char SEPERATOR = '/';

		public const ushort DEFAULT_PORT = 1883;

		/// <summary>
		/// Creates an MQTT topic path from the given items.
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		[NotNull]
		public static string Join([NotNull] IEnumerable<string> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			return string.Join(SEPERATOR.ToString(), items.ToArray())
			             .Replace(" ", ""); // Remove spaces for more consistent paths
		}

		/// <summary>
		/// Splits an MQTT topic path into its individual items.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<string> Split([CanBeNull] string path)
		{
			path = path ?? string.Empty;
			return path.Split(SEPERATOR);
		}
	}
}