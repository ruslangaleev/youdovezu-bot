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

    public async Task<DriverDocuments?> GetDocumentByIdAsync(long documentsId)
    {
        return await _documentsRepository.GetByIdAsync(documentsId);
    }

    public async Task<List<DriverDocuments>> GetPendingDocumentsAsync()
    {
        return await _documentsRepository.GetPendingDocumentsAsync();
    }

    public async Task<DriverDocuments> ApproveDocumentsAsync(
        long documentsId,
        long moderatorId,
        string driverLastName,
        string driverFirstName,
        string driverMiddleName,
        string vehicleBrand,
        string vehicleModel,
        string vehicleColor,
        string vehicleLicensePlate)
    {
        var documents = await _documentsRepository.GetByIdAsync(documentsId);
        if (documents == null)
        {
            throw new ArgumentException("Документы не найдены", nameof(documentsId));
        }

        // Проверяем, что документы находятся на проверке
        if (documents.Status != DocumentVerificationStatus.Pending && 
            documents.Status != DocumentVerificationStatus.UnderReview)
        {
            throw new InvalidOperationException("Документы уже были обработаны");
        }

        // Заполняем информацию о водителе и автомобиле
        documents.DriverLastName = driverLastName;
        documents.DriverFirstName = driverFirstName;
        documents.DriverMiddleName = driverMiddleName;
        documents.VehicleBrand = vehicleBrand;
        documents.VehicleModel = vehicleModel;
        documents.VehicleColor = vehicleColor;
        documents.VehicleLicensePlate = vehicleLicensePlate;

        // Устанавливаем статус и информацию о модерации
        documents.Status = DocumentVerificationStatus.Approved;
        documents.ModeratedByUserId = moderatorId;
        documents.VerifiedAt = DateTime.UtcNow;
        documents.AdminComment = null; // Очищаем комментарий при одобрении

        // Включаем возможность быть водителем для пользователя
        var user = await _userRepository.GetByIdAsync(documents.UserId);
        if (user != null)
        {
            user.CanBeDriver = true;
            if (!user.TrialStartDate.HasValue)
            {
                user.TrialStartDate = DateTime.UtcNow;
                user.TrialEndDate = DateTime.UtcNow.AddDays(14);
            }
            await _userRepository.UpdateAsync(user);
        }

        return await _documentsRepository.UpdateAsync(documents);
    }

    public async Task<DriverDocuments> RejectDocumentsAsync(
        long documentsId,
        long moderatorId,
        string adminComment)
    {
        var documents = await _documentsRepository.GetByIdAsync(documentsId);
        if (documents == null)
        {
            throw new ArgumentException("Документы не найдены", nameof(documentsId));
        }

        // Проверяем, что документы находятся на проверке
        if (documents.Status != DocumentVerificationStatus.Pending && 
            documents.Status != DocumentVerificationStatus.UnderReview)
        {
            throw new InvalidOperationException("Документы уже были обработаны");
        }

        // Устанавливаем статус и комментарий
        documents.Status = DocumentVerificationStatus.Rejected;
        documents.ModeratedByUserId = moderatorId;
        documents.VerifiedAt = DateTime.UtcNow;
        documents.AdminComment = adminComment;

        return await _documentsRepository.UpdateAsync(documents);
    }
}

