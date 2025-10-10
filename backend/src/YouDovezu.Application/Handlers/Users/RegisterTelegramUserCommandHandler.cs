using MediatR;
using Microsoft.EntityFrameworkCore;
using YouDovezu.Application.Commands.Users;
using YouDovezu.Application.Common.Interfaces;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Handlers.Users;

/// <summary>
/// Обработчик команды регистрации пользователя через Telegram
/// </summary>
/// <remarks>
/// Создает нового пользователя в системе или возвращает ID существующего.
/// Устанавливает согласие на обработку ПД и триал период 14 дней.
/// </remarks>
public class RegisterTelegramUserCommandHandler : IRequestHandler<RegisterTelegramUserCommand, Guid>
{
    private readonly IYouDovezuDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр RegisterTelegramUserCommandHandler
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public RegisterTelegramUserCommandHandler(IYouDovezuDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Обрабатывает команду регистрации пользователя
    /// </summary>
    /// <param name="request">Команда регистрации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>ID пользователя</returns>
    public async Task<Guid> Handle(RegisterTelegramUserCommand request, CancellationToken cancellationToken)
    {
        // Проверяем, не существует ли уже пользователь с таким TelegramId
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == request.TelegramId, cancellationToken);

        if (existingUser != null)
        {
            // Возвращаем ID существующего пользователя
            return existingUser.Id;
        }

        // Создаем нового пользователя
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = $"{request.FirstName} {request.LastName}".Trim(),
            Email = $"{request.TelegramUsername ?? request.TelegramId.ToString()}@telegram.local",
            PhoneNumber = request.PhoneNumber,
            TelegramId = request.TelegramId,
            TelegramUsername = request.TelegramUsername,
            Role = UserRole.User,
            PdConsent = true, // Согласие дается при регистрации через бота
            PdConsentAt = DateTime.UtcNow,
            TrialStart = DateTime.UtcNow,
            TrialEnd = DateTime.UtcNow.AddDays(14), // 14 дней триал периода
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
