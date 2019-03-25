#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class UpdatableTelemetryNodeItem<T> : AbstractUpdatableTelemetryNodeItem<T>
	{
		public UpdatableTelemetryNodeItem(string name, ITelemetryProvider parent, PropertyInfo propertyInfo)
			: base(name, parent, propertyInfo)
		{
			
		}
	}
}