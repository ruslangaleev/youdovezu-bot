using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using YouDovezu.Application.Commands.Telegram;

namespace YouDovezu.API.Controllers;

/// <summary>
/// Контроллер для обработки webhook'ов от Telegram Bot API
/// </summary>
/// <remarks>
/// Отвечает только за маршрутизацию запросов к соответствующим командам MediatR.
/// Вся бизнес-логика находится в Application слое.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TelegramController> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр TelegramController
    /// </summary>
    /// <param name="mediator">MediatR для отправки команд</param>
    /// <param name="logger">Логгер</param>
    public TelegramController(IMediator mediator, ILogger<TelegramController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатывает webhook от Telegram Bot API
    /// </summary>
    /// <param name="update">Обновление от Telegram</param>
    /// <returns>Результат обработки</returns>
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update)
    {
        try
        {
            // Маршрутизация к соответствующей команде MediatR
            if (update.Message?.Text == "/start")
            {
                await HandleStartCommandAsync(update.Message);
            }
            else if (update.CallbackQuery != null)
            {
                await HandleCallbackQueryAsync(update.CallbackQuery);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Telegram webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Обрабатывает команду /start
    /// </summary>
    /// <param name="message">Сообщение от пользователя</param>
    private async Task HandleStartCommandAsync(Message message)
    {
        var user = message.From;
        if (user == null) return;

        // Создаем команду для обработки /start
        var command = new HandleStartCommand
        {
            TelegramId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            ChatId = message.Chat.Id
        };

        // Отправляем команду в Application слой
        await _mediator.Send(command);
    }

    /// <summary>
    /// Обрабатывает callback query от inline клавиатур
    /// </summary>
    /// <param name="callbackQuery">Callback query от пользователя</param>
    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        // Создаем команду для обработки callback
        var command = new HandleCallbackQueryCommand
        {
            CallbackQueryId = callbackQuery.Id,
            Data = callbackQuery.Data ?? string.Empty,
            TelegramId = callbackQuery.From.Id,
            FirstName = callbackQuery.From.FirstName,
            LastName = callbackQuery.From.LastName,
            Username = callbackQuery.From.Username,
            ChatId = callbackQuery.Message!.Chat.Id
        };

        // Отправляем команду в Application слой
        await _mediator.Send(command);
    }
}
