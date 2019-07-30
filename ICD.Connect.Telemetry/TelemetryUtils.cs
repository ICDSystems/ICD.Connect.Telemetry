using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Comparers;
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
		private static readonly Dictionary<Type, Dictionary<PropertyInfo, ITelemetryAttribute>> s_TypeToPropertyInfo;
		private static readonly Dictionary<Type, Dictionary<MethodInfo, IMethodTelemetryAttribute>> s_TypeToMethodInfo;
		private static readonly Dictionary<Type, Dictionary<EventInfo, IEventTelemetryAttribute>> s_TypeToEventInfo;
		private static readonly SafeCriticalSection s_CacheSection;

		private static BindingFlags s_BindingFlags =
			BindingFlags.Instance | // Non-static classes
			BindingFlags.Static | // Static classes
			BindingFlags.Public | // Public members only
			BindingFlags.DeclaredOnly; // No inherited members

		/// <summary>
		/// Static constructor.
		/// </summary>
		static TelemetryUtils()
		{
			s_TypeToExternalTelemetryAttributes = new Dictionary<Type, IcdHashSet<ExternalTelemetryAttribute>>();
			s_TypeToPropertyInfo = new Dictionary<Type, Dictionary<PropertyInfo, ITelemetryAttribute>>();
			s_TypeToMethodInfo = new Dictionary<Type, Dictionary<MethodInfo, IMethodTelemetryAttribute>>();
			s_TypeToEventInfo = new Dictionary<Type, Dictionary<EventInfo, IEventTelemetryAttribute>>();
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
			IEnumerable<KeyValuePair<PropertyInfo, ITelemetryAttribute>> properties =
				GetPropertiesWithTelemetryAttributes(instance.GetType());

			foreach (KeyValuePair<PropertyInfo, ITelemetryAttribute> kvp in properties)
			{
				IPropertyTelemetryAttribute singleTelemetryAttribute = kvp.Value as IPropertyTelemetryAttribute;
				if (singleTelemetryAttribute != null)
					collection.Add(singleTelemetryAttribute.InstantiateTelemetryItem(instance, kvp.Key));

				ICollectionTelemetryAttribute multiTelemetryAttribute = kvp.Value as ICollectionTelemetryAttribute;
				if (multiTelemetryAttribute != null)
					collection.Add(multiTelemetryAttribute.InstantiateTelemetryItem(instance, kvp.Key));
			}
		}

		private static void InstantiateMethodTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<KeyValuePair<MethodInfo, IMethodTelemetryAttribute>> methods =
				GetMethodsWithTelemetryAttributes(instance.GetType());

			foreach (KeyValuePair<MethodInfo, IMethodTelemetryAttribute> kvp in methods)
				collection.Add(kvp.Value.InstantiateTelemetryItem(instance, kvp.Key));
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

		[NotNull]
		public static EventInfo GetEventInfo(ITelemetryProvider instance, string eventName)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			Type type = instance.GetType();
			return GetEventInfo(type, eventName);
		}

		[NotNull]
		public static EventInfo GetEventInfo(Type type, string eventName)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return GetEventsWithTelemetryAttributes(type).First(kvp => kvp.Value.Name == eventName).Key;
		}

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

		private static IEnumerable<KeyValuePair<PropertyInfo, ITelemetryAttribute>>
			GetPropertiesWithTelemetryAttributes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				Dictionary<PropertyInfo, ITelemetryAttribute> propertyInfos;

				if (!s_TypeToPropertyInfo.TryGetValue(type, out propertyInfos))
				{
					propertyInfos = new Dictionary<PropertyInfo, ITelemetryAttribute>(TelemetryPropertyInfoEqualityComparer.Instance);
					s_TypeToPropertyInfo.Add(type, propertyInfos);

					// First get the inherited properties
					IEnumerable<KeyValuePair<PropertyInfo, ITelemetryAttribute>> inheritedProperties =
						type.GetAllTypes()
						    .Except(type)
						    .SelectMany(t => GetPropertiesWithTelemetryAttributes(t));

					// Then get the properties for this type
					IEnumerable<KeyValuePair<PropertyInfo, ITelemetryAttribute>> typeProperties =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
						    .GetProperties(s_BindingFlags)
						    .Select(p => new KeyValuePair<PropertyInfo, ITelemetryAttribute>(p, GetTelemetryAttribute(p)))
						    .Where(kvp => kvp.Value != null);

					// Then insert everything - Type properties win
					foreach (KeyValuePair<PropertyInfo, ITelemetryAttribute> kvp in inheritedProperties.Concat(typeProperties))
						propertyInfos[kvp.Key] = kvp.Value;
				}

				return propertyInfos;
			}
			finally
			{
				s_CacheSection.Leave();
			}
		}

		private static IEnumerable<KeyValuePair<MethodInfo, IMethodTelemetryAttribute>> GetMethodsWithTelemetryAttributes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				Dictionary<MethodInfo, IMethodTelemetryAttribute> methodInfos;

				if (!s_TypeToMethodInfo.TryGetValue(type, out methodInfos))
				{
					methodInfos = new Dictionary<MethodInfo, IMethodTelemetryAttribute>(TelemetryMethodInfoEqualityComparer.Instance);
					s_TypeToMethodInfo.Add(type, methodInfos);

					// First get the inherited methods
					IEnumerable<KeyValuePair<MethodInfo, IMethodTelemetryAttribute>> inheritedMethods =
						type.GetAllTypes()
						    .Except(type)
						    .SelectMany(t => GetMethodsWithTelemetryAttributes(t));

					// Then get the methods for this type
					IEnumerable<KeyValuePair<MethodInfo, IMethodTelemetryAttribute>> typeMethods =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
						    .GetMethods(s_BindingFlags)
						    .Select(m => new KeyValuePair<MethodInfo, IMethodTelemetryAttribute>(m, GetTelemetryAttribute(m)))
						    .Where(kvp => kvp.Value != null);

					// Then insert everything - Type methods win
					foreach (KeyValuePair<MethodInfo, IMethodTelemetryAttribute> kvp in inheritedMethods.Concat(typeMethods))
						methodInfos[kvp.Key] = kvp.Value;
				}

				return methodInfos;
			}
			finally
			{
				s_CacheSection.Leave();
			}
		}

		private static IEnumerable<KeyValuePair<EventInfo, IEventTelemetryAttribute>> GetEventsWithTelemetryAttributes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				Dictionary<EventInfo, IEventTelemetryAttribute> eventInfos;

				if (!s_TypeToEventInfo.TryGetValue(type, out eventInfos))
				{
					eventInfos = new Dictionary<EventInfo, IEventTelemetryAttribute>(TelemetryEventInfoEqualityComparer.Instance);
					s_TypeToEventInfo.Add(type, eventInfos);

					// First get the inherited events
					IEnumerable<KeyValuePair<EventInfo, IEventTelemetryAttribute>> inheritedEvents =
						type.GetAllTypes()
						    .Except(type)
						    .SelectMany(t => GetEventsWithTelemetryAttributes(t));

					// Then get the events for this type
					IEnumerable<KeyValuePair<EventInfo, IEventTelemetryAttribute>> typeEvents =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
						    .GetEvents(s_BindingFlags)
						    .Select(e => new KeyValuePair<EventInfo, IEventTelemetryAttribute>(e, GetTelemetryAttribute(e)))
						    .Where(kvp => kvp.Value != null);

					// Then insert everything - Type events win
					foreach (KeyValuePair<EventInfo, IEventTelemetryAttribute> kvp in inheritedEvents.Concat(typeEvents))
						eventInfos[kvp.Key] = kvp.Value;
				}

				return eventInfos;
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

		[CanBeNull]
		private static IEventTelemetryAttribute GetTelemetryAttribute(EventInfo eventInfo)
		{
			if (eventInfo == null)
				throw new ArgumentException("eventInfo");

			// ReSharper disable InvokeAsExtensionMethod
			return ReflectionExtensions.GetCustomAttributes<IEventTelemetryAttribute>(eventInfo, true).FirstOrDefault();
			// ReSharper restore InvokeAsExtensionMethod
		}

		#endregion
	}
}