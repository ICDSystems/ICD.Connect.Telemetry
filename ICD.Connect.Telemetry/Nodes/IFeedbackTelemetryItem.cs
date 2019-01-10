using System;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Relfection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public interface IFeedbackTelemetryItem : ITelemetryItem
	{
		Type ValueType { get; }
		object Value { get; }
		PropertyInfo PropertyInfo { get; }
	}

	public interface IFeedbackTelemetryItem<T> : IFeedbackTelemetryItem
	{
		new T Value { get; }
	}
}