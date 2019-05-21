#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
 {
	public sealed class StaticTelemetryNodeItem<T> : AbstractFeedbackTelemetryNodeItem<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="parent"></param>
		/// <param name="propertyInfo"></param>
		public StaticTelemetryNodeItem(string name, T value, ITelemetryProvider parent, PropertyInfo propertyInfo)
			: base(name, parent, propertyInfo)
		{
			Value = value;
		}
	}
}
