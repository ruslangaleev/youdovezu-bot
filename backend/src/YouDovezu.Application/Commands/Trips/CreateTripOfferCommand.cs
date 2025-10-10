using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Trips;

public class CreateTripOfferCommand : BaseCommand<Guid>
{
    public Guid TripRequestId { get; set; }
    public Guid DriverId { get; set; }
    public decimal Price { get; set; }
    public string Message { get; set; } = string.Empty;
}
