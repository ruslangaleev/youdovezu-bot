using YouDovezu.Application.Common.Models;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Queries.Users;

/// <summary>
/// Запрос для получения пользователя по Telegram ID
/// </summary>
/// <remarks>
/// Используется для проверки существования пользователя в системе
/// при обработке команд от Telegram бота.
/// </remarks>
public class GetUserByTelegramIdQuery : BaseQuery<User?>
{
    /// <summary>
    /// Уникальный идентификатор пользователя в Telegram
    /// </summary>
    public long TelegramId { get; set; }
}
