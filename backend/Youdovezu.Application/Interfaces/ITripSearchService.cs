using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;

namespace Youdovezu.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для поиска поездок
/// </summary>
public interface ITripSearchService
{
    /// <summary>
    /// Получить населенные пункты с количеством активных поездок
    /// </summary>
    /// <returns>Список населенных пунктов с количеством поездок</returns>
    Task<List<SettlementWithTripsCount>> GetSettlementsWithTripsCountAsync();

    /// <summary>
    /// Поиск поездок по населенному пункту
    /// </summary>
    /// <param name="settlementName">Название населенного пункта</param>
    /// <param name="fromSettlement">Фильтр по населенному пункту отправления (опционально)</param>
    /// <param name="toSettlement">Фильтр по населенному пункту назначения (опционально)</param>
    /// <param name="searchQuery">Поисковый запрос (опционально)</param>
    /// <returns>Список активных поездок</returns>
    Task<List<Trip>> SearchTripsBySettlementAsync(string settlementName, string? fromSettlement = null, string? toSettlement = null, string? searchQuery = null);

    /// <summary>
    /// Получить детальную информацию об объявлении
    /// </summary>
    /// <param name="tripId">ID объявления</param>
    /// <param name="offererId">ID Offerer'а</param>
    /// <returns>Детальная информация об объявлении или null</returns>
    Task<TripDetailsDto?> GetTripDetailsAsync(long tripId, long offererId);
}

/// <summary>
/// Детальная информация об объявлении для Offerer'а
/// </summary>
public class TripDetailsDto
{
    /// <summary>
    /// Информация о поездке
    /// </summary>
    public Trip Trip { get; set; } = null!;

    /// <summary>
    /// Информация о Requester'е
    /// </summary>
    public RequesterInfo Requester { get; set; } = null!;

    /// <summary>
    /// Флаг, указывающий, отправлял ли уже Offerer предложение на это объявление
    /// </summary>
    public bool HasExistingOffer { get; set; }

    /// <summary>
    /// Существующее предложение (если есть)
    /// </summary>
    public TripOffer? ExistingOffer { get; set; }
}

/// <summary>
/// Информация о Requester'е
/// </summary>
public class RequesterInfo
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Имя
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Имя пользователя в Telegram
    /// </summary>
    public string? Username { get; set; }
}


