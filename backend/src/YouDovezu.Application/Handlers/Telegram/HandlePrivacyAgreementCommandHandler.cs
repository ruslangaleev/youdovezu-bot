using MediatR;
using YouDovezu.Application.Commands.Telegram;
using YouDovezu.Application.Commands.Users;
using YouDovezu.Application.Common.Interfaces;

namespace YouDovezu.Application.Handlers.Telegram;

/// <summary>
/// Обработчик согласия пользователя с политикой конфиденциальности
/// </summary>
/// <remarks>
/// Регистрирует пользователя в системе и отправляет сообщение с подтверждением телефона.
/// </remarks>
public class HandlePrivacyAgreementCommandHandler : IRequestHandler<HandlePrivacyAgreementCommand, bool>
{
    private readonly IMediator _mediator;
    private readonly ITelegramService _telegramService;
    private readonly IMessageTemplateService _messageTemplateService;

    /// <summary>
    /// Инициализирует новый экземпляр HandlePrivacyAgreementCommandHandler
    /// </summary>
    /// <param name="mediator">MediatR для отправки команд</param>
    /// <param name="telegramService">Сервис для работы с Telegram</param>
    /// <param name="messageTemplateService">Сервис шаблонов сообщений</param>
    public HandlePrivacyAgreementCommandHandler(
        IMediator mediator, 
        ITelegramService telegramService, 
        IMessageTemplateService messageTemplateService)
    {
        _mediator = mediator;
        _telegramService = telegramService;
        _messageTemplateService = messageTemplateService;
    }

    /// <summary>
    /// Обрабатывает согласие с политикой конфиденциальности
    /// </summary>
    /// <param name="request">Запрос с данными пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат обработки</returns>
    public async Task<bool> Handle(HandlePrivacyAgreementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Регистрируем пользователя в системе
            var registerCommand = new RegisterTelegramUserCommand
            {
                TelegramId = request.TelegramId,
                TelegramUsername = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = "" // Будет заполнено при подтверждении телефона
            };

            var userGuid = await _mediator.Send(registerCommand, cancellationToken);

            // Отправляем сообщение с подтверждением телефона
            await SendPhoneConfirmationMessage(request.ChatId);

            return true;
        }
        catch (Exception)
        {
            // В случае ошибки возвращаем false
            return false;
        }
    }

    /// <summary>
    /// Отправляет сообщение с подтверждением номера телефона
    /// </summary>
    /// <param name="chatId">Идентификатор чата</param>
    private async Task SendPhoneConfirmationMessage(long chatId)
    {
        var message = _messageTemplateService.GetPhoneConfirmationMessage();
        var keyboard = _messageTemplateService.GetPhoneConfirmationKeyboard();
        
        await _telegramService.SendMessageAsync(chatId, message, keyboard);
    }
}
