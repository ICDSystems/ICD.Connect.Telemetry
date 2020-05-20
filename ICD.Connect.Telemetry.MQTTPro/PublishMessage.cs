using System;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Json;
using Newtonsoft.Json;

namespace ICD.Connect.Telemetry.MQTTPro
{
	[JsonConverter(typeof(PublishMessageJsonConverter))]
	public sealed class PublishMessage
	{
		public DateTime Date { get; set; }
		public object Data { get; set; }
	}

	public sealed class PublishMessageJsonConverter : AbstractGenericJsonConverter<PublishMessage>
	{
		private const string TOKEN_DATE = "date";
		private const string TOKEN_DATA = "data";

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, PublishMessage value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			writer.WriteProperty(TOKEN_DATE, value.Date);

			writer.WritePropertyName(TOKEN_DATA);
			serializer.Serialize(writer, value.Data);
		}
	}
}
