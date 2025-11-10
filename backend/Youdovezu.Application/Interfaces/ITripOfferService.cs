using Youdovezu.Domain.Entities;

namespace Youdovezu.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с предложениями от Offerer'ов
/// </summary>
public interface ITripOfferService
{
    /// <summary>
    /// Создать предложение от Offerer'а
    /// </summary>
    /// <param name="tripId">ID объявления</param>
    /// <param name="offererId">ID Offerer'а</param>
    /// <param name="price">Предложенная цена</param>
    /// <param name="comment">Комментарий к предложению (опционально)</param>
    /// <returns>Созданное предложение</returns>
    Task<TripOffer> CreateOfferAsync(long tripId, long offererId, decimal price, string? comment);

    /// <summary>
    /// Получить все предложения Offerer'а
    /// </summary>
    /// <param name="offererId">ID Offerer'а</param>
    /// <returns>Список предложений Offerer'а</returns>
    Task<List<TripOffer>> GetMyOffersAsync(long offererId);

    /// <summary>
    /// Проверить существование предложения от Offerer'а на объявление
    /// </summary>
    /// <param name="tripId">ID объявления</param>
    /// <param name="offererId">ID Offerer'а</param>
    /// <returns>Предложение или null</returns>
    Task<TripOffer?> CheckIfOfferExistsAsync(long tripId, long offererId);
}


