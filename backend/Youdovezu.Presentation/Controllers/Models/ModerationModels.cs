namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Модель запроса для одобрения документов
/// </summary>
public class ApproveDocumentsRequest
{
    /// <summary>
    /// ID документа
    /// </summary>
    public long DocumentsId { get; set; }

    /// <summary>
    /// Фамилия водителя
    /// </summary>
    public string DriverLastName { get; set; } = string.Empty;

    /// <summary>
    /// Имя водителя
    /// </summary>
    public string DriverFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество водителя
    /// </summary>
    public string DriverMiddleName { get; set; } = string.Empty;

    /// <summary>
    /// Марка автомобиля
    /// </summary>
    public string VehicleBrand { get; set; } = string.Empty;

    /// <summary>
    /// Модель автомобиля
    /// </summary>
    public string VehicleModel { get; set; } = string.Empty;

    /// <summary>
    /// Цвет автомобиля
    /// </summary>
    public string VehicleColor { get; set; } = string.Empty;

    /// <summary>
    /// Государственный номер автомобиля
    /// </summary>
    public string VehicleLicensePlate { get; set; } = string.Empty;
}

/// <summary>
/// Модель запроса для отклонения документов
/// </summary>
public class RejectDocumentsRequest
{
    /// <summary>
    /// ID документа
    /// </summary>
    public long DocumentsId { get; set; }

    /// <summary>
    /// Комментарий администратора
    /// </summary>
    public string AdminComment { get; set; } = string.Empty;
}


