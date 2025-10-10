using YouDovezu.Application.Common.Models;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Commands.Ratings;

public class CreateRatingCommand : BaseCommand<Guid>
{
    public Guid TripRequestId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public int Stars { get; set; } // 1-5
    public string? Comment { get; set; }
    public RatingType Type { get; set; }
}
