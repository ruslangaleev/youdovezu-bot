using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Telegram;

/// <summary>
/// Команда для обработки подтверждения номера телефона пользователем
/// </summary>
/// <remarks>
/// Завершает процесс регистрации, обновляет номер телефона и отправляет кнопку Web App.
/// </remarks>
public class HandlePhoneConfirmationCommand : BaseCommand<bool>
{
    /// <summary>
    /// Идентификатор пользователя в Telegram
    /// </summary>
    public long TelegramId { get; set; }

    /// <summary>
    /// Номер телефона пользователя
    /// </summary>
    /// <remarks>
    /// Получается из Telegram initData или вводится пользователем
    /// </remarks>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор чата
    /// </summary>
    public long ChatId { get; set; }
}
