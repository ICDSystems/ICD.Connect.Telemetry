using System.Collections.Generic;
using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Nodes.Collections
{
	public interface ITelemetryCollection : ICollection<ITelemetryItem>
	{
		[CanBeNull]
		ITelemetryItem GetChildByName(string name);
	}
}
