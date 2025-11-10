namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Модель запроса для создания поездки
/// </summary>
public class CreateTripRequest
{
    /// <summary>
    /// Адрес отправления (название улицы)
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Населенный пункт отправления
    /// </summary>
    public string FromSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта пункта отправления
    /// </summary>
    public double? FromLatitude { get; set; }

    /// <summary>
    /// Долгота пункта отправления
    /// </summary>
    public double? FromLongitude { get; set; }

    /// <summary>
    /// Адрес назначения (название улицы)
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Населенный пункт назначения
    /// </summary>
    public string ToSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта пункта назначения
    /// </summary>
    public double? ToLatitude { get; set; }

    /// <summary>
    /// Долгота пункта назначения
    /// </summary>
    public double? ToLongitude { get; set; }

    /// <summary>
    /// Комментарий к поездке (дополнительная информация)
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Модель запроса для обновления поездки
/// </summary>
public class UpdateTripRequest
{
    /// <summary>
    /// Адрес отправления (название улицы)
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Населенный пункт отправления
    /// </summary>
    public string FromSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта пункта отправления
    /// </summary>
    public double? FromLatitude { get; set; }

    /// <summary>
    /// Долгота пункта отправления
    /// </summary>
    public double? FromLongitude { get; set; }

    /// <summary>
    /// Адрес назначения (название улицы)
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Населенный пункт назначения
    /// </summary>
    public string ToSettlement { get; set; } = string.Empty;

    /// <summary>
    /// Широта пункта назначения
    /// </summary>
    public double? ToLatitude { get; set; }

    /// <summary>
    /// Долгота пункта назначения
    /// </summary>
    public double? ToLongitude { get; set; }

    /// <summary>
    /// Комментарий к поездке (дополнительная информация)
    /// </summary>
    public string? Comment { get; set; }
}


