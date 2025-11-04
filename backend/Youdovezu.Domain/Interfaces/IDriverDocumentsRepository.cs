using Youdovezu.Domain.Entities;

namespace Youdovezu.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория для работы с документами водителя
/// </summary>
public interface IDriverDocumentsRepository
{
    /// <summary>
    /// Получить документы по ID
    /// </summary>
    Task<DriverDocuments?> GetByIdAsync(long id);

    /// <summary>
    /// Получить документы пользователя
    /// </summary>
    Task<DriverDocuments?> GetByUserIdAsync(long userId);

    /// <summary>
    /// Создать новые документы
    /// </summary>
    Task<DriverDocuments> CreateAsync(DriverDocuments documents);

    /// <summary>
    /// Обновить документы
    /// </summary>
    Task<DriverDocuments> UpdateAsync(DriverDocuments documents);

    /// <summary>
    /// Получить все документы на проверке
    /// </summary>
    Task<List<DriverDocuments>> GetPendingDocumentsAsync();
}

