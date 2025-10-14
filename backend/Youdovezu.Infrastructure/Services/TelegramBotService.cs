using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Youdovezu.Application.Interfaces;
using Youdovezu.Domain.Models;

namespace Youdovezu.Infrastructure.Services;

/// <summary>
/// Сервис для работы с Telegram Bot API
/// </summary>
public class TelegramBotService : ITelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotService> _logger;

    /// <summary>
    /// Конструктор сервиса
    /// </summary>
    /// <param name="botClient">Клиент Telegram Bot API</param>
    /// <param name="logger">Логгер для записи событий</param>
    public TelegramBotService(ITelegramBotClient botClient, ILogger<TelegramBotService> logger)
    {
        _botClient = botClient;
        _logger = logger;
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
    /// Обрабатывает входящее сообщение от пользователя
    /// Реализует эхо-функциональность: отправляет то же сообщение обратно
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

        // Эхо-логика: отправляем то же сообщение обратно пользователю
        await SendMessageAsync(message.ChatId, message.Text);
    }
}
