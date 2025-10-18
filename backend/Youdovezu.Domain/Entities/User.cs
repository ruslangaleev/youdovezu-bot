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
    /// Роль пользователя в системе (права доступа)
    /// </summary>
    public SystemRole SystemRole { get; set; } = SystemRole.User;

    /// <summary>
    /// Согласие с политикой конфиденциальности
    /// </summary>
    public bool PrivacyConsent { get; set; } = false;

    /// <summary>
    /// Дата согласия с политикой конфиденциальности
    /// </summary>
    public DateTime? PrivacyConsentDate { get; set; }

    /// <summary>
    /// Может ли пользователь быть пассажиром (после согласия с ПД)
    /// </summary>
    public bool CanBePassenger { get; set; } = false;

    /// <summary>
    /// Может ли пользователь быть водителем (после подтверждения водительских данных)
    /// </summary>
    public bool CanBeDriver { get; set; } = false;

    /// <summary>
    /// Дата начала триального периода (для водителей)
    /// </summary>
    public DateTime? TrialStartDate { get; set; }

    /// <summary>
    /// Дата окончания триального периода (для водителей)
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
    /// Проверяет, может ли пользователь быть пассажиром
    /// </summary>
    /// <returns>True, если пользователь может быть пассажиром</returns>
    public bool CanActAsPassenger()
    {
        return PrivacyConsent && CanBePassenger && Status == UserStatus.Active;
    }

    /// <summary>
    /// Проверяет, может ли пользователь быть водителем
    /// </summary>
    /// <returns>True, если пользователь может быть водителем</returns>
    public bool CanActAsDriver()
    {
        return PrivacyConsent && CanBeDriver && Status == UserStatus.Active;
    }

    /// <summary>
    /// Проверяет, является ли пользователь модератором или администратором
    /// </summary>
    /// <returns>True, если пользователь имеет права модерации</returns>
    public bool IsModerator()
    {
        return SystemRole == SystemRole.Moderator || SystemRole == SystemRole.Admin;
    }

    /// <summary>
    /// Проверяет, является ли пользователь администратором
    /// </summary>
    /// <returns>True, если пользователь администратор</returns>
    public bool IsAdmin()
    {
        return SystemRole == SystemRole.Admin;
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
/// Системные роли пользователей (права доступа)
/// </summary>
public enum SystemRole
{
    /// <summary>
    /// Обычный пользователь
    /// </summary>
    User = 0,

    /// <summary>
    /// Модератор
    /// </summary>
    Moderator = 1,

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
