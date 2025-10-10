using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Telegram;

/// <summary>
/// Команда для обработки callback запросов от inline клавиатур
/// </summary>
/// <remarks>
/// Маршрутизирует различные типы callback запросов к соответствующим обработчикам.
/// </remarks>
public class HandleCallbackQueryCommand : BaseCommand<bool>
{
    /// <summary>
    /// Идентификатор callback query
    /// </summary>
    public string CallbackQueryId { get; set; } = string.Empty;

    /// <summary>
    /// Данные callback query
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор пользователя в Telegram
    /// </summary>
    public long TelegramId { get; set; }

    /// <summary>
    /// Имя пользователя в Telegram
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия пользователя в Telegram (может быть null)
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Имя пользователя в Telegram без @ (может быть null)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Идентификатор чата
    /// </summary>
    public long ChatId { get; set; }
}
