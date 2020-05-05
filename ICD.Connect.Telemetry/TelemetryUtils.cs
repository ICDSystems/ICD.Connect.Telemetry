using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Nodes;
using ICD.Connect.Telemetry.Nodes.Collections;
using ICD.Connect.Telemetry.Nodes.External;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.Telemetry
{
	public static class TelemetryUtils
	{
		#region Instantiate

		/// <summary>
		/// Recursively creates the telemetry for the given provider.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		[PublicAPI]
		public static ITelemetryCollection InstantiateTelemetry([NotNull] ITelemetryProvider instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			TelemetryCollection collection = new TelemetryCollection();
			InstantiatePropertyTelemetry(instance, collection);
			InstantiateMethodTelemetry(instance, collection);
			InstantiateCollectionTelemetry(instance, collection);
			InstantiateExternalTelemetry(instance, collection);
			return collection;
		}

		private static void InstantiatePropertyTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<KeyValuePair<PropertyInfo, PropertyTelemetryAttribute>> properties =
				PropertyTelemetryAttribute.GetProperties(instance.GetType());

			foreach (KeyValuePair<PropertyInfo, PropertyTelemetryAttribute> kvp in properties)
				collection.Add(kvp.Value.InstantiateTelemetryItem(instance, kvp.Key));
		}

		private static void InstantiateCollectionTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<ICollectionTelemetryItem> properties =
				CollectionTelemetryAttribute.GetProperties(instance.GetType())
				                            .Select(kvp => kvp.Value.InstantiateTelemetryItem(instance, kvp.Key));

			foreach (ICollectionTelemetryItem item in properties)
				collection.Add(item);
		}

		private static void InstantiateMethodTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<KeyValuePair<MethodInfo, MethodTelemetryAttribute>> methods =
				MethodTelemetryAttribute.GetMethods(instance.GetType());

			foreach (KeyValuePair<MethodInfo, MethodTelemetryAttribute> kvp in methods)
				collection.Add(kvp.Value.InstantiateTelemetryItem(instance, kvp.Key));
		}

		private static void InstantiateExternalTelemetry(ITelemetryProvider instance, ITelemetryCollection collection)
		{
			IEnumerable<IExternalTelemetryProvider> externalTelemetryProviders =
				ExternalTelemetryAttribute.InstantiateExternalTelemetryProviders(instance);

			foreach (IExternalTelemetryProvider provider in externalTelemetryProviders)
			{
				InstantiatePropertyTelemetry(provider, collection);
				InstantiateMethodTelemetry(provider, collection);
				InstantiateExternalTelemetry(provider, collection);
			}
		}

		#endregion

		#region Reflection

		[NotNull]
		public static EventInfo GetEventInfo([NotNull] ITelemetryProvider instance, [NotNull] string eventName)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (string.IsNullOrEmpty(eventName))
				throw new ArgumentException("Event name must not be null or empty", "eventName");

			Type type = instance.GetType();
			return EventTelemetryAttribute.GetEvents(type).First(kvp => kvp.Value.Name == eventName).Key;
		}

		[NotNull]
		public static MethodInfo GetMethodInfo([NotNull] ITelemetryProvider instance, [NotNull] string methodName)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentException("Method name must not be null or empty", "methodName");

			Type type = instance.GetType();
			return MethodTelemetryAttribute.GetMethods(type).First(kvp => kvp.Value.Name == methodName).Key;
		}

		#endregion
	}
}
