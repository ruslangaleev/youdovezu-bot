using Microsoft.AspNetCore.Mvc;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Контроллер для работы с пользователями
/// </summary>
[ApiController]
[Route("api/webapp/users")]
public class UserController : WebAppControllerBase
{
    public UserController(
        IUserService userService,
        TelegramWebAppValidationService validationService,
        ILogger<UserController> logger)
        : base(userService, validationService, logger)
    {
    }

    /// <summary>
    /// Получает информацию о пользователе для веб-приложения
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Информация о пользователе и его возможностях</returns>
    [HttpPost("info")]
    public async Task<IActionResult> GetUserInfo([FromForm] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            Logger.LogInformation("Getting user info for Telegram ID: {TelegramId}", user.TelegramId);

            // Проверяем статус регистрации
            Logger.LogInformation("User {TelegramId} registration status: PrivacyConsent={PrivacyConsent}, PhoneNumber={PhoneNumber}",
                user.TelegramId, user.PrivacyConsent, user.PhoneNumber);

            if (!user.PrivacyConsent)
            {
                Logger.LogInformation("User {TelegramId} has not given privacy consent", user.TelegramId);
                return Ok(new
                {
                    isRegistered = false,
                    isPrivacyConsentGiven = false,
                    message = "Для использования веб-приложения необходимо согласиться с политикой конфиденциальности. Пожалуйста, завершите регистрацию в боте."
                });
            }

            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                Logger.LogInformation("User {TelegramId} has given privacy consent but phone not confirmed", user.TelegramId);
                return Ok(new
                {
                    isRegistered = false,
                    isPrivacyConsentGiven = user.PrivacyConsent,
                    isPhoneConfirmed = false,
                    message = "Для использования веб-приложения необходимо подтвердить номер телефона. Пожалуйста, завершите регистрацию в боте."
                });
            }

            // Пользователь полностью зарегистрирован
            Logger.LogInformation("User {TelegramId} is fully registered: PrivacyConsent={PrivacyConsent}, PhoneConfirmed={PhoneConfirmed}",
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
            Logger.LogError(ex, "Error getting user info");
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
            var user = await ValidateAndGetUserAsync(initData);
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
            Logger.LogError(ex, "Error checking registration status");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}

