using System;
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
		[NotNull]
		Type ValueType { get; }
		object Value { get; }
		PropertyInfo PropertyInfo { get; }
	}

	public interface IFeedbackTelemetryItem<T> : IFeedbackTelemetryItem
	{
		new T Value { get; }
	}
}
