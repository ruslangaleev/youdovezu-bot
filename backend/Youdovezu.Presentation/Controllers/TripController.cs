using Microsoft.AspNetCore.Mvc;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Контроллер для работы с поездками
/// </summary>
[ApiController]
[Route("api/webapp/trips")]
public class TripController : WebAppControllerBase
{
    private readonly ITripService _tripService;

    public TripController(
        IUserService userService,
        ITripService tripService,
        TelegramWebAppValidationService validationService,
        ILogger<TripController> logger)
        : base(userService, validationService, logger)
    {
        _tripService = tripService;
    }

    /// <summary>
    /// Создает новую поездку
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="request">Данные для создания поездки</param>
    /// <returns>Созданная поездка</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTrip([FromQuery] string initData, [FromBody] CreateTripRequest request)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            Logger.LogInformation("Creating trip for user {TelegramId}: {From} -> {To}",
                user.TelegramId, request.FromAddress, request.ToAddress);

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

            Logger.LogInformation("Trip {TripId} created successfully for user {TelegramId}",
                trip.Id, user.TelegramId);

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
            Logger.LogWarning(ex, "Unauthorized attempt to create trip");
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating trip");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получает все поездки пользователя
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Список поездок пользователя</returns>
    [HttpPost("my")]
    public async Task<IActionResult> GetMyTrips([FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            Logger.LogInformation("Getting trips for user {TelegramId}", user.TelegramId);

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
            Logger.LogError(ex, "Error getting user trips");
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
    [HttpPut("{tripId}")]
    public async Task<IActionResult> UpdateTrip(long tripId, [FromQuery] string initData, [FromBody] UpdateTripRequest request)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            Logger.LogInformation("Updating trip {TripId} for user {TelegramId}", tripId, user.TelegramId);

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

            Logger.LogInformation("Trip {TripId} updated successfully for user {TelegramId}", trip.Id, user.TelegramId);

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
            Logger.LogWarning(ex, "Trip not found for update");
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex, "Unauthorized attempt to update trip");
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating trip");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удаляет поездку
    /// </summary>
    /// <param name="tripId">ID поездки</param>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Результат удаления</returns>
    [HttpDelete("{tripId}")]
    public async Task<IActionResult> DeleteTrip(long tripId, [FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            Logger.LogInformation("Deleting trip {TripId} for user {TelegramId}", tripId, user.TelegramId);

            // Удаляем поездку
            var trip = await _tripService.DeleteTripAsync(tripId, user.Id);

            Logger.LogInformation("Trip {TripId} deleted successfully for user {TelegramId}", trip.Id, user.TelegramId);

            return Ok(new
            {
                id = trip.Id,
                message = "Поездка успешно удалена"
            });
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Trip not found for deletion");
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex, "Unauthorized attempt to delete trip");
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting trip");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}


