using System;
using System.Collections.Generic;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class TelemetryCollectionIdentityAttribute : AbstractIcdAttribute
	{
		private static readonly Dictionary<Type, PropertyInfo> s_IdentityCache;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static TelemetryCollectionIdentityAttribute()
		{
			s_IdentityCache = new Dictionary<Type, PropertyInfo>();
		}

		/// <summary>
		/// Gets the identity property for the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		[CanBeNull]
		public static PropertyInfo GetProperty([NotNull] object instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			Type type = instance.GetType();
			return s_IdentityCache.GetOrAddNew(type, () => GetPropertyUncached(instance));
		}

		/// <summary>
		/// Gets the identity property for the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		[CanBeNull]
		private static PropertyInfo GetPropertyUncached([NotNull] object instance)
		{
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