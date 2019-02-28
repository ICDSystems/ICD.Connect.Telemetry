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
		public static ITelemetryCollection InstantiateTelemetry(ITelemetryProvider instance)
		{
			TelemetryCollection collection = new TelemetryCollection();
			InstantiatePropertyTelemetry(instance, collection);
			InstantiateMethodTelemetry(instance, collection);
			InstantiateExternalTelemetry(instance, collection);
			return collection;
		}

		private static void InstantiatePropertyTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<PropertyInfo> properties = GetPropertiesWithTelemetryAttributes(instance.GetType());

			foreach (PropertyInfo property in properties)
			{
				ITelemetryAttribute attribute = GetTelemetryAttribute(property);
				if (attribute == null)
					continue;

				IPropertyTelemetryAttribute singleTelemetryAttribute = attribute as IPropertyTelemetryAttribute;
				if(singleTelemetryAttribute != null)
					collection.Add(singleTelemetryAttribute.InstantiateTelemetryItem(instance, property));

				ICollectionTelemetryAttribute multiTelemetryAttribute = attribute as ICollectionTelemetryAttribute;
				if(multiTelemetryAttribute != null)
					collection.Add(multiTelemetryAttribute.InstantiateTelemetryItem(instance, property));
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

		private static void InstantiateExternalTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<IExternalTelemetryProvider> externalTelemetryProviders = GetExternalTelemetryProviders(instance);

			foreach (IExternalTelemetryProvider provider in externalTelemetryProviders)
			{
				InstantiatePropertyTelemetry(provider, collection);
				InstantiateMethodTelemetry(provider, collection);
			}
		}

		private static IEnumerable<IExternalTelemetryProvider> GetExternalTelemetryProviders(ITelemetryProvider instance)
		{
			IEnumerable<ExternalTelemetryAttribute> externalTelemetryAttributes = GetExternalTelemetryAttributes(instance);

			return externalTelemetryAttributes.Select(attr => BuildExternalTelemetryProviderFromAttribute(instance, attr));
		}

		private static IExternalTelemetryProvider BuildExternalTelemetryProviderFromAttribute(ITelemetryProvider instance, IExternalTelemetryAttribute attr)
		{
			return attr.InstantiateTelemetryItem(instance);
		}

		private static IEnumerable<ExternalTelemetryAttribute> GetExternalTelemetryAttributes(ITelemetryProvider instance)
		{
			return Attribute.GetCustomAttributes(instance.GetType()).OfType<ExternalTelemetryAttribute>();
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
		private static ITelemetryAttribute GetTelemetryAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

// ReSharper disable InvokeAsExtensionMethod
			return ReflectionExtensions.GetCustomAttributes<ITelemetryAttribute>(property, true).FirstOrDefault();
// ReSharper restore InvokeAsExtensionMethod
		}

		[CanBeNull]
		private static IMethodTelemetryAttribute GetTelemetryAttribute(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentException("method");

// ReSharper disable InvokeAsExtensionMethod
			return ReflectionExtensions.GetCustomAttributes<IMethodTelemetryAttribute>(method, true).FirstOrDefault();
// ReSharper restore InvokeAsExtensionMethod
		}
	}
}