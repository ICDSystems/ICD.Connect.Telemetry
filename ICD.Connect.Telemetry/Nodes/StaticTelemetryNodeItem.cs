#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Properties;

namespace ICD.Connect.Telemetry.Nodes
 {
	public sealed class StaticTelemetryNodeItem<T> : AbstractFeedbackTelemetryNodeItem<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="propertyInfo"></param>
		public StaticTelemetryNodeItem(string name, [NotNull] ITelemetryProvider parent, [NotNull] PropertyInfo propertyInfo)
			: base(name, parent, propertyInfo)
		{
		}
	}
}
