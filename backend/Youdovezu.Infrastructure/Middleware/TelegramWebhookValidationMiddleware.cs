using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Youdovezu.Application.Models;

namespace Youdovezu.Infrastructure.Middleware;

/// <summary>
/// Middleware для валидации webhook запросов от Telegram
/// Проверяет секретный токен для защиты от сторонних запросов
/// </summary>
public class TelegramWebhookValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TelegramWebhookValidationMiddleware> _logger;
    private readonly TelegramSettings _telegramSettings;

    /// <summary>
    /// Конструктор middleware
    /// </summary>
    /// <param name="next">Следующий middleware в pipeline</param>
    /// <param name="logger">Логгер для записи событий</param>
    /// <param name="telegramSettings">Настройки Telegram бота</param>
    public TelegramWebhookValidationMiddleware(RequestDelegate next, ILogger<TelegramWebhookValidationMiddleware> logger, IOptions<TelegramSettings> telegramSettings)
    {
        _next = next;
        _logger = logger;
        _telegramSettings = telegramSettings.Value;
    }

    /// <summary>
    /// Основной метод middleware для обработки HTTP запросов
    /// </summary>
    /// <param name="context">HTTP контекст запроса</param>
    /// <returns>Task, представляющий асинхронную операцию</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Проверяем только webhook endpoint
        if (context.Request.Path.StartsWithSegments("/api/bot/webhook"))
        {
            // Используем настройки из IOptions<TelegramSettings>
            var secretToken = _telegramSettings.SecretToken;
            
            if (!string.IsNullOrEmpty(secretToken))
            {
                if (!await ValidateTelegramWebhook(context, secretToken))
                {
                    _logger.LogWarning("Invalid webhook request from IP: {RemoteIpAddress}", context.Connection.RemoteIpAddress);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
                
                _logger.LogInformation("Valid webhook request from IP: {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            }
            else
            {
                _logger.LogWarning("SecretToken not configured - webhook validation disabled");
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Валидирует webhook запрос от Telegram, проверяя секретный токен
    /// </summary>
    /// <param name="context">HTTP контекст запроса</param>
    /// <param name="secretToken">Ожидаемый секретный токен</param>
    /// <returns>True, если токен валиден, иначе False</returns>
    private async Task<bool> ValidateTelegramWebhook(HttpContext context, string secretToken)
    {
        try
        {
            // Читаем тело запроса для возможной будущей валидации
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;
            
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Получаем заголовок X-Telegram-Bot-Api-Secret-Token от Telegram
            if (!context.Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var receivedToken))
            {
                _logger.LogWarning("Missing X-Telegram-Bot-Api-Secret-Token header");
                return false;
            }

            // Сравниваем полученный токен с ожидаемым
            var isValid = string.Equals(receivedToken.ToString(), secretToken, StringComparison.Ordinal);
            
            if (!isValid)
            {
                _logger.LogWarning("Invalid secret token. Expected: {Expected}, Received: {Received}", 
                    secretToken, receivedToken.ToString());
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Telegram webhook");
            return false;
        }
    }
}
