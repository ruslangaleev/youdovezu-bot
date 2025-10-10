using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Telegram;

/// <summary>
/// Команда для обработки команды /start от пользователя Telegram
/// </summary>
/// <remarks>
/// Инициирует процесс регистрации или приветствует существующего пользователя.
/// Содержит информацию о пользователе из Telegram.
/// </remarks>
public class HandleStartCommand : BaseCommand<bool>
{
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
