using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry
{
	public sealed class TelemetryMethodInfoEqualityComparer : IEqualityComparer<MethodInfo>
	{
		private static readonly TelemetryMethodInfoEqualityComparer s_Instance;

		public static TelemetryMethodInfoEqualityComparer Instance { get { return s_Instance; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static TelemetryMethodInfoEqualityComparer()
		{
			s_Instance = new TelemetryMethodInfoEqualityComparer();
		}

		public bool Equals(MethodInfo x, MethodInfo y)
		{
			return x.Name == y.Name;
		}

		public int GetHashCode(MethodInfo obj)
		{
			return obj.Name.GetHashCode();
		}
	}
}