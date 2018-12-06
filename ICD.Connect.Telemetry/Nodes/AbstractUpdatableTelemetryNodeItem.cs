using Crestron.SimplSharp.Reflection;
#if SIMPLSHARP

#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractUpdatableTelemetryNodeItem<T> : AbstractFeedbackTelemetryNodeItem<T>, IUpdatableTelemetryNodeItem
	{

		private readonly PropertyInfo m_PropertyInfo;
		private readonly object m_Parent;

		protected AbstractUpdatableTelemetryNodeItem(string name, object parent, PropertyInfo propertyInfo)
			: base(name)
		{
			m_PropertyInfo = propertyInfo;
			m_Parent = parent;
		}

		public void Update()
		{
			Value = (T)m_PropertyInfo.GetValue(m_Parent, null);
		}
	}
}