using System.Text.Json;
using System.Text.Json.Serialization;

namespace Youdovezu.Infrastructure.Converters;

public class UnixDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        var unixTimestamp = reader.GetInt64();
        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTimestamp = new DateTimeOffset(value).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTimestamp);
    }
}
