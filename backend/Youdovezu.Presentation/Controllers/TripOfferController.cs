using Microsoft.AspNetCore.Mvc;
using Youdovezu.Application.Interfaces;
using Youdovezu.Domain.Entities;
using Youdovezu.Infrastructure.Services;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Контроллер для работы с предложениями от Offerer'ов
/// </summary>
[ApiController]
[Route("api/webapp/trips/offers")]
public class TripOfferController : WebAppControllerBase
{
    private readonly ITripOfferService _tripOfferService;
    private readonly IDriverDocumentsService _driverDocumentsService;
    private readonly ITelegramBotService _telegramBotService;

    public TripOfferController(
        IUserService userService,
        ITripOfferService tripOfferService,
        IDriverDocumentsService driverDocumentsService,
        ITelegramBotService telegramBotService,
        TelegramWebAppValidationService validationService,
        ILogger<TripOfferController> logger)
        : base(userService, validationService, logger)
    {
        _tripOfferService = tripOfferService;
        _driverDocumentsService = driverDocumentsService;
        _telegramBotService = telegramBotService;
    }

    /// <summary>
    /// Отправить предложение цены на объявление
    /// </summary>
    /// <param name="tripId">ID объявления</param>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="request">Данные предложения</param>
    /// <returns>Созданное предложение</returns>
    [HttpPost("{tripId}")]
    public async Task<IActionResult> CreateOffer(long tripId, [FromQuery] string initData, [FromBody] CreateTripOfferRequest request)
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

            // Валидация цены
            if (request.Price <= 0)
            {
                return BadRequest(new { error = "Цена должна быть положительным числом" });
            }

            Logger.LogInformation("Creating offer for trip {TripId} by Offerer {TelegramId}, Price: {Price}",
                tripId, user.TelegramId, request.Price);

            // Создаем предложение
            var offer = await _tripOfferService.CreateOfferAsync(tripId, user.Id, request.Price, request.Comment);

            // TODO: Отправить уведомление Requester'у в Telegram бот
            // Для этого нужно получить информацию о Requester'е из поездки через TripRepository
            // и отправить уведомление через _telegramBotService.SendMessageAsync

            Logger.LogInformation("Offer {OfferId} created successfully for trip {TripId} by Offerer {TelegramId}",
                offer.Id, tripId, user.TelegramId);

            return Ok(new
            {
                id = offer.Id,
                tripId = offer.TripId,
                price = offer.Price,
                comment = offer.Comment,
                status = offer.Status.ToString(),
                createdAt = offer.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Invalid arguments for creating offer");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "Invalid operation when creating offer");
            return StatusCode(400, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating offer");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Проверить, отправлял ли Offerer предложение на это объявление
    /// </summary>
    /// <param name="tripId">ID объявления</param>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Информация о предложении или null</returns>
    [HttpPost("{tripId}/my")]
    public async Task<IActionResult> GetMyOffer(long tripId, [FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            Logger.LogInformation("Checking offer for trip {TripId} by Offerer {TelegramId}", tripId, user.TelegramId);

            var offer = await _tripOfferService.CheckIfOfferExistsAsync(tripId, user.Id);

            if (offer == null)
            {
                return Ok(new { hasOffer = false });
            }

            return Ok(new
            {
                hasOffer = true,
                offer = new
                {
                    id = offer.Id,
                    price = offer.Price,
                    comment = offer.Comment,
                    status = offer.Status.ToString(),
                    createdAt = offer.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking offer");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить историю предложений Offerer'а
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Список предложений Offerer'а</returns>
    [HttpPost("my")]
    public async Task<IActionResult> GetMyOffers([FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            Logger.LogInformation("Getting offers for Offerer {TelegramId}", user.TelegramId);

            var offers = await _tripOfferService.GetMyOffersAsync(user.Id);

            return Ok(offers.Select(o => new
            {
                id = o.Id,
                trip = o.Trip != null ? new
                {
                    id = o.Trip.Id,
                    fromAddress = o.Trip.FromAddress,
                    fromSettlement = o.Trip.FromSettlement,
                    toAddress = o.Trip.ToAddress,
                    toSettlement = o.Trip.ToSettlement,
                    status = o.Trip.Status.ToString()
                } : null,
                price = o.Price,
                comment = o.Comment,
                status = o.Status.ToString(),
                createdAt = o.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting offers");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}

/// <summary>
/// Модель запроса для создания предложения
/// </summary>
public class CreateTripOfferRequest
{
    /// <summary>
    /// Предложенная цена за поездку
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Комментарий к предложению (опционально)
    /// </summary>
    public string? Comment { get; set; }
}

