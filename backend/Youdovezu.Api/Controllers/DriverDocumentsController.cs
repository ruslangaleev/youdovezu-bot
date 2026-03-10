using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;
using System.IO;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;

namespace Youdovezu.Api.Controllers;

/// <summary>
/// Контроллер для работы с документами водителя
/// </summary>
[ApiController]
[Route("api/webapp/driver-documents")]
public class DriverDocumentsController : WebAppControllerBase
{
    private readonly IDriverDocumentsService _driverDocumentsService;
    private readonly ITelegramBotService _telegramBotService;
    private readonly IWebHostEnvironment _environment;

    public DriverDocumentsController(
        IUserService userService,
        IDriverDocumentsService driverDocumentsService,
        TelegramWebAppValidationService validationService,
        ITelegramBotService telegramBotService,
        ILogger<DriverDocumentsController> logger,
        IWebHostEnvironment environment)
        : base(userService, validationService, logger)
    {
        _driverDocumentsService = driverDocumentsService;
        _telegramBotService = telegramBotService;
        _environment = environment;
    }

    /// <summary>
    /// Получает статус проверки документов водителя
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Статус проверки документов</returns>
    [HttpPost("status")]
    public async Task<IActionResult> GetDriverDocumentsStatus([FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            var documents = await _driverDocumentsService.GetUserDocumentsAsync(user.Id);

            if (documents == null)
            {
                return Ok(new
                {
                    status = "not_submitted",
                    message = "Документы не отправлены"
                });
            }

            return Ok(new
            {
                status = documents.Status.ToString(),
                statusName = GetStatusName(documents.Status),
                submittedAt = documents.SubmittedAt,
                verifiedAt = documents.VerifiedAt,
                adminComment = documents.AdminComment
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting driver documents status");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Обрабатывает preflight OPTIONS запросы для CORS
    /// </summary>
    //[HttpOptions("upload")]
    //public IActionResult UploadDriverDocumentsOptions()
    //{
    //    Response.Headers.Append("Access-Control-Allow-Origin", "*");
    //    Response.Headers.Append("Access-Control-Allow-Methods", "POST, OPTIONS");
    //    Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
    //    Response.Headers.Append("Access-Control-Max-Age", "86400");
    //    return Ok();
    //}

    /// <summary>
    /// Загружает документы водителя
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="driverLicenseFront">Фото водительского удостоверения (лицевая сторона)</param>
    /// <param name="driverLicenseBack">Фото водительского удостоверения (обратная сторона)</param>
    /// <param name="vehicleRegistrationFront">Фото СТС (лицевая сторона)</param>
    /// <param name="vehicleRegistrationBack">Фото СТС (обратная сторона)</param>
    /// <param name="avatar">Аватарка пользователя</param>
    /// <returns>Результат загрузки</returns>
    [HttpPost("upload")]
    [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024, ValueLengthLimit = int.MaxValue)] // 50 MB
    [DisableRequestSizeLimit] // Отключаем ограничение размера для этого endpoint
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDriverDocuments(
        [FromQuery] string initData,
        IFormFile? driverLicenseFront,
        IFormFile? driverLicenseBack,
        IFormFile? vehicleRegistrationFront,
        IFormFile? vehicleRegistrationBack,
        IFormFile? avatar)
    {
        try
        {
            Logger.LogInformation("UploadDriverDocuments called. Content-Type: {ContentType}, ContentLength: {ContentLength}", 
                Request.ContentType, Request.ContentLength);
            
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            // Создаем базовую директорию uploads, если её нет
            var baseUploadsPath = Path.Combine(_environment.ContentRootPath, "uploads");
            if (!Directory.Exists(baseUploadsPath))
            {
                try
                {
                    Directory.CreateDirectory(baseUploadsPath);
                    Logger.LogInformation("Created base uploads directory at: {BaseUploadsPath}", baseUploadsPath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to create base uploads directory at: {BaseUploadsPath}", baseUploadsPath);
                    throw;
                }
            }

            // Создаем директорию для driver-documents, если её нет
            var driverDocumentsPath = Path.Combine(baseUploadsPath, "driver-documents");
            if (!Directory.Exists(driverDocumentsPath))
            {
                try
                {
                    Directory.CreateDirectory(driverDocumentsPath);
                    Logger.LogInformation("Created driver-documents directory at: {DriverDocumentsPath}", driverDocumentsPath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to create driver-documents directory at: {DriverDocumentsPath}", driverDocumentsPath);
                    throw;
                }
            }

            // Создаем директорию для конкретного пользователя
            var uploadsPath = Path.Combine(driverDocumentsPath, user.Id.ToString());
            if (!Directory.Exists(uploadsPath))
            {
                try
                {
                    Directory.CreateDirectory(uploadsPath);
                    Logger.LogInformation("Created uploads directory for user {UserId} at: {UploadsPath}", user.Id, uploadsPath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to create uploads directory for user {UserId} at: {UploadsPath}", user.Id, uploadsPath);
                    throw;
                }
            }
            else
            {
                Logger.LogInformation("Uploads directory for user {UserId} exists at: {UploadsPath}", user.Id, uploadsPath);
            }

            string? driverLicenseFrontPath = null;
            string? driverLicenseBackPath = null;
            string? vehicleRegistrationFrontPath = null;
            string? vehicleRegistrationBackPath = null;
            string? avatarPath = null;

            // Сохраняем файлы
            if (driverLicenseFront != null && driverLicenseFront.Length > 0)
            {
                var fileName = $"driver_license_front_{DateTime.UtcNow.Ticks}{Path.GetExtension(driverLicenseFront.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await driverLicenseFront.CopyToAsync(stream);
                }
                driverLicenseFrontPath = Path.Combine("uploads", "driver-documents", user.Id.ToString(), fileName);
                Logger.LogInformation("Saved driver license front image: {FilePath} (URL path: {UrlPath})", filePath, driverLicenseFrontPath);
                
                // Проверяем, что файл действительно существует
                if (System.IO.File.Exists(filePath))
                {
                    var fileInfo = new System.IO.FileInfo(filePath);
                    Logger.LogInformation("File exists: {FilePath}, Size: {Size} bytes", filePath, fileInfo.Length);
                }
                else
                {
                    Logger.LogError("File was not saved correctly: {FilePath}", filePath);
                }
            }

            if (driverLicenseBack != null && driverLicenseBack.Length > 0)
            {
                var fileName = $"driver_license_back_{DateTime.UtcNow.Ticks}{Path.GetExtension(driverLicenseBack.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await driverLicenseBack.CopyToAsync(stream);
                }
                driverLicenseBackPath = Path.Combine("uploads", "driver-documents", user.Id.ToString(), fileName);
            }

            if (vehicleRegistrationFront != null && vehicleRegistrationFront.Length > 0)
            {
                var fileName = $"vehicle_registration_front_{DateTime.UtcNow.Ticks}{Path.GetExtension(vehicleRegistrationFront.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vehicleRegistrationFront.CopyToAsync(stream);
                }
                vehicleRegistrationFrontPath = Path.Combine("uploads", "driver-documents", user.Id.ToString(), fileName);
            }

            if (vehicleRegistrationBack != null && vehicleRegistrationBack.Length > 0)
            {
                var fileName = $"vehicle_registration_back_{DateTime.UtcNow.Ticks}{Path.GetExtension(vehicleRegistrationBack.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vehicleRegistrationBack.CopyToAsync(stream);
                }
                vehicleRegistrationBackPath = Path.Combine("uploads", "driver-documents", user.Id.ToString(), fileName);
            }

            if (avatar != null && avatar.Length > 0)
            {
                var fileName = $"avatar_{DateTime.UtcNow.Ticks}{Path.GetExtension(avatar.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }
                avatarPath = Path.Combine("uploads", "driver-documents", user.Id.ToString(), fileName);
            }

            // Сохраняем информацию о документах в базу данных
            var documents = await _driverDocumentsService.SubmitDocumentsAsync(
                user.Id,
                driverLicenseFrontPath,
                driverLicenseBackPath,
                vehicleRegistrationFrontPath,
                vehicleRegistrationBackPath,
                avatarPath);

            Logger.LogInformation("Driver documents uploaded for user {TelegramId}", user.TelegramId);

            // Отправляем уведомление всем администраторам
            try
            {
                var admins = await UserService.GetAllAdminsAsync();
                var userDisplayName = user.GetDisplayName();
                var message = $"📋 Новые документы на проверке\n\n" +
                             $"Пользователь: {userDisplayName}\n" +
                             $"Дата отправки: {documents.SubmittedAt:dd.MM.yyyy HH:mm}\n\n" +
                             $"Откройте веб-приложение для проверки документов.";

                foreach (var admin in admins)
                {
                    try
                    {
                        await _telegramBotService.SendMessageAsync(admin.TelegramId, message);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Failed to send notification to admin {AdminId}", admin.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending notifications to admins");
                // Не прерываем выполнение, если уведомление не отправилось
            }

            return Ok(new
            {
                id = documents.Id,
                status = documents.Status.ToString(),
                statusName = GetStatusName(documents.Status),
                message = "Документы успешно отправлены на проверку",
                submittedAt = documents.SubmittedAt
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading driver documents");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    private string GetStatusName(Domain.Entities.DocumentVerificationStatus status)
    {
        return status switch
        {
            Domain.Entities.DocumentVerificationStatus.Pending => "Ожидает проверки",
            Domain.Entities.DocumentVerificationStatus.UnderReview => "На проверке",
            Domain.Entities.DocumentVerificationStatus.Approved => "Одобрено",
            Domain.Entities.DocumentVerificationStatus.Rejected => "Отклонено",
            _ => "Неизвестно"
        };
    }
}

