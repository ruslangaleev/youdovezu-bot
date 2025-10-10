namespace YouDovezu.Domain.Entities;

/// <summary>
/// Предложение водителя на запрос о поездке
/// </summary>
/// <remarks>
/// Представляет предложение водителя выполнить поездку по запросу клиента.
/// Содержит цену, сообщение и статус обработки предложения.
/// </remarks>
public class TripOffer : BaseEntity
{
    /// <summary>
    /// Идентификатор запроса на поездку
    /// </summary>
    public Guid TripRequestId { get; set; }

    /// <summary>
    /// Запрос на поездку, к которому относится предложение
    /// </summary>
    public TripRequest TripRequest { get; set; } = null!;

    /// <summary>
    /// Идентификатор водителя
    /// </summary>
    public Guid DriverId { get; set; }

    /// <summary>
    /// Водитель, сделавший предложение
    /// </summary>
    public User Driver { get; set; } = null!;

    /// <summary>
    /// Предлагаемая цена за поездку
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Сообщение от водителя к клиенту
    /// </summary>
    /// <remarks>
    /// Может содержать дополнительную информацию о поездке
    /// </remarks>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Статус предложения
    /// </summary>
    public TripOfferStatus Status { get; set; } = TripOfferStatus.Pending;
}

/// <summary>
/// Статусы предложения водителя
/// </summary>
public enum TripOfferStatus
{
    /// <summary>
    /// Ожидает рассмотрения клиентом
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Принято клиентом
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Отклонено клиентом
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Отменено водителем
    /// </summary>
    Cancelled = 3
}
