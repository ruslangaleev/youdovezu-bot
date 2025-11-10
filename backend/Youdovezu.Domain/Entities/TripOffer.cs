namespace Youdovezu.Domain.Entities;

/// <summary>
/// Сущность предложения от Offerer'а на объявление Requester'а
/// </summary>
public class TripOffer
{
    /// <summary>
    /// Уникальный идентификатор предложения
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// ID объявления (поездки)
    /// </summary>
    public long TripId { get; set; }

    /// <summary>
    /// Навигационное свойство - поездка
    /// </summary>
    public Trip? Trip { get; set; }

    /// <summary>
    /// ID Offerer'а (пользователя, который делает предложение)
    /// </summary>
    public long OffererId { get; set; }

    /// <summary>
    /// Навигационное свойство - Offerer
    /// </summary>
    public User? Offerer { get; set; }

    /// <summary>
    /// Предложенная цена за поездку
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Комментарий к предложению
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Статус предложения
    /// </summary>
    public TripOfferStatus Status { get; set; } = TripOfferStatus.Pending;

    /// <summary>
    /// Дата создания предложения
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Статусы предложения
/// </summary>
public enum TripOfferStatus
{
    /// <summary>
    /// Ожидает ответа от Requester'а
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Предложение принято Requester'ом
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Предложение отклонено Requester'ом
    /// </summary>
    Rejected = 2
}


