using Microsoft.EntityFrameworkCore;
using YouDovezu.Domain.Entities;

namespace YouDovezu.Application.Common.Interfaces;

public interface IYouDovezuDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<TripRequest> TripRequests { get; set; }
    DbSet<TripOffer> TripOffers { get; set; }
    DbSet<Rating> Ratings { get; set; }
    DbSet<DriverInfo> DriverInfos { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
