using Microsoft.EntityFrameworkCore;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Infrastructure.Data;

namespace Youdovezu.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с предложениями от Offerer'ов
/// </summary>
public class TripOfferRepository : ITripOfferRepository
{
    private readonly YoudovezuDbContext _context;

    /// <summary>
    /// Конструктор репозитория
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public TripOfferRepository(YoudovezuDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Создать новое предложение
    /// </summary>
    public async Task<TripOffer> CreateAsync(TripOffer offer)
    {
        offer.CreatedAt = DateTime.UtcNow;
        offer.UpdatedAt = DateTime.UtcNow;

        _context.TripOffers.Add(offer);
        await _context.SaveChangesAsync();

        return offer;
    }

    /// <summary>
    /// Получить предложение по ID
    /// </summary>
    public async Task<TripOffer?> GetByIdAsync(long id)
    {
        return await _context.TripOffers
            .Include(o => o.Trip)
                .ThenInclude(t => t!.User)
            .Include(o => o.Offerer)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// Проверить существование предложения от Offerer'а на объявление
    /// </summary>
    public async Task<TripOffer?> GetByTripIdAndOffererIdAsync(long tripId, long offererId)
    {
        return await _context.TripOffers
            .Include(o => o.Trip)
            .Include(o => o.Offerer)
            .FirstOrDefaultAsync(o => o.TripId == tripId && o.OffererId == offererId);
    }

    /// <summary>
    /// Получить все предложения Offerer'а
    /// </summary>
    public async Task<List<TripOffer>> GetByOffererIdAsync(long offererId)
    {
        return await _context.TripOffers
            .Include(o => o.Trip)
                .ThenInclude(t => t!.User)
            .Include(o => o.Offerer)
            .Where(o => o.OffererId == offererId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Получить все предложения для объявления
    /// </summary>
    public async Task<List<TripOffer>> GetByTripIdAsync(long tripId)
    {
        return await _context.TripOffers
            .Include(o => o.Offerer)
            .Where(o => o.TripId == tripId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Обновить статус предложения
    /// </summary>
    public async Task<TripOffer> UpdateStatusAsync(long offerId, TripOfferStatus status)
    {
        var offer = await _context.TripOffers.FindAsync(offerId);
        if (offer == null)
        {
            throw new ArgumentException($"Предложение с ID {offerId} не найдено", nameof(offerId));
        }

        offer.Status = status;
        offer.UpdatedAt = DateTime.UtcNow;

        _context.TripOffers.Update(offer);
        await _context.SaveChangesAsync();

        return offer;
    }

    /// <summary>
    /// Обновить предложение
    /// </summary>
    public async Task<TripOffer> UpdateAsync(TripOffer offer)
    {
        offer.UpdatedAt = DateTime.UtcNow;

        _context.TripOffers.Update(offer);
        await _context.SaveChangesAsync();

        return offer;
    }
}


