using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs.DocumentNames;
using IkeaDocuScan.Shared.Exceptions;
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

    /// <summary>
    /// Create a new document name (SuperUser only)
    /// </summary>
    public async Task<DocumentNameDto> CreateAsync(CreateDocumentNameDto createDto)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} creating new document name: {Name}",
            currentUser.AccountName, createDto.Name);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if document name already exists for this document type
        var exists = await context.DocumentNames
            .AnyAsync(dn => dn.Name == createDto.Name && dn.DocumentTypeId == createDto.DocumentTypeId);

        if (exists)
        {
            throw new ValidationException(
                $"Document name '{createDto.Name}' already exists for this document type");
        }

        var entity = new DocumentName
        {
            Name = createDto.Name,
            DocumentTypeId = createDto.DocumentTypeId
        };

        context.DocumentNames.Add(entity);
        await context.SaveChangesAsync();

        _logger.LogInformation("Document name created with ID {Id} by user {User}",
            entity.Id, currentUser.AccountName);

        // Load the created entity with navigation properties
        var createdDto = await context.DocumentNames
            .AsNoTracking()
            .Include(dn => dn.DocumentType)
            .Where(dn => dn.Id == entity.Id)
            .Select(dn => new DocumentNameDto
            {
                Id = dn.Id,
                Name = dn.Name ?? string.Empty,
                DocumentTypeId = dn.DocumentTypeId,
                DocumentTypeName = dn.DocumentType != null ? dn.DocumentType.DtName : null
            })
            .FirstAsync();

        return createdDto;
    }

    /// <summary>
    /// Update an existing document name (SuperUser only)
    /// </summary>
    public async Task<DocumentNameDto> UpdateAsync(UpdateDocumentNameDto updateDto)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} updating document name ID {Id}",
            currentUser.AccountName, updateDto.Id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.DocumentNames
            .FirstOrDefaultAsync(dn => dn.Id == updateDto.Id);

        if (entity == null)
        {
            throw new ValidationException($"Document name with ID {updateDto.Id} not found");
        }

        // Check if the new name conflicts with another document name for the same type
        var conflictExists = await context.DocumentNames
            .AnyAsync(dn => dn.Id != updateDto.Id &&
                           dn.Name == updateDto.Name &&
                           dn.DocumentTypeId == updateDto.DocumentTypeId);

        if (conflictExists)
        {
            throw new ValidationException(
                $"Document name '{updateDto.Name}' already exists for this document type");
        }

        entity.Name = updateDto.Name;
        entity.DocumentTypeId = updateDto.DocumentTypeId;

        await context.SaveChangesAsync();

        _logger.LogInformation("Document name ID {Id} updated by user {User}",
            updateDto.Id, currentUser.AccountName);

        // Load the updated entity with navigation properties
        var updatedDto = await context.DocumentNames
            .AsNoTracking()
            .Include(dn => dn.DocumentType)
            .Where(dn => dn.Id == entity.Id)
            .Select(dn => new DocumentNameDto
            {
                Id = dn.Id,
                Name = dn.Name ?? string.Empty,
                DocumentTypeId = dn.DocumentTypeId,
                DocumentTypeName = dn.DocumentType != null ? dn.DocumentType.DtName : null
            })
            .FirstAsync();

        return updatedDto;
    }

    /// <summary>
    /// Delete a document name (SuperUser only)
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        _logger.LogInformation("User {User} deleting document name ID {Id}",
            currentUser.AccountName, id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.DocumentNames
            .Include(dn => dn.Documents)
            .FirstOrDefaultAsync(dn => dn.Id == id);

        if (entity == null)
        {
            throw new ValidationException($"Document name with ID {id} not found");
        }

        // Check if the document name is in use
        if (entity.Documents.Any())
        {
            throw new ValidationException(
                $"Cannot delete document name '{entity.Name}' because it is used by {entity.Documents.Count} document(s)");
        }

        context.DocumentNames.Remove(entity);
        await context.SaveChangesAsync();

        _logger.LogInformation("Document name ID {Id} ('{Name}') deleted by user {User}",
            id, entity.Name, currentUser.AccountName);
    }
}
