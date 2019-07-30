using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Comparers
{
	public sealed class TelemetryEventInfoEqualityComparer : IEqualityComparer<EventInfo>
	{
		private static readonly TelemetryEventInfoEqualityComparer s_Instance;

		public static TelemetryEventInfoEqualityComparer Instance { get { return s_Instance; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static TelemetryEventInfoEqualityComparer()
		{
			s_Instance = new TelemetryEventInfoEqualityComparer();
		}

		public bool Equals(EventInfo x, EventInfo y)
		{
			return x.Name == y.Name;
		}

		public int GetHashCode(EventInfo obj)
		{
			return obj.Name.GetHashCode();
		}
	}
}