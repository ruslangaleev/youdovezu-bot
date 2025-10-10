namespace YouDovezu.Domain.Entities;

/// <summary>
/// Пользователь платформы YouDovezu
/// </summary>
/// <remarks>
/// Основная сущность, представляющая пользователя системы.
/// Может быть обычным пользователем, водителем или администратором.
/// Содержит информацию о регистрации через Telegram, согласии на обработку ПД,
/// триал периоде и рейтинге.
/// </remarks>
public class User : BaseEntity
{
    /// <summary>
    /// Полное имя пользователя
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email адрес пользователя
    /// </summary>
    /// <remarks>
    /// Для пользователей из Telegram генерируется автоматически
    /// </remarks>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Номер телефона пользователя
    /// </summary>
    /// <remarks>
    /// Получается из Telegram при регистрации
    /// </remarks>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Уникальный идентификатор пользователя в Telegram
    /// </summary>
    public long? TelegramId { get; set; }

    /// <summary>
    /// Имя пользователя в Telegram (без @)
    /// </summary>
    public string? TelegramUsername { get; set; }

    /// <summary>
    /// Роль пользователя в системе
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>
    /// Согласие на обработку персональных данных
    /// </summary>
    /// <remarks>
    /// Согласие дается при регистрации через Telegram бота
    /// </remarks>
    public bool PdConsent { get; set; } = false;

    /// <summary>
    /// Дата и время дачи согласия на обработку ПД
    /// </summary>
    public DateTime? PdConsentAt { get; set; }

    /// <summary>
    /// Дата начала триал периода
    /// </summary>
    /// <remarks>
    /// Для будущих водителей - 14 дней бесплатного использования
    /// </remarks>
    public DateTime? TrialStart { get; set; }

    /// <summary>
    /// Дата окончания триал периода
    /// </summary>
    public DateTime? TrialEnd { get; set; }

    /// <summary>
    /// Статус верификации пользователя
    /// </summary>
    /// <remarks>
    /// Для водителей - проверка документов администратором
    /// </remarks>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Средний рейтинг пользователя
    /// </summary>
    /// <remarks>
    /// Рассчитывается автоматически на основе всех полученных оценок
    /// </remarks>
    public double Rating { get; set; } = 0.0;

    /// <summary>
    /// Количество полученных оценок
    /// </summary>
    public int RatingCount { get; set; } = 0;
}

/// <summary>
/// Роли пользователей в системе
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Обычный пользователь (клиент)
    /// </summary>
    User = 0,

    /// <summary>
    /// Водитель
    /// </summary>
    Driver = 1,

    /// <summary>
    /// Администратор системы
    /// </summary>
    Admin = 2
}