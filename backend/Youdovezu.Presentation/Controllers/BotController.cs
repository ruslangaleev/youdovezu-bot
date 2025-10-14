using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;
using Youdovezu.Infrastructure.Converters;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Контроллер для обработки webhook запросов от Telegram Bot API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
    private readonly ITelegramBotService _telegramBotService;
    private readonly ILogger<BotController> _logger;

    /// <summary>
    /// Конструктор контроллера
    /// </summary>
    /// <param name="telegramBotService">Сервис для работы с Telegram ботом</param>
    /// <param name="logger">Логгер для записи событий</param>
    public BotController(ITelegramBotService telegramBotService, ILogger<BotController> logger)
    {
        _telegramBotService = telegramBotService;
        _logger = logger;
    }

    /// <summary>
    /// Webhook endpoint для получения обновлений от Telegram
    /// </summary>
    /// <param name="update">Объект обновления от Telegram API</param>
    /// <returns>HTTP 200 OK при успешной обработке</returns>
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update)
    {
        try
        {
            _logger.LogInformation("Received webhook update: {UpdateId}", update.Id);

            // Проверяем наличие сообщения в обновлении
            if (update.Message != null)
            {
                _logger.LogInformation("Message details - ChatId: {ChatId}, Text: {Text}, From: {FromId}", 
                    update.Message.Chat?.Id, 
                    update.Message.Text,
                    update.Message.From?.Id);
                    
                // Преобразуем Telegram сообщение в доменную модель
                var domainMessage = TelegramMessageMapper.ToDomainModel(update.Message);
                
                // Обрабатываем сообщение через сервис
                await _telegramBotService.ProcessMessageAsync(domainMessage);
            }
            else
            {
                _logger.LogWarning("Update has no message. Update type: {UpdateType}", update.Type);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook update");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Endpoint для проверки состояния бота
    /// </summary>
    /// <returns>Информация о состоянии сервиса</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
