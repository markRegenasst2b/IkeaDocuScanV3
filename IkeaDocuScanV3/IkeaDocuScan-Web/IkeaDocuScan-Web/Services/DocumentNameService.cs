using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.DocumentNames;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for managing document names.
/// Document names are predefined names that can be associated with documents,
/// typically filtered by document type.
/// </summary>
public class DocumentNameService : IDocumentNameService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<DocumentNameService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public DocumentNameService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<DocumentNameService> logger,
        ICurrentUserService currentUserService)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all document names
    /// </summary>
    public async Task<List<DocumentNameDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all document names");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var documentNames = await context.DocumentNames
            .AsNoTracking()
            .Include(dn => dn.DocumentType)
            .OrderBy(dn => dn.Name)
            .Select(dn => new DocumentNameDto
            {
                Id = dn.Id,
                Name = dn.Name ?? string.Empty,
                DocumentTypeId = dn.DocumentTypeId,
                DocumentTypeName = dn.DocumentType != null ? dn.DocumentType.DtName : null
            })
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} document names", documentNames.Count);
        return documentNames;
    }

    /// <summary>
    /// Get document names filtered by document type ID
    /// </summary>
    public async Task<List<DocumentNameDto>> GetByDocumentTypeIdAsync(int documentTypeId)
    {
        _logger.LogInformation("Retrieving document names for DocumentTypeId {DocumentTypeId}", documentTypeId);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var documentNames = await context.DocumentNames
            .AsNoTracking()
            .Where(dn => dn.DocumentTypeId == documentTypeId)
            .Include(dn => dn.DocumentType)
            .OrderBy(dn => dn.Name)
            .Select(dn => new DocumentNameDto
            {
                Id = dn.Id,
                Name = dn.Name ?? string.Empty,
                DocumentTypeId = dn.DocumentTypeId,
                DocumentTypeName = dn.DocumentType != null ? dn.DocumentType.DtName : null
            })
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} document names for DocumentTypeId {DocumentTypeId}",
            documentNames.Count, documentTypeId);

        return documentNames;
    }

    /// <summary>
    /// Get a specific document name by ID
    /// </summary>
    public async Task<DocumentNameDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving document name with ID {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var documentName = await context.DocumentNames
            .AsNoTracking()
            .Include(dn => dn.DocumentType)
            .Where(dn => dn.Id == id)
            .Select(dn => new DocumentNameDto
            {
                Id = dn.Id,
                Name = dn.Name ?? string.Empty,
                DocumentTypeId = dn.DocumentTypeId,
                DocumentTypeName = dn.DocumentType != null ? dn.DocumentType.DtName : null
            })
            .FirstOrDefaultAsync();

        if (documentName == null)
        {
            _logger.LogWarning("Document name with ID {Id} not found", id);
        }

        return documentName;
    }
}
