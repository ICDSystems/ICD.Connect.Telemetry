using System;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry
{
	public sealed class MethodTelemetryNodeItem : AbstractTelemetryNodeItemBase, IManagementTelemetryItem
	{
		private readonly MethodInfo m_MethodInfo;

		public int ParameterCount { get { return ParameterTypes.Length; } }
		public Type[] ParameterTypes { get; private set; }
		public object Parent { get; private set; }

		public MethodTelemetryNodeItem(string name, object parent ,MethodInfo info) : base(name)
		{
			m_MethodInfo = info;
			Parent = parent;

			ParameterTypes = m_MethodInfo.GetParameters().Select(p => (Type)p.ParameterType).ToArray();
		}

		
		public void Invoke(object[] parameters)
		{
			m_MethodInfo.Invoke(Parent, parameters);
		}
	}
}