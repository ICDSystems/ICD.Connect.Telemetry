using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.API.Nodes;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Telemetry.Nodes
{
	public interface ITelemetryNode : IDisposable, IConsoleNode
	{
		/// <summary>
		/// Gets the name of the telemetry node.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the provider that this telemetry item is attached to.
		/// </summary>
		[NotNull]
		ITelemetryProvider Provider { get; }

		/// <summary>
		/// Gets the child telemetry nodes.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ITelemetryNode> GetChildren();
	}
}