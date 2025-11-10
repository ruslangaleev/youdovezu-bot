using Youdovezu.Domain.Entities;

namespace Youdovezu.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с документами водителя
/// </summary>
public interface IDriverDocumentsService
{
    /// <summary>
    /// Получить документы пользователя
    /// </summary>
    Task<DriverDocuments?> GetUserDocumentsAsync(long userId);

    /// <summary>
    /// Загрузить документы водителя
    /// </summary>
    Task<DriverDocuments> SubmitDocumentsAsync(
        long userId,
        string? driverLicenseFrontPath,
        string? driverLicenseBackPath,
        string? vehicleRegistrationFrontPath,
        string? vehicleRegistrationBackPath,
        string? avatarPath);

    /// <summary>
    /// Получить статус проверки документов пользователя
    /// </summary>
    Task<DocumentVerificationStatus?> GetVerificationStatusAsync(long userId);

    /// <summary>
    /// Получить документ по ID
    /// </summary>
    Task<DriverDocuments?> GetDocumentByIdAsync(long documentsId);

    /// <summary>
    /// Получить все документы на проверке (для модерации)
    /// </summary>
    Task<List<DriverDocuments>> GetPendingDocumentsAsync();

    /// <summary>
    /// Одобрить документы (для модерации)
    /// </summary>
    Task<DriverDocuments> ApproveDocumentsAsync(
        long documentsId,
        long moderatorId,
        string driverLastName,
        string driverFirstName,
        string driverMiddleName,
        string vehicleBrand,
        string vehicleModel,
        string vehicleColor,
        string vehicleLicensePlate);

    /// <summary>
    /// Отклонить документы (для модерации)
    /// </summary>
    Task<DriverDocuments> RejectDocumentsAsync(
        long documentsId,
        long moderatorId,
        string adminComment);
}

