using System;
using System.Collections;
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
	public sealed class CollectionTelemetryAttribute : AbstractTelemetryAttribute
	{
		private const BindingFlags BINDING_FLAGS =
			BindingFlags.Instance | // Non-static classes
			BindingFlags.Static | // Static classes
			BindingFlags.Public | // Public members only
			BindingFlags.DeclaredOnly; // No inherited members

		private static readonly Dictionary<Type, Dictionary<PropertyInfo, CollectionTelemetryAttribute>> s_TypeToPropertyInfo;
		private static readonly SafeCriticalSection s_CacheSection;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static CollectionTelemetryAttribute()
		{
			s_TypeToPropertyInfo = new Dictionary<Type, Dictionary<PropertyInfo, CollectionTelemetryAttribute>>();
			s_CacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		public CollectionTelemetryAttribute(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Returns the child telemetry nodes for the given instance and collection property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="getTelemetryForProvider"></param>
		/// <returns></returns>
		public IEnumerable<ITelemetryNode> InstantiateTelemetryNodes([NotNull] ITelemetryProvider instance,
		                                                             [NotNull] PropertyInfo propertyInfo,
		                                                             Func<string, ITelemetryProvider,
			                                                             TelemetryCollection> getTelemetryForProvider)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (propertyInfo == null)
				throw new ArgumentNullException("propertyInfo");

			IEnumerable childInstances =
				typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType)
					? (IEnumerable)propertyInfo.GetValue(instance, null)
					: propertyInfo.GetValue(instance, null).Yield();

			IEnumerable<ITelemetryProvider> childrenProviders = childInstances.OfType<ITelemetryProvider>();

			int index = 0;
			foreach (ITelemetryProvider provider in childrenProviders)
			{
				PropertyInfo idProperty = TelemetryCollectionIdentityAttribute.GetProperty(provider);

				string name =
					idProperty == null
						? string.Format("{0}{1}", Name, index)
						: idProperty.GetValue(provider, null).ToString();

				yield return getTelemetryForProvider(name, provider);
				index++;
			}
		}

		/// <summary>
		/// Gets the properties on the given type decorated for telemetry.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<PropertyInfo, CollectionTelemetryAttribute>> GetProperties([NotNull] Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				Dictionary<PropertyInfo, CollectionTelemetryAttribute> propertyInfos;

				if (!s_TypeToPropertyInfo.TryGetValue(type, out propertyInfos))
				{
					propertyInfos =
						new Dictionary<PropertyInfo, CollectionTelemetryAttribute>(TelemetryPropertyInfoEqualityComparer.Instance);
					s_TypeToPropertyInfo.Add(type, propertyInfos);

					// First get the inherited properties
					IEnumerable<KeyValuePair<PropertyInfo, CollectionTelemetryAttribute>> inheritedProperties =
						type.GetAllTypes()
							.Except(type)
							.SelectMany(t => GetProperties(t));

					// Then get the properties for this type
					IEnumerable<KeyValuePair<PropertyInfo, CollectionTelemetryAttribute>> typeProperties =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
							.GetProperties(BINDING_FLAGS)
							.Select(p => new KeyValuePair<PropertyInfo, CollectionTelemetryAttribute>(p, GetTelemetryAttribute(p)))
							.Where(kvp => kvp.Value != null);

					// Then insert everything - Type properties win
					foreach (KeyValuePair<PropertyInfo, CollectionTelemetryAttribute> kvp in inheritedProperties.Concat(typeProperties))
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
		private static CollectionTelemetryAttribute GetTelemetryAttribute([NotNull] PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			// ReSharper disable InvokeAsExtensionMethod
			return ReflectionExtensions.GetCustomAttributes<CollectionTelemetryAttribute>(property, true).FirstOrDefault();
			// ReSharper restore InvokeAsExtensionMethod
		}
	}
}
