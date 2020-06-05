using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Comparers;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class PropertyTelemetryAttribute : AbstractTelemetryAttribute
	{
		private const BindingFlags BINDING_FLAGS =
			BindingFlags.Instance | // Non-static classes
			BindingFlags.Static | // Static classes
			BindingFlags.Public | // Public members only
			BindingFlags.DeclaredOnly; // No inherited members

		private static readonly Dictionary<Type, Dictionary<PropertyInfo, PropertyTelemetryAttribute>> s_TypeToPropertyInfo;
		private static readonly SafeCriticalSection s_CacheSection;

		[CanBeNull] private readonly string m_MethodName;
		[CanBeNull] private readonly string m_EventName;

		/// <summary>
		/// Gets the associated event name for this attribute.
		/// </summary>
		[CanBeNull]
		public string EventName { get { return m_EventName; } }

		/// <summary>
		/// Gets the associated method name for this attribute.
		/// </summary>
		[CanBeNull]
		public string MethodName { get { return m_MethodName; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static PropertyTelemetryAttribute()
		{
			s_TypeToPropertyInfo = new Dictionary<Type, Dictionary<PropertyInfo, PropertyTelemetryAttribute>>();
			s_CacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="methodName"></param>
		/// <param name="eventName"></param>
		public PropertyTelemetryAttribute([NotNull] string name, [CanBeNull] string methodName, [CanBeNull] string eventName)
			: base(name)
		{
			m_MethodName = methodName;
			m_EventName = eventName;
		}

		/// <summary>
		/// Gets the properties on the given type decorated for telemetry.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<PropertyInfo, PropertyTelemetryAttribute>> GetProperties(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				Dictionary<PropertyInfo, PropertyTelemetryAttribute> propertyInfos;

				if (!s_TypeToPropertyInfo.TryGetValue(type, out propertyInfos))
				{
					propertyInfos =
						new Dictionary<PropertyInfo, PropertyTelemetryAttribute>(TelemetryPropertyInfoEqualityComparer.Instance);
					s_TypeToPropertyInfo.Add(type, propertyInfos);

					// First get the inherited properties
					IEnumerable<KeyValuePair<PropertyInfo, PropertyTelemetryAttribute>> inheritedProperties =
						type.GetAllTypes()
						    .Except(type)
						    .SelectMany(t => GetProperties(t));

					// Then get the properties for this type
					IEnumerable<KeyValuePair<PropertyInfo, PropertyTelemetryAttribute>> typeProperties =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
							.GetProperties(BINDING_FLAGS)
							.Select(p => new KeyValuePair<PropertyInfo, PropertyTelemetryAttribute>(p, GetTelemetryAttribute(p)))
							.Where(kvp => kvp.Value != null);

					// Then insert everything - Type properties win
					foreach (KeyValuePair<PropertyInfo, PropertyTelemetryAttribute> kvp in inheritedProperties.Concat(typeProperties))
						propertyInfos[kvp.Key] = kvp.Value;
				}

				return propertyInfos;
			}
			finally
			{
				s_CacheSection.Leave();
			}
		}

		[CanBeNull]
		private static PropertyTelemetryAttribute GetTelemetryAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			// ReSharper disable InvokeAsExtensionMethod
			return ReflectionExtensions.GetCustomAttributes<PropertyTelemetryAttribute>(property, true).FirstOrDefault();
			// ReSharper restore InvokeAsExtensionMethod
		}
	}
}
