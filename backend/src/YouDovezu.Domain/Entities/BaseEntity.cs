namespace YouDovezu.Domain.Entities;

/// <summary>
/// Базовый класс для всех доменных сущностей
/// </summary>
/// <remarks>
/// Содержит общие свойства, которые должны быть у всех сущностей в системе:
/// - Уникальный идентификатор
/// - Временные метки создания и обновления
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>
    /// Уникальный идентификатор сущности
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Дата и время создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата и время последнего обновления записи
    /// </summary>
    /// <remarks>
    /// Может быть null, если запись никогда не обновлялась
    /// </remarks>
    public DateTime? UpdatedAt { get; set; }
}
