using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Json;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry
{
	[JsonConverter(typeof(TelemetryMetadataConverter))]
	public sealed class TelemetryMetadata
	{
        /// <summary>
		/// Gets the datatype of the property this metadata is for 
		/// </summary>
		public Type DataType { get; set; }

		/// <summary>
		/// Sets which properties of this metadata should be serialized.
		/// </summary>
		public eMetadataSupport Supports { get; set; }

		#region Enum
		
		/// <summary>
		/// Returns if this metadata pertains to an enumeration
		/// </summary>
		public bool IsEnumeration { get { return DataType != null && DataType.IsEnum; } }

		#endregion

		#region Range

		/// <summary>
		/// get/set the minimum range
		/// </summary>
		public double RangeMin { get; set; }

		/// <summary>
		/// get/set the maximum range
		/// </summary>
		public double RangeMax { get; set; }
		#endregion
	}

	[Flags]
	public enum eMetadataSupport
	{
		None = 0,
		EnumerationValues = 1,
		Range = 2
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

			if (value.DataType == null)
				return;

			writer.WriteProperty(TYPE_TOKEN, value.DataType.GetMinimalName());

			if(value.Supports.HasFlag(eMetadataSupport.EnumerationValues))
			{
				Dictionary<int, string> enumStrings = EnumUtils.GetValues(value.DataType)
				                                               .Select(e => new KeyValuePair<int, string>((int)e, e.ToString()))
				                                               .ToDictionary();
				writer.WritePropertyName(ENUM_ITEMS_TOKEN);
				serializer.SerializeDict(writer, enumStrings);
			}

			if(value.Supports.HasFlag(eMetadataSupport.Range))
			{
				writer.WritePropertyName(RANGE_TOKEN);

				writer.WriteStartObject();
				writer.WriteProperty(MIN_TOKEN, value.RangeMin);
				writer.WriteProperty(MAX_TOKEN, value.RangeMax);
				writer.WriteEndObject();
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, TelemetryMetadata instance, JsonSerializer serializer)
		{
			//Reading not required - these items are only for sending out to telemetry.

			base.ReadProperty(property, reader, instance, serializer);
		}
	}
}