namespace Youdovezu.Domain.Entities;

/// <summary>
/// Сущность поездки (trip request)
/// </summary>
public class Trip
{
    /// <summary>
    /// Уникальный идентификатор поездки
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// ID пользователя (создателя поездки)
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Навигационное свойство - пользователь
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Адрес отправления
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Название населенного пункта отправления
    /// </summary>
    public string FromSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта отправления
    /// </summary>
    public double? FromLatitude { get; set; }

    /// <summary>
    /// Долгота отправления
    /// </summary>
    public double? FromLongitude { get; set; }

    /// <summary>
    /// Адрес назначения
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Название населенного пункта назначения
    /// </summary>
    public string ToSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта назначения
    /// </summary>
    public double? ToLatitude { get; set; }

    /// <summary>
    /// Долгота назначения
    /// </summary>
    public double? ToLongitude { get; set; }

    /// <summary>
    /// Комментарий к поездке
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Статус поездки
    /// </summary>
    public TripStatus Status { get; set; } = TripStatus.Active;

    /// <summary>
    /// Дата создания поездки
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Статусы поездки
/// </summary>
public enum TripStatus
{
    /// <summary>
    /// Активная поездка (доступна для поиска)
    /// </summary>
    Active = 0,

    /// <summary>
    /// Поездка закрыта (найдены попутчики или отменена)
    /// </summary>
    Closed = 1,

    /// <summary>
    /// Поездка удалена
    /// </summary>
    Deleted = 2
}

