using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs.DocumentTypes;
using IkeaDocuScan.Shared.Exceptions;
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

    public async Task<List<DocumentTypeDto>> GetAllIncludingDisabledAsync()
    {
        _logger.LogInformation("Fetching all document types including disabled ones");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var documentTypes = await context.DocumentTypes
            .AsNoTracking()
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

    public async Task<DocumentTypeDto> CreateAsync(CreateDocumentTypeDto dto)
    {
        _logger.LogInformation("Creating document type: {Name}", dto.DtName);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if document type name already exists
        var exists = await context.DocumentTypes
            .AnyAsync(dt => dt.DtName == dto.DtName);

        if (exists)
        {
            throw new ValidationException($"Document type with name '{dto.DtName}' already exists");
        }

        var entity = new DocumentType
        {
            DtName = dto.DtName,
            IsEnabled = dto.IsEnabled,
            BarCode = dto.BarCode,
            CounterParty = dto.CounterParty,
            DateOfContract = dto.DateOfContract,
            Comment = dto.Comment,
            ReceivingDate = dto.ReceivingDate,
            DispatchDate = dto.DispatchDate,
            Fax = dto.Fax,
            OriginalReceived = dto.OriginalReceived,
            DocumentNo = dto.DocumentNo,
            AssociatedToPua = dto.AssociatedToPua,
            VersionNo = dto.VersionNo,
            AssociatedToAppendix = dto.AssociatedToAppendix,
            ValidUntil = dto.ValidUntil,
            Currency = dto.Currency,
            Amount = dto.Amount,
            Authorisation = dto.Authorisation,
            BankConfirmation = dto.BankConfirmation,
            TranslatedVersionReceived = dto.TranslatedVersionReceived,
            ActionDate = dto.ActionDate,
            ActionDescription = dto.ActionDescription,
            ReminderGroup = dto.ReminderGroup,
            Confidential = dto.Confidential,
            CounterPartyAlpha = dto.CounterPartyAlpha,
            SendingOutDate = dto.SendingOutDate,
            ForwardedToSignatoriesDate = dto.ForwardedToSignatoriesDate,
            IsAppendix = dto.IsAppendix
        };

        context.DocumentTypes.Add(entity);
        await context.SaveChangesAsync();

        ClearCache();
        _logger.LogInformation("Document type created with ID: {Id}", entity.DtId);

        return MapToDto(entity);
    }

    public async Task<DocumentTypeDto> UpdateAsync(int id, UpdateDocumentTypeDto dto)
    {
        _logger.LogInformation("Updating document type ID: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.DocumentTypes
            .FirstOrDefaultAsync(dt => dt.DtId == id);

        if (entity == null)
        {
            throw new ValidationException($"Document type with ID {id} not found");
        }

        // Check if another document type has the same name
        var duplicateName = await context.DocumentTypes
            .AnyAsync(dt => dt.DtName == dto.DtName && dt.DtId != id);

        if (duplicateName)
        {
            throw new ValidationException($"Document type with name '{dto.DtName}' already exists");
        }

        entity.DtName = dto.DtName;
        entity.IsEnabled = dto.IsEnabled;
        entity.BarCode = dto.BarCode;
        entity.CounterParty = dto.CounterParty;
        entity.DateOfContract = dto.DateOfContract;
        entity.Comment = dto.Comment;
        entity.ReceivingDate = dto.ReceivingDate;
        entity.DispatchDate = dto.DispatchDate;
        entity.Fax = dto.Fax;
        entity.OriginalReceived = dto.OriginalReceived;
        entity.DocumentNo = dto.DocumentNo;
        entity.AssociatedToPua = dto.AssociatedToPua;
        entity.VersionNo = dto.VersionNo;
        entity.AssociatedToAppendix = dto.AssociatedToAppendix;
        entity.ValidUntil = dto.ValidUntil;
        entity.Currency = dto.Currency;
        entity.Amount = dto.Amount;
        entity.Authorisation = dto.Authorisation;
        entity.BankConfirmation = dto.BankConfirmation;
        entity.TranslatedVersionReceived = dto.TranslatedVersionReceived;
        entity.ActionDate = dto.ActionDate;
        entity.ActionDescription = dto.ActionDescription;
        entity.ReminderGroup = dto.ReminderGroup;
        entity.Confidential = dto.Confidential;
        entity.CounterPartyAlpha = dto.CounterPartyAlpha;
        entity.SendingOutDate = dto.SendingOutDate;
        entity.ForwardedToSignatoriesDate = dto.ForwardedToSignatoriesDate;
        entity.IsAppendix = dto.IsAppendix;

        await context.SaveChangesAsync();

        ClearCache();
        _logger.LogInformation("Document type ID {Id} updated successfully", id);

        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting document type ID: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if document type is in use
        var documentCount = await context.Documents
            .Where(d => d.DtId == id)
            .CountAsync();

        var documentNameCount = await context.DocumentNames
            .Where(dn => dn.DocumentTypeId == id)
            .CountAsync();

        var userPermissionCount = await context.UserPermissions
            .Where(up => up.DocumentTypeId == id)
            .CountAsync();

        var totalUsage = documentCount + documentNameCount + userPermissionCount;

        if (totalUsage > 0)
        {
            var usageDetails = new List<string>();
            if (documentCount > 0)
                usageDetails.Add($"{documentCount} document{(documentCount != 1 ? "s" : "")}");
            if (documentNameCount > 0)
                usageDetails.Add($"{documentNameCount} document name{(documentNameCount != 1 ? "s" : "")}");
            if (userPermissionCount > 0)
                usageDetails.Add($"{userPermissionCount} user permission{(userPermissionCount != 1 ? "s" : "")}");

            throw new ValidationException(
                $"Cannot delete document type. It is currently used by {string.Join(", ", usageDetails)}.");
        }

        var entity = await context.DocumentTypes
            .FirstOrDefaultAsync(dt => dt.DtId == id);

        if (entity == null)
        {
            throw new ValidationException($"Document type with ID {id} not found");
        }

        context.DocumentTypes.Remove(entity);
        await context.SaveChangesAsync();

        ClearCache();
        _logger.LogInformation("Document type ID {Id} deleted successfully", id);
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        _logger.LogInformation("Checking if document type ID {Id} is in use", id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var isInUse = await context.Documents.AnyAsync(d => d.DtId == id) ||
                      await context.DocumentNames.AnyAsync(dn => dn.DocumentTypeId == id) ||
                      await context.UserPermissions.AnyAsync(up => up.DocumentTypeId == id);

        return isInUse;
    }

    public async Task<(int documentCount, int documentNameCount, int userPermissionCount)> GetUsageCountAsync(int id)
    {
        _logger.LogInformation("Getting usage count for document type ID {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync();

        var documentCount = await context.Documents
            .Where(d => d.DtId == id)
            .CountAsync();

        var documentNameCount = await context.DocumentNames
            .Where(dn => dn.DocumentTypeId == id)
            .CountAsync();

        var userPermissionCount = await context.UserPermissions
            .Where(up => up.DocumentTypeId == id)
            .CountAsync();

        return (documentCount, documentNameCount, userPermissionCount);
    }

    /// <summary>
    /// Clear the document types cache. Call this when document types are added, updated, or deleted.
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Document types cache cleared");
    }

    private static DocumentTypeDto MapToDto(DocumentType entity)
    {
        return new DocumentTypeDto
        {
            DtId = entity.DtId,
            DtName = entity.DtName,
            IsEnabled = entity.IsEnabled,
            BarCode = entity.BarCode,
            CounterParty = entity.CounterParty,
            DateOfContract = entity.DateOfContract,
            Comment = entity.Comment,
            ReceivingDate = entity.ReceivingDate,
            DispatchDate = entity.DispatchDate,
            Fax = entity.Fax,
            OriginalReceived = entity.OriginalReceived,
            DocumentNo = entity.DocumentNo,
            AssociatedToPua = entity.AssociatedToPua,
            VersionNo = entity.VersionNo,
            AssociatedToAppendix = entity.AssociatedToAppendix,
            ValidUntil = entity.ValidUntil,
            Currency = entity.Currency,
            Amount = entity.Amount,
            Authorisation = entity.Authorisation,
            BankConfirmation = entity.BankConfirmation,
            TranslatedVersionReceived = entity.TranslatedVersionReceived,
            ActionDate = entity.ActionDate,
            ActionDescription = entity.ActionDescription,
            ReminderGroup = entity.ReminderGroup,
            Confidential = entity.Confidential,
            CounterPartyAlpha = entity.CounterPartyAlpha,
            SendingOutDate = entity.SendingOutDate,
            ForwardedToSignatoriesDate = entity.ForwardedToSignatoriesDate,
            IsAppendix = entity.IsAppendix
        };
    }
}
