namespace Youdovezu.Domain.Entities;

/// <summary>
/// Сущность населенного пункта
/// </summary>
public class Settlement
{
    /// <summary>
    /// Уникальный идентификатор населенного пункта
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Название населенного пункта
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Тип населенного пункта
    /// </summary>
    public SettlementType Type { get; set; }

    /// <summary>
    /// Код населенного пункта (опционально, для интеграции с внешними системами)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Флаг активности населенного пункта
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Типы населенных пунктов
/// </summary>
public enum SettlementType
{
    /// <summary>
    /// Город
    /// </summary>
    City = 0,

    /// <summary>
    /// Село
    /// </summary>
    Village = 1,

    /// <summary>
    /// Поселок городского типа
    /// </summary>
    Town = 2,

    /// <summary>
    /// Деревня
    /// </summary>
    RuralSettlement = 3,

    /// <summary>
    /// Поселок
    /// </summary>
    UrbanSettlement = 4
}


