using Microsoft.EntityFrameworkCore;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Infrastructure.Data;

namespace Youdovezu.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с населенными пунктами
/// </summary>
public class SettlementRepository : ISettlementRepository
{
    private readonly YoudovezuDbContext _context;

    /// <summary>
    /// Конструктор репозитория
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public SettlementRepository(YoudovezuDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить все активные населенные пункты
    /// </summary>
    public async Task<List<Settlement>> GetAllActiveAsync()
    {
        return await _context.Settlements
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Получить населенный пункт по ID
    /// </summary>
    public async Task<Settlement?> GetByIdAsync(long id)
    {
        return await _context.Settlements
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Получить населенный пункт по названию
    /// </summary>
    public async Task<Settlement?> GetByNameAsync(string name)
    {
        return await _context.Settlements
            .FirstOrDefaultAsync(s => s.Name == name);
    }

    /// <summary>
    /// Получить населенные пункты с количеством активных поездок для каждого
    /// </summary>
    public async Task<List<SettlementWithTripsCount>> GetSettlementsWithTripsCountAsync()
    {
        var settlements = await _context.Settlements
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        var result = new List<SettlementWithTripsCount>();

        foreach (var settlement in settlements)
        {
            var tripsCount = await _context.Trips
                .CountAsync(t => t.Status == TripStatus.Active &&
                                (t.FromSettlement == settlement.Name || t.ToSettlement == settlement.Name));

            result.Add(new SettlementWithTripsCount
            {
                Settlement = settlement,
                TripsCount = tripsCount
            });
        }

        return result;
    }

    /// <summary>
    /// Получить уникальные населенные пункты из существующих поездок
    /// </summary>
    public async Task<List<string>> GetSettlementsFromTripsAsync()
    {
        var fromSettlements = await _context.Trips
            .Where(t => t.Status == TripStatus.Active && !string.IsNullOrEmpty(t.FromSettlement))
            .Select(t => t.FromSettlement)
            .Distinct()
            .ToListAsync();

        var toSettlements = await _context.Trips
            .Where(t => t.Status == TripStatus.Active && !string.IsNullOrEmpty(t.ToSettlement))
            .Select(t => t.ToSettlement)
            .Distinct()
            .ToListAsync();

        var allSettlements = fromSettlements.Union(toSettlements).Distinct().OrderBy(s => s).ToList();
        return allSettlements;
    }

    /// <summary>
    /// Создать новый населенный пункт
    /// </summary>
    public async Task<Settlement> CreateAsync(Settlement settlement)
    {
        settlement.CreatedAt = DateTime.UtcNow;
        settlement.UpdatedAt = DateTime.UtcNow;

        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync();

        return settlement;
    }

    /// <summary>
    /// Обновить населенный пункт
    /// </summary>
    public async Task<Settlement> UpdateAsync(Settlement settlement)
    {
        settlement.UpdatedAt = DateTime.UtcNow;

        _context.Settlements.Update(settlement);
        await _context.SaveChangesAsync();

        return settlement;
    }
}


