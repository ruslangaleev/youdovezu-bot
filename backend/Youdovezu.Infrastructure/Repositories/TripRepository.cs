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

    /// <summary>
    /// Получить активные поездки по населенному пункту
    /// </summary>
    public async Task<List<Trip>> GetActiveTripsBySettlementAsync(string settlementName, string? fromSettlement = null, string? toSettlement = null, string? searchQuery = null)
    {
        var query = _context.Trips
            .Include(t => t.User)
            .Where(t => t.Status == TripStatus.Active &&
                       (t.FromSettlement == settlementName || t.ToSettlement == settlementName));

        // Фильтрация по населенному пункту отправления
        if (!string.IsNullOrWhiteSpace(fromSettlement))
        {
            query = query.Where(t => t.FromSettlement.Contains(fromSettlement));
        }

        // Фильтрация по населенному пункту назначения
        if (!string.IsNullOrWhiteSpace(toSettlement))
        {
            query = query.Where(t => t.ToSettlement.Contains(toSettlement));
        }

        // Поиск по ключевым словам
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(t =>
                t.FromSettlement.Contains(searchQuery) ||
                t.ToSettlement.Contains(searchQuery) ||
                t.FromAddress.Contains(searchQuery) ||
                t.ToAddress.Contains(searchQuery) ||
                (t.Comment != null && t.Comment.Contains(searchQuery)));
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Получить количество активных поездок в населенном пункте
    /// </summary>
    public async Task<int> GetActiveTripsCountBySettlementAsync(string settlementName)
    {
        return await _context.Trips
            .CountAsync(t => t.Status == TripStatus.Active &&
                            (t.FromSettlement == settlementName || t.ToSettlement == settlementName));
    }
}

