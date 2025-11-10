using Microsoft.Extensions.Logging;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Application.Interfaces;

namespace Youdovezu.Application.Services;

/// <summary>
/// Сервис для поиска поездок
/// </summary>
public class TripSearchService : ITripSearchService
{
    private readonly ITripRepository _tripRepository;
    private readonly ISettlementRepository _settlementRepository;
    private readonly ITripOfferRepository _tripOfferRepository;
    private readonly ILogger<TripSearchService> _logger;

    /// <summary>
    /// Конструктор сервиса
    /// </summary>
    public TripSearchService(
        ITripRepository tripRepository,
        ISettlementRepository settlementRepository,
        ITripOfferRepository tripOfferRepository,
        ILogger<TripSearchService> logger)
    {
        _tripRepository = tripRepository;
        _settlementRepository = settlementRepository;
        _tripOfferRepository = tripOfferRepository;
        _logger = logger;
    }

    /// <summary>
    /// Получить населенные пункты с количеством активных поездок
    /// </summary>
    public async Task<List<SettlementWithTripsCount>> GetSettlementsWithTripsCountAsync()
    {
        _logger.LogInformation("Getting settlements with trips count");

        // Сначала получаем уникальные населенные пункты из существующих поездок
        var settlementsFromTrips = await _settlementRepository.GetSettlementsFromTripsAsync();

        // Создаем список населенных пунктов с количеством поездок
        var result = new List<SettlementWithTripsCount>();

        foreach (var settlementName in settlementsFromTrips)
        {
            var tripsCount = await _tripRepository.GetActiveTripsCountBySettlementAsync(settlementName);

            // Создаем временный объект Settlement для отображения
            // В будущем можно использовать нормализованный справочник
            result.Add(new SettlementWithTripsCount
            {
                Settlement = new Settlement
                {
                    Id = 0, // Временное значение, так как используем названия напрямую
                    Name = settlementName,
                    Type = SettlementType.City, // По умолчанию, можно улучшить определение типа
                    IsActive = true
                },
                TripsCount = tripsCount
            });
        }

        // Сортируем по количеству поездок (по убыванию), затем по названию
        return result
            .OrderByDescending(s => s.TripsCount)
            .ThenBy(s => s.Settlement.Name)
            .ToList();
    }

    /// <summary>
    /// Поиск поездок по населенному пункту
    /// </summary>
    public async Task<List<Trip>> SearchTripsBySettlementAsync(string settlementName, string? fromSettlement = null, string? toSettlement = null, string? searchQuery = null)
    {
        _logger.LogInformation("Searching trips by settlement: {SettlementName}, From: {FromSettlement}, To: {ToSettlement}, Query: {SearchQuery}",
            settlementName, fromSettlement, toSettlement, searchQuery);

        return await _tripRepository.GetActiveTripsBySettlementAsync(settlementName, fromSettlement, toSettlement, searchQuery);
    }

    /// <summary>
    /// Получить детальную информацию об объявлении
    /// </summary>
    public async Task<TripDetailsDto?> GetTripDetailsAsync(long tripId, long offererId)
    {
        _logger.LogInformation("Getting trip details: TripId={TripId}, OffererId={OffererId}", tripId, offererId);

        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null || trip.Status != TripStatus.Active)
        {
            _logger.LogWarning("Trip {TripId} not found or not active", tripId);
            return null;
        }

        // Проверяем, отправлял ли уже Offerer предложение
        var existingOffer = await _tripOfferRepository.GetByTripIdAndOffererIdAsync(tripId, offererId);

        return new TripDetailsDto
        {
            Trip = trip,
            Requester = new RequesterInfo
            {
                Id = trip.User?.Id ?? 0,
                FirstName = trip.User?.FirstName,
                LastName = trip.User?.LastName,
                Username = trip.User?.Username
            },
            HasExistingOffer = existingOffer != null,
            ExistingOffer = existingOffer
        };
    }
}


