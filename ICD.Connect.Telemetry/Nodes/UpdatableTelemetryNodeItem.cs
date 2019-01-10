#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class UpdatableTelemetryNodeItem<T> : AbstractUpdatableTelemetryNodeItem<T>
	{
		public UpdatableTelemetryNodeItem(string name, object parent, PropertyInfo propertyInfo)
			: base(name, parent, propertyInfo)
		{
			
		}
	}
}