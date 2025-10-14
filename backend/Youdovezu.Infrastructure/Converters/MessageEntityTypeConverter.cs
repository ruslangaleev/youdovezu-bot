using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;

namespace Youdovezu.Infrastructure.Converters;

public class MessageEntityTypeConverter : JsonConverter<MessageEntityType>
{
    public override MessageEntityType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        var stringValue = reader.GetString();
        return stringValue?.ToLowerInvariant() switch
        {
            "mention" => MessageEntityType.Mention,
            "hashtag" => MessageEntityType.Hashtag,
            "cashtag" => MessageEntityType.Cashtag,
            "bot_command" => MessageEntityType.BotCommand,
            "url" => MessageEntityType.Url,
            "email" => MessageEntityType.Email,
            "phone_number" => MessageEntityType.PhoneNumber,
            "bold" => MessageEntityType.Bold,
            "italic" => MessageEntityType.Italic,
            "underline" => MessageEntityType.Underline,
            "strikethrough" => MessageEntityType.Strikethrough,
            "spoiler" => MessageEntityType.Spoiler,
            "code" => MessageEntityType.Code,
            "pre" => MessageEntityType.Pre,
            "text_link" => MessageEntityType.TextLink,
            "text_mention" => MessageEntityType.TextMention,
            "custom_emoji" => MessageEntityType.CustomEmoji,
            _ => throw new JsonException($"Unknown MessageEntityType: {stringValue}")
        };
    }

    public override void Write(Utf8JsonWriter writer, MessageEntityType value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            MessageEntityType.Mention => "mention",
            MessageEntityType.Hashtag => "hashtag",
            MessageEntityType.Cashtag => "cashtag",
            MessageEntityType.BotCommand => "bot_command",
            MessageEntityType.Url => "url",
            MessageEntityType.Email => "email",
            MessageEntityType.PhoneNumber => "phone_number",
            MessageEntityType.Bold => "bold",
            MessageEntityType.Italic => "italic",
            MessageEntityType.Underline => "underline",
            MessageEntityType.Strikethrough => "strikethrough",
            MessageEntityType.Spoiler => "spoiler",
            MessageEntityType.Code => "code",
            MessageEntityType.Pre => "pre",
            MessageEntityType.TextLink => "text_link",
            MessageEntityType.TextMention => "text_mention",
            MessageEntityType.CustomEmoji => "custom_emoji",
            _ => throw new JsonException($"Unknown MessageEntityType: {value}")
        };
        
        writer.WriteStringValue(stringValue);
    }
}
