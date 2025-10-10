using MediatR;
using Microsoft.EntityFrameworkCore;
using YouDovezu.Application.Queries.Users;
using YouDovezu.Application.Common.Interfaces;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Handlers.Users;

public class GetUserByTelegramIdQueryHandler : IRequestHandler<GetUserByTelegramIdQuery, User?>
{
    private readonly IYouDovezuDbContext _context;

    public GetUserByTelegramIdQueryHandler(IYouDovezuDbContext context)
    {
        _context = context;
    }

    public async Task<User?> Handle(GetUserByTelegramIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == request.TelegramId, cancellationToken);
    }
}
