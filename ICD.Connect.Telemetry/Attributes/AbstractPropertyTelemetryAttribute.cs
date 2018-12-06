using System;
using Crestron.SimplSharp.Reflection;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public abstract class AbstractPropertyTelemetryAttribute : AbstractTelemetryAttribute, IPropertyTelemetryAttribute
	{
		protected AbstractPropertyTelemetryAttribute(string name) : base(name)
		{
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public abstract IFeedbackTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, PropertyInfo propertyInfo);
	}
}