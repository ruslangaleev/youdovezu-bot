using MediatR;
using YouDovezu.Application.Commands.Trips;
using YouDovezu.Application.Common.Interfaces;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Handlers.Trips;

public class CreateTripOfferCommandHandler : IRequestHandler<CreateTripOfferCommand, Guid>
{
    private readonly IYouDovezuDbContext _context;

    public CreateTripOfferCommandHandler(IYouDovezuDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTripOfferCommand request, CancellationToken cancellationToken)
    {
        var tripOffer = new TripOffer
        {
            Id = Guid.NewGuid(),
            TripRequestId = request.TripRequestId,
            DriverId = request.DriverId,
            Price = request.Price,
            Message = request.Message,
            Status = TripOfferStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TripOffers.Add(tripOffer);
        await _context.SaveChangesAsync(cancellationToken);

        return tripOffer.Id;
    }
}
