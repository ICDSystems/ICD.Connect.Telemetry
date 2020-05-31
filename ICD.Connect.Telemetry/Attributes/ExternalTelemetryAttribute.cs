using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Connect.Telemetry.Providers;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class ExternalTelemetryAttribute : AbstractTelemetryAttribute
	{
		private static readonly Dictionary<Type, IcdHashSet<ExternalTelemetryAttribute>> s_TypeToExternalTelemetryAttributes;
		private static readonly SafeCriticalSection s_CacheSection;

		private readonly Type m_ExternalTelemetryProviderType;

		/// <summary>
		/// Constructor.
		/// </summary>
		static ExternalTelemetryAttribute()
		{
			s_TypeToExternalTelemetryAttributes = new Dictionary<Type, IcdHashSet<ExternalTelemetryAttribute>>();
			s_CacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public ExternalTelemetryAttribute(string name, Type type)
			: base(name)
		{
			m_ExternalTelemetryProviderType = type;
		}

		public static IEnumerable<IExternalTelemetryProvider> InstantiateExternalTelemetryProviders([NotNull] ITelemetryProvider instance)
		{
			return GetAttributes(instance).Select(attr => attr.InstantiateTelemetryItem(instance));
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		[NotNull]
		public IExternalTelemetryProvider InstantiateTelemetryItem([NotNull] ITelemetryProvider parent)
		{
			IExternalTelemetryProvider provider = (IExternalTelemetryProvider)ReflectionUtils.CreateInstance(m_ExternalTelemetryProviderType);
			provider.SetParent(parent);
			return provider;
		}

		private static IEnumerable<ExternalTelemetryAttribute> GetAttributes([NotNull] Type type)
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
									 .SelectMany(t => GetAttributes(t))
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

		private static IEnumerable<ExternalTelemetryAttribute> GetAttributes([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return GetAttributes(instance.GetType());
		}
	}
}
