using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.MQTTPro
{
	public static class TopicUtils
	{
		public const string PROGRAM_TO_SERVICE_PREFIX = "ProgramToService";
		public const string SERVICE_TO_PROGRAM_PREFIX = "ServiceToProgram";
		private const string SYSTEMS_PREFIX = "Systems";

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="pathPrefix"></param>
		/// <param name="path"></param>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public static string GetProgramToServiceTopic([NotNull] string clientId, [CanBeNull] string pathPrefix,
													  [NotNull] IEnumerable<ITelemetryNode> path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			return GetProgramToServiceTopic(clientId, pathPrefix, path.Select(n => n.Name));
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="pathPrefix"></param>
		/// <param name="path"></param>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public static string GetProgramToServiceTopic([NotNull] string clientId, [CanBeNull] string pathPrefix,
		                                              [NotNull] params string[] path)
		{
			return GetProgramToServiceTopic(clientId, pathPrefix, (IEnumerable<string>)path);
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="pathPrefix"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetProgramToServiceTopic([NotNull] string clientId, [CanBeNull] string pathPrefix,
		                                              [NotNull] IEnumerable<string> path)
		{
			if (clientId == null)
				throw new ArgumentNullException("clientId");

			if (string.IsNullOrEmpty(clientId))
				throw new ArgumentException("Client ID must not be empty", "clientId");

			if (path == null)
				throw new ArgumentNullException("path");

			IEnumerable<string> pathEnumerable =
				PROGRAM_TO_SERVICE_PREFIX.Yield()
				                         .Append(SYSTEMS_PREFIX)
				                         .Append(clientId)
				                         .Concat(path);

			if (!string.IsNullOrEmpty(pathPrefix))
				pathEnumerable = pathEnumerable.Prepend(pathPrefix);

			return MqttUtils.Join(pathEnumerable);
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="pathPrefix"></param>
		/// <param name="path"></param>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public static string GetServiceToProgramTopic([NotNull] string clientId, [CanBeNull] string pathPrefix,
		                                              [NotNull] IEnumerable<ITelemetryNode> path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			return GetServiceToProgramTopic(clientId, pathPrefix, path.Select(n => n.Name));
		}

		/// <summary>
		/// Returns an absolute topic for the given system telemetry path.
		/// </summary>
		/// <param name="pathPrefix"></param>
		/// <param name="path"></param>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public static string GetServiceToProgramTopic([NotNull] string clientId, [CanBeNull] string pathPrefix,
		                                              [NotNull] IEnumerable<string> path)
		{
			if (clientId == null)
				throw new ArgumentNullException("clientId");

			if (string.IsNullOrEmpty(clientId))
				throw new ArgumentException("Client ID must not be empty", "clientId");

			if (path == null)
				throw new ArgumentNullException("path");

			IEnumerable<string> pathEnumerable =
				SERVICE_TO_PROGRAM_PREFIX.Yield()
				                         .Append(SYSTEMS_PREFIX)
				                         .Append(clientId)
				                         .Concat(path);

			if (!string.IsNullOrEmpty(pathPrefix))
				pathEnumerable = pathEnumerable.Prepend(pathPrefix);

			return MqttUtils.Join(pathEnumerable);
		}

		/// <summary>
		/// Returns true if the given topic starts with the wildcard topic.
		/// </summary>
		/// <param name="wildcardTopic"></param>
		/// <param name="topic"></param>
		/// <returns></returns>
		public static bool MatchesWildcard([NotNull] string wildcardTopic, [NotNull] string topic)
		{
			if (wildcardTopic == null)
				throw new ArgumentNullException("wildcardTopic");

			if (topic == null)
				throw new ArgumentNullException("topic");

			if (!wildcardTopic.EndsWith("/#"))
				throw new FormatException("Not a wildcard topic");

			wildcardTopic = wildcardTopic.Substring(0, wildcardTopic.Length - 1);
			return topic.StartsWith(wildcardTopic);
		}
	}
}
