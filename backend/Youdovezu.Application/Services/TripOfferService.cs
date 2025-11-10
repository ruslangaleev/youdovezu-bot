using Microsoft.Extensions.Logging;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Application.Interfaces;

namespace Youdovezu.Application.Services;

/// <summary>
/// Сервис для работы с предложениями от Offerer'ов
/// </summary>
public class TripOfferService : ITripOfferService
{
    private readonly ITripOfferRepository _tripOfferRepository;
    private readonly ITripRepository _tripRepository;
    private readonly ILogger<TripOfferService> _logger;

    /// <summary>
    /// Конструктор сервиса
    /// </summary>
    public TripOfferService(
        ITripOfferRepository tripOfferRepository,
        ITripRepository tripRepository,
        ILogger<TripOfferService> logger)
    {
        _tripOfferRepository = tripOfferRepository;
        _tripRepository = tripRepository;
        _logger = logger;
    }

    /// <summary>
    /// Создать предложение от Offerer'а
    /// </summary>
    public async Task<TripOffer> CreateOfferAsync(long tripId, long offererId, decimal price, string? comment)
    {
        _logger.LogInformation("Creating offer: TripId={TripId}, OffererId={OffererId}, Price={Price}", tripId, offererId, price);

        // Проверяем, что объявление существует и активно
        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null)
        {
            throw new ArgumentException($"Объявление с ID {tripId} не найдено", nameof(tripId));
        }

        if (trip.Status != TripStatus.Active)
        {
            throw new InvalidOperationException($"Объявление с ID {tripId} не активно");
        }

        // Проверяем, что Offerer не отправлял уже предложение
        var existingOffer = await _tripOfferRepository.GetByTripIdAndOffererIdAsync(tripId, offererId);
        if (existingOffer != null)
        {
            throw new InvalidOperationException($"Предложение на объявление {tripId} уже было отправлено");
        }

        // Проверяем, что Offerer не является Requester'ом этого объявления
        if (trip.UserId == offererId)
        {
            throw new InvalidOperationException("Нельзя отправить предложение на свое собственное объявление");
        }

        // Создаем предложение
        var offer = new TripOffer
        {
            TripId = tripId,
            OffererId = offererId,
            Price = price,
            Comment = comment,
            Status = TripOfferStatus.Pending
        };

        var createdOffer = await _tripOfferRepository.CreateAsync(offer);
        _logger.LogInformation("Offer created successfully: OfferId={OfferId}", createdOffer.Id);

        return createdOffer;
    }

    /// <summary>
    /// Получить все предложения Offerer'а
    /// </summary>
    public async Task<List<TripOffer>> GetMyOffersAsync(long offererId)
    {
        _logger.LogInformation("Getting offers for Offerer: {OffererId}", offererId);
        return await _tripOfferRepository.GetByOffererIdAsync(offererId);
    }

    /// <summary>
    /// Проверить существование предложения от Offerer'а на объявление
    /// </summary>
    public async Task<TripOffer?> CheckIfOfferExistsAsync(long tripId, long offererId)
    {
        return await _tripOfferRepository.GetByTripIdAndOffererIdAsync(tripId, offererId);
    }
}


