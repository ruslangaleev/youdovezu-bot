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
    private readonly TelegramWebAppValidationService _validationService;
    private readonly ILogger<WebAppController> _logger;

    /// <summary>
    /// Конструктор контроллера
    /// </summary>
    /// <param name="userService">Сервис для работы с пользователями</param>
    /// <param name="validationService">Сервис для проверки initData</param>
    /// <param name="logger">Логгер для записи событий</param>
    public WebAppController(IUserService userService, TelegramWebAppValidationService validationService, ILogger<WebAppController> logger)
    {
        _userService = userService;
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
            if (!user.PrivacyConsent)
            {
                return Ok(new
                {
                    isRegistered = false,
                    isPrivacyConsentGiven = false,
                    message = "Для использования веб-приложения необходимо согласиться с политикой конфиденциальности. Пожалуйста, завершите регистрацию в боте."
                });
            }

            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                return Ok(new
                {
                    isRegistered = false,
                    isPrivacyConsentGiven = true,
                    isPhoneConfirmed = false,
                    message = "Для использования веб-приложения необходимо подтвердить номер телефона. Пожалуйста, завершите регистрацию в боте."
                });
            }

            // Пользователь полностью зарегистрирован
            return Ok(new
            {
                isRegistered = true,
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
}
