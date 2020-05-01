using System;
using ICD.Common.Properties;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Telemetry.Nodes
{
	public abstract class AbstractFeedbackTelemetryNodeItem<T> : AbstractTelemetryNodeItemBase, IFeedbackTelemetryItem
	{
		private readonly PropertyInfo m_PropertyInfo;

		/// <summary>
		/// Gets the value from the property.
		/// </summary>
		object IFeedbackTelemetryItem.Value { get { return Value; } }

		/// <summary>
		/// Gets the value from the property.
		/// </summary>
		public T Value { get { return (T)m_PropertyInfo.GetValue(Parent, null); } }

		/// <summary>
		/// Gets the property info for the property.
		/// </summary>
		public PropertyInfo PropertyInfo { get { return m_PropertyInfo; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="propertyInfo"></param>
		protected AbstractFeedbackTelemetryNodeItem(string name, [NotNull] ITelemetryProvider parent, [NotNull] PropertyInfo propertyInfo) 
			: base(name, parent)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException("propertyInfo");

			m_PropertyInfo = propertyInfo;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Property Type", PropertyInfo.PropertyType);
			addRow("Property Value", Value.ToString());
		}
	}
}