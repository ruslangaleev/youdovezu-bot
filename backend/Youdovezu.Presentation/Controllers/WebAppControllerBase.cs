using Microsoft.AspNetCore.Mvc;
using Youdovezu.Application.Interfaces;
using Youdovezu.Domain.Entities;
using Youdovezu.Infrastructure.Services;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Базовый класс для контроллеров веб-приложения Telegram с общими методами валидации
/// </summary>
public abstract class WebAppControllerBase : ControllerBase
{
    protected readonly IUserService UserService;
    protected readonly TelegramWebAppValidationService ValidationService;
    protected readonly ILogger Logger;

    protected WebAppControllerBase(
        IUserService userService,
        TelegramWebAppValidationService validationService,
        ILogger logger)
    {
        UserService = userService;
        ValidationService = validationService;
        Logger = logger;
    }

    /// <summary>
    /// Проверяет и извлекает пользователя из initData
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Пользователь или null, если валидация не прошла</returns>
    protected async Task<User?> ValidateAndGetUserAsync(string initData)
    {
        // Проверяем подлинность initData
        if (!ValidationService.ValidateInitData(initData))
        {
            Logger.LogWarning("Invalid initData provided");
            return null;
        }

        // Извлекаем Telegram ID из initData
        var telegramId = ValidationService.ExtractTelegramId(initData);
        if (!telegramId.HasValue)
        {
            Logger.LogWarning("Could not extract Telegram ID from initData");
            return null;
        }

        // Получаем пользователя
        var user = await UserService.GetUserByTelegramIdAsync(telegramId.Value);
        if (user == null)
        {
            Logger.LogWarning("User not found for Telegram ID: {TelegramId}", telegramId.Value);
        }

        return user;
    }

    /// <summary>
    /// Проверяет, является ли пользователь администратором
    /// </summary>
    /// <param name="user">Пользователь</param>
    /// <returns>True, если пользователь администратор</returns>
    protected bool IsAdmin(User user)
    {
        return user.IsAdmin();
    }
}


