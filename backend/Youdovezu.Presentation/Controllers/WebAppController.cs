using Microsoft.AspNetCore.Mvc;
using Youdovezu.Application.Interfaces;
using Youdovezu.Domain.Entities;
using Youdovezu.Infrastructure.Services;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Контроллер для работы с веб-приложением Telegram
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebAppController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITripService _tripService;
    private readonly TelegramWebAppValidationService _validationService;
    private readonly ILogger<WebAppController> _logger;

    /// <summary>
    /// Конструктор контроллера
    /// </summary>
    /// <param name="userService">Сервис для работы с пользователями</param>
    /// <param name="tripService">Сервис для работы с поездками</param>
    /// <param name="validationService">Сервис для проверки initData</param>
    /// <param name="logger">Логгер для записи событий</param>
    public WebAppController(IUserService userService, ITripService tripService, TelegramWebAppValidationService validationService, ILogger<WebAppController> logger)
    {
        _userService = userService;
        _tripService = tripService;
        _validationService = validationService;
        _logger = logger;
    }

    /// <summary>
    /// Получает информацию о пользователе для веб-приложения
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Информация о пользователе и его возможностях</returns>
    [HttpPost("user")]
    public async Task<IActionResult> GetUserInfo([FromForm] string initData)
    {
        try
        {
            // Проверяем подлинность initData
            if (!_validationService.ValidateInitData(initData))
            {
                _logger.LogWarning("Invalid initData provided");
                return Unauthorized(new { error = "Неверные данные авторизации" });
            }

            // Извлекаем Telegram ID из initData
            var telegramId = _validationService.ExtractTelegramId(initData);
            if (!telegramId.HasValue)
            {
                _logger.LogWarning("Could not extract Telegram ID from initData");
                return BadRequest(new { error = "Не удалось определить пользователя" });
            }

            _logger.LogInformation("Getting user info for Telegram ID: {TelegramId}", telegramId.Value);

            var user = await _userService.GetUserByTelegramIdAsync(telegramId.Value);
            
            if (user == null)
            {
                return Ok(new
                {
                    isRegistered = false,
                    message = "Пользователь не найден. Пожалуйста, завершите регистрацию в боте."
                });
            }

            // Проверяем статус регистрации
            _logger.LogInformation("User {TelegramId} registration status: PrivacyConsent={PrivacyConsent}, PhoneNumber={PhoneNumber}", 
                user.TelegramId, user.PrivacyConsent, user.PhoneNumber);
            
            if (!user.PrivacyConsent)
            {
                _logger.LogInformation("User {TelegramId} has not given privacy consent", user.TelegramId);
                return Ok(new
                {
                    isRegistered = false,
                    isPrivacyConsentGiven = false,
                    message = "Для использования веб-приложения необходимо согласиться с политикой конфиденциальности. Пожалуйста, завершите регистрацию в боте."
                });
            }

            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                _logger.LogInformation("User {TelegramId} has given privacy consent but phone not confirmed", user.TelegramId);
                return Ok(new
                {
                    isRegistered = false,
                    isPrivacyConsentGiven = user.PrivacyConsent,
                    isPhoneConfirmed = false,
                    message = "Для использования веб-приложения необходимо подтвердить номер телефона. Пожалуйста, завершите регистрацию в боте."
                });
            }

            // Пользователь полностью зарегистрирован
            _logger.LogInformation("User {TelegramId} is fully registered: PrivacyConsent={PrivacyConsent}, PhoneConfirmed={PhoneConfirmed}", 
                user.TelegramId, user.PrivacyConsent, !string.IsNullOrEmpty(user.PhoneNumber));
            
            return Ok(new
            {
                isRegistered = true,
                isPrivacyConsentGiven = user.PrivacyConsent,
                isPhoneConfirmed = !string.IsNullOrEmpty(user.PhoneNumber),
                user = new
                {
                    id = user.Id,
                    telegramId = user.TelegramId,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    username = user.Username,
                    phoneNumber = user.PhoneNumber,
                    systemRole = user.SystemRole.ToString(),
                    canBePassenger = user.CanBePassenger,
                    canBeDriver = user.CanBeDriver,
                    isTrialActive = user.IsTrialActive(),
                    trialEndDate = user.TrialEndDate
                },
                capabilities = new
                {
                    canSearchTrips = user.CanActAsPassenger(),
                    canCreateTrips = user.CanActAsDriver(),
                    isModerator = user.IsModerator(),
                    isAdmin = user.IsAdmin()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Проверяет статус регистрации пользователя
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Статус регистрации</returns>
    [HttpPost("registration-status")]
    public async Task<IActionResult> GetRegistrationStatus([FromForm] string initData)
    {
        try
        {
            // Проверяем подлинность initData
            if (!_validationService.ValidateInitData(initData))
            {
                _logger.LogWarning("Invalid initData provided for registration status check");
                return Unauthorized(new { error = "Неверные данные авторизации" });
            }

            // Извлекаем Telegram ID из initData
            var telegramId = _validationService.ExtractTelegramId(initData);
            if (!telegramId.HasValue)
            {
                _logger.LogWarning("Could not extract Telegram ID from initData for registration status");
                return BadRequest(new { error = "Не удалось определить пользователя" });
            }

            var user = await _userService.GetUserByTelegramIdAsync(telegramId.Value);
            
            if (user == null)
            {
                return Ok(new
                {
                    status = "not_registered",
                    message = "Пользователь не зарегистрирован"
                });
            }

            if (!user.PrivacyConsent)
            {
                return Ok(new
                {
                    status = "privacy_not_consented",
                    message = "Не дано согласие с политикой конфиденциальности"
                });
            }

            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                return Ok(new
                {
                    status = "phone_not_confirmed",
                    message = "Номер телефона не подтвержден"
                });
            }

            return Ok(new
            {
                status = "registered",
                message = "Пользователь полностью зарегистрирован",
                canBePassenger = user.CanBePassenger,
                canBeDriver = user.CanBeDriver
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking registration status");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Создает новую поездку
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="request">Данные для создания поездки</param>
    /// <returns>Созданная поездка</returns>
    [HttpPost("trips")]
    public async Task<IActionResult> CreateTrip([FromQuery] string initData, [FromBody] CreateTripRequest request)
    {
        try
        {
            // Проверяем подлинность initData
            if (!_validationService.ValidateInitData(initData))
            {
                _logger.LogWarning("Invalid initData provided for trip creation");
                return Unauthorized(new { error = "Неверные данные авторизации" });
            }

            // Извлекаем Telegram ID из initData
            var telegramId = _validationService.ExtractTelegramId(initData);
            if (!telegramId.HasValue)
            {
                _logger.LogWarning("Could not extract Telegram ID from initData for trip creation");
                return BadRequest(new { error = "Не удалось определить пользователя" });
            }

            // Получаем пользователя
            var user = await _userService.GetUserByTelegramIdAsync(telegramId.Value);
            if (user == null)
            {
                return Unauthorized(new { error = "Пользователь не найден" });
            }

            _logger.LogInformation("Creating trip for user {TelegramId}: {From} -> {To}", 
                telegramId.Value, request.FromAddress, request.ToAddress);

            // Создаем поездку
            var trip = await _tripService.CreateTripAsync(
                user.Id,
                request.FromAddress,
                request.FromSettlement,
                request.FromLatitude,
                request.FromLongitude,
                request.ToAddress,
                request.ToSettlement,
                request.ToLatitude,
                request.ToLongitude,
                request.Comment);

            _logger.LogInformation("Trip {TripId} created successfully for user {TelegramId}", 
                trip.Id, telegramId.Value);

            return Ok(new
            {
                id = trip.Id,
                fromAddress = trip.FromAddress,
                fromSettlement = trip.FromSettlement,
                toAddress = trip.ToAddress,
                toSettlement = trip.ToSettlement,
                comment = trip.Comment,
                status = trip.Status.ToString(),
                createdAt = trip.CreatedAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to create trip");
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trip");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получает все поездки пользователя
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Список поездок пользователя</returns>
    [HttpPost("trips/my")]
    public async Task<IActionResult> GetMyTrips([FromQuery] string initData)
    {
        try
        {
            // Проверяем подлинность initData
            if (!_validationService.ValidateInitData(initData))
            {
                _logger.LogWarning("Invalid initData provided for getting trips");
                return Unauthorized(new { error = "Неверные данные авторизации" });
            }

            // Извлекаем Telegram ID из initData
            var telegramId = _validationService.ExtractTelegramId(initData);
            if (!telegramId.HasValue)
            {
                _logger.LogWarning("Could not extract Telegram ID from initData for getting trips");
                return BadRequest(new { error = "Не удалось определить пользователя" });
            }

            // Получаем пользователя
            var user = await _userService.GetUserByTelegramIdAsync(telegramId.Value);
            if (user == null)
            {
                return Unauthorized(new { error = "Пользователь не найден" });
            }

            _logger.LogInformation("Getting trips for user {TelegramId}", telegramId.Value);

            // Получаем поездки пользователя
            var trips = await _tripService.GetUserTripsAsync(user.Id);

            return Ok(new
            {
                trips = trips.Select(t => new
                {
                    id = t.Id,
                    fromAddress = t.FromAddress,
                    fromSettlement = t.FromSettlement,
                    toAddress = t.ToAddress,
                    toSettlement = t.ToSettlement,
                    comment = t.Comment,
                    status = t.Status.ToString(),
                    createdAt = t.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user trips");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Обновляет поездку
    /// </summary>
    /// <param name="tripId">ID поездки</param>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="request">Данные для обновления поездки</param>
    /// <returns>Обновленная поездка</returns>
    [HttpPut("trips/{tripId}")]
    public async Task<IActionResult> UpdateTrip(long tripId, [FromQuery] string initData, [FromBody] UpdateTripRequest request)
    {
        try
        {
            // Проверяем подлинность initData
            if (!_validationService.ValidateInitData(initData))
            {
                _logger.LogWarning("Invalid initData provided for trip update");
                return Unauthorized(new { error = "Неверные данные авторизации" });
            }

            // Извлекаем Telegram ID из initData
            var telegramId = _validationService.ExtractTelegramId(initData);
            if (!telegramId.HasValue)
            {
                _logger.LogWarning("Could not extract Telegram ID from initData for trip update");
                return BadRequest(new { error = "Не удалось определить пользователя" });
            }

            // Получаем пользователя
            var user = await _userService.GetUserByTelegramIdAsync(telegramId.Value);
            if (user == null)
            {
                return Unauthorized(new { error = "Пользователь не найден" });
            }

            _logger.LogInformation("Updating trip {TripId} for user {TelegramId}", tripId, telegramId.Value);

            // Обновляем поездку
            var trip = await _tripService.UpdateTripAsync(
                tripId,
                user.Id,
                request.FromAddress,
                request.FromSettlement,
                request.FromLatitude,
                request.FromLongitude,
                request.ToAddress,
                request.ToSettlement,
                request.ToLatitude,
                request.ToLongitude,
                request.Comment);

            _logger.LogInformation("Trip {TripId} updated successfully for user {TelegramId}", trip.Id, telegramId.Value);

            return Ok(new
            {
                id = trip.Id,
                fromAddress = trip.FromAddress,
                fromSettlement = trip.FromSettlement,
                toAddress = trip.ToAddress,
                toSettlement = trip.ToSettlement,
                comment = trip.Comment,
                status = trip.Status.ToString(),
                updatedAt = trip.UpdatedAt
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Trip not found for update");
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update trip");
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating trip");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удаляет поездку
    /// </summary>
    /// <param name="tripId">ID поездки</param>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Результат удаления</returns>
    [HttpDelete("trips/{tripId}")]
    public async Task<IActionResult> DeleteTrip(long tripId, [FromQuery] string initData)
    {
        try
        {
            // Проверяем подлинность initData
            if (!_validationService.ValidateInitData(initData))
            {
                _logger.LogWarning("Invalid initData provided for trip deletion");
                return Unauthorized(new { error = "Неверные данные авторизации" });
            }

            // Извлекаем Telegram ID из initData
            var telegramId = _validationService.ExtractTelegramId(initData);
            if (!telegramId.HasValue)
            {
                _logger.LogWarning("Could not extract Telegram ID from initData for trip deletion");
                return BadRequest(new { error = "Не удалось определить пользователя" });
            }

            // Получаем пользователя
            var user = await _userService.GetUserByTelegramIdAsync(telegramId.Value);
            if (user == null)
            {
                return Unauthorized(new { error = "Пользователь не найден" });
            }

            _logger.LogInformation("Deleting trip {TripId} for user {TelegramId}", tripId, telegramId.Value);

            // Удаляем поездку
            var trip = await _tripService.DeleteTripAsync(tripId, user.Id);

            _logger.LogInformation("Trip {TripId} deleted successfully for user {TelegramId}", trip.Id, telegramId.Value);

            return Ok(new
            {
                id = trip.Id,
                message = "Поездка успешно удалена"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Trip not found for deletion");
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to delete trip");
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting trip");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}

/// <summary>
/// Модель запроса для создания поездки
/// </summary>
public class CreateTripRequest
{
    /// <summary>
    /// Адрес отправления (название улицы)
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Населенный пункт отправления
    /// </summary>
    public string FromSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта пункта отправления
    /// </summary>
    public double? FromLatitude { get; set; }

    /// <summary>
    /// Долгота пункта отправления
    /// </summary>
    public double? FromLongitude { get; set; }

    /// <summary>
    /// Адрес назначения (название улицы)
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Населенный пункт назначения
    /// </summary>
    public string ToSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта пункта назначения
    /// </summary>
    public double? ToLatitude { get; set; }

    /// <summary>
    /// Долгота пункта назначения
    /// </summary>
    public double? ToLongitude { get; set; }

    /// <summary>
    /// Комментарий к поездке (дополнительная информация)
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Модель запроса для обновления поездки
/// </summary>
public class UpdateTripRequest
{
    /// <summary>
    /// Адрес отправления (название улицы)
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Населенный пункт отправления
    /// </summary>
    public string FromSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта пункта отправления
    /// </summary>
    public double? FromLatitude { get; set; }

    /// <summary>
    /// Долгота пункта отправления
    /// </summary>
    public double? FromLongitude { get; set; }

    /// <summary>
    /// Адрес назначения (название улицы)
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Населенный пункт назначения
    /// </summary>
    public string ToSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта пункта назначения
    /// </summary>
    public double? ToLatitude { get; set; }

    /// <summary>
    /// Долгота пункта назначения
    /// </summary>
    public double? ToLongitude { get; set; }

    /// <summary>
    /// Комментарий к поездке (дополнительная информация)
    /// </summary>
    public string? Comment { get; set; }
}
