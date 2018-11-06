using System;

namespace ICD.Connect.Telemetry
{
	public sealed class UpdatableTelemetryNodeItem<T> : AbstractUpdatableTelemetryNodeItem<T>
	{
		public UpdatableTelemetryNodeItem(string name, Func<T> getValue)
			: base(name, getValue)
		{
			
		}

		public override void Update()
		{
			Value = m_Callback();
		}
	}
}