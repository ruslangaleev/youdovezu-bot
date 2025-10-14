using Telegram.Bot.Types;
using Youdovezu.Domain.Models;

namespace Youdovezu.Infrastructure.Services;

/// <summary>
/// Статический класс для преобразования объектов Telegram API в доменные модели
/// </summary>
public static class TelegramMessageMapper
{
    /// <summary>
    /// Преобразует объект Message из Telegram Bot API в доменную модель TelegramMessage
    /// </summary>
    /// <param name="telegramMessage">Сообщение из Telegram Bot API</param>
    /// <returns>Доменная модель сообщения</returns>
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
