using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Youdovezu.Application.Interfaces;
using Youdovezu.Application.Models;
using Youdovezu.Domain.Models;
using Youdovezu.Domain.Entities;

namespace Youdovezu.Infrastructure.Services;

/// <summary>
/// Сервис для работы с Telegram Bot API
/// </summary>
public class TelegramBotService : ITelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly IUserService _userService;
    private readonly TelegramSettings _telegramSettings;

    /// <summary>
    /// Конструктор сервиса
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot API</param>
    /// <param name="logger">Логгер для записи событий</param>
    /// <param name="userService">Сервис для работы с пользователями</param>
    /// <param name="telegramSettings">Настройки Telegram</param>
    public TelegramBotService(ITelegramBotClient botClient, ILogger<TelegramBotService> logger, IUserService userService, IOptions<TelegramSettings> telegramSettings)
    {
        _botClient = botClient;
        _logger = logger;
        _userService = userService;
        _telegramSettings = telegramSettings.Value;
    }

    /// <summary>
    /// Отправляет текстовое сообщение в указанный чат
    /// </summary>
    /// <param name="chatId">ID чата для отправки сообщения</param>
    /// <param name="message">Текст сообщения</param>
    /// <returns>Task, представляющий асинхронную операцию</returns>
    public async Task SendMessageAsync(long chatId, string message)
    {
        try
        {
            await _botClient.SendTextMessageAsync(chatId, message);
            _logger.LogInformation("Message sent to chat {ChatId}: {Message}", chatId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to chat {ChatId}", chatId);
            throw;
        }
    }

    /// <summary>
    /// Отправляет текстовое сообщение с клавиатурой в указанный чат
    /// </summary>
    /// <param name="chatId">ID чата для отправки сообщения</param>
    /// <param name="message">Текст сообщения</param>
    /// <param name="replyMarkup">Разметка клавиатуры</param>
    /// <returns>Task, представляющий асинхронную операцию</returns>
    public async Task SendMessageAsync(long chatId, string message, object replyMarkup)
    {
        try
        {
            await _botClient.SendTextMessageAsync(chatId, message, replyMarkup: replyMarkup as IReplyMarkup);
            _logger.LogInformation("Message with keyboard sent to chat {ChatId}: {Message}", chatId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message with keyboard to chat {ChatId}", chatId);
            throw;
        }
    }

    /// <summary>
    /// Обрабатывает входящее сообщение от пользователя
    /// Реализует логику регистрации и обработки команд
    /// </summary>
    /// <param name="message">Доменная модель сообщения</param>
    /// <returns>Task, представляющий асинхронную операцию</returns>
    public async Task ProcessMessageAsync(TelegramMessage message)
    {
        // Проверяем наличие текста в сообщении
        if (string.IsNullOrEmpty(message.Text))
        {
            _logger.LogWarning("Received message without text from chat {ChatId}", message.ChatId);
            return;
        }

        _logger.LogInformation("Processing message from chat {ChatId}: {Message}", message.ChatId, message.Text);

        try
        {
            // Обработка команды /start
            if (message.Text.StartsWith("/start"))
            {
                await HandleStartCommandAsync(message);
                return;
            }

            // Обработка согласия с политикой конфиденциальности
            if (message.Text.Contains("Согласен") || message.Text.Contains("согласен"))
            {
                await HandlePrivacyConsentAsync(message);
                return;
            }

            // Проверяем, зарегистрирован ли пользователь. Если нет — обрабатываем как /start
            var user = await _userService.GetUserByTelegramIdAsync(message.UserId);
            if (user == null)
            {
                await HandleStartCommandAsync(message);
                return;
            }

            // Если пользователь не дал согласие с политикой конфиденциальности
            if (!user.PrivacyConsent)
            {
                await SendPrivacyPolicyAsync(message.ChatId);
                return;
            }

            // Если пользователь не подтвердил номер телефона
            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                await SendPhoneConfirmationRequestAsync(message.ChatId);
                return;
            }

            // Пользователь полностью зарегистрирован - показываем главное меню
            await SendMainMenuAsync(message.ChatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from chat {ChatId}", message.ChatId);
            await SendMessageAsync(message.ChatId, "Произошла ошибка при обработке сообщения. Попробуйте позже.");
        }
    }

    /// <summary>
    /// Обрабатывает команду /start
    /// </summary>
    /// <param name="message">Сообщение с командой /start</param>
    private async Task HandleStartCommandAsync(TelegramMessage message)
    {
        _logger.LogInformation("Processing /start command from user {UserId}", message.UserId);

        // Проверяем, зарегистрирован ли пользователь
        var existingUser = await _userService.GetUserByTelegramIdAsync(message.UserId);
        
        if (existingUser != null)
        {
            // Пользователь уже зарегистрирован
            if (existingUser.PrivacyConsent && !string.IsNullOrEmpty(existingUser.PhoneNumber))
            {
                await SendMainMenuAsync(message.ChatId);
            }
            else if (!existingUser.PrivacyConsent)
            {
                await SendPrivacyPolicyAsync(message.ChatId);
            }
            else
            {
                await SendPhoneConfirmationRequestAsync(message.ChatId);
            }
        }
        else
        {
            // Регистрируем нового пользователя
            var user = await _userService.RegisterUserAsync(
                message.UserId,
                message.Username,
                message.FirstName,
                message.LastName);

            _logger.LogInformation("New user registered with ID: {UserId}", user.Id);
            
            // Отправляем политику конфиденциальности
            await SendPrivacyPolicyAsync(message.ChatId);
        }
    }

    /// <summary>
    /// Обрабатывает согласие с политикой конфиденциальности
    /// </summary>
    /// <param name="message">Сообщение с согласием</param>
    private async Task HandlePrivacyConsentAsync(TelegramMessage message)
    {
        _logger.LogInformation("Processing privacy consent from user {UserId}", message.UserId);

        try
        {
            var user = await _userService.UpdatePrivacyConsentAsync(message.UserId);
            _logger.LogInformation("Privacy consent updated for user {UserId}", user.Id);

            // Отправляем запрос на подтверждение номера телефона
            await SendPhoneConfirmationRequestAsync(message.ChatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating privacy consent for user {UserId}", message.UserId);
            await SendMessageAsync(message.ChatId, "Произошла ошибка при обработке согласия. Попробуйте позже.");
        }
    }

    /// <summary>
    /// Отправляет политику конфиденциальности
    /// </summary>
    /// <param name="chatId">ID чата</param>
    private async Task SendPrivacyPolicyAsync(long chatId)
    {
        var privacyPolicyText = @"🔒 **Политика конфиденциальности**

Добро пожаловать в YouDovezu - P2P-платформу для организации поездок в Башкирии!

**Обработка персональных данных:**
• Мы обрабатываем ваши данные в соответствии с ФЗ-152 «О персональных данных»
• Используем только необходимые данные: имя, номер телефона, Telegram ID
• Данные используются исключительно для функционирования сервиса
• Мы не передаем ваши данные третьим лицам

**Цели обработки:**
• Связь между водителями и пассажирами
• Обеспечение безопасности поездок
• Поддержка пользователей

Нажимая кнопку «Согласен», вы подтверждаете согласие на обработку персональных данных.";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Согласен", "privacy_consent")
            }
        });

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: privacyPolicyText,
            replyMarkup: keyboard,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

    /// <summary>
    /// Отправляет запрос на подтверждение номера телефона
    /// </summary>
    /// <param name="chatId">ID чата</param>
    private async Task SendPhoneConfirmationRequestAsync(long chatId)
    {
        var phoneText = @"📱 **Подтверждение номера телефона**

Для завершения регистрации необходимо подтвердить ваш номер телефона.

**Как это работает:**
• Telegram автоматически предоставляет ваш номер телефона
• Мы используем его для связи между участниками поездок
• Номер будет виден только другим участникам поездки

Нажмите кнопку ниже для подтверждения:";

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                KeyboardButton.WithRequestContact("📱 Подтвердить номер телефона")
            }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: phoneText,
            replyMarkup: keyboard,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

    /// <summary>
    /// Отправляет главное меню после завершения регистрации
    /// </summary>
    /// <param name="chatId">ID чата</param>
    public async Task SendMainMenuAsync(long chatId)
    {
        var welcomeText = @"🎉 **Добро пожаловать в YouDovezu!**

Вы успешно зарегистрированы в P2P-платформе для организации поездок в Башкирии.

**Что вы можете делать:**
• 🚗 Найти попутчиков для поездок
• 🗺️ Предложить свою поездку
• ⭐ Оставить отзыв о поездке
• 💬 Связаться с участниками

Для удобства используйте веб-приложение:";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp("🌐 Открыть веб-приложение", new Telegram.Bot.Types.WebAppInfo { Url = _telegramSettings.WebAppUrl })
            }
        });

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: welcomeText,
            replyMarkup: keyboard,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
    }

}
