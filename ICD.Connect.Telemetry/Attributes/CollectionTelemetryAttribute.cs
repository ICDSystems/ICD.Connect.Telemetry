using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Utils.Collections;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	public sealed class CollectionTelemetryAttribute : AbstractTelemetryAttribute, ICollectionTelemetryAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		public CollectionTelemetryAttribute(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public ICollectionTelemetryItem InstantiateTelemetryItem(ITelemetryProvider instance, PropertyInfo propertyInfo)
		{
			if (!typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
				throw new InvalidOperationException(
					string.Format("Cannot generate collection telemetry for non-enumerable property {0}, {1}", propertyInfo.Name,
					              propertyInfo.PropertyType));

			IEnumerable childInstance = (IEnumerable)propertyInfo.GetValue(instance, null);
			if (childInstance == null)
				throw new InvalidProgramException(string.Format("Property {0} value is null", propertyInfo.Name));

			IEnumerable<ITelemetryProvider> childrenProviders = childInstance.OfType<ITelemetryProvider>();

			IcdHashSet<ITelemetryItem> listItemNodes = new IcdHashSet<ITelemetryItem>();
			int index = 0;
			foreach (var provider in childrenProviders)
			{
				ITelemetryCollection innerTelemetry = TelemetryUtils.InstantiateTelemetry(provider);
				listItemNodes.Add(new CollectionTelemetryNodeItem(string.Format("{0}[{1}]", Name, index), instance, innerTelemetry));
				index++;
			}

			return new CollectionTelemetryNodeItem(Name, instance, listItemNodes);
		}
	}
}
