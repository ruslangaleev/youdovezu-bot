using Telegram.Bot;
using Telegram.Bot.Types;
using Youdovezu.Application.Interfaces;

namespace Youdovezu.Application.Services;

public class TelegramBotService : ITelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotService> _logger;

    public TelegramBotService(ITelegramBotClient botClient, ILogger<TelegramBotService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        try
        {
            await _botClient.SendTextMessageAsync(chatId, message);
            _logger.LogInformation("Message sent to chat {ChatId}: {Message}", chatId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to chat {ChatId}", chatId);
            throw;
        }
    }

    public async Task ProcessMessageAsync(Message message)
    {
        if (message.Text == null)
        {
            _logger.LogWarning("Received message without text from chat {ChatId}", message.Chat.Id);
            return;
        }

        _logger.LogInformation("Processing message from chat {ChatId}: {Message}", message.Chat.Id, message.Text);

        // Эхо-логика: отправляем то же сообщение обратно
        await SendMessageAsync(message.Chat.Id, message.Text);
    }
}
