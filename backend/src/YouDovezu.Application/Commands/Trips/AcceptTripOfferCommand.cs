using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Trips;

public class AcceptTripOfferCommand : BaseCommand<bool>
{
    public Guid TripRequestId { get; set; }
    public Guid OfferId { get; set; }
}
