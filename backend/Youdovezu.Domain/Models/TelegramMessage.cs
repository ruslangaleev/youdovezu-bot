namespace Youdovezu.Domain.Models;

/// <summary>
/// Доменная модель сообщения от Telegram
/// </summary>
public class TelegramMessage
{
    /// <summary>
    /// ID чата, из которого пришло сообщение
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// ID сообщения в Telegram
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// Дата и время отправки сообщения
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// ID пользователя, отправившего сообщение
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Имя пользователя (username) в Telegram
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string? LastName { get; set; }
}
