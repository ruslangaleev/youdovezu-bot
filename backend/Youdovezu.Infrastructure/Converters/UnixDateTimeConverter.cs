using System.Text.Json;
using System.Text.Json.Serialization;

namespace Youdovezu.Infrastructure.Converters;

/// <summary>
/// JSON конвертер для преобразования Unix timestamp в DateTime
/// Используется для корректной десериализации дат от Telegram API
/// </summary>
public class UnixDateTimeConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Читает Unix timestamp из JSON и преобразует его в DateTime
    /// </summary>
    /// <param name="reader">JSON reader</param>
    /// <param name="typeToConvert">Тип для преобразования</param>
    /// <param name="options">Опции сериализации</param>
    /// <returns>Значение DateTime</returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        var unixTimestamp = reader.GetInt64();
        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
    }

    /// <summary>
    /// Записывает DateTime в JSON как Unix timestamp
    /// </summary>
    /// <param name="writer">JSON writer</param>
    /// <param name="value">Значение DateTime для записи</param>
    /// <param name="options">Опции сериализации</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTimestamp = new DateTimeOffset(value).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTimestamp);
    }
}
