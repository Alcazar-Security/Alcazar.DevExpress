using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alcazar.Playpen.Razor
{
	/// <summary>
	/// Custom DateTime converter that preserves the original DateTime values without timezone conversion.
	/// All of this is needed because OData serialisation messes up DateTime values by converting them to local time.
	/// </summary>
	public class PreserveDateTimeConverter : JsonConverter<DateTime>
	{
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				var dateString = reader.GetString();
				if (DateTime.TryParse(dateString, out var date))
				{
					return date;
				}
			}
			throw new JsonException("Unable to parse DateTime");
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			// Write DateTime as ISO 8601 string without timezone conversion
			// This preserves the exact DateTime value that was set in your controller
			switch (value.Kind)
			{
				case DateTimeKind.Utc:
					writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
					break;
				case DateTimeKind.Local:
					// For local times, write without timezone info to prevent browser conversion
					writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
					break;
				case DateTimeKind.Unspecified:
				default:
					// For unspecified, write as-is without timezone info
					writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
					break;
			}
		}
	}

	/// <summary>
	/// Custom OData serializer provider to handle DateTime serialization
	/// </summary>
	public class CustomODataSerializerProvider : IODataSerializerProvider
	{
		public IODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
		{
			throw new NotImplementedException();
		}

		public IODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
		{
			return new CustomDateTimeSerializer(this);
		}
	}
	/// <summary>
	/// Custom DateTime serializer for OData
	/// </summary>
	public class CustomDateTimeSerializer : IODataSerializer, IODataEdmTypeSerializer
	{
		public CustomDateTimeSerializer(IODataSerializerProvider serializerProvider)
		{
		}

		public ODataPayloadKind ODataPayloadKind
		{
			get { return ODataPayloadKind.Batch; }
		}

		public ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
		{
			if (graph is DateTime dateTime)
			{
				// Return the DateTime as-is without conversion
				// This preserves the original value from your controller
				// return new DateTimeOffset(dateTime);
				return null;
			}

			return null;
		}

		public async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
		{
			// Write DateTime as ISO 8601 string without timezone conversion
			// This preserves the exact DateTime value that was set in your controller
			if (graph is DateTime value)
			{
				switch (value.Kind)
				{
					case DateTimeKind.Utc:
						messageWriter.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
						break;
					case DateTimeKind.Local:
						// For local times, write without timezone info to prevent browser conversion
						messageWriter.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
						break;
					case DateTimeKind.Unspecified:
					default:
						// For unspecified, write as-is without timezone info
						messageWriter.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
						break;
				}
			}
		}

		public Task WriteObjectInlineAsync(object graph, IEdmTypeReference expectedType, ODataWriter writer, Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext writeContext)
		{
			throw new NotImplementedException();
		}
	}
}
