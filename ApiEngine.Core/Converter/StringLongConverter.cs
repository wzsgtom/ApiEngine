using Newtonsoft.Json;

namespace ApiEngine.Core.Converter;

public class StringLongConverter : JsonConverter
{
    /// <summary>
    ///     Determines whether this converter can convert the specified object type.
    /// </summary>
    /// <param name="objectType">The type of the object to convert.</param>
    /// <returns>true if the converter can convert the specified type; otherwise, false.</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(long);
    }

    /// <summary>
    ///     Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="serializer">The JSON serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }

    /// <summary>
    ///     Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="objectType">The type of the object to convert.</param>
    /// <param name="existingValue">The existing value of the object being read.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <returns>The deserialized object.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
        JsonSerializer serializer)
    {
        return reader.TokenType switch
        {
            JsonToken.String when long.TryParse(reader.Value?.ToString(), out var result) => result,
            JsonToken.String => throw new JsonSerializationException($"Unable to parse '{reader.Value}' as long."),
            JsonToken.Integer => Convert.ToInt64(reader.Value),
            _ => throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}")
        };
    }
}