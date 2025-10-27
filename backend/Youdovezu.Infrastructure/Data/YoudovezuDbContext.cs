using Microsoft.EntityFrameworkCore;
using Youdovezu.Domain.Entities;

namespace Youdovezu.Infrastructure.Data;

/// <summary>
/// DbContext для работы с базой данных Youdovezu
/// </summary>
public class YoudovezuDbContext : DbContext
{
    /// <summary>
    /// Конструктор DbContext
    /// </summary>
    /// <param name="options">Опции конфигурации Entity Framework</param>
    public YoudovezuDbContext(DbContextOptions<YoudovezuDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Пользователи платформы
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Поездки (запросы на поездки)
    /// </summary>
    public DbSet<Trip> Trips { get; set; }

    /// <summary>
    /// Настройка модели данных при создании контекста
    /// </summary>
    /// <param name="modelBuilder">Построитель модели данных</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка сущности User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TelegramId).IsUnique();
            entity.Property(e => e.TelegramId).IsRequired();
            entity.Property(e => e.SystemRole).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Индексы для быстрого поиска по возможностям
            entity.HasIndex(e => e.CanBePassenger);
            entity.HasIndex(e => e.CanBeDriver);
            entity.HasIndex(e => e.SystemRole);
        });

        // Настройка сущности Trip
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            
            // Связь с User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
