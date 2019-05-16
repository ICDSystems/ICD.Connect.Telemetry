using System;
using ICD.Common.Utils;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class ExternalTelemetryAttribute : AbstractTelemetryAttribute, IExternalTelemetryAttribute
	{
		private readonly Type m_ExternalTelemetryProviderType;

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

		/// <summary>
		/// Instantiates a new telemetry item for the given instance.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public IExternalTelemetryProvider InstantiateTelemetryItem(ITelemetryProvider parent)
		{
			IExternalTelemetryProvider provider = (IExternalTelemetryProvider)ReflectionUtils.CreateInstance(m_ExternalTelemetryProviderType);
			provider.SetParent(parent);
			return provider;
		}
	}
}