using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;

namespace Youdovezu.Infrastructure.Converters;

public class ChatTypeConverter : JsonConverter<ChatType>
{
    public override ChatType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        var stringValue = reader.GetString();
        return stringValue?.ToLowerInvariant() switch
        {
            "private" => ChatType.Private,
            "group" => ChatType.Group,
            "supergroup" => ChatType.Supergroup,
            "channel" => ChatType.Channel,
            _ => throw new JsonException($"Unknown ChatType: {stringValue}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ChatType value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            ChatType.Private => "private",
            ChatType.Group => "group",
            ChatType.Supergroup => "supergroup",
            ChatType.Channel => "channel",
            _ => throw new JsonException($"Unknown ChatType: {value}")
        };
        
        writer.WriteStringValue(stringValue);
    }
}
