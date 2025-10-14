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
    /// Настройка модели данных при создании контекста
    /// </summary>
    /// <param name="modelBuilder">Построитель модели данных</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Здесь будут настройки для сущностей базы данных
        // Пока что оставляем пустым, так как сущности еще не созданы
    }

    // DbSet свойства будут добавлены по мере создания сущностей
    // Например:
    // public DbSet<TelegramMessage> TelegramMessages { get; set; }
}
