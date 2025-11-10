using Youdovezu.Domain.Entities;

namespace Youdovezu.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с предложениями от Offerer'ов
/// </summary>
public interface ITripOfferRepository
{
    /// <summary>
    /// Создать новое предложение
    /// </summary>
    /// <param name="offer">Предложение</param>
    /// <returns>Созданное предложение</returns>
    Task<TripOffer> CreateAsync(TripOffer offer);

    /// <summary>
    /// Получить предложение по ID
    /// </summary>
    /// <param name="id">ID предложения</param>
    /// <returns>Предложение или null</returns>
    Task<TripOffer?> GetByIdAsync(long id);

    /// <summary>
    /// Проверить существование предложения от Offerer'а на объявление
    /// </summary>
    /// <param name="tripId">ID объявления</param>
    /// <param name="offererId">ID Offerer'а</param>
    /// <returns>Предложение или null</returns>
    Task<TripOffer?> GetByTripIdAndOffererIdAsync(long tripId, long offererId);

    /// <summary>
    /// Получить все предложения Offerer'а
    /// </summary>
    /// <param name="offererId">ID Offerer'а</param>
    /// <returns>Список предложений Offerer'а</returns>
    Task<List<TripOffer>> GetByOffererIdAsync(long offererId);

    /// <summary>
    /// Получить все предложения для объявления
    /// </summary>
    /// <param name="tripId">ID объявления</param>
    /// <returns>Список предложений для объявления</returns>
    Task<List<TripOffer>> GetByTripIdAsync(long tripId);

    /// <summary>
    /// Обновить статус предложения
    /// </summary>
    /// <param name="offerId">ID предложения</param>
    /// <param name="status">Новый статус</param>
    /// <returns>Обновленное предложение</returns>
    Task<TripOffer> UpdateStatusAsync(long offerId, TripOfferStatus status);

    /// <summary>
    /// Обновить предложение
    /// </summary>
    /// <param name="offer">Предложение</param>
    /// <returns>Обновленное предложение</returns>
    Task<TripOffer> UpdateAsync(TripOffer offer);
}


