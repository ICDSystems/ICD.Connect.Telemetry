using System;

namespace ICD.Connect.Telemetry
{
	public abstract class AbstractUpdatableTelemetryNodeItem<T> : AbstractTelemetryNodeItem<T>, IUpdatableTelemetryNodeItem
	{
		protected readonly Func<T> m_Callback;

		protected AbstractUpdatableTelemetryNodeItem(string name, Func<T> updateFunction)
			: base(name)
		{
			m_Callback = updateFunction;
		}

		public abstract void Update();
	}
}