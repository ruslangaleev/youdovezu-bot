namespace Youdovezu.Application.Models;

/// <summary>
/// Модель конфигурации для Telegram Bot
/// </summary>
public class TelegramSettings
{
    /// <summary>
    /// Токен бота от BotFather
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// Секретный токен для защиты webhook от сторонних запросов
    /// </summary>
    public string SecretToken { get; set; } = string.Empty;
}
