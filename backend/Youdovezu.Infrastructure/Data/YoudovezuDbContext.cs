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
            entity.Property(e => e.Role).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
