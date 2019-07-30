using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
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
		private static readonly Dictionary<Type, IcdHashSet<ExternalTelemetryAttribute>> s_TypeToExternalTelemetryAttributes;
		private static readonly Dictionary<Type, IcdHashSet<PropertyInfo>> s_TypeToPropertyInfo;
		private static readonly Dictionary<Type, IcdHashSet<MethodInfo>> s_TypeToMethodInfo;
		private static readonly SafeCriticalSection s_CacheSection;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static TelemetryUtils()
		{
			s_TypeToExternalTelemetryAttributes = new Dictionary<Type, IcdHashSet<ExternalTelemetryAttribute>>();
			s_TypeToPropertyInfo = new Dictionary<Type, IcdHashSet<PropertyInfo>>();
			s_TypeToMethodInfo = new Dictionary<Type, IcdHashSet<MethodInfo>>();
			s_CacheSection = new SafeCriticalSection();
		}

		#region Instantiate

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
				InstantiateExternalTelemetry(provider, collection);
			}
		}

		#endregion

		#region Reflection

		private static IEnumerable<IExternalTelemetryProvider> GetExternalTelemetryProviders(ITelemetryProvider instance)
		{
			IEnumerable<ExternalTelemetryAttribute> externalTelemetryAttributes = GetExternalTelemetryAttributes(instance);

			return externalTelemetryAttributes.Select(attr => attr.InstantiateTelemetryItem(instance));
		}

		private static IEnumerable<ExternalTelemetryAttribute> GetExternalTelemetryAttributes([NotNull]ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return GetExternalTelemetryAttributes(instance.GetType());
		}

		private static IEnumerable<ExternalTelemetryAttribute> GetExternalTelemetryAttributes([NotNull]Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				IcdHashSet<ExternalTelemetryAttribute> attributes;

				if (!s_TypeToExternalTelemetryAttributes.TryGetValue(type, out attributes))
				{
					attributes = type.GetAllTypes()
					                 .Except(type)
					                 .SelectMany(t => GetExternalTelemetryAttributes(t))
									 .ToIcdHashSet();

					IEnumerable<ExternalTelemetryAttribute> externalAttributes =
						AttributeUtils.GetClassAttributes<ExternalTelemetryAttribute>(type);

					attributes.AddRange(externalAttributes);

					s_TypeToExternalTelemetryAttributes.Add(type, attributes);
				}

				return attributes;
			}
			finally
			{
				s_CacheSection.Leave();
			}
		}

		private static IEnumerable<PropertyInfo> GetPropertiesWithTelemetryAttributes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				IcdHashSet<PropertyInfo> propertyInfos;

				if (!s_TypeToPropertyInfo.TryGetValue(type, out propertyInfos))
				{
					propertyInfos = type.GetAllTypes()
										.Except(type)
										.SelectMany(t => GetPropertiesWithTelemetryAttributes(t))
										.ToIcdHashSet(TelemetryPropertyInfoEqualityComparer.Instance);

					IEnumerable<PropertyInfo> typeProperties =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
						    .GetProperties()
						    .Where(p => GetTelemetryAttribute(p) != null);

					propertyInfos.AddRange(typeProperties);

					s_TypeToPropertyInfo.Add(type, propertyInfos);
				}

				return propertyInfos;
			}
			finally
			{
				s_CacheSection.Leave();
			}
		}

		private static IEnumerable<MethodInfo> GetMethodsWithTelemetryAttributes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				IcdHashSet<MethodInfo> methodInfos;

				if (!s_TypeToMethodInfo.TryGetValue(type, out methodInfos))
				{
					methodInfos = type.GetAllTypes()
					                  .Except(type)
					                  .SelectMany(t => GetMethodsWithTelemetryAttributes(t))
					                  .ToIcdHashSet(TelemetryMethodInfoEqualityComparer.Instance);

					IEnumerable<MethodInfo> typeMethods =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
						    .GetMethods()
						    .Where(m => GetTelemetryAttribute(m) != null);

					methodInfos.AddRange(typeMethods);

					s_TypeToMethodInfo.Add(type, methodInfos);
				}

				return methodInfos;
			}
			finally
			{
				s_CacheSection.Leave();
			}
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

		#endregion
	}
}