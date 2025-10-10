using MediatR;
using Microsoft.EntityFrameworkCore;
using YouDovezu.Application.Commands.Ratings;
using YouDovezu.Application.Common.Interfaces;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Handlers.Ratings;

public class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, Guid>
{
    private readonly IYouDovezuDbContext _context;

    public CreateRatingCommandHandler(IYouDovezuDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
    {
        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            TripRequestId = request.TripRequestId,
            FromUserId = request.FromUserId,
            ToUserId = request.ToUserId,
            Stars = request.Stars,
            Comment = request.Comment,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);

        // Обновляем рейтинг пользователя
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ToUserId, cancellationToken);

        if (user != null)
        {
            var userRatings = await _context.Ratings
                .Where(r => r.ToUserId == request.ToUserId)
                .ToListAsync(cancellationToken);

            user.Rating = userRatings.Average(r => r.Stars);
            user.RatingCount = userRatings.Count;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return rating.Id;
    }
}
