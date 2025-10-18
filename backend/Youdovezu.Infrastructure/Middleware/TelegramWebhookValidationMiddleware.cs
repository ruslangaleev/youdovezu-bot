using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Youdovezu.Application.Models;
using Youdovezu.Infrastructure.Services;

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
        // Проверяем webhook endpoint
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
        // Проверяем WebApp endpoints - валидация будет выполнена в контроллере
        else if (context.Request.Path.StartsWithSegments("/api/webapp"))
        {
            _logger.LogInformation("WebApp request from IP: {RemoteIpAddress} - validation will be performed in controller", context.Connection.RemoteIpAddress);
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
            // Читаем тело запроса для возможной будущей валидации (только если есть контент)
            if (context.Request.ContentLength > 0 && context.Request.ContentLength != null)
            {
                try
                {
                    // Проверяем, можно ли читать поток
                    if (context.Request.Body.CanRead)
                    {
                        context.Request.EnableBuffering();
                        
                        if (context.Request.Body.CanSeek)
                        {
                            context.Request.Body.Position = 0;
                        }

                        // Читаем тело запроса по частям для избежания проблем с потоком
                        using var memoryStream = new MemoryStream();
                        await context.Request.Body.CopyToAsync(memoryStream);
                        var body = Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading request body in webhook validation, continuing with header validation");
                }
                finally
                {
                    try
                    {
                        if (context.Request.Body.CanSeek)
                        {
                            context.Request.Body.Position = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not reset request body position in webhook validation");
                    }
                }
            }

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

    /// <summary>
    /// Валидирует запрос от Telegram WebApp, проверяя initData
    /// </summary>
    /// <param name="context">HTTP контекст запроса</param>
    /// <returns>True, если initData валиден, иначе False</returns>
    private async Task<bool> ValidateTelegramWebApp(HttpContext context)
    {
        try
        {
            // Проверяем, есть ли тело запроса
            if (context.Request.ContentLength == 0 || context.Request.ContentLength == null)
            {
                _logger.LogWarning("Empty or missing request body in WebApp request");
                return false;
            }

            _logger.LogInformation("WebApp request - ContentLength: {ContentLength}", context.Request.ContentLength);

            // Для WebApp endpoints с [FromForm] параметрами не читаем тело запроса
            // ASP.NET Core автоматически парсит multipart/form-data
            // Вместо этого проверяем наличие initData в форме после моделирования
            // Пока что пропускаем валидацию initData, так как она будет выполнена в контроллере
            _logger.LogInformation("WebApp request validation skipped - will be validated in controller");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Telegram WebApp request");
            return false;
        }
    }

    /// <summary>
    /// Парсит initData из multipart/form-data
    /// </summary>
    /// <param name="formData">Сырые данные формы</param>
    /// <returns>initData или null</returns>
    private string? ParseInitDataFromFormData(string formData)
    {
        try
        {
            // Простой парсинг multipart/form-data для поиска initData
            var lines = formData.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("name=\"initData\""))
                {
                    // Следующая строка должна содержать значение
                    if (i + 2 < lines.Length)
                    {
                        return lines[i + 2].Trim();
                    }
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing initData from form data");
            return null;
        }
    }

    /// <summary>
    /// Валидирует initData используя HMAC-SHA256
    /// </summary>
    /// <param name="initData">initData от Telegram WebApp</param>
    /// <returns>True, если initData валиден</returns>
    private bool ValidateInitData(string initData)
    {
        try
        {
            var botToken = _telegramSettings.BotToken;
            if (string.IsNullOrEmpty(botToken))
            {
                _logger.LogWarning("BotToken not configured for WebApp validation");
                return false;
            }

            if (string.IsNullOrEmpty(initData))
            {
                _logger.LogWarning("InitData is null or empty");
                return false;
            }

            // Парсим initData
            var queryParams = HttpUtility.ParseQueryString(initData);
            var hash = queryParams["hash"];
            
            if (string.IsNullOrEmpty(hash))
            {
                _logger.LogWarning("Hash not found in initData");
                return false;
            }

            // Удаляем hash из параметров
            queryParams.Remove("hash");

            // Создаем строку для проверки
            var dataCheckString = string.Join("\n", 
                queryParams.AllKeys
                    .OrderBy(key => key)
                    .Select(key => $"{key}={queryParams[key]}"));

            // Создаем секретный ключ
            var secretKey = ComputeHmacSha256(botToken, "WebAppData");

            // Вычисляем хеш
            var calculatedHash = ComputeHmacSha256(dataCheckString, secretKey);

            // Сравниваем хеши
            var isValid = string.Equals(hash, calculatedHash, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
            {
                _logger.LogWarning("Invalid initData hash");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating initData");
            return false;
        }
    }

    /// <summary>
    /// Вычисляет HMAC-SHA256
    /// </summary>
    /// <param name="data">Данные для хеширования</param>
    /// <param name="key">Ключ</param>
    /// <returns>Хеш в hex формате</returns>
    private string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
