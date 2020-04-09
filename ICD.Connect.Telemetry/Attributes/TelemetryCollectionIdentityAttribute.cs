using System;
using System.Linq;
using Crestron.SimplSharp.Reflection;
using ICD.Common.Properties;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class TelemetryCollectionIdentityAttribute : AbstractIcdAttribute
	{
		[CanBeNull]
		public static PropertyInfo GetProperty([NotNull] object instance)
		{
			// todo - cache
			return
				instance.GetType()
						.GetAllTypes()
#if SIMPLSHARP
						.Select(t => t.GetCType())
#endif
						.SelectMany(t => t.GetProperties())
						.SingleOrDefault(p => p.GetCustomAttributes(typeof(TelemetryCollectionIdentityAttribute), true).Any());
		}
	}
}