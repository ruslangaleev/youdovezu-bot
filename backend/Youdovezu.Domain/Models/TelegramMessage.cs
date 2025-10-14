namespace Youdovezu.Domain.Models;

public class TelegramMessage
{
    public long ChatId { get; set; }
    public string? Text { get; set; }
    public long MessageId { get; set; }
    public DateTime Date { get; set; }
    public long UserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
