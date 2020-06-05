using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Json;
using ICD.Connect.Telemetry.Nodes;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry.MQTTPro
{
	public sealed class TelemetryMetadataConverter : AbstractGenericJsonConverter<TelemetryMetadata>
	{
		private const string TOKEN_IO_MASK = "ioMask";
		private const string TOKEN_PUBLISH_METADATA = "publishMetadata";
		private const string TOKEN_SUBSCRIBE_METADATA = "subscribeMetadata";

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, TelemetryMetadata value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.IoMask != eTelemetryIoMask.Na)
				writer.WriteProperty(TOKEN_IO_MASK, value.IoMask.ToString());

			if (value.PublishMetadata != null)
			{
				writer.WritePropertyName(TOKEN_PUBLISH_METADATA);
				serializer.Serialize(writer, value.PublishMetadata);
			}

			if (value.SubscribeMetadata != null)
			{
				writer.WritePropertyName(TOKEN_SUBSCRIBE_METADATA);
				serializer.Serialize(writer, value.SubscribeMetadata);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, TelemetryMetadata instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case TOKEN_IO_MASK:
					instance.IoMask = reader.GetValueAsEnum<eTelemetryIoMask>();
					break;

				case TOKEN_PUBLISH_METADATA:
					instance.PublishMetadata = serializer.Deserialize<TelemetryPublishMetadata>(reader);
					break;

				case TOKEN_SUBSCRIBE_METADATA:
					instance.SubscribeMetadata = serializer.Deserialize<TelemetrySubscribeMetadata>(reader);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}

	public sealed class TelemetryPublishMetadataConverter : AbstractGenericJsonConverter<TelemetryPublishMetadata>
	{
		private const string TOKEN_PROPERTY = "property";

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, TelemetryPublishMetadata value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.Property != null)
			{
				writer.WritePropertyName(TOKEN_PROPERTY);
				serializer.Serialize(writer, value.Property);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, TelemetryPublishMetadata instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case TOKEN_PROPERTY:
					instance.Property = serializer.Deserialize<TelemetryMemberMetadata>(reader);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}

	public sealed class TelemetrySubscribeMetadataConverter : AbstractGenericJsonConverter<TelemetrySubscribeMetadata>
	{
		private const string TOKEN_PARAMETERS = "parameters";

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, TelemetrySubscribeMetadata value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.Parameters != null)
			{
				writer.WritePropertyName(TOKEN_PARAMETERS);
				serializer.SerializeArray(writer, value.Parameters);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, TelemetrySubscribeMetadata instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case TOKEN_PARAMETERS:
					instance.Parameters = serializer.DeserializeArray<TelemetryMemberMetadata>(reader);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}

	public sealed class TelemetryMemberMetadataConverter : AbstractGenericJsonConverter<TelemetryMemberMetadata>
	{
		private const string TOKEN_TYPE = "type";
		private const string TOKEN_ENUMERATION = "enumeration";
		private const string TOKEN_RANGE_MIN = "rangeMin";
		private const string TOKEN_RANGE_MAX = "rangeMax";

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, TelemetryMemberMetadata value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.Type != null)
				writer.WriteProperty(TOKEN_TYPE, value.Type);

			if (value.Enumeration != null)
			{
				writer.WritePropertyName(TOKEN_ENUMERATION);
				serializer.SerializeDict(writer, value.Enumeration);
			}

			if (value.RangeMin != null)
				writer.WriteProperty(TOKEN_RANGE_MIN, value.RangeMin.Value);

			if (value.RangeMax != null)
				writer.WriteProperty(TOKEN_RANGE_MAX, value.RangeMax.Value);
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, TelemetryMemberMetadata instance, JsonSerializer serializer)
		{
			switch (property)
			{
				case TOKEN_TYPE:
					instance.Type = reader.GetValueAsString();
					break;

				case TOKEN_ENUMERATION:
					IEnumerable<KeyValuePair<int, string>> kvps = serializer.DeserializeDict<int, string>(reader);
					instance.Enumeration = kvps == null ? null : kvps.ToDictionary();
					break;

				case TOKEN_RANGE_MIN:
					instance.RangeMin = reader.TokenType == JsonToken.Null ? (double?)null : reader.GetValueAsDouble();
					break;

				case TOKEN_RANGE_MAX:
					instance.RangeMax = reader.TokenType == JsonToken.Null ? (double?)null : reader.GetValueAsDouble();
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
