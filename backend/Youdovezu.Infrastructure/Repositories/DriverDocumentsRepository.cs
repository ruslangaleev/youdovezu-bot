using Microsoft.EntityFrameworkCore;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;
using Youdovezu.Infrastructure.Data;

namespace Youdovezu.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с документами водителя
/// </summary>
public class DriverDocumentsRepository : IDriverDocumentsRepository
{
    private readonly YoudovezuDbContext _context;

    public DriverDocumentsRepository(YoudovezuDbContext context)
    {
        _context = context;
    }

    public async Task<DriverDocuments?> GetByIdAsync(long id)
    {
        return await _context.DriverDocuments
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DriverDocuments?> GetByUserIdAsync(long userId)
    {
        return await _context.DriverDocuments
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<DriverDocuments> CreateAsync(DriverDocuments documents)
    {
        documents.CreatedAt = DateTime.UtcNow;
        documents.UpdatedAt = DateTime.UtcNow;

        _context.DriverDocuments.Add(documents);
        await _context.SaveChangesAsync();

        return documents;
    }

    public async Task<DriverDocuments> UpdateAsync(DriverDocuments documents)
    {
        documents.UpdatedAt = DateTime.UtcNow;

        _context.DriverDocuments.Update(documents);
        await _context.SaveChangesAsync();

        return documents;
    }

    public async Task<List<DriverDocuments>> GetPendingDocumentsAsync()
    {
        return await _context.DriverDocuments
            .Include(d => d.User)
            .Where(d => d.Status == DocumentVerificationStatus.Pending || 
                       d.Status == DocumentVerificationStatus.UnderReview)
            .OrderBy(d => d.SubmittedAt)
            .ToListAsync();
    }
}

