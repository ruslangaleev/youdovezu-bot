using Telegram.Bot.Types;

namespace Youdovezu.Application.Interfaces;

public interface ITelegramBotService
{
    Task SendMessageAsync(long chatId, string message);
    Task ProcessMessageAsync(Message message);
}
