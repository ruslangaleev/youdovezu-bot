using Youdovezu.Domain.Entities;

namespace Youdovezu.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с пользователями
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Получает пользователя по Telegram ID
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>Пользователь или null, если не найден</returns>
    Task<User?> GetByTelegramIdAsync(long telegramId);

    /// <summary>
    /// Получает пользователя по ID
    /// </summary>
    /// <param name="id">ID пользователя</param>
    /// <returns>Пользователь или null, если не найден</returns>
    Task<User?> GetByIdAsync(long id);

    /// <summary>
    /// Создает нового пользователя
    /// </summary>
    /// <param name="user">Пользователь для создания</param>
    /// <returns>Созданный пользователь</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Обновляет существующего пользователя
    /// </summary>
    /// <param name="user">Пользователь для обновления</param>
    /// <returns>Обновленный пользователь</returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Проверяет, существует ли пользователь с указанным Telegram ID
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>True, если пользователь существует</returns>
    Task<bool> ExistsByTelegramIdAsync(long telegramId);

    /// <summary>
    /// Получает всех администраторов
    /// </summary>
    /// <returns>Список администраторов</returns>
    Task<List<User>> GetAllAdminsAsync();
}
