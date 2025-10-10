using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Telegram;

/// <summary>
/// Команда для обработки согласия пользователя с политикой конфиденциальности
/// </summary>
/// <remarks>
/// Регистрирует пользователя в системе и переводит к следующему шагу - подтверждению телефона.
/// </remarks>
public class HandlePrivacyAgreementCommand : BaseCommand<bool>
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
