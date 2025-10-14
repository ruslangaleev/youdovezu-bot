using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;

namespace Youdovezu.Infrastructure.Converters;

/// <summary>
/// JSON конвертер для преобразования строковых значений в enum MessageEntityType
/// Используется для корректной десериализации типов сущностей сообщений от Telegram API
/// </summary>
public class MessageEntityTypeConverter : JsonConverter<MessageEntityType>
{
    /// <summary>
    /// Читает строковое значение из JSON и преобразует его в MessageEntityType enum
    /// </summary>
    /// <param name="reader">JSON reader</param>
    /// <param name="typeToConvert">Тип для преобразования</param>
    /// <param name="options">Опции сериализации</param>
    /// <returns>Значение MessageEntityType enum</returns>
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

    /// <summary>
    /// Записывает MessageEntityType enum в JSON как строковое значение
    /// </summary>
    /// <param name="writer">JSON writer</param>
    /// <param name="value">Значение MessageEntityType для записи</param>
    /// <param name="options">Опции сериализации</param>
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
