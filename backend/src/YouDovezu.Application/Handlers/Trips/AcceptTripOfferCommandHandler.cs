using MediatR;
using Microsoft.EntityFrameworkCore;
using YouDovezu.Application.Commands.Trips;
using YouDovezu.Application.Common.Interfaces;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Handlers.Trips;

public class AcceptTripOfferCommandHandler : IRequestHandler<AcceptTripOfferCommand, bool>
{
    private readonly IYouDovezuDbContext _context;

    public AcceptTripOfferCommandHandler(IYouDovezuDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AcceptTripOfferCommand request, CancellationToken cancellationToken)
    {
        var tripRequest = await _context.TripRequests
            .FirstOrDefaultAsync(tr => tr.Id == request.TripRequestId, cancellationToken);

        var offer = await _context.TripOffers
            .FirstOrDefaultAsync(to => to.Id == request.OfferId, cancellationToken);

        if (tripRequest == null || offer == null || offer.TripRequestId != request.TripRequestId)
        {
            return false;
        }

        // Обновляем статус запроса
        tripRequest.Status = TripRequestStatus.Accepted;
        tripRequest.AcceptedOfferId = offer.Id;
        tripRequest.UpdatedAt = DateTime.UtcNow;

        // Обновляем статус предложения
        offer.Status = TripOfferStatus.Accepted;
        offer.UpdatedAt = DateTime.UtcNow;

        // Отклоняем все остальные предложения для этого запроса
        var otherOffers = await _context.TripOffers
            .Where(to => to.TripRequestId == request.TripRequestId && to.Id != request.OfferId)
            .ToListAsync(cancellationToken);

        foreach (var otherOffer in otherOffers)
        {
            otherOffer.Status = TripOfferStatus.Rejected;
            otherOffer.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
