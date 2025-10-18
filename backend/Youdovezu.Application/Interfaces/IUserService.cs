using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;

namespace Youdovezu.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с пользователями
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="username">Имя пользователя в Telegram</param>
    /// <param name="firstName">Имя пользователя</param>
    /// <param name="lastName">Фамилия пользователя</param>
    /// <returns>Созданный пользователь</returns>
    Task<User> RegisterUserAsync(long telegramId, string? username, string? firstName, string? lastName);

    /// <summary>
    /// Получает пользователя по Telegram ID
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>Пользователь или null, если не найден</returns>
    Task<User?> GetUserByTelegramIdAsync(long telegramId);

    /// <summary>
    /// Обновляет согласие с политикой конфиденциальности
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>Обновленный пользователь</returns>
    Task<User> UpdatePrivacyConsentAsync(long telegramId);

    /// <summary>
    /// Обновляет номер телефона пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <returns>Обновленный пользователь</returns>
    Task<User> UpdatePhoneNumberAsync(long telegramId, string phoneNumber);

    /// <summary>
    /// Проверяет, зарегистрирован ли пользователь
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>True, если пользователь зарегистрирован</returns>
    Task<bool> IsUserRegisteredAsync(long telegramId);

    /// <summary>
    /// Включает возможность быть водителем для пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>Обновленный пользователь</returns>
    Task<User> EnableDriverCapabilityAsync(long telegramId);

    /// <summary>
    /// Обновляет системную роль пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="systemRole">Новая системная роль</param>
    /// <returns>Обновленный пользователь</returns>
    Task<User> UpdateSystemRoleAsync(long telegramId, SystemRole systemRole);
}
