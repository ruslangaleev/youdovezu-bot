namespace YouDovezu.Domain.Entities;

/// <summary>
/// Информация о водителе и его транспортном средстве
/// </summary>
/// <remarks>
/// Содержит данные, необходимые для верификации водителя администратором:
/// документы, информация об автомобиле, статус проверки.
/// </remarks>
public class DriverInfo : BaseEntity
{
    /// <summary>
    /// Идентификатор пользователя-водителя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь-водитель
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Номер водительского удостоверения
    /// </summary>
    public string LicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// URL фотографии водительского удостоверения
    /// </summary>
    /// <remarks>
    /// Загружается пользователем при регистрации как водитель
    /// </remarks>
    public string LicensePhotoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Марка автомобиля
    /// </summary>
    public string CarMake { get; set; } = string.Empty;

    /// <summary>
    /// Модель автомобиля
    /// </summary>
    public string CarModel { get; set; } = string.Empty;

    /// <summary>
    /// Цвет автомобиля
    /// </summary>
    public string CarColor { get; set; } = string.Empty;

    /// <summary>
    /// Государственный номер автомобиля
    /// </summary>
    public string CarNumber { get; set; } = string.Empty;

    /// <summary>
    /// URL фотографии СТС (свидетельства о регистрации ТС)
    /// </summary>
    /// <remarks>
    /// Загружается пользователем при регистрации как водитель
    /// </remarks>
    public string StsPhotoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Статус верификации водителя
    /// </summary>
    /// <remarks>
    /// Устанавливается администратором после проверки документов
    /// </remarks>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Дата и время верификации
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Идентификатор администратора, который провел верификацию
    /// </summary>
    public Guid? VerifiedByAdminId { get; set; }

    /// <summary>
    /// Администратор, который провел верификацию
    /// </summary>
    public User? VerifiedByAdmin { get; set; }
}
