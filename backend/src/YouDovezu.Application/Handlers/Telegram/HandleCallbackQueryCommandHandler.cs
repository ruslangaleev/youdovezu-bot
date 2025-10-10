using MediatR;
using YouDovezu.Application.Commands.Telegram;
using YouDovezu.Application.Common.Interfaces;

namespace YouDovezu.Application.Handlers.Telegram;

/// <summary>
/// Обработчик callback запросов от inline клавиатур
/// </summary>
/// <remarks>
/// Маршрутизирует различные типы callback запросов к соответствующим обработчикам.
/// </remarks>
public class HandleCallbackQueryCommandHandler : IRequestHandler<HandleCallbackQueryCommand, bool>
{
    private readonly IMediator _mediator;
    private readonly ITelegramService _telegramService;
    private readonly IMessageTemplateService _messageTemplateService;

    /// <summary>
    /// Инициализирует новый экземпляр HandleCallbackQueryCommandHandler
    /// </summary>
    /// <param name="mediator">MediatR для отправки команд</param>
    /// <param name="telegramService">Сервис для работы с Telegram</param>
    /// <param name="messageTemplateService">Сервис шаблонов сообщений</param>
    public HandleCallbackQueryCommandHandler(
        IMediator mediator, 
        ITelegramService telegramService, 
        IMessageTemplateService messageTemplateService)
    {
        _mediator = mediator;
        _telegramService = telegramService;
        _messageTemplateService = messageTemplateService;
    }

    /// <summary>
    /// Обрабатывает callback query
    /// </summary>
    /// <param name="request">Запрос с данными callback</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат обработки</returns>
    public async Task<bool> Handle(HandleCallbackQueryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Отвечаем на callback query
            await _telegramService.AnswerCallbackQueryAsync(request.CallbackQueryId);

            // Маршрутизируем в зависимости от типа callback
            switch (request.Data)
            {
                case "privacy_agree":
                    return await HandlePrivacyAgreement(request, cancellationToken);
                
                case "privacy_disagree":
                    return await HandlePrivacyDecline(request);
                
                case "phone_confirm":
                    return await HandlePhoneConfirmation(request, cancellationToken);
                
                default:
                    return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Обрабатывает согласие с политикой конфиденциальности
    /// </summary>
    /// <param name="request">Исходный запрос</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат обработки</returns>
    private async Task<bool> HandlePrivacyAgreement(HandleCallbackQueryCommand request, CancellationToken cancellationToken)
    {
        var privacyCommand = new HandlePrivacyAgreementCommand
        {
            TelegramId = request.TelegramId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Username = request.Username,
            ChatId = request.ChatId
        };

        return await _mediator.Send(privacyCommand, cancellationToken);
    }

    /// <summary>
    /// Обрабатывает отказ от политики конфиденциальности
    /// </summary>
    /// <param name="request">Исходный запрос</param>
    /// <returns>Результат обработки</returns>
    private async Task<bool> HandlePrivacyDecline(HandleCallbackQueryCommand request)
    {
        var message = _messageTemplateService.GetPrivacyDeclineMessage();
        await _telegramService.SendMessageAsync(request.ChatId, message);
        return true;
    }

    /// <summary>
    /// Обрабатывает подтверждение номера телефона
    /// </summary>
    /// <param name="request">Исходный запрос</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат обработки</returns>
    private async Task<bool> HandlePhoneConfirmation(HandleCallbackQueryCommand request, CancellationToken cancellationToken)
    {
        // TODO: В реальном приложении здесь будет получение номера из Telegram initData
        var phoneNumber = "+1234567890"; // Заглушка для MVP

        var phoneCommand = new HandlePhoneConfirmationCommand
        {
            TelegramId = request.TelegramId,
            PhoneNumber = phoneNumber,
            ChatId = request.ChatId
        };

        return await _mediator.Send(phoneCommand, cancellationToken);
    }
}
