using Youdovezu.Domain.Entities;

namespace Youdovezu.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с населенными пунктами
/// </summary>
public interface ISettlementRepository
{
    /// <summary>
    /// Получить все активные населенные пункты
    /// </summary>
    /// <returns>Список активных населенных пунктов</returns>
    Task<List<Settlement>> GetAllActiveAsync();

    /// <summary>
    /// Получить населенный пункт по ID
    /// </summary>
    /// <param name="id">ID населенного пункта</param>
    /// <returns>Населенный пункт или null</returns>
    Task<Settlement?> GetByIdAsync(long id);

    /// <summary>
    /// Получить населенный пункт по названию
    /// </summary>
    /// <param name="name">Название населенного пункта</param>
    /// <returns>Населенный пункт или null</returns>
    Task<Settlement?> GetByNameAsync(string name);

    /// <summary>
    /// Получить населенные пункты с количеством активных поездок для каждого
    /// </summary>
    /// <returns>Список населенных пунктов с количеством поездок</returns>
    Task<List<SettlementWithTripsCount>> GetSettlementsWithTripsCountAsync();

    /// <summary>
    /// Получить уникальные населенные пункты из существующих поездок
    /// </summary>
    /// <returns>Список уникальных названий населенных пунктов из поездок</returns>
    Task<List<string>> GetSettlementsFromTripsAsync();

    /// <summary>
    /// Создать новый населенный пункт
    /// </summary>
    /// <param name="settlement">Населенный пункт</param>
    /// <returns>Созданный населенный пункт</returns>
    Task<Settlement> CreateAsync(Settlement settlement);

    /// <summary>
    /// Обновить населенный пункт
    /// </summary>
    /// <param name="settlement">Населенный пункт</param>
    /// <returns>Обновленный населенный пункт</returns>
    Task<Settlement> UpdateAsync(Settlement settlement);
}

/// <summary>
/// Населенный пункт с количеством активных поездок
/// </summary>
public class SettlementWithTripsCount
{
    /// <summary>
    /// Населенный пункт
    /// </summary>
    public Settlement Settlement { get; set; } = null!;

    /// <summary>
    /// Количество активных поездок в этом населенном пункте
    /// </summary>
    public int TripsCount { get; set; }
}


