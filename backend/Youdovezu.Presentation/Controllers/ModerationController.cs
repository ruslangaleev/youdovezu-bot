using Microsoft.AspNetCore.Mvc;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Контроллер для модерации документов (только для администраторов)
/// </summary>
[ApiController]
[Route("api/webapp/moderation")]
public class ModerationController : WebAppControllerBase
{
    private readonly IDriverDocumentsService _driverDocumentsService;
    private readonly ITelegramBotService _telegramBotService;

    public ModerationController(
        IUserService userService,
        IDriverDocumentsService driverDocumentsService,
        TelegramWebAppValidationService validationService,
        ITelegramBotService telegramBotService,
        ILogger<ModerationController> logger)
        : base(userService, validationService, logger)
    {
        _driverDocumentsService = driverDocumentsService;
        _telegramBotService = telegramBotService;
    }

    /// <summary>
    /// Проверяет, является ли пользователь администратором
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Информация о правах пользователя</returns>
    [HttpPost("check-admin")]
    public async Task<IActionResult> CheckAdminStatus([FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Ok(new { isAdmin = false });
            }

            return Ok(new { isAdmin = user.IsAdmin() });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking admin status");
            return Ok(new { isAdmin = false });
        }
    }

    /// <summary>
    /// Получает список всех документов на проверке (для модерации)
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <returns>Список документов на проверке</returns>
    [HttpPost("documents-list")]
    public async Task<IActionResult> GetDocumentsForModeration([FromQuery] string initData)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            // Проверяем, является ли пользователь администратором
            if (!IsAdmin(user))
            {
                return StatusCode(403, new { error = "Доступ запрещен. Требуются права администратора" });
            }

            // Получаем список документов на проверке
            var documents = await _driverDocumentsService.GetPendingDocumentsAsync();

            var result = documents.Select(d => new
            {
                id = d.Id,
                userId = d.UserId,
                userName = d.User?.GetDisplayName() ?? "Неизвестный пользователь",
                status = d.Status.ToString(),
                statusName = GetStatusName(d.Status),
                submittedAt = d.SubmittedAt,
                createdAt = d.CreatedAt
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting documents for moderation");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получает детальную информацию о документе для модерации
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="documentsId">ID документа</param>
    /// <returns>Детальная информация о документе</returns>
    [HttpPost("document-details")]
    public async Task<IActionResult> GetDocumentDetails([FromQuery] string initData, [FromQuery] long documentsId)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            // Проверяем, является ли пользователь администратором
            if (!IsAdmin(user))
            {
                return StatusCode(403, new { error = "Доступ запрещен. Требуются права администратора" });
            }

            // Получаем документ
            var documents = await _driverDocumentsService.GetDocumentByIdAsync(documentsId);
            if (documents == null)
            {
                return NotFound(new { error = "Документы не найдены" });
            }

            // Формируем URL для доступа к изображениям
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                id = documents.Id,
                userId = documents.UserId,
                userName = documents.User?.GetDisplayName() ?? "Неизвестный пользователь",
                status = documents.Status.ToString(),
                statusName = GetStatusName(documents.Status),
                submittedAt = documents.SubmittedAt,
                verifiedAt = documents.VerifiedAt,
                adminComment = documents.AdminComment,
                // Информация о водителе и автомобиле (может быть заполнена при модерации)
                driverLastName = documents.DriverLastName,
                driverFirstName = documents.DriverFirstName,
                driverMiddleName = documents.DriverMiddleName,
                vehicleBrand = documents.VehicleBrand,
                vehicleModel = documents.VehicleModel,
                vehicleColor = documents.VehicleColor,
                vehicleLicensePlate = documents.VehicleLicensePlate,
                // Пути к изображениям
                driverLicenseFrontUrl = documents.DriverLicenseFrontPath != null
                    ? $"{baseUrl}/{documents.DriverLicenseFrontPath}"
                    : null,
                driverLicenseBackUrl = documents.DriverLicenseBackPath != null
                    ? $"{baseUrl}/{documents.DriverLicenseBackPath}"
                    : null,
                vehicleRegistrationFrontUrl = documents.VehicleRegistrationFrontPath != null
                    ? $"{baseUrl}/{documents.VehicleRegistrationFrontPath}"
                    : null,
                vehicleRegistrationBackUrl = documents.VehicleRegistrationBackPath != null
                    ? $"{baseUrl}/{documents.VehicleRegistrationBackPath}"
                    : null,
                avatarUrl = documents.AvatarPath != null
                    ? $"{baseUrl}/{documents.AvatarPath}"
                    : null
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting document details");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Одобряет документы водителя
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="request">Данные для одобрения</param>
    /// <returns>Результат одобрения</returns>
    [HttpPost("approve")]
    public async Task<IActionResult> ApproveDocuments([FromQuery] string initData, [FromBody] ApproveDocumentsRequest request)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            // Проверяем, является ли пользователь администратором
            if (!IsAdmin(user))
            {
                return StatusCode(403, new { error = "Доступ запрещен. Требуются права администратора" });
            }

            // Валидация полей
            if (string.IsNullOrWhiteSpace(request.DriverLastName) ||
                string.IsNullOrWhiteSpace(request.DriverFirstName) ||
                string.IsNullOrWhiteSpace(request.DriverMiddleName) ||
                string.IsNullOrWhiteSpace(request.VehicleBrand) ||
                string.IsNullOrWhiteSpace(request.VehicleModel) ||
                string.IsNullOrWhiteSpace(request.VehicleColor) ||
                string.IsNullOrWhiteSpace(request.VehicleLicensePlate))
            {
                return BadRequest(new { error = "Все поля должны быть заполнены" });
            }

            // Одобряем документы
            var documents = await _driverDocumentsService.ApproveDocumentsAsync(
                request.DocumentsId,
                user.Id,
                request.DriverLastName,
                request.DriverFirstName,
                request.DriverMiddleName,
                request.VehicleBrand,
                request.VehicleModel,
                request.VehicleColor,
                request.VehicleLicensePlate);

            Logger.LogInformation("Documents approved by admin {AdminId} for user {UserId}", user.Id, documents.UserId);

            // Отправляем уведомление пользователю об одобрении
            try
            {
                // Получаем документы с загруженным User для получения TelegramId
                var documentsWithUser = await _driverDocumentsService.GetDocumentByIdAsync(request.DocumentsId);
                if (documentsWithUser?.User != null)
                {
                    var message = "✅ Ваши документы одобрены!\n\n" +
                                 "Теперь вы можете просматривать запросы других попутчиков и предлагать свои услуги.\n\n" +
                                 "Откройте веб-приложение для начала работы.";
                    await _telegramBotService.SendMessageAsync(documentsWithUser.User.TelegramId, message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending approval notification to user {UserId}", documents.UserId);
                // Не прерываем выполнение, если уведомление не отправилось
            }

            return Ok(new
            {
                id = documents.Id,
                status = documents.Status.ToString(),
                statusName = GetStatusName(documents.Status),
                message = "Документы успешно одобрены",
                verifiedAt = documents.VerifiedAt
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error approving documents");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Отклоняет документы водителя
    /// </summary>
    /// <param name="initData">Telegram WebApp initData</param>
    /// <param name="request">Данные для отклонения</param>
    /// <returns>Результат отклонения</returns>
    [HttpPost("reject")]
    public async Task<IActionResult> RejectDocuments([FromQuery] string initData, [FromBody] RejectDocumentsRequest request)
    {
        try
        {
            var user = await ValidateAndGetUserAsync(initData);
            if (user == null)
            {
                return Unauthorized(new { error = "Неверные данные авторизации или пользователь не найден" });
            }

            // Проверяем, является ли пользователь администратором
            if (!IsAdmin(user))
            {
                return StatusCode(403, new { error = "Доступ запрещен. Требуются права администратора" });
            }

            // Валидация комментария
            if (string.IsNullOrWhiteSpace(request.AdminComment))
            {
                return BadRequest(new { error = "Комментарий администратора обязателен при отклонении" });
            }

            // Отклоняем документы
            var documents = await _driverDocumentsService.RejectDocumentsAsync(
                request.DocumentsId,
                user.Id,
                request.AdminComment);

            Logger.LogInformation("Documents rejected by admin {AdminId} for user {UserId}", user.Id, documents.UserId);

            // Отправляем уведомление пользователю об отклонении
            try
            {
                // Получаем документы с загруженным User для получения TelegramId
                var documentsWithUser = await _driverDocumentsService.GetDocumentByIdAsync(request.DocumentsId);
                if (documentsWithUser?.User != null)
                {
                    var message = "❌ Ваши документы отклонены\n\n" +
                                 $"Причина: {request.AdminComment}\n\n" +
                                 "Вы можете загрузить документы заново, исправив указанные недочеты.";
                    await _telegramBotService.SendMessageAsync(documentsWithUser.User.TelegramId, message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending rejection notification to user {UserId}", documents.UserId);
                // Не прерываем выполнение, если уведомление не отправилось
            }

            return Ok(new
            {
                id = documents.Id,
                status = documents.Status.ToString(),
                statusName = GetStatusName(documents.Status),
                message = "Документы отклонены",
                verifiedAt = documents.VerifiedAt,
                adminComment = documents.AdminComment
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error rejecting documents");
            return StatusCode(500, new { error = ex.Message });
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


