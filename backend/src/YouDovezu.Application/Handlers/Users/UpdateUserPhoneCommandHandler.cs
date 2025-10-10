using MediatR;
using Microsoft.EntityFrameworkCore;
using YouDovezu.Application.Commands.Users;
using YouDovezu.Application.Common.Interfaces;

namespace YouDovezu.Application.Handlers.Users;

public class UpdateUserPhoneCommandHandler : IRequestHandler<UpdateUserPhoneCommand, bool>
{
    private readonly IYouDovezuDbContext _context;

    public UpdateUserPhoneCommandHandler(IYouDovezuDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateUserPhoneCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == request.TelegramId, cancellationToken);

        if (user == null)
        {
            return false;
        }

        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
