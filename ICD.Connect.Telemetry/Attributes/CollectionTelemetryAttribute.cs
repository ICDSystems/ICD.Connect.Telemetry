using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.Reflection;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Connect.Telemetry.Nodes;

namespace ICD.Connect.Telemetry.Attributes
{
	public class CollectionTelemetryAttribute : AbstractTelemetryAttribute, ICollectionTelemetryAttribute
	{
		public CollectionTelemetryAttribute(string name) : base(name)
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
			if(propertyInfo.PropertyType != typeof(IEnumerable))
				throw new InvalidOperationException(string.Format("Cannot generate collection telemetry for non-enumerable property {0}", propertyInfo.Name));
            
			IEnumerable<ITelemetryProvider> childrenProviders =
				((IEnumerable)propertyInfo.GetValue(instance, null)).OfType<ITelemetryProvider>();

			IcdHashSet<ITelemetryItem> listItemNodes = new IcdHashSet<ITelemetryItem>();
			int index = 0;
			foreach (var provider in childrenProviders)
			{
				ITelemetryCollection innerTelemetry = TelemetryUtils.InstantiateTelemetry(provider);
				listItemNodes.Add(new CollectionTelemetryNodeItem(string.Format("{0}[{1}]", Name, index), innerTelemetry));
				index++;
			}

			return (ICollectionTelemetryItem)ReflectionUtils.CreateInstance(typeof(CollectionTelemetryNodeItem), Name, listItemNodes);
		}
	}
}