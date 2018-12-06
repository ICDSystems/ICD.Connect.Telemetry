using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Nodes;
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
		public static ITelemetryCollection InstantiateTelemetry(ITelemetryProvider instance)
		{
			//TODO: replace this with an actual root node item
			var collection = new StaticTelemetryNodeItem<object>("Telemetry", null);
			InstantiatePropertyTelemetry(instance, collection);
			InstantiateMethodTelemetry(instance, collection);
			return collection;
		}

		private static void InstantiatePropertyTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<PropertyInfo> properties = GetPropertiesWithTelemetryAttributes(instance.GetType());

			foreach (PropertyInfo property in properties)
			{
				IPropertyTelemetryAttribute attribute = GetTelemetryAttribute(property);
				if (attribute == null)
					continue;

				collection.Add(attribute.InstantiateTelemetryItem(instance, property));
			}
		}

		private static void InstantiateMethodTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<MethodInfo> methods = GetMethodsWithTelemetryAttributes(instance.GetType());

			foreach (MethodInfo method in methods)
			{
				IMethodTelemetryAttribute attribute = GetTelemetryAttribute(method);
				if (attribute == null)
					continue;

				collection.Add(attribute.InstantiateTelemetryItem(instance, method));
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

		private static IEnumerable<MethodInfo> GetMethodsWithTelemetryAttributes(Type type)
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
					                .GetMethods())
				    .Where(m => GetTelemetryAttribute(m) != null)
				    .Distinct(TelemetryMethodInfoEqualityComparer.Instance);
		}
			
		[CanBeNull]
		private static IPropertyTelemetryAttribute GetTelemetryAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			return property.GetCustomAttributes<IPropertyTelemetryAttribute>(true).FirstOrDefault();
		}

		[CanBeNull]
		private static IMethodTelemetryAttribute GetTelemetryAttribute(MethodInfo method)
		{
			if(method == null)
				throw new ArgumentException("method");

			return method.GetCustomAttributes<IMethodTelemetryAttribute>(true).FirstOrDefault();
		}
	}
}