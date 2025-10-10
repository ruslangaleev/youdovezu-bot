using MediatR;
using YouDovezu.Application.Commands.Telegram;
using YouDovezu.Application.Common.Interfaces;
using YouDovezu.Application.Queries.Users;

namespace YouDovezu.Application.Handlers.Telegram;

/// <summary>
/// Обработчик команды /start от пользователя Telegram
/// </summary>
/// <remarks>
/// Определяет, является ли пользователь новым или существующим,
/// и отправляет соответствующее сообщение.
/// </remarks>
public class HandleStartCommandHandler : IRequestHandler<HandleStartCommand, bool>
{
    private readonly IMediator _mediator;
    private readonly ITelegramService _telegramService;
    private readonly IMessageTemplateService _messageTemplateService;

    /// <summary>
    /// Инициализирует новый экземпляр HandleStartCommandHandler
    /// </summary>
    /// <param name="mediator">MediatR для отправки запросов</param>
    /// <param name="telegramService">Сервис для работы с Telegram</param>
    /// <param name="messageTemplateService">Сервис шаблонов сообщений</param>
    public HandleStartCommandHandler(
        IMediator mediator, 
        ITelegramService telegramService, 
        IMessageTemplateService messageTemplateService)
    {
        _mediator = mediator;
        _telegramService = telegramService;
        _messageTemplateService = messageTemplateService;
    }

    /// <summary>
    /// Обрабатывает команду /start
    /// </summary>
    /// <param name="request">Запрос с данными пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат обработки</returns>
    public async Task<bool> Handle(HandleStartCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Проверяем, существует ли пользователь в системе
            var existingUserQuery = new GetUserByTelegramIdQuery { TelegramId = request.TelegramId };
            var existingUser = await _mediator.Send(existingUserQuery, cancellationToken);

            if (existingUser == null)
            {
                // Новый пользователь - показываем политику конфиденциальности
                await SendPrivacyPolicyMessage(request.ChatId, request.FirstName);
            }
            else if (!existingUser.PdConsent)
            {
                // Пользователь существует, но не дал согласие
                await SendPrivacyPolicyMessage(request.ChatId, request.FirstName);
            }
            else
            {
                // Пользователь уже зарегистрирован
                await SendWelcomeBackMessage(request.ChatId, request.FirstName);
            }

            return true;
        }
        catch (Exception)
        {
            // В случае ошибки возвращаем false
            return false;
        }
    }

    /// <summary>
    /// Отправляет сообщение с политикой конфиденциальности
    /// </summary>
    /// <param name="chatId">Идентификатор чата</param>
    /// <param name="firstName">Имя пользователя</param>
    private async Task SendPrivacyPolicyMessage(long chatId, string firstName)
    {
        var message = _messageTemplateService.GetPrivacyPolicyMessage(firstName);
        var keyboard = _messageTemplateService.GetPrivacyPolicyKeyboard();
        
        await _telegramService.SendMessageAsync(chatId, message, keyboard);
    }

    /// <summary>
    /// Отправляет приветственное сообщение для существующего пользователя
    /// </summary>
    /// <param name="chatId">Идентификатор чата</param>
    /// <param name="firstName">Имя пользователя</param>
    private async Task SendWelcomeBackMessage(long chatId, string firstName)
    {
        var message = _messageTemplateService.GetWelcomeBackMessage(firstName);
        var keyboard = _messageTemplateService.GetWebAppKeyboard("http://localhost:3000");
        
        await _telegramService.SendMessageAsync(chatId, message, keyboard);
    }
}
