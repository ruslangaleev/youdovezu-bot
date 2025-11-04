namespace Youdovezu.Domain.Entities;

/// <summary>
/// Документы водителя для проверки
/// </summary>
public class DriverDocuments
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// ID пользователя (владельца документов)
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Навигационное свойство - пользователь
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Путь к фото водительского удостоверения (лицевая сторона)
    /// </summary>
    public string? DriverLicenseFrontPath { get; set; }

    /// <summary>
    /// Путь к фото водительского удостоверения (обратная сторона)
    /// </summary>
    public string? DriverLicenseBackPath { get; set; }

    /// <summary>
    /// Путь к фото СТС (лицевая сторона)
    /// </summary>
    public string? VehicleRegistrationFrontPath { get; set; }

    /// <summary>
    /// Путь к фото СТС (обратная сторона)
    /// </summary>
    public string? VehicleRegistrationBackPath { get; set; }

    /// <summary>
    /// Путь к аватарке пользователя
    /// </summary>
    public string? AvatarPath { get; set; }

    /// <summary>
    /// Статус проверки документов
    /// </summary>
    public DocumentVerificationStatus Status { get; set; } = DocumentVerificationStatus.Pending;

    /// <summary>
    /// Комментарий администратора (при отклонении)
    /// </summary>
    public string? AdminComment { get; set; }

    /// <summary>
    /// Дата отправки документов на проверку
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата проверки документов администратором
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

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
/// Статусы проверки документов
/// </summary>
public enum DocumentVerificationStatus
{
    /// <summary>
    /// Ожидает проверки
    /// </summary>
    Pending = 0,

    /// <summary>
    /// На проверке
    /// </summary>
    UnderReview = 1,

    /// <summary>
    /// Одобрено
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Отклонено
    /// </summary>
    Rejected = 3
}

