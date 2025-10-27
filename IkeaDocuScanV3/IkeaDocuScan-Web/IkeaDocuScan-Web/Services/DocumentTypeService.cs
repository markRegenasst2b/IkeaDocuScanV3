using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.DocumentTypes;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for DocumentType operations with caching
/// </summary>
public class DocumentTypeService : IDocumentTypeService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<DocumentTypeService> _logger;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "DocumentTypes_All";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public DocumentTypeService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<DocumentTypeService> logger,
        IMemoryCache cache)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<DocumentTypeDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            _logger.LogInformation("Fetching all enabled document types from database (cache miss)");

            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            entry.SetPriority(CacheItemPriority.High);

            await using var context = await _contextFactory.CreateDbContextAsync();
            var documentTypes = await context.DocumentTypes
                .AsNoTracking()
                .Where(dt => dt.IsEnabled)
                .OrderBy(dt => dt.DtName)
                .ToListAsync();

            var result = documentTypes.Select(MapToDto).ToList();
            _logger.LogInformation("Cached {Count} document types for {Duration}", result.Count, CacheDuration);

            return result;
        }) ?? new List<DocumentTypeDto>();
    }

    public async Task<DocumentTypeDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching document type with ID: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var documentType = await context.DocumentTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(dt => dt.DtId == id);

        if (documentType == null)
        {
            _logger.LogWarning("Document type with ID {Id} not found", id);
            return null;
        }

        return MapToDto(documentType);
    }

    /// <summary>
    /// Clear the document types cache. Call this when document types are added, updated, or deleted.
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Document types cache cleared");
    }

    private static DocumentTypeDto MapToDto(IkeaDocuScan.Infrastructure.Entities.DocumentType entity)
    {
        return new DocumentTypeDto
        {
            DtId = entity.DtId,
            DtName = entity.DtName,
            IsEnabled = entity.IsEnabled
        };
    }
}
