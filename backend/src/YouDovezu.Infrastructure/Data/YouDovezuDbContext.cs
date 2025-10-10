using Microsoft.EntityFrameworkCore;
using YouDovezu.Domain.Entities;
using YouDovezu.Application.Common.Interfaces;

namespace YouDovezu.Infrastructure.Data;

public class YouDovezuDbContext : DbContext, IYouDovezuDbContext
{
    public YouDovezuDbContext(DbContextOptions<YouDovezuDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<TripRequest> TripRequests { get; set; }
    public DbSet<TripOffer> TripOffers { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<DriverInfo> DriverInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TelegramUsername).HasMaxLength(50);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TelegramId).IsUnique();
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
        });

        // TripRequest configuration
        modelBuilder.Entity<TripRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromAddress).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ToAddress).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TripOffer configuration
        modelBuilder.Entity<TripOffer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Message).HasMaxLength(500);
            
            entity.HasOne(e => e.TripRequest)
                .WithMany()
                .HasForeignKey(e => e.TripRequestId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Rating configuration
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(500);
            
            entity.HasOne(e => e.TripRequest)
                .WithMany()
                .HasForeignKey(e => e.TripRequestId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.FromUser)
                .WithMany()
                .HasForeignKey(e => e.FromUserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ToUser)
                .WithMany()
                .HasForeignKey(e => e.ToUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DriverInfo configuration
        modelBuilder.Entity<DriverInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CarMake).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CarModel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CarColor).IsRequired().HasMaxLength(30);
            entity.Property(e => e.CarNumber).IsRequired().HasMaxLength(20);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.VerifiedByAdmin)
                .WithMany()
                .HasForeignKey(e => e.VerifiedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}