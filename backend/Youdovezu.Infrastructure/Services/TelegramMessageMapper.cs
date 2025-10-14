using Telegram.Bot.Types;
using Youdovezu.Domain.Models;

namespace Youdovezu.Infrastructure.Services;

public static class TelegramMessageMapper
{
    public static TelegramMessage ToDomainModel(Message telegramMessage)
    {
        return new TelegramMessage
        {
            ChatId = telegramMessage.Chat.Id,
            Text = telegramMessage.Text,
            MessageId = telegramMessage.MessageId,
            Date = telegramMessage.Date,
            UserId = telegramMessage.From?.Id ?? 0,
            Username = telegramMessage.From?.Username,
            FirstName = telegramMessage.From?.FirstName,
            LastName = telegramMessage.From?.LastName
        };
    }
}
