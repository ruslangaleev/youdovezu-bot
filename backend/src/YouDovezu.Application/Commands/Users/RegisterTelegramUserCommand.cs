using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Users;

/// <summary>
/// Команда для регистрации пользователя через Telegram
/// </summary>
/// <remarks>
/// Создает нового пользователя в системе на основе данных из Telegram.
/// Устанавливает согласие на обработку ПД и триал период.
/// </remarks>
public class RegisterTelegramUserCommand : BaseCommand<Guid>
{
    /// <summary>
    /// Уникальный идентификатор пользователя в Telegram
    /// </summary>
    public long TelegramId { get; set; }

    /// <summary>
    /// Имя пользователя в Telegram без @ (может быть null)
    /// </summary>
    public string? TelegramUsername { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия пользователя (может быть null)
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Номер телефона пользователя
    /// </summary>
    /// <remarks>
    /// Может быть пустым при первой регистрации, заполняется позже
    /// </remarks>
    public string PhoneNumber { get; set; } = string.Empty;
}
