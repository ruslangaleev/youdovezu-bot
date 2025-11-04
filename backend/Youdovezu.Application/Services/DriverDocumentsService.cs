using Youdovezu.Application.Interfaces;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;

namespace Youdovezu.Application.Services;

/// <summary>
/// Сервис для работы с документами водителя
/// </summary>
public class DriverDocumentsService : IDriverDocumentsService
{
    private readonly IDriverDocumentsRepository _documentsRepository;
    private readonly IUserRepository _userRepository;

    public DriverDocumentsService(IDriverDocumentsRepository documentsRepository, IUserRepository userRepository)
    {
        _documentsRepository = documentsRepository;
        _userRepository = userRepository;
    }

    public async Task<DriverDocuments?> GetUserDocumentsAsync(long userId)
    {
        return await _documentsRepository.GetByUserIdAsync(userId);
    }

    public async Task<DriverDocuments> SubmitDocumentsAsync(
        long userId,
        string? driverLicenseFrontPath,
        string? driverLicenseBackPath,
        string? vehicleRegistrationFrontPath,
        string? vehicleRegistrationBackPath,
        string? avatarPath)
    {
        // Проверяем существование пользователя
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("Пользователь не найден", nameof(userId));
        }

        // Проверяем, есть ли уже документы у пользователя
        var existingDocuments = await _documentsRepository.GetByUserIdAsync(userId);
        
        if (existingDocuments != null)
        {
            // Обновляем существующие документы
            existingDocuments.DriverLicenseFrontPath = driverLicenseFrontPath;
            existingDocuments.DriverLicenseBackPath = driverLicenseBackPath;
            existingDocuments.VehicleRegistrationFrontPath = vehicleRegistrationFrontPath;
            existingDocuments.VehicleRegistrationBackPath = vehicleRegistrationBackPath;
            existingDocuments.AvatarPath = avatarPath;
            existingDocuments.Status = DocumentVerificationStatus.UnderReview;
            existingDocuments.SubmittedAt = DateTime.UtcNow;
            existingDocuments.AdminComment = null;
            existingDocuments.VerifiedAt = null;

            return await _documentsRepository.UpdateAsync(existingDocuments);
        }
        else
        {
            // Создаем новые документы
            var documents = new DriverDocuments
            {
                UserId = userId,
                DriverLicenseFrontPath = driverLicenseFrontPath,
                DriverLicenseBackPath = driverLicenseBackPath,
                VehicleRegistrationFrontPath = vehicleRegistrationFrontPath,
                VehicleRegistrationBackPath = vehicleRegistrationBackPath,
                AvatarPath = avatarPath,
                Status = DocumentVerificationStatus.UnderReview,
                SubmittedAt = DateTime.UtcNow
            };

            return await _documentsRepository.CreateAsync(documents);
        }
    }

    public async Task<DocumentVerificationStatus?> GetVerificationStatusAsync(long userId)
    {
        var documents = await _documentsRepository.GetByUserIdAsync(userId);
        return documents?.Status;
    }
}

