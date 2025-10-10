using MediatR;
using YouDovezu.Application.Commands.Trips;
using YouDovezu.Application.Common.Interfaces;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Handlers.Trips;

public class CreateTripRequestCommandHandler : IRequestHandler<CreateTripRequestCommand, Guid>
{
    private readonly IYouDovezuDbContext _context;

    public CreateTripRequestCommandHandler(IYouDovezuDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTripRequestCommand request, CancellationToken cancellationToken)
    {
        var tripRequest = new TripRequest
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            FromAddress = request.FromAddress,
            ToAddress = request.ToAddress,
            FromLatitude = request.FromLatitude,
            FromLongitude = request.FromLongitude,
            ToLatitude = request.ToLatitude,
            ToLongitude = request.ToLongitude,
            Description = request.Description,
            PassengerCount = request.PassengerCount,
            PreferredDateTime = request.PreferredDateTime,
            Status = TripRequestStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TripRequests.Add(tripRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return tripRequest.Id;
    }
}
