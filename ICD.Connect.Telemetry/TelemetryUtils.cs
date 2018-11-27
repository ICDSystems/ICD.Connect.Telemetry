using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Attributes;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry
{
	public static class TelemetryUtils
	{
		[PublicAPI]
		public static void InstantiateTelemetry(ITelemetryProvider instance)
		{
			InstantiatePropertyTelemetry(instance);
			//InstantiateMethodTelemetry(instance);
		}

		private static void InstantiatePropertyTelemetry(ITelemetryProvider instance)
		{
			IEnumerable<PropertyInfo> properties = GetPropertiesWithTelemetryAttributes(instance.GetType());

			foreach (PropertyInfo property in properties)
			{
				ITelemetryPropertyAttribute attribute = GetTelemetryAttribute(property);
				if (attribute == null)
					continue;

				instance.Telemetry.Add(attribute.InstantiateTelemetryItem(instance, property));
			}
		}

		private static IEnumerable<PropertyInfo> GetPropertiesWithTelemetryAttributes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return
				type.GetAllTypes()
				    .SelectMany(t =>
#if SIMPLSHARP
				                ((CType)t)
#else
										t.GetTypeInfo()
#endif
					                .GetProperties())
				    .Where(p => GetTelemetryAttribute(p) != null)
				    .Distinct(TelemetryPropertyInfoEqualityComparer.Instance);
		}

		[CanBeNull]
		private static ITelemetryPropertyAttribute GetTelemetryAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			return property.GetCustomAttributes<ITelemetryPropertyAttribute>(true).FirstOrDefault();
		}
	}
}