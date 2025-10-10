namespace YouDovezu.Application.Common.Interfaces;

/// <summary>
/// Интерфейс для работы с Telegram Bot API
/// </summary>
/// <remarks>
/// Предоставляет методы для отправки сообщений и обработки callback запросов.
/// Реализация находится в Infrastructure слое, так как это внешний сервис.
/// </remarks>
public interface ITelegramService
{
    /// <summary>
    /// Отправляет текстовое сообщение в чат
    /// </summary>
    /// <param name="chatId">Идентификатор чата</param>
    /// <param name="text">Текст сообщения</param>
    /// <param name="replyMarkup">Клавиатура (опционально)</param>
    /// <returns>Задача отправки сообщения</returns>
    Task SendMessageAsync(long chatId, string text, object? replyMarkup = null);

    /// <summary>
    /// Отвечает на callback query
    /// </summary>
    /// <param name="callbackQueryId">Идентификатор callback query</param>
    /// <param name="text">Текст ответа (опционально)</param>
    /// <returns>Задача ответа на callback</returns>
    Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null);
}
