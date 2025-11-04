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
}

