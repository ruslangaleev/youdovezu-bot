using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Users;

/// <summary>
/// Команда для обновления номера телефона пользователя
/// </summary>
/// <remarks>
/// Обновляет номер телефона существующего пользователя по его Telegram ID.
/// Используется при подтверждении номера телефона в процессе регистрации.
/// </remarks>
public class UpdateUserPhoneCommand : BaseCommand<bool>
{
    /// <summary>
    /// Уникальный идентификатор пользователя в Telegram
    /// </summary>
    public long TelegramId { get; set; }

    /// <summary>
    /// Новый номер телефона пользователя
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}
