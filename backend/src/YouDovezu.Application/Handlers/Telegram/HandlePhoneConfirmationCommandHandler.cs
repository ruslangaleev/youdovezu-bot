using MediatR;
using YouDovezu.Application.Commands.Telegram;
using YouDovezu.Application.Commands.Users;
using YouDovezu.Application.Common.Interfaces;

namespace YouDovezu.Application.Handlers.Telegram;

/// <summary>
/// Обработчик подтверждения номера телефона пользователем
/// </summary>
/// <remarks>
/// Завершает процесс регистрации, обновляет номер телефона и отправляет кнопку Web App.
/// </remarks>
public class HandlePhoneConfirmationCommandHandler : IRequestHandler<HandlePhoneConfirmationCommand, bool>
{
    private readonly IMediator _mediator;
    private readonly ITelegramService _telegramService;
    private readonly IMessageTemplateService _messageTemplateService;

    /// <summary>
    /// Инициализирует новый экземпляр HandlePhoneConfirmationCommandHandler
    /// </summary>
    /// <param name="mediator">MediatR для отправки команд</param>
    /// <param name="telegramService">Сервис для работы с Telegram</param>
    /// <param name="messageTemplateService">Сервис шаблонов сообщений</param>
    public HandlePhoneConfirmationCommandHandler(
        IMediator mediator, 
        ITelegramService telegramService, 
        IMessageTemplateService messageTemplateService)
    {
        _mediator = mediator;
        _telegramService = telegramService;
        _messageTemplateService = messageTemplateService;
    }

    /// <summary>
    /// Обрабатывает подтверждение номера телефона
    /// </summary>
    /// <param name="request">Запрос с данными пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат обработки</returns>
    public async Task<bool> Handle(HandlePhoneConfirmationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Обновляем номер телефона пользователя
            var updatePhoneCommand = new UpdateUserPhoneCommand
            {
                TelegramId = request.TelegramId,
                PhoneNumber = request.PhoneNumber
            };

            var phoneUpdated = await _mediator.Send(updatePhoneCommand, cancellationToken);

            if (phoneUpdated)
            {
                // Отправляем финальное сообщение с кнопкой Web App
                await SendRegistrationCompleteMessage(request.ChatId);
            }

            return phoneUpdated;
        }
        catch (Exception)
        {
            // В случае ошибки возвращаем false
            return false;
        }
    }

    /// <summary>
    /// Отправляет сообщение о завершении регистрации
    /// </summary>
    /// <param name="chatId">Идентификатор чата</param>
    private async Task SendRegistrationCompleteMessage(long chatId)
    {
        var message = _messageTemplateService.GetRegistrationCompleteMessage();
        var keyboard = _messageTemplateService.GetWebAppKeyboard("http://localhost:3000");
        
        await _telegramService.SendMessageAsync(chatId, message, keyboard);
    }
}
