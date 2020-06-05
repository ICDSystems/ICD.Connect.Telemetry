using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Telemetry.Nodes;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry.MQTTPro
{
	[JsonConverter(typeof(TelemetryMetadataConverter))]
	public sealed class TelemetryMetadata
	{
		/// <summary>
		/// Gets/sets the IO mask for the telemetry.
		/// </summary>
		public eTelemetryIoMask IoMask { get; set; }

		/// <summary>
		/// Gets/sets metadata about the messages being published from the application.
		/// </summary>
		[CanBeNull]
		public TelemetryPublishMetadata PublishMetadata { get; set; }

		/// <summary>
		/// Gets/sets metadata about the messages that will be accepted from the service.
		/// </summary>
		[CanBeNull]
		public TelemetrySubscribeMetadata SubscribeMetadata { get; set; }

		/// <summary>
		/// Creates metadata for the given telemetry.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <returns></returns>
		public static TelemetryMetadata FromTelemetry([NotNull] TelemetryLeaf telemetry)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			return new TelemetryMetadata
			{
				IoMask = telemetry.IoMask,
				PublishMetadata = TelemetryPublishMetadata.FromTelemetry(telemetry),
				SubscribeMetadata = TelemetrySubscribeMetadata.FromTelemetry(telemetry)
			};
		}
	}

	[JsonConverter(typeof(TelemetryPublishMetadataConverter))]
	public sealed class TelemetryPublishMetadata
	{
		/// <summary>
		/// Gets/sets the metadata for the published property.
		/// </summary>
		[CanBeNull]
		public TelemetryMemberMetadata Property { get; set; }

		/// <summary>
		/// Creates metadata for the given telemetry.
		/// Returns null if the telemetry does not publish.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <returns></returns>
		[CanBeNull]
		public static TelemetryPublishMetadata FromTelemetry([NotNull] TelemetryLeaf telemetry)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			TelemetryMemberMetadata property;

			if (telemetry.PropertyInfo != null)
				property = TelemetryMemberMetadata.FromProperty(telemetry.PropertyInfo);
			else if (telemetry.EventInfo != null)
				property = TelemetryMemberMetadata.FromEvent(telemetry.EventInfo);
			else
				return null;

			return new TelemetryPublishMetadata
			{
				Property = property
			};
		}
	}

	[JsonConverter(typeof(TelemetrySubscribeMetadataConverter))]
	public sealed class TelemetrySubscribeMetadata
	{
		/// <summary>
		/// Gets/sets the metadata for the parameters that will be accepted.
		/// </summary>
		[CanBeNull]
		public IEnumerable<TelemetryMemberMetadata> Parameters { get; set; }

		/// <summary>
		/// Creates metadata for the given telemetry.
		/// Returns null if the telemetry does not subscribe.
		/// </summary>
		/// <param name="telemetry"></param>
		/// <returns></returns>
		[CanBeNull]
		public static TelemetrySubscribeMetadata FromTelemetry([NotNull] TelemetryLeaf telemetry)
		{
			if (telemetry == null)
				throw new ArgumentNullException("telemetry");

			if (telemetry.MethodInfo == null)
				return null;

			return new TelemetrySubscribeMetadata
			{
				Parameters =
					telemetry.Parameters
					         .Select(p => TelemetryMemberMetadata.FromParameter(p))
					         .ToArray()
			};
		}
	}

	[JsonConverter(typeof(TelemetryMemberMetadataConverter))]
	public sealed class TelemetryMemberMetadata
	{
		/// <summary>
		/// Gets/sets the parameter type.
		/// </summary>
		[CanBeNull]
		public string Type { get; set; }

		/// <summary>
		/// Gets/sets a dictionary describing the valid enumeration values.
		/// </summary>
		[CanBeNull]
		public Dictionary<int, string> Enumeration { get; set; }

		/// <summary>
		/// Gets/sets the range minimum.
		/// </summary>
		[CanBeNull]
		public double? RangeMin { get; set; }

		/// <summary>
		/// Gets/sets the range maximum.
		/// </summary>
		[CanBeNull]
		public double? RangeMax { get; set; }

		/// <summary>
		/// Creates member metadata for the given method parameter.
		/// </summary>
		/// <param name="parameterInfo"></param>
		/// <returns></returns>
		[NotNull]
		public static TelemetryMemberMetadata FromParameter([NotNull] ParameterInfo parameterInfo)
		{
			if (parameterInfo == null)
				throw new ArgumentNullException("parameterInfo");

			Type type = parameterInfo.ParameterType;
			return FromMember(parameterInfo, type);
		}

		/// <summary>
		/// Creates member metadata for the given property.
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		[NotNull]
		public static TelemetryMemberMetadata FromProperty([NotNull] PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException("propertyInfo");

			Type type = propertyInfo.PropertyType;
			return FromMember(propertyInfo, type);
		}

		/// <summary>
		/// Creates member metadata for the given event.
		/// </summary>
		/// <param name="eventInfo"></param>
		/// <returns></returns>
		[NotNull]
		public static TelemetryMemberMetadata FromEvent([NotNull] EventInfo eventInfo)
		{
			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			Type eventArgsType = eventInfo.GetEventArgsType();
			Type type = eventArgsType.GetInnerGenericTypes(typeof(IGenericEventArgs<>)).Single(); 

			return FromMember(eventInfo, type);
		}

		/// <summary>
		/// Creates member metadata for the given member and member type.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="memberType"></param>
		/// <returns></returns>
		[NotNull]
		private static TelemetryMemberMetadata FromMember([NotNull] ICustomAttributeProvider member, [NotNull] Type memberType)
		{
			if (member == null)
				throw new ArgumentNullException("member");

			if (memberType == null)
				throw new ArgumentNullException("memberType");

			double? rangeMin = null;
			double? rangeMax = null;

			RangeAttribute rangeAttr = member.GetCustomAttributes<RangeAttribute>().FirstOrDefault();
			if (rangeAttr != null)
			{
				rangeMin = (double)Convert.ChangeType(rangeAttr.Min, TypeCode.Double, CultureInfo.InvariantCulture);
				rangeMax = (double)Convert.ChangeType(rangeAttr.Max, TypeCode.Double, CultureInfo.InvariantCulture);
			}

			return new TelemetryMemberMetadata
			{
				Type = memberType.GetMinimalName(),
				Enumeration = BuildEnumerationMetadata(memberType),
				RangeMin = rangeMin,
				RangeMax = rangeMax
			};
		}

		/// <summary>
		/// Builds a dictionary of integer to enum value for the given type.
		/// Returns null if the type is not an enum type.
		/// </summary>
		/// <param name="memberType"></param>
		/// <returns></returns>
		[CanBeNull]
		private static Dictionary<int, string> BuildEnumerationMetadata([NotNull] Type memberType)
		{
			if (memberType == null)
				throw new ArgumentNullException("memberType");

			return EnumUtils.IsEnumType(memberType)
				       ? EnumUtils.GetValues(memberType)
				                  .ToDictionary(k => (int)k, k => k.ToString())
				       : null;
		}
	}
}
