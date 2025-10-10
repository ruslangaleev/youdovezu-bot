namespace YouDovezu.Domain.Entities;

/// <summary>
/// Оценка пользователя после поездки
/// </summary>
/// <remarks>
/// Представляет оценку, которую один пользователь ставит другому после завершения поездки.
/// Может быть оценкой клиента водителю или наоборот.
/// </remarks>
public class Rating : BaseEntity
{
    /// <summary>
    /// Идентификатор поездки, за которую ставится оценка
    /// </summary>
    public Guid TripRequestId { get; set; }

    /// <summary>
    /// Поездка, за которую ставится оценка
    /// </summary>
    public TripRequest TripRequest { get; set; } = null!;

    /// <summary>
    /// Идентификатор пользователя, который ставит оценку
    /// </summary>
    public Guid FromUserId { get; set; }

    /// <summary>
    /// Пользователь, который ставит оценку
    /// </summary>
    public User FromUser { get; set; } = null!;

    /// <summary>
    /// Идентификатор пользователя, которому ставится оценка
    /// </summary>
    public Guid ToUserId { get; set; }

    /// <summary>
    /// Пользователь, которому ставится оценка
    /// </summary>
    public User ToUser { get; set; } = null!;

    /// <summary>
    /// Количество звезд (от 1 до 5)
    /// </summary>
    public int Stars { get; set; }

    /// <summary>
    /// Комментарий к оценке
    /// </summary>
    /// <remarks>
    /// Может быть null, если пользователь не оставил комментарий
    /// </remarks>
    public string? Comment { get; set; }

    /// <summary>
    /// Тип оценки
    /// </summary>
    public RatingType Type { get; set; }
}

/// <summary>
/// Типы оценок в системе
/// </summary>
public enum RatingType
{
    /// <summary>
    /// Оценка клиента водителю
    /// </summary>
    ClientToDriver = 0,

    /// <summary>
    /// Оценка водителя клиенту
    /// </summary>
    DriverToClient = 1
}
