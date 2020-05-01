using ICD.Common.Properties;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public interface IFeedbackTelemetryItem : ITelemetryItem
	{
		/// <summary>
		/// Gets the value from the property.
		/// </summary>
		[CanBeNull]
		object Value { get; }

		/// <summary>
		/// Gets the property info for the property.
		/// </summary>
		[NotNull]
		PropertyInfo PropertyInfo { get; }
	}
}
