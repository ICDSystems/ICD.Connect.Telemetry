using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
namespace ICD.Connect.Telemetry
{
	public sealed class TelemetryPropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
	{
		private static readonly TelemetryPropertyInfoEqualityComparer s_Instance;

		public static TelemetryPropertyInfoEqualityComparer Instance { get { return s_Instance; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static TelemetryPropertyInfoEqualityComparer()
		{
			s_Instance = new TelemetryPropertyInfoEqualityComparer();
		}

		public bool Equals(PropertyInfo x, PropertyInfo y)
		{
			return x.Name == y.Name;
		}

		public int GetHashCode(PropertyInfo obj)
		{
			return obj.Name.GetHashCode();
		}
	}
}