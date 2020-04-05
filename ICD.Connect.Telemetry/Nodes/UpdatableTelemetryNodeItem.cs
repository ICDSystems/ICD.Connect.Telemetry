﻿#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry.Nodes
{
	public sealed class UpdatableTelemetryNodeItem<T> : AbstractUpdatableTelemetryNodeItem<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="propertyInfo"></param>
		/// <param name="setTelemetry"></param>
		public UpdatableTelemetryNodeItem(string name, ITelemetryProvider parent, PropertyInfo propertyInfo, string setTelemetry)
			: base(name, parent, propertyInfo, setTelemetry)
		{
		}
	}
}
