using System;

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractFeedbackTelemetryNodeItem<T> : AbstractTelemetryNodeItemBase, IFeedbackTelemetryItem<T>
	{
		object IFeedbackTelemetryItem.Value { get { return Value; } }
		public T Value { get; protected set; }
		public Type ValueType { get { return typeof(T); } }

		protected AbstractFeedbackTelemetryNodeItem(string name) 
			: base(name)
		{
			
		}


	}
}