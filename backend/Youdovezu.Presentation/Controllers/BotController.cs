using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Youdovezu.Application.Interfaces;

namespace Youdovezu.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
    private readonly ITelegramBotService _telegramBotService;
    private readonly ILogger<BotController> _logger;

    public BotController(ITelegramBotService telegramBotService, ILogger<BotController> logger)
    {
        _telegramBotService = telegramBotService;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update)
    {
        try
        {
            _logger.LogInformation("Received webhook update: {UpdateId}", update.Id);

            if (update.Message != null)
            {
                await _telegramBotService.ProcessMessageAsync(update.Message);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook update");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
