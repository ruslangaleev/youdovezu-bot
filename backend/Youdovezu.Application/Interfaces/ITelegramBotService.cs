using Youdovezu.Domain.Models;

namespace Youdovezu.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с Telegram Bot API
/// </summary>
public interface ITelegramBotService
{
    /// <summary>
    /// Отправляет текстовое сообщение в указанный чат
    /// </summary>
    /// <param name="chatId">ID чата для отправки сообщения</param>
    /// <param name="message">Текст сообщения</param>
    /// <returns>Task, представляющий асинхронную операцию</returns>
    Task SendMessageAsync(long chatId, string message);

    /// <summary>
    /// Обрабатывает входящее сообщение от пользователя
    /// </summary>
    /// <param name="message">Доменная модель сообщения</param>
    /// <returns>Task, представляющий асинхронную операцию</returns>
    Task ProcessMessageAsync(TelegramMessage message);
}
