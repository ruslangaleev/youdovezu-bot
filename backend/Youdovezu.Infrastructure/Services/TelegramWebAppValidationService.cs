using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Youdovezu.Infrastructure.Services;

/// <summary>
/// Сервис для проверки подлинности Telegram WebApp initData
/// </summary>
public class TelegramWebAppValidationService
{
    private readonly string _botToken;
    private readonly ILogger<TelegramWebAppValidationService> _logger;

    /// <summary>
    /// Конструктор сервиса
    /// </summary>
    /// <param name="botToken">Токен бота</param>
    /// <param name="logger">Логгер</param>
    public TelegramWebAppValidationService(string botToken, ILogger<TelegramWebAppValidationService> logger)
    {
        _botToken = botToken;
        _logger = logger;
    }

    /// <summary>
    /// Проверяет подлинность initData от Telegram WebApp
    /// </summary>
    /// <param name="initData">Строка initData от Telegram</param>
    /// <returns>True, если данные подлинные</returns>
    public bool ValidateInitData(string initData)
    {
        try
        {
            if (string.IsNullOrEmpty(initData))
            {
                _logger.LogWarning("InitData is null or empty");
                return false;
            }

            // 1. Разбираем initData на пары ключ-значение
            var hash = "";
            var parameters = new Dictionary<string, string>();
            
            var paramPairs = initData.Split('&');
            foreach (var pair in paramPairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0];
                    var value = keyValue[1];
                    
                    if (key == "hash")
                    {
                        hash = value;
                    }
                    else
                    {
                        // 2. Исключаем поле hash из проверки, так как оно используется для сравнения
                        

                        if (key == "user")
                        {
                            string decodedUser = Uri.UnescapeDataString(value);

                            value = decodedUser;
                        }

                        parameters[key] = value;
                    }
                }
            }
            
            if (string.IsNullOrEmpty(hash))
            {
                _logger.LogWarning("Hash not found in initData");
                return false;
            }

            // Проверяем freshness auth_date (не старше 24 часов)
            if (parameters.TryGetValue("auth_date", out var authDateStr) && long.TryParse(authDateStr, out var authDate))
            {
                var authTime = DateTimeOffset.FromUnixTimeSeconds(authDate);
                if (DateTimeOffset.UtcNow - authTime > TimeSpan.FromHours(24))
                {
                    _logger.LogWarning("InitData is too old: {AuthTime}", authTime);
                    return false;
                }
            }

            // 3. Сортируем оставшиеся пары по ключам в алфавитном порядке
            // 4. Объединяем пары в строку формата key1=value1\nkey2=value2\n....
            var dataCheckString = string.Join("\n", 
                parameters.Keys
                    .OrderBy(key => key)
                    .Select(key => $"{key}={parameters[key]}"));

            // Создаем секретный ключ (HMAC-SHA256 от токена бота с ключом "WebAppData")
            var secretKeyBytes = ComputeHmacSha256Bytes(_botToken, "WebAppData");
            
            _logger.LogInformation("Validation debug - BotToken length: {BotTokenLength}, DataCheckString: {DataCheckString}", 
                _botToken?.Length ?? 0, dataCheckString);

            // Вычисляем хеш используя байты секретного ключа
            var calculatedHash = ComputeHmacSha256WithKeyBytes(dataCheckString, secretKeyBytes);

            // Сравниваем хеши
            var isValid = string.Equals(hash, calculatedHash, StringComparison.OrdinalIgnoreCase);
            
            _logger.LogInformation("Hash comparison - Received: {ReceivedHash}, Calculated: {CalculatedHash}, Valid: {IsValid}", 
                hash, calculatedHash, isValid);
            
            // Дополнительная отладка
            _logger.LogInformation("SecretKey hex: {SecretKeyHex}", BitConverter.ToString(secretKeyBytes).Replace("-", "").ToLowerInvariant());
            _logger.LogInformation("DataCheckString bytes: {DataCheckStringBytes}", Encoding.UTF8.GetBytes(dataCheckString).Length);
            
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
    /// Извлекает Telegram ID пользователя из initData
    /// </summary>
    /// <param name="initData">Строка initData от Telegram</param>
    /// <returns>Telegram ID пользователя или null</returns>
    public long? ExtractTelegramId(string initData)
    {
        try
        {
            if (string.IsNullOrEmpty(initData))
                return null;

            var queryParams = HttpUtility.ParseQueryString(initData);
            var userParam = queryParams["user"];

            if (string.IsNullOrEmpty(userParam))
                return null;

            // Парсим JSON пользователя
            var userData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(userParam);
            
            if (userData != null && userData.TryGetValue("id", out var idValue))
            {
                if (long.TryParse(idValue.ToString(), out var telegramId))
                {
                    return telegramId;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting Telegram ID from initData");
            return null;
        }
    }

    /// <summary>
    /// Вычисляет HMAC-SHA256 и возвращает байты
    /// </summary>
    /// <param name="data">Данные для хеширования</param>
    /// <param name="key">Ключ</param>
    /// <returns>Хеш в виде байтов</returns>
    private byte[] ComputeHmacSha256Bytes(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// Вычисляет HMAC-SHA256 используя байты как ключ
    /// </summary>
    /// <param name="data">Данные для хеширования</param>
    /// <param name="keyBytes">Ключ в виде байтов</param>
    /// <returns>Хеш в hex формате</returns>
    private string ComputeHmacSha256WithKeyBytes(string data, byte[] keyBytes)
    {
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}

