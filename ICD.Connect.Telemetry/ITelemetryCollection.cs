using System.Collections.Generic;
using ICD.Common.Properties;

namespace ICD.Connect.Telemetry
{
	public interface ITelemetryCollection : ICollection<ITelemetryItem>
	{
		IEnumerable<ITelemetryItem> GetChildren();
		IEnumerable<T> GetChildren<T>() where T : ITelemetryItem;

		[CanBeNull]
		ITelemetryItem GetChildByName(string name);
	}
}