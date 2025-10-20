using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Youdovezu.Application.Interfaces;
using Youdovezu.Infrastructure.Services;
using Youdovezu.Infrastructure.Converters;
using System.Text.Json;

namespace Youdovezu.Presentation.Controllers;

/// <summary>
/// Контроллер для обработки webhook запросов от Telegram Bot API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
    private readonly ITelegramBotService _telegramBotService;
    private readonly IUserService _userService;
    private readonly ILogger<BotController> _logger;

    /// <summary>
    /// Конструктор контроллера
    /// </summary>
    /// <param name="telegramBotService">Сервис для работы с Telegram ботом</param>
    /// <param name="userService">Сервис для работы с пользователями</param>
    /// <param name="logger">Логгер для записи событий</param>
    public BotController(ITelegramBotService telegramBotService, IUserService userService, ILogger<BotController> logger)
    {
        _telegramBotService = telegramBotService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Webhook endpoint для получения обновлений от Telegram
    /// </summary>
    /// <param name="updateObject">Объект обновления от Telegram API</param>
    /// <returns>HTTP 200 OK при успешной обработке</returns>
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] object updateObject)
    {
        try
        {
            // Логируем входящий объект для отладки
            _logger.LogInformation("Received webhook object: {UpdateObject}", JsonSerializer.Serialize(updateObject));

            // Маппим объект в Update с правильной конфигурацией JSON
            Update update;
            try
            {
                // Сериализуем объект в JSON
                var jsonString = JsonSerializer.Serialize(updateObject);
                
                // Используем JsonDocument для более гибкой десериализации
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                
                // Создаем Update объект вручную
                update = new Update
                {
                    Id = root.TryGetProperty("update_id", out var updateIdElement) ? (int)updateIdElement.GetInt64() : 0
                };
                
                // Проверяем тип обновления и заполняем соответствующие поля
                if (root.TryGetProperty("message", out var messageElement))
                {
                    update.Message = DeserializeMessage(messageElement);
                }
                else if (root.TryGetProperty("callback_query", out var callbackQueryElement))
                {
                    update.CallbackQuery = DeserializeCallbackQuery(callbackQueryElement);
                }
                
                _logger.LogInformation("Successfully mapped to Update: {UpdateId}, Type: {UpdateType}", update.Id, update.Type);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to deserialize update object to Update");
                return BadRequest("Invalid update format");
            }

            // Обрабатываем разные типы обновлений
            if (update.Message != null)
            {
                _logger.LogInformation("Processing message - ChatId: {ChatId}, Text: {Text}, From: {FromId}", 
                    update.Message.Chat?.Id, 
                    update.Message.Text,
                    update.Message.From?.Id);
                
                // Проверяем, есть ли контактные данные (номер телефона)
                if (update.Message.Contact != null)
                {
                    await ProcessContactMessage(update.Message);
                }
                else
                {
                    // Преобразуем Telegram сообщение в доменную модель
                    var domainMessage = TelegramMessageMapper.ToDomainModel(update.Message);
                    
                    // Обрабатываем сообщение через сервис
                    await _telegramBotService.ProcessMessageAsync(domainMessage);
                }
            }
            else if (update.CallbackQuery != null)
            {
                _logger.LogInformation("Processing callback query - From: {FromId}, Data: {Data}", 
                    update.CallbackQuery.From?.Id,
                    update.CallbackQuery.Data);
                
                // Обрабатываем callback query
                await ProcessCallbackQuery(update.CallbackQuery);
            }
            else
            {
                _logger.LogWarning("Update has no message or callback query. Update type: {UpdateType}", update.Type);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook update");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Десериализует Message из JsonElement
    /// </summary>
    /// <param name="messageElement">JsonElement с данными сообщения</param>
    /// <returns>Объект Message или null</returns>
    private Message? DeserializeMessage(JsonElement messageElement)
    {
        try
        {
            var message = new Message
            {
                MessageId = messageElement.TryGetProperty("message_id", out var msgId) ? msgId.GetInt32() : 0,
                Text = messageElement.TryGetProperty("text", out var text) ? text.GetString() : null,
                Date = messageElement.TryGetProperty("date", out var date) ? DateTimeOffset.FromUnixTimeSeconds(date.GetInt64()).DateTime : DateTime.MinValue
            };

            // Десериализуем Contact (если есть)
            if (messageElement.TryGetProperty("contact", out var contactElement))
            {
                message.Contact = new Contact
                {
                    PhoneNumber = contactElement.TryGetProperty("phone_number", out var phone) ? phone.GetString() : null,
                    FirstName = contactElement.TryGetProperty("first_name", out var contactFirstName) ? contactFirstName.GetString() : null,
                    LastName = contactElement.TryGetProperty("last_name", out var contactLastName) ? contactLastName.GetString() : null,
                    UserId = contactElement.TryGetProperty("user_id", out var contactUserId) ? contactUserId.GetInt64() : null
                };
            }

            // Десериализуем From
            if (messageElement.TryGetProperty("from", out var fromElement))
            {
                message.From = new User
                {
                    Id = fromElement.TryGetProperty("id", out var fromId) ? fromId.GetInt64() : 0,
                    FirstName = fromElement.TryGetProperty("first_name", out var firstName) ? firstName.GetString() : null,
                    LastName = fromElement.TryGetProperty("last_name", out var lastName) ? lastName.GetString() : null,
                    Username = fromElement.TryGetProperty("username", out var username) ? username.GetString() : null,
                    IsBot = fromElement.TryGetProperty("is_bot", out var isBot) ? isBot.GetBoolean() : false
                };
            }

            // Десериализуем Chat
            if (messageElement.TryGetProperty("chat", out var chatElement))
            {
                message.Chat = new Chat
                {
                    Id = chatElement.TryGetProperty("id", out var chatId) ? chatId.GetInt64() : 0,
                    Type = chatElement.TryGetProperty("type", out var type) ? 
                        Enum.Parse<ChatType>(type.GetString() ?? "Private", true) : ChatType.Private,
                    FirstName = chatElement.TryGetProperty("first_name", out var chatFirstName) ? chatFirstName.GetString() : null,
                    LastName = chatElement.TryGetProperty("last_name", out var chatLastName) ? chatLastName.GetString() : null,
                    Username = chatElement.TryGetProperty("username", out var chatUsername) ? chatUsername.GetString() : null
                };
            }

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing message");
            return null;
        }
    }

    /// <summary>
    /// Десериализует CallbackQuery из JsonElement
    /// </summary>
    /// <param name="callbackQueryElement">JsonElement с данными callback query</param>
    /// <returns>Объект CallbackQuery или null</returns>
    private CallbackQuery? DeserializeCallbackQuery(JsonElement callbackQueryElement)
    {
        try
        {
            var callbackQuery = new CallbackQuery
            {
                Id = callbackQueryElement.TryGetProperty("id", out var id) ? id.GetString() : null,
                Data = callbackQueryElement.TryGetProperty("data", out var data) ? data.GetString() : null,
                ChatInstance = callbackQueryElement.TryGetProperty("chat_instance", out var chatInstance) ? chatInstance.GetString() : null
            };

            // Десериализуем From
            if (callbackQueryElement.TryGetProperty("from", out var fromElement))
            {
                callbackQuery.From = new User
                {
                    Id = fromElement.TryGetProperty("id", out var fromId) ? fromId.GetInt64() : 0,
                    FirstName = fromElement.TryGetProperty("first_name", out var firstName) ? firstName.GetString() : null,
                    LastName = fromElement.TryGetProperty("last_name", out var lastName) ? lastName.GetString() : null,
                    Username = fromElement.TryGetProperty("username", out var username) ? username.GetString() : null,
                    IsBot = fromElement.TryGetProperty("is_bot", out var isBot) ? isBot.GetBoolean() : false
                };
            }

            // Десериализуем Message (если есть)
            if (callbackQueryElement.TryGetProperty("message", out var messageElement))
            {
                callbackQuery.Message = DeserializeMessage(messageElement);
            }

            return callbackQuery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing callback query");
            return null;
        }
    }

    /// <summary>
    /// Обрабатывает сообщение с контактными данными (номер телефона)
    /// </summary>
    /// <param name="message">Сообщение с контактными данными</param>
    private async Task ProcessContactMessage(Message message)
    {
        try
        {
            if (message.Contact == null || message.From == null)
            {
                _logger.LogWarning("Contact message has no contact or user data");
                return;
            }

            var telegramId = message.From.Id;
            var phoneNumber = message.Contact.PhoneNumber;

            _logger.LogInformation("Processing contact message from user {UserId}, phone: {PhoneNumber}", 
                telegramId, phoneNumber);

            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogWarning("Contact message has no phone number");
                await _telegramBotService.SendMessageAsync(telegramId, 
                    "❌ Не удалось получить номер телефона. Пожалуйста, попробуйте снова.");
                return;
            }

            try
            {
                // Обновляем номер телефона пользователя
                var updatedUser = await _userService.UpdatePhoneNumberAsync(telegramId, phoneNumber);
                
                _logger.LogInformation("Phone number updated successfully for user {UserId}", updatedUser.Id);
                
                // Отправляем сообщение о завершении регистрации
                var completionMessage = "🎉 Поздравляем! Регистрация завершена успешно!\n\n" +
                    "✅ Вы согласились с политикой конфиденциальности\n" +
                    "✅ Подтвердили номер телефона\n\n" +
                    "Теперь вы можете использовать все функции YouDovezu:\n" +
                    "🔍 Искать поездки\n" +
                    "🚗 Предлагать свои поездки\n\n" +
                    "Добро пожаловать в YouDovezu! 🚗";
                
                await _telegramBotService.SendMessageAsync(telegramId, completionMessage);
                
                _logger.LogInformation("Registration completion message sent to user {UserId}", telegramId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "User {UserId} not found when trying to update phone number", telegramId);
                
                await _telegramBotService.SendMessageAsync(telegramId, 
                    "❌ Произошла ошибка при обработке номера телефона. Пожалуйста, обратитесь в поддержку.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating phone number for user {UserId}", telegramId);
                
                await _telegramBotService.SendMessageAsync(telegramId, 
                    "❌ Произошла ошибка при обработке номера телефона. Пожалуйста, попробуйте позже.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact message");
        }
    }

    /// <summary>
    /// Обрабатывает callback query от пользователя
    /// </summary>
    /// <param name="callbackQuery">Callback query для обработки</param>
    private async Task ProcessCallbackQuery(CallbackQuery callbackQuery)
    {
        try
        {
            _logger.LogInformation("Processing callback query with data: {Data}", callbackQuery.Data);

            // Проверяем, что у нас есть пользователь
            if (callbackQuery.From == null)
            {
                _logger.LogWarning("Callback query has no user information");
                return;
            }

            var telegramId = callbackQuery.From.Id;

            // Обрабатываем разные типы callback query
            switch (callbackQuery.Data)
            {
                case "privacy_consent":
                    _logger.LogInformation("User {UserId} accepted privacy policy", telegramId);
                    
                    try
                    {
                        // Обновляем согласие с политикой конфиденциальности в базе данных
                        var updatedUser = await _userService.UpdatePrivacyConsentAsync(telegramId);
                        
                        _logger.LogInformation("Privacy consent updated successfully for user {UserId}. " +
                            "User can now be passenger: {CanBePassenger}, Status: {Status}", 
                            updatedUser.Id, updatedUser.CanBePassenger, updatedUser.Status);
                        
                        // Отправляем сообщение с запросом номера телефона
                        var phoneRequestMessage = "📱 Отлично! Теперь для завершения регистрации поделитесь своим номером телефона.\n\n" +
                            "Нажмите кнопку \"📱 Поделиться номером\" ниже:";
                        
                        // Создаем клавиатуру для запроса номера телефона
                        var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                KeyboardButton.WithRequestContact("📱 Поделиться номером")
                            }
                        })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        };
                        
                        await _telegramBotService.SendMessageAsync(telegramId, phoneRequestMessage, keyboard);
                        
                        _logger.LogInformation("Phone number request sent to user {UserId}", telegramId);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogError(ex, "User {UserId} not found when trying to update privacy consent", telegramId);
                        
                        // Отправляем сообщение об ошибке
                        await _telegramBotService.SendMessageAsync(telegramId, 
                            "❌ Произошла ошибка при обработке вашего согласия. Пожалуйста, обратитесь в поддержку.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating privacy consent for user {UserId}", telegramId);
                        
                        // Отправляем сообщение об ошибке
                        await _telegramBotService.SendMessageAsync(telegramId, 
                            "❌ Произошла ошибка при обработке вашего согласия. Пожалуйста, попробуйте позже.");
                    }
                    break;
                    
                default:
                    _logger.LogWarning("Unknown callback query data: {Data}", callbackQuery.Data);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing callback query");
        }
    }

    /// <summary>
    /// Endpoint для проверки состояния бота
    /// </summary>
    /// <returns>Информация о состоянии сервиса</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
