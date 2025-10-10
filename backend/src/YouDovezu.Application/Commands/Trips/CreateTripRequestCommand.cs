using YouDovezu.Application.Common.Models;

namespace YouDovezu.Application.Commands.Trips;

public class CreateTripRequestCommand : BaseCommand<Guid>
{
    public Guid UserId { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public double FromLatitude { get; set; }
    public double FromLongitude { get; set; }
    public double ToLatitude { get; set; }
    public double ToLongitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PassengerCount { get; set; } = 1;
    public DateTime? PreferredDateTime { get; set; }
}
