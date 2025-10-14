using Youdovezu.Domain.Models;

namespace Youdovezu.Application.Interfaces;

public interface ITelegramBotService
{
    Task SendMessageAsync(long chatId, string message);
    Task ProcessMessageAsync(TelegramMessage message);
}
