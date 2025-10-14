using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;

namespace Youdovezu.Infrastructure.Converters;

/// <summary>
/// JSON конвертер для преобразования строковых значений в enum ChatType
/// Используется для корректной десериализации данных от Telegram API
/// </summary>
public class ChatTypeConverter : JsonConverter<ChatType>
{
    /// <summary>
    /// Читает строковое значение из JSON и преобразует его в ChatType enum
    /// </summary>
    /// <param name="reader">JSON reader</param>
    /// <param name="typeToConvert">Тип для преобразования</param>
    /// <param name="options">Опции сериализации</param>
    /// <returns>Значение ChatType enum</returns>
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

    /// <summary>
    /// Записывает ChatType enum в JSON как строковое значение
    /// </summary>
    /// <param name="writer">JSON writer</param>
    /// <param name="value">Значение ChatType для записи</param>
    /// <param name="options">Опции сериализации</param>
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
