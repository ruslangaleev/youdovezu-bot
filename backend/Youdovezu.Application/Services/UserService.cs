using Microsoft.Extensions.Logging;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Application.Interfaces;

namespace Youdovezu.Application.Services;

/// <summary>
/// Сервис для работы с пользователями
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Конструктор сервиса
    /// </summary>
    /// <param name="userRepository">Репозиторий пользователей</param>
    /// <param name="logger">Логгер для записи событий</param>
    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="username">Имя пользователя в Telegram</param>
    /// <param name="firstName">Имя пользователя</param>
    /// <param name="lastName">Фамилия пользователя</param>
    /// <returns>Созданный пользователь</returns>
    public async Task<User> RegisterUserAsync(long telegramId, string? username, string? firstName, string? lastName)
    {
        _logger.LogInformation("Registering new user with Telegram ID: {TelegramId}", telegramId);

        // Проверяем, не зарегистрирован ли уже пользователь
        var existingUser = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (existingUser != null)
        {
            _logger.LogWarning("User with Telegram ID {TelegramId} is already registered", telegramId);
            return existingUser;
        }

        var user = new User
        {
            TelegramId = telegramId,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            SystemRole = SystemRole.User,
            Status = UserStatus.PendingRegistration,
            PrivacyConsent = false,
            CanBePassenger = false,
            CanBeDriver = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);
        _logger.LogInformation("User registered successfully with ID: {UserId}", createdUser.Id);

        return createdUser;
    }

    /// <summary>
    /// Получает пользователя по Telegram ID
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>Пользователь или null, если не найден</returns>
    public async Task<User?> GetUserByTelegramIdAsync(long telegramId)
    {
        return await _userRepository.GetByTelegramIdAsync(telegramId);
    }

    /// <summary>
    /// Обновляет согласие с политикой конфиденциальности
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>Обновленный пользователь</returns>
    public async Task<User> UpdatePrivacyConsentAsync(long telegramId)
    {
        _logger.LogInformation("Updating privacy consent for user with Telegram ID: {TelegramId}", telegramId);

        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with Telegram ID {telegramId} not found");
        }

        user.PrivacyConsent = true;
        user.PrivacyConsentDate = DateTime.UtcNow;
        user.CanBePassenger = true; // После согласия с ПД может быть пассажиром
        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Privacy consent updated for user ID: {UserId}", updatedUser.Id);

        return updatedUser;
    }

    /// <summary>
    /// Обновляет номер телефона пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <returns>Обновленный пользователь</returns>
    public async Task<User> UpdatePhoneNumberAsync(long telegramId, string phoneNumber)
    {
        _logger.LogInformation("Updating phone number for user with Telegram ID: {TelegramId}", telegramId);

        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with Telegram ID {telegramId} not found");
        }

        user.PhoneNumber = phoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Phone number updated for user ID: {UserId}", updatedUser.Id);

        return updatedUser;
    }

    /// <summary>
    /// Проверяет, зарегистрирован ли пользователь
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>True, если пользователь зарегистрирован</returns>
    public async Task<bool> IsUserRegisteredAsync(long telegramId)
    {
        return await _userRepository.ExistsByTelegramIdAsync(telegramId);
    }

    /// <summary>
    /// Включает возможность быть водителем для пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <returns>Обновленный пользователь</returns>
    public async Task<User> EnableDriverCapabilityAsync(long telegramId)
    {
        _logger.LogInformation("Enabling driver capability for user with Telegram ID: {TelegramId}", telegramId);

        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with Telegram ID {telegramId} not found");
        }

        user.CanBeDriver = true;
        user.TrialStartDate = DateTime.UtcNow;
        user.TrialEndDate = DateTime.UtcNow.AddDays(14); // 14 дней триала
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Driver capability enabled for user ID: {UserId}", updatedUser.Id);

        return updatedUser;
    }

    /// <summary>
    /// Обновляет системную роль пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="systemRole">Новая системная роль</param>
    /// <returns>Обновленный пользователь</returns>
    public async Task<User> UpdateSystemRoleAsync(long telegramId, SystemRole systemRole)
    {
        _logger.LogInformation("Updating system role for user with Telegram ID: {TelegramId} to {Role}", telegramId, systemRole);

        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with Telegram ID {telegramId} not found");
        }

        user.SystemRole = systemRole;
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("System role updated for user ID: {UserId}", updatedUser.Id);

        return updatedUser;
    }
}
