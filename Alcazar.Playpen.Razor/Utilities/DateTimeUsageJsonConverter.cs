using System.Globalization;
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
				string dateString = reader.GetString();

				if (string.IsNullOrEmpty(dateString))
					throw new JsonException("DateTime string is empty");

				bool isUtc = false;
				if (dateString?.EndsWith("Z") == true)
				{
					isUtc = true;
					dateString = dateString.Substring(0, dateString.Length - 1);
				}

				// Adjust the kind to UTC or local, based on the presence of 'Z'
				if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, out DateTime date))
					return DateTime.SpecifyKind(date, isUtc ? DateTimeKind.Utc : DateTimeKind.Local);
			}

			throw new JsonException("Unable to parse DateTime ");
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			// The JSON converter has no context of the property being serialized, therefore we cannot inspect the [DateTimeUsage] attribute.
			// We dont know if the DateTime is intended to be date-only or date-time.
			// But we have the DateTimeKind, so we can at least handle UTC correctly.
			string stringValue;

			// Format datetime without timezone offset
			if (value.TimeOfDay == TimeSpan.Zero)
			{
				// Date with no time component
				stringValue = value.ToString("yyyy-MM-dd");
			}
			else
			{
				// DateTime with time component
				stringValue = value.ToString("yyyy-MM-ddTHH:mm:ss.fff");
			}

			// Append 'Z' for UTC times. NO longer, the Datagrid inspects the attribute instead
			// if (value.Kind == DateTimeKind.Utc)
			// 	stringValue += "Z";

			// Write the date/time value
			writer.WriteStringValue(stringValue);
		}
	}
}
