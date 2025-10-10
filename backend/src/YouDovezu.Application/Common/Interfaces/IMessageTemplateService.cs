namespace YouDovezu.Application.Common.Interfaces;

/// <summary>
/// Интерфейс для работы с шаблонами сообщений Telegram
/// </summary>
/// <remarks>
/// Предоставляет методы для получения текстов сообщений и клавиатур.
/// Содержит бизнес-логику формирования сообщений для пользователей.
/// </remarks>
public interface IMessageTemplateService
{
    /// <summary>
    /// Получает сообщение с политикой конфиденциальности
    /// </summary>
    /// <param name="firstName">Имя пользователя</param>
    /// <returns>Текст сообщения</returns>
    string GetPrivacyPolicyMessage(string firstName);

    /// <summary>
    /// Получает клавиатуру для согласия с политикой конфиденциальности
    /// </summary>
    /// <returns>Inline клавиатура</returns>
    object GetPrivacyPolicyKeyboard();

    /// <summary>
    /// Получает сообщение об отказе от политики конфиденциальности
    /// </summary>
    /// <returns>Текст сообщения</returns>
    string GetPrivacyDeclineMessage();

    /// <summary>
    /// Получает сообщение с подтверждением номера телефона
    /// </summary>
    /// <returns>Текст сообщения</returns>
    string GetPhoneConfirmationMessage();

    /// <summary>
    /// Получает клавиатуру для подтверждения номера телефона
    /// </summary>
    /// <returns>Inline клавиатура</returns>
    object GetPhoneConfirmationKeyboard();

    /// <summary>
    /// Получает сообщение о завершении регистрации
    /// </summary>
    /// <returns>Текст сообщения</returns>
    string GetRegistrationCompleteMessage();

    /// <summary>
    /// Получает клавиатуру с кнопкой Web App
    /// </summary>
    /// <param name="webAppUrl">URL веб-приложения</param>
    /// <returns>Inline клавиатура</returns>
    object GetWebAppKeyboard(string webAppUrl);

    /// <summary>
    /// Получает приветственное сообщение для существующего пользователя
    /// </summary>
    /// <param name="firstName">Имя пользователя</param>
    /// <returns>Текст сообщения</returns>
    string GetWelcomeBackMessage(string firstName);
}
