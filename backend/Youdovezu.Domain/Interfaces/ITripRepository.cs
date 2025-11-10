using Youdovezu.Domain.Entities;

namespace Youdovezu.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с поездками
/// </summary>
public interface ITripRepository
{
    /// <summary>
    /// Получить поездку по ID
    /// </summary>
    /// <param name="id">ID поездки</param>
    /// <returns>Поездка или null</returns>
    Task<Trip?> GetByIdAsync(long id);

    /// <summary>
    /// Получить все поездки пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <returns>Список поездок пользователя</returns>
    Task<List<Trip>> GetByUserIdAsync(long userId);

    /// <summary>
    /// Создать новую поездку
    /// </summary>
    /// <param name="trip">Поездка</param>
    /// <returns>Созданная поездка</returns>
    Task<Trip> CreateAsync(Trip trip);

    /// <summary>
    /// Обновить поездку
    /// </summary>
    /// <param name="trip">Поездка</param>
    /// <returns>Обновленная поездка</returns>
    Task<Trip> UpdateAsync(Trip trip);

    /// <summary>
    /// Удалить поездку (мягкое удаление)
    /// </summary>
    /// <param name="trip">Поездка</param>
    /// <returns>Удаленная поездка</returns>
    Task<Trip> DeleteAsync(Trip trip);

    /// <summary>
    /// Получить активные поездки по населенному пункту
    /// </summary>
    /// <param name="settlementName">Название населенного пункта</param>
    /// <param name="fromSettlement">Фильтр по населенному пункту отправления (опционально)</param>
    /// <param name="toSettlement">Фильтр по населенному пункту назначения (опционально)</param>
    /// <param name="searchQuery">Поисковый запрос (опционально)</param>
    /// <returns>Список активных поездок</returns>
    Task<List<Trip>> GetActiveTripsBySettlementAsync(string settlementName, string? fromSettlement = null, string? toSettlement = null, string? searchQuery = null);

    /// <summary>
    /// Получить количество активных поездок в населенном пункте
    /// </summary>
    /// <param name="settlementName">Название населенного пункта</param>
    /// <returns>Количество активных поездок</returns>
    Task<int> GetActiveTripsCountBySettlementAsync(string settlementName);
}

