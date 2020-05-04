using System.Collections.Generic;
using ICD.Common.Utils;

namespace ICD.Connect.Telemetry.Nodes.Collections
{
	public sealed class CollectionTelemetryNodeItem : AbstractCollectionTelemetryWithParent, ICollectionTelemetryItem
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="childNodes"></param>
		public CollectionTelemetryNodeItem(string name, ITelemetryProvider parent, IEnumerable<ITelemetryItem> childNodes) 
			: base(parent)
		{
			Name = name;

			foreach (ITelemetryItem item in childNodes)
				Add(item);
		}

		public override string ToString()
		{
			var x = new ReprBuilder(this);
			x.AppendProperty("Name", Name);
			x.AppendProperty("Parent", Parent);
			return x.ToString();
		}
	}
}
