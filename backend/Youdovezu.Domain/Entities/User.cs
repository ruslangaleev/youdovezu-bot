namespace Youdovezu.Domain.Entities;

/// <summary>
/// Сущность пользователя платформы YouDovezu
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Telegram ID пользователя
    /// </summary>
    public long TelegramId { get; set; }

    /// <summary>
    /// Имя пользователя в Telegram
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Номер телефона пользователя
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Роль пользователя в системе
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>
    /// Согласие с политикой конфиденциальности
    /// </summary>
    public bool PrivacyConsent { get; set; } = false;

    /// <summary>
    /// Дата согласия с политикой конфиденциальности
    /// </summary>
    public DateTime? PrivacyConsentDate { get; set; }

    /// <summary>
    /// Дата начала триального периода (для будущих водителей)
    /// </summary>
    public DateTime? TrialStartDate { get; set; }

    /// <summary>
    /// Дата окончания триального периода (для будущих водителей)
    /// </summary>
    public DateTime? TrialEndDate { get; set; }

    /// <summary>
    /// Статус пользователя
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>
    /// Дата создания пользователя
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Проверяет, активен ли триальный период
    /// </summary>
    /// <returns>True, если триальный период активен</returns>
    public bool IsTrialActive()
    {
        if (!TrialStartDate.HasValue || !TrialEndDate.HasValue)
            return false;

        var now = DateTime.UtcNow;
        return now >= TrialStartDate.Value && now <= TrialEndDate.Value;
    }

    /// <summary>
    /// Получает полное имя пользователя
    /// </summary>
    /// <returns>Полное имя или username, или "Пользователь"</returns>
    public string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            return $"{FirstName} {LastName}";
        
        if (!string.IsNullOrEmpty(FirstName))
            return FirstName;
            
        if (!string.IsNullOrEmpty(Username))
            return $"@{Username}";
            
        return "Пользователь";
    }
}

/// <summary>
/// Роли пользователей в системе
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Обычный пользователь
    /// </summary>
    User = 0,

    /// <summary>
    /// Водитель
    /// </summary>
    Driver = 1,

    /// <summary>
    /// Администратор
    /// </summary>
    Admin = 2
}

/// <summary>
/// Статусы пользователей
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Активный пользователь
    /// </summary>
    Active = 0,

    /// <summary>
    /// Заблокированный пользователь
    /// </summary>
    Blocked = 1,

    /// <summary>
    /// Пользователь в процессе регистрации
    /// </summary>
    PendingRegistration = 2
}
