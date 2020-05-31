﻿using System;
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
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class MethodTelemetryAttribute : AbstractTelemetryAttribute
	{
		private const BindingFlags BINDING_FLAGS =
			BindingFlags.Instance | // Non-static classes
			BindingFlags.Static | // Static classes
			BindingFlags.Public | // Public members only
			BindingFlags.DeclaredOnly; // No inherited members

		private static readonly Dictionary<Type, Dictionary<MethodInfo, MethodTelemetryAttribute>> s_TypeToMethodInfo;
		private static readonly SafeCriticalSection s_CacheSection;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static MethodTelemetryAttribute()
		{
			s_TypeToMethodInfo = new Dictionary<Type, Dictionary<MethodInfo, MethodTelemetryAttribute>>();
			s_CacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		public MethodTelemetryAttribute(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Instantiates a new telemetry item for the given instance and property.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="methodInfo"></param>
		/// <returns></returns>
		[NotNull]
		public MethodTelemetryNode InstantiateTelemetryItem(ITelemetryProvider instance, MethodInfo methodInfo)
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();

			if (parameters.Length > 1)
				throw new NotSupportedException("Method Telemetry is unsupported for methods with 2 or more parameters");

			return new MethodTelemetryNode(Name, instance, methodInfo);
		}

		/// <summary>
		/// Gets the methods on the given type decorated for telemetry.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<MethodInfo, MethodTelemetryAttribute>> GetMethods([NotNull] Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_CacheSection.Enter();

			try
			{
				Dictionary<MethodInfo, MethodTelemetryAttribute> methodInfos;

				if (!s_TypeToMethodInfo.TryGetValue(type, out methodInfos))
				{
					methodInfos = new Dictionary<MethodInfo, MethodTelemetryAttribute>(TelemetryMethodInfoEqualityComparer.Instance);
					s_TypeToMethodInfo.Add(type, methodInfos);

					// First get the inherited methods
					IEnumerable<KeyValuePair<MethodInfo, MethodTelemetryAttribute>> inheritedMethods =
						type.GetAllTypes()
						    .Except(type)
						    .SelectMany(t => GetMethods(t));

					// Then get the methods for this type
					IEnumerable<KeyValuePair<MethodInfo, MethodTelemetryAttribute>> typeMethods =
#if SIMPLSHARP
						((CType)type)
#else
						type.GetTypeInfo()
#endif
							.GetMethods(BINDING_FLAGS)
							.Select(m => new KeyValuePair<MethodInfo, MethodTelemetryAttribute>(m, GetTelemetryAttribute(m)))
							.Where(kvp => kvp.Value != null);

					// Then insert everything - Type methods win
					foreach (KeyValuePair<MethodInfo, MethodTelemetryAttribute> kvp in inheritedMethods.Concat(typeMethods))
						methodInfos[kvp.Key] = kvp.Value;
				}

				return methodInfos;
			}
			finally
			{
				s_CacheSection.Leave();
			}
		}

		[NotNull]
		public static MethodInfo GetMethodInfo([NotNull] ITelemetryProvider instance, [NotNull] string methodName)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentException("Method name must not be null or empty", "methodName");

			Type type = instance.GetType();
			return GetMethods(type).First(kvp => kvp.Value.Name == methodName).Key;
		}

		[CanBeNull]
		private static MethodTelemetryAttribute GetTelemetryAttribute([NotNull] ICustomAttributeProvider method)
		{
			if (method == null)
				throw new ArgumentException("method");

			// ReSharper disable InvokeAsExtensionMethod
			return ReflectionExtensions.GetCustomAttributes<MethodTelemetryAttribute>(method, true).FirstOrDefault();
			// ReSharper restore InvokeAsExtensionMethod
		}
	}
}
