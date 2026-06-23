using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Drivolution.Converters
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string _dateFormat = "yyyy-MM-ddTHH:mm:ss"; 

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string dateString = reader.GetString();
                if (DateTime.TryParseExact(dateString, _dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    return date;
                }
                else
                {
                    throw new JsonException($"Invalid date format. Expected format is {_dateFormat}.");
                }
            }

            throw new JsonException("Invalid JSON format for DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_dateFormat));  
        }
    }

 
    public class PropertyConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                {
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("value", out JsonElement valueProperty))
                    {
                        return valueProperty.GetString();
                    }
                    if (root.TryGetProperty("object", out JsonElement objectProperty))
                    {
                        return objectProperty.GetString();
                    }
                    if (root.TryGetProperty("SectorId", out JsonElement sectoridProperty))
                    {
                        return sectoridProperty.GetString();
                    }
                }
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            throw new JsonException("Invalid JSON format for the property.");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            if (value.Contains("some_specific_type_condition"))  
            {
                writer.WriteString("type", "Property");
                writer.WriteString("value", value);
            }
            else
            {
                writer.WriteString("name", value);
            }

            writer.WriteEndObject();
        }
    }
}
