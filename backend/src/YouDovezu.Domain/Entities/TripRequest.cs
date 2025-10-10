namespace YouDovezu.Domain.Entities;

/// <summary>
/// Запрос на поездку от клиента
/// </summary>
/// <remarks>
/// Представляет запрос клиента на поиск водителя для поездки.
/// Содержит информацию о маршруте, количестве пассажиров и предпочтениях.
/// </remarks>
public class TripRequest : BaseEntity
{
    /// <summary>
    /// Идентификатор пользователя, создавшего запрос
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь, создавший запрос
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Адрес отправления
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Адрес назначения
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Широта точки отправления
    /// </summary>
    public double FromLatitude { get; set; }

    /// <summary>
    /// Долгота точки отправления
    /// </summary>
    public double FromLongitude { get; set; }

    /// <summary>
    /// Широта точки назначения
    /// </summary>
    public double ToLatitude { get; set; }

    /// <summary>
    /// Долгота точки назначения
    /// </summary>
    public double ToLongitude { get; set; }

    /// <summary>
    /// Дополнительное описание поездки
    /// </summary>
    /// <remarks>
    /// Может содержать особые требования или пожелания
    /// </remarks>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Количество пассажиров
    /// </summary>
    public int PassengerCount { get; set; } = 1;

    /// <summary>
    /// Текущий статус запроса
    /// </summary>
    public TripRequestStatus Status { get; set; } = TripRequestStatus.Active;

    /// <summary>
    /// Предпочтительная дата и время поездки
    /// </summary>
    /// <remarks>
    /// Может быть null, если время не критично
    /// </remarks>
    public DateTime? PreferredDateTime { get; set; }

    /// <summary>
    /// Идентификатор принятого предложения
    /// </summary>
    /// <remarks>
    /// Заполняется, когда клиент выбирает предложение водителя
    /// </remarks>
    public Guid? AcceptedOfferId { get; set; }
}

/// <summary>
/// Статусы запроса на поездку
/// </summary>
public enum TripRequestStatus
{
    /// <summary>
    /// Активный запрос, ищет водителей
    /// </summary>
    Active = 0,

    /// <summary>
    /// Запрос принят, водитель найден
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Поездка завершена
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Запрос отменен
    /// </summary>
    Cancelled = 3
}
