using Microsoft.EntityFrameworkCore;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Infrastructure.Data;

namespace Youdovezu.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с поездками
/// </summary>
public class TripRepository : ITripRepository
{
    private readonly YoudovezuDbContext _context;

    /// <summary>
    /// Конструктор репозитория
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public TripRepository(YoudovezuDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить поездку по ID
    /// </summary>
    public async Task<Trip?> GetByIdAsync(long id)
    {
        return await _context.Trips
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Получить все поездки пользователя
    /// </summary>
    public async Task<List<Trip>> GetByUserIdAsync(long userId)
    {
        return await _context.Trips
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Создать новую поездку
    /// </summary>
    public async Task<Trip> CreateAsync(Trip trip)
    {
        trip.CreatedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        return trip;
    }

    /// <summary>
    /// Обновить поездку
    /// </summary>
    public async Task<Trip> UpdateAsync(Trip trip)
    {
        trip.UpdatedAt = DateTime.UtcNow;

        _context.Trips.Update(trip);
        await _context.SaveChangesAsync();

        return trip;
    }

    /// <summary>
    /// Удалить поездку (мягкое удаление)
    /// </summary>
    public async Task<Trip> DeleteAsync(Trip trip)
    {
        trip.Status = TripStatus.Deleted;
        trip.UpdatedAt = DateTime.UtcNow;

        _context.Trips.Update(trip);
        await _context.SaveChangesAsync();

        return trip;
    }
}

