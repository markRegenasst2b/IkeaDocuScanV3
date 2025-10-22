using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.DocumentTypes;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for DocumentType operations
/// </summary>
public class DocumentTypeService : IDocumentTypeService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<DocumentTypeService> _logger;

    public DocumentTypeService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<DocumentTypeService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<DocumentTypeDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all enabled document types");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var documentTypes = await context.DocumentTypes
            .AsNoTracking()
            .Where(dt => dt.IsEnabled)
            .OrderBy(dt => dt.DtName)
            .ToListAsync();

        return documentTypes.Select(MapToDto).ToList();
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
