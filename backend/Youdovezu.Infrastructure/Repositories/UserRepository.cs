using Microsoft.EntityFrameworkCore;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Infrastructure.Data;

namespace Youdovezu.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с пользователями
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly YoudovezuDbContext _context;

    /// <summary>
    /// Конструктор репозитория
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public UserRepository(YoudovezuDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает пользователя по Telegram ID
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>Пользователь или null, если не найден</returns>
    public async Task<User?> GetByTelegramIdAsync(long telegramId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
    }

    /// <summary>
    /// Получает пользователя по ID
    /// </summary>
    /// <param name="id">ID пользователя</param>
    /// <returns>Пользователь или null, если не найден</returns>
    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Создает нового пользователя
    /// </summary>
    /// <param name="user">Пользователь для создания</param>
    /// <returns>Созданный пользователь</returns>
    public async Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return user;
    }

    /// <summary>
    /// Обновляет существующего пользователя
    /// </summary>
    /// <param name="user">Пользователь для обновления</param>
    /// <returns>Обновленный пользователь</returns>
    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        
        return user;
    }

    /// <summary>
    /// Проверяет, существует ли пользователь с указанным Telegram ID
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>True, если пользователь существует</returns>
    public async Task<bool> ExistsByTelegramIdAsync(long telegramId)
    {
        return await _context.Users
            .AnyAsync(u => u.TelegramId == telegramId);
    }
}
