using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Json;
using ICD.Connect.Telemetry.Nodes;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry
{
	[Flags]
	public enum eMetadataSupport
	{
		None = 0,
		EnumerationValues = 1,
		Range = 2
	}

	[JsonConverter(typeof(TelemetryMetadataConverter))]
	public sealed class TelemetryMetadata
	{
		#region Properties

		/// <summary>
		/// Gets the datatype of the property this metadata is for 
		/// </summary>
		public Type DataType { get; private set; }

		/// <summary>
		/// Sets which properties of this metadata should be serialized.
		/// </summary>
		public eMetadataSupport Supports { get; private set; }

		#endregion

		#region Range

		/// <summary>
		/// get/set the minimum range
		/// </summary>
		public double RangeMin { get; private set; }

		/// <summary>
		/// get/set the maximum range
		/// </summary>
		public double RangeMax { get; private set; }

		#endregion

		/// <summary>
		/// Creates metadata for the given feedback telemetry.
		/// </summary>
		/// <param name="feedbackTelemetryItem"></param>
		public static TelemetryMetadata FromFeedbackTelemetry([NotNull] IFeedbackTelemetryItem feedbackTelemetryItem)
		{
			TelemetryMetadata output = new TelemetryMetadata
			{
				DataType = feedbackTelemetryItem.ValueType
			};

			eMetadataSupport supports = eMetadataSupport.None;

			if (feedbackTelemetryItem.ValueType.IsEnum)
				supports = supports | eMetadataSupport.EnumerationValues;

			RangeAttribute rangeAttr = feedbackTelemetryItem.PropertyInfo.GetCustomAttributes<RangeAttribute>().FirstOrDefault();
			if (rangeAttr != null)
			{
				supports = supports | eMetadataSupport.Range;
				output.RangeMin = (double)Convert.ChangeType(rangeAttr.Min, TypeCode.Double, CultureInfo.InvariantCulture);
				output.RangeMax = (double)Convert.ChangeType(rangeAttr.Max, TypeCode.Double, CultureInfo.InvariantCulture);
			}

			output.Supports = supports;

			return output;
		}
	}

	public sealed class TelemetryMetadataConverter : AbstractGenericJsonConverter<TelemetryMetadata>
	{
		private const string TYPE_TOKEN = "type";
		private const string ENUM_ITEMS_TOKEN = "enum";
		private const string RANGE_TOKEN = "range";
		private const string MIN_TOKEN = "min";
		private const string MAX_TOKEN = "max";

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, TelemetryMetadata value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.DataType != null)
				writer.WriteProperty(TYPE_TOKEN, value.DataType.GetMinimalName());

			if (value.Supports.HasFlag(eMetadataSupport.EnumerationValues))
			{
				Dictionary<int, string> enumStrings =
					EnumUtils.GetValues(value.DataType)
					         .ToDictionary(k => (int)k, k => k.ToString());

				writer.WritePropertyName(ENUM_ITEMS_TOKEN);
				serializer.SerializeDict(writer, enumStrings);
			}

			if (value.Supports.HasFlag(eMetadataSupport.Range))
			{
				writer.WritePropertyName(RANGE_TOKEN);

				writer.WriteStartObject();
				writer.WriteProperty(MIN_TOKEN, value.RangeMin);
				writer.WriteProperty(MAX_TOKEN, value.RangeMax);
				writer.WriteEndObject();
			}
		}
	}
}
