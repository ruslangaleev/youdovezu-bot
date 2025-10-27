using Youdovezu.Domain.Entities;

namespace Youdovezu.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с поездками
/// </summary>
public interface ITripService
{
    /// <summary>
    /// Создать новую поездку
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="fromAddress">Адрес отправления</param>
    /// <param name="fromSettlement">Населенный пункт отправления</param>
    /// <param name="fromLatitude">Широта отправления</param>
    /// <param name="fromLongitude">Долгота отправления</param>
    /// <param name="toAddress">Адрес назначения</param>
    /// <param name="toSettlement">Населенный пункт назначения</param>
    /// <param name="toLatitude">Широта назначения</param>
    /// <param name="toLongitude">Долгота назначения</param>
    /// <param name="comment">Комментарий</param>
    /// <returns>Созданная поездка</returns>
    Task<Trip> CreateTripAsync(
        long userId,
        string fromAddress,
        string fromSettlement,
        double? fromLatitude,
        double? fromLongitude,
        string toAddress,
        string toSettlement,
        double? toLatitude,
        double? toLongitude,
        string? comment);

    /// <summary>
    /// Получить все поездки пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <returns>Список поездок</returns>
    Task<List<Trip>> GetUserTripsAsync(long userId);

    /// <summary>
    /// Получить поездку по ID
    /// </summary>
    /// <param name="tripId">ID поездки</param>
    /// <returns>Поездка</returns>
    Task<Trip?> GetTripByIdAsync(long tripId);

    /// <summary>
    /// Обновить поездку
    /// </summary>
    /// <param name="tripId">ID поездки</param>
    /// <param name="userId">ID пользователя (владельца)</param>
    /// <param name="fromAddress">Адрес отправления</param>
    /// <param name="fromSettlement">Населенный пункт отправления</param>
    /// <param name="fromLatitude">Широта отправления</param>
    /// <param name="fromLongitude">Долгота отправления</param>
    /// <param name="toAddress">Адрес назначения</param>
    /// <param name="toSettlement">Населенный пункт назначения</param>
    /// <param name="toLatitude">Широта назначения</param>
    /// <param name="toLongitude">Долгота назначения</param>
    /// <param name="comment">Комментарий</param>
    /// <returns>Обновленная поездка</returns>
    Task<Trip> UpdateTripAsync(
        long tripId,
        long userId,
        string fromAddress,
        string fromSettlement,
        double? fromLatitude,
        double? fromLongitude,
        string toAddress,
        string toSettlement,
        double? toLatitude,
        double? toLongitude,
        string? comment);

    /// <summary>
    /// Удалить поездку
    /// </summary>
    /// <param name="tripId">ID поездки</param>
    /// <param name="userId">ID пользователя (владельца)</param>
    /// <returns>Удаленная поездка</returns>
    Task<Trip> DeleteTripAsync(long tripId, long userId);
}

