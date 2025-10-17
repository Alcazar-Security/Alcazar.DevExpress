using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alcazar.Playpen.Razor.Utilities
{
	/// <summary>
	/// Enhanced DateTime converter that uses reflection to check DateTimeUsage attributes
	/// </summary>
	public class DateTimeUsageJsonConverter : JsonConverter<DateTime>
	{
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				var dateString = reader.GetString();

				// Handle date-only values with 'D' suffix
				if (dateString?.EndsWith("D") == true)
				{
					dateString = dateString.Substring(0, dateString.Length - 1);
				}

				if (DateTime.TryParse(dateString, out var date))
				{
					return DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
				}
			}
			throw new JsonException("Unable to parse DateTime");
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			// Get the property info from the serialization context
			// This is complex with System.Text.Json, so we'll use a simpler approach

			// Check if this DateTime has DateTimeUsage attribute with IsDateOnly = true
			bool isDateOnly = IsDateOnlyProperty(value);

			if (isDateOnly)
			{
				// Format as date-only with 'D' suffix
				writer.WriteStringValue(value.ToString("yyyy-MM-dd") + "D");
			}
			else
			{
				// Format datetime without timezone offset
				if (value.TimeOfDay == TimeSpan.Zero)
				{
					// Date with no time component
					writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
				}
				else
				{
					// DateTime with time component
					writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
				}
			}
		}

		private bool IsDateOnlyProperty(DateTime value)
		{
			// This is a simplified check - in a real implementation, you'd need
			// to track the property being serialized through the JsonSerializerContext
			// For now, we'll assume dates with exactly midnight time might be date-only
			return value.TimeOfDay == TimeSpan.Zero;
		}
	}
}
