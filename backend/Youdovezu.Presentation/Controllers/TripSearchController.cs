using Microsoft.AspNetCore.Mvc;
using Youdovezu.Application.Interfaces;
using Youdovezu.Domain.Entities;
using Youdovezu.Infrastructure.Services;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Контроллер для поиска объявлений от Requester'ов
/// </summary>
[ApiController]
[Route("api/webapp/trips/search")]
public class TripSearchController : WebAppControllerBase
{
    private readonly ITripSearchService _tripSearchService;
    private readonly IDriverDocumentsService _driverDocumentsService;

    public TripSearchController(
        IUserService userService,
        ITripSearchService tripSearchService,
        IDriverDocumentsService driverDocumentsService,
        TelegramWebAppValidationService validationService,
        ILogger<TripSearchController> logger)
        : base(userService, validationService, logger)
    {
        _tripSearchService = tripSearchService;
        _driverDocumentsService = driverDocumentsService;
    }

    /// <summary>
    /// Получить список населенных пунктов с количеством активных поездок
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Список населенных пунктов с количеством поездок</returns>
    [HttpPost("settlements")]
    public async Task<IActionResult> GetSettlements([FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            // Проверяем, что пользователь является Offerer'ом с одобренными документами
            var documentsStatus = await _driverDocumentsService.GetVerificationStatusAsync(user.Id);
            if (documentsStatus != DocumentVerificationStatus.Approved)
            {
                return StatusCode(403, new { error = "Доступ разрешен только Offerer'ам с одобренными документами" });
            }

            Logger.LogInformation("Getting settlements for Offerer {TelegramId}", user.TelegramId);

            var settlements = await _tripSearchService.GetSettlementsWithTripsCountAsync();

            return Ok(settlements.Select(s => new
            {
                id = s.Settlement.Id,
                name = s.Settlement.Name,
                type = s.Settlement.Type.ToString(),
                tripsCount = s.TripsCount
            }));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting settlements");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Поиск объявлений по населенному пункту
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="settlement">Название населенного пункта</param>
    /// <param name="fromSettlement">Фильтр по населенному пункту отправления (опционально)</param>
    /// <param name="toSettlement">Фильтр по населенному пункту назначения (опционально)</param>
    /// <param name="search">Поисковый запрос (опционально)</param>
    /// <returns>Список активных объявлений</returns>
    [HttpPost]
    public async Task<IActionResult> SearchTrips(
        [FromQuery] string initData,
        [FromQuery] string settlement,
        [FromQuery] string? fromSettlement = null,
        [FromQuery] string? toSettlement = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            // Проверяем, что пользователь является Offerer'ом с одобренными документами
            var documentsStatus = await _driverDocumentsService.GetVerificationStatusAsync(user.Id);
            if (documentsStatus != DocumentVerificationStatus.Approved)
            {
                return StatusCode(403, new { error = "Доступ разрешен только Offerer'ам с одобренными документами" });
            }

            if (string.IsNullOrWhiteSpace(settlement))
            {
                return BadRequest(new { error = "Название населенного пункта обязательно" });
            }

            Logger.LogInformation("Searching trips for Offerer {TelegramId} in settlement {Settlement}",
                user.TelegramId, settlement);

            var trips = await _tripSearchService.SearchTripsBySettlementAsync(settlement, fromSettlement, toSettlement, search);

            return Ok(trips.Select(t => new
            {
                id = t.Id,
                fromAddress = t.FromAddress,
                fromSettlement = t.FromSettlement,
                toAddress = t.ToAddress,
                toSettlement = t.ToSettlement,
                comment = t.Comment,
                status = t.Status.ToString(),
                createdAt = t.CreatedAt,
                requester = t.User != null ? new
                {
                    id = t.User.Id,
                    firstName = t.User.FirstName,
                    lastName = t.User.LastName
                } : null
            }));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching trips");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить детальную информацию об объявлении
    /// </summary>
    /// <param name="tripId">ID объявления</param>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Детальная информация об объявлении</returns>
    [HttpPost("{tripId}/details")]
    public async Task<IActionResult> GetTripDetails(long tripId, [FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            // Проверяем, что пользователь является Offerer'ом с одобренными документами
            var documentsStatus = await _driverDocumentsService.GetVerificationStatusAsync(user.Id);
            if (documentsStatus != DocumentVerificationStatus.Approved)
            {
                return StatusCode(403, new { error = "Доступ разрешен только Offerer'ам с одобренными документами" });
            }

            Logger.LogInformation("Getting trip details {TripId} for Offerer {TelegramId}", tripId, user.TelegramId);

            var tripDetails = await _tripSearchService.GetTripDetailsAsync(tripId, user.Id);

            if (tripDetails == null)
            {
                return NotFound(new { error = "Объявление не найдено или не активно" });
            }

            return Ok(new
            {
                trip = new
                {
                    id = tripDetails.Trip.Id,
                    fromAddress = tripDetails.Trip.FromAddress,
                    fromSettlement = tripDetails.Trip.FromSettlement,
                    fromLatitude = tripDetails.Trip.FromLatitude,
                    fromLongitude = tripDetails.Trip.FromLongitude,
                    toAddress = tripDetails.Trip.ToAddress,
                    toSettlement = tripDetails.Trip.ToSettlement,
                    toLatitude = tripDetails.Trip.ToLatitude,
                    toLongitude = tripDetails.Trip.ToLongitude,
                    comment = tripDetails.Trip.Comment,
                    status = tripDetails.Trip.Status.ToString(),
                    createdAt = tripDetails.Trip.CreatedAt
                },
                requester = new
                {
                    id = tripDetails.Requester.Id,
                    firstName = tripDetails.Requester.FirstName,
                    lastName = tripDetails.Requester.LastName,
                    username = tripDetails.Requester.Username
                },
                hasExistingOffer = tripDetails.HasExistingOffer,
                existingOffer = tripDetails.ExistingOffer != null ? new
                {
                    id = tripDetails.ExistingOffer.Id,
                    price = tripDetails.ExistingOffer.Price,
                    comment = tripDetails.ExistingOffer.Comment,
                    status = tripDetails.ExistingOffer.Status.ToString(),
                    createdAt = tripDetails.ExistingOffer.CreatedAt
                } : null
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting trip details");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}

