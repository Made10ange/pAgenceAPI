using System.Text.Json;
using System.Text.Json.Serialization;

namespace pAgenceAPI
{
    /// <summary>
    /// Conversor personalizado para TimeSpan en JSON
    /// Permite deserializar TimeSpan desde strings formato "HH:mm:ss"
    /// </summary>
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? timeString = reader.GetString();
            if (string.IsNullOrWhiteSpace(timeString)) 
                return TimeSpan.Zero;
            
            if (TimeSpan.TryParse(timeString, out var result))
                return result;
            
            return TimeSpan.Zero;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
        }
    }
}
