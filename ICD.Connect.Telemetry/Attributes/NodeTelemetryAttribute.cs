using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Comparers;
using ICD.Connect.Telemetry.Providers;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	public sealed class NodeTelemetryAttribute : AbstractTelemetryAttribute
	{
		private const BindingFlags BINDING_FLAGS =
			BindingFlags.Instance | // Non-static classes
			BindingFlags.Static | // Static classes
			BindingFlags.Public | // Public members only
			BindingFlags.DeclaredOnly; // No inherited members

		private static readonly Dictionary<Type, Dictionary<PropertyInfo, NodeTelemetryAttribute>> s_TypeToPropertyInfo;
		private static readonly SafeCriticalSection s_CacheSection;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static NodeTelemetryAttribute()
		{
			s_TypeToPropertyInfo = new Dictionary<Type, Dictionary<PropertyInfo, NodeTelemetryAttribute>>();
			s_CacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		public NodeTelemetryAttribute(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Returns the child telemetry node for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="getTelemetryForProvider"></param>
		/// <returns></returns>
		public TelemetryProviderNode InstantiateTelemetryNode([NotNull] ITelemetryProvider instance,
		                                                      [NotNull] PropertyInfo propertyInfo,
		                                                      Func<string, ITelemetryProvider,
			                                                      TelemetryProviderNode> getTelemetryForProvider)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (propertyInfo == null)
				throw new ArgumentNullException("propertyInfo");

			ITelemetryProvider childInstance = (ITelemetryProvider)propertyInfo.GetValue(instance, null);
			return getTelemetryForProvider(Name, childInstance);
		}

		/// <summary>
		/// Gets the properties on the given type decorated for telemetry.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<PropertyInfo, NodeTelemetryAttribute>> GetProperties([NotNull] Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				Dictionary<PropertyInfo, NodeTelemetryAttribute> propertyInfos;

				if (!s_TypeToPropertyInfo.TryGetValue(type, out propertyInfos))
				{
					propertyInfos =
						new Dictionary<PropertyInfo, NodeTelemetryAttribute>(TelemetryPropertyInfoEqualityComparer.Instance);
					s_TypeToPropertyInfo.Add(type, propertyInfos);

					// First get the inherited properties
					IEnumerable<KeyValuePair<PropertyInfo, NodeTelemetryAttribute>> inheritedProperties =
						type.GetAllTypes()
							.Except(type)
							.SelectMany(t => GetProperties(t));

					// Then get the properties for this type
					IEnumerable<KeyValuePair<PropertyInfo, NodeTelemetryAttribute>> typeProperties =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
							.GetProperties(BINDING_FLAGS)
							.Select(p => new KeyValuePair<PropertyInfo, NodeTelemetryAttribute>(p, GetTelemetryAttribute(p)))
							.Where(kvp => kvp.Value != null);

					// Then insert everything - Type properties win
					foreach (KeyValuePair<PropertyInfo, NodeTelemetryAttribute> kvp in inheritedProperties.Concat(typeProperties))
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
		private static NodeTelemetryAttribute GetTelemetryAttribute([NotNull] PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			// ReSharper disable InvokeAsExtensionMethod
			return ReflectionExtensions.GetCustomAttributes<NodeTelemetryAttribute>(property, true).FirstOrDefault();
			// ReSharper restore InvokeAsExtensionMethod
		}
	}
}
