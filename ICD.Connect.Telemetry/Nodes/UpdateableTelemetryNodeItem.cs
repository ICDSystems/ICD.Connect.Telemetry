#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class UpdateableTelemetryNodeItem<T> : AbstractUpdateableTelemetryNodeItem<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="setTelemetry"></param>
		public UpdateableTelemetryNodeItem(string name, ITelemetryProvider parent, PropertyInfo propertyInfo, string setTelemetry)
			: base(name, parent, propertyInfo, setTelemetry)
		{
		}
	}
}
