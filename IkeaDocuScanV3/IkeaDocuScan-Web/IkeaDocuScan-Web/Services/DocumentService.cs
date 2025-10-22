using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Exceptions;
using IkeaDocuScan.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using IkeaDocuScan_Web.Hubs;

namespace IkeaDocuScan_Web.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<DataUpdateHub> _hubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditTrailService _auditTrailService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        AppDbContext context,
        IHubContext<DataUpdateHub> hubContext,
        IHttpContextAccessor httpContextAccessor,
        IAuditTrailService auditTrailService,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
        _auditTrailService = auditTrailService;
        _logger = logger;
    }

    public async Task<List<DocumentDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all documents");

        // Load entities from database first (with navigation properties)
        var entities = await _context.Documents
            .Include(d => d.DocumentName)
            .Include(d => d.Dt)
            .Include(d => d.CounterParty)
            .ToListAsync();

        // Then map to DTOs in memory
        return entities.Select(d => MapToDto(d)).ToList();
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching document {DocumentId}", id);

        var entity = await _context.Documents
            .Include(d => d.DocumentName)
            .Include(d => d.Dt)
            .Include(d => d.CounterParty)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (entity == null)
            throw new DocumentNotFoundException(id);

        return MapToDto(entity);
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentDto dto)
    {
        _logger.LogInformation("Creating new document: {Name}", dto.Name);

        // Validate DocumentName exists if provided
        if (dto.DocumentNameId.HasValue)
        {
            var documentNameExists = await _context.DocumentNames.AnyAsync(c => c.Id == dto.DocumentNameId.Value);
            if (!documentNameExists)
            {
                throw new ValidationException("Invalid DocumentName", new Dictionary<string, string[]>
                {
                    { "DocumentNameId", new[] { $"DocumentName with ID {dto.DocumentNameId} does not exist" } }
                });
            }
        }

        var entity = new Document
        {
            Name = dto.Name,
            BarCode = await GenerateNextBarCode(),
            DtId = dto.DocumentTypeId,
            CounterPartyId = dto.CounterPartyId,
            DocumentNameId = dto.DocumentNameId,
            FileId = dto.FileId,
            DateOfContract = dto.DateOfContract,
            Comment = dto.Comment,
            ReceivingDate = dto.ReceivingDate,
            DispatchDate = dto.DispatchDate,
            Fax = dto.Fax,
            OriginalReceived = dto.OriginalReceived,
            ActionDate = dto.ActionDate,
            ActionDescription = dto.ActionDescription,
            ReminderGroup = dto.ReminderGroup,
            DocumentNo = dto.DocumentNo,
            AssociatedToPua = dto.AssociatedToPua,
            VersionNo = dto.VersionNo,
            AssociatedToAppendix = dto.AssociatedToAppendix,
            ValidUntil = dto.ValidUntil,
            CurrencyCode = dto.CurrencyCode,
            Amount = dto.Amount,
            Authorisation = dto.Authorisation,
            BankConfirmation = dto.BankConfirmation,
            TranslatedVersionReceived = dto.TranslatedVersionReceived,
            Confidential = dto.Confidential,
            ThirdParty = dto.ThirdParty,
            ThirdPartyId = dto.ThirdPartyId,
            SendingOutDate = dto.SendingOutDate,
            ForwardedToSignatoriesDate = dto.ForwardedToSignatoriesDate,
            CreatedBy = GetCurrentUsername(),
            CreatedOn = DateTime.Now
        };

        _context.Documents.Add(entity);
        await _context.SaveChangesAsync();

        // Log the registration action to audit trail
        await _auditTrailService.LogAsync(
            AuditAction.Register,
            entity.BarCode.ToString(),
            $"Document registered: {entity.Name}"
        );

        var result = await GetByIdAsync(entity.Id);

        // Notify all clients
        await _hubContext.Clients.All.SendAsync("DocumentCreated", result);

        return result!;
    }

    public async Task<DocumentDto> UpdateAsync(UpdateDocumentDto dto)
    {
        _logger.LogInformation("Updating document {DocumentId}", dto.Id);

        var entity = await _context.Documents.FindAsync(dto.Id);
        if (entity == null)
            throw new DocumentNotFoundException(dto.Id);

        // Validate DocumentName exists if provided
        if (dto.DocumentNameId.HasValue)
        {
            var documentNameExists = await _context.DocumentNames.AnyAsync(c => c.Id == dto.DocumentNameId.Value);
            if (!documentNameExists)
            {
                throw new ValidationException("Invalid DocumentName", new Dictionary<string, string[]>
                {
                    { "DocumentNameId", new[] { $"DocumentName with ID {dto.DocumentNameId} does not exist" } }
                });
            }
        }

        // Capture changes for audit details
        var changes = new List<string>();
        if (entity.Name != dto.Name)
            changes.Add($"Name: '{entity.Name}' -> '{dto.Name}'");
        if (entity.DtId != dto.DocumentTypeId)
            changes.Add("DocumentType changed");
        if (entity.CounterPartyId != dto.CounterPartyId)
            changes.Add("CounterParty changed");
        if (entity.Comment != dto.Comment)
            changes.Add("Comment updated");

        entity.Name = dto.Name;
        entity.DtId = dto.DocumentTypeId;
        entity.CounterPartyId = dto.CounterPartyId;
        entity.DocumentNameId = dto.DocumentNameId;
        entity.FileId = dto.FileId;
        entity.DateOfContract = dto.DateOfContract;
        entity.Comment = dto.Comment;
        entity.ReceivingDate = dto.ReceivingDate;
        entity.DispatchDate = dto.DispatchDate;
        entity.Fax = dto.Fax;
        entity.OriginalReceived = dto.OriginalReceived;
        entity.ActionDate = dto.ActionDate;
        entity.ActionDescription = dto.ActionDescription;
        entity.ReminderGroup = dto.ReminderGroup;
        entity.DocumentNo = dto.DocumentNo;
        entity.AssociatedToPua = dto.AssociatedToPua;
        entity.VersionNo = dto.VersionNo;
        entity.AssociatedToAppendix = dto.AssociatedToAppendix;
        entity.ValidUntil = dto.ValidUntil;
        entity.CurrencyCode = dto.CurrencyCode;
        entity.Amount = dto.Amount;
        entity.Authorisation = dto.Authorisation;
        entity.BankConfirmation = dto.BankConfirmation;
        entity.TranslatedVersionReceived = dto.TranslatedVersionReceived;
        entity.Confidential = dto.Confidential;
        entity.ThirdParty = dto.ThirdParty;
        entity.ThirdPartyId = dto.ThirdPartyId;
        entity.SendingOutDate = dto.SendingOutDate;
        entity.ForwardedToSignatoriesDate = dto.ForwardedToSignatoriesDate;
        entity.ModifiedBy = GetCurrentUsername();
        entity.ModifiedOn = DateTime.Now;

        await _context.SaveChangesAsync();

        // Log the edit action to audit trail
        var auditDetails = changes.Any()
            ? $"Document edited: {string.Join(", ", changes)}"
            : "Document edited";

        await _auditTrailService.LogAsync(
            AuditAction.Edit,
            entity.BarCode.ToString(),
            auditDetails
        );

        var result = await GetByIdAsync(entity.Id);

        // Notify all clients
        await _hubContext.Clients.All.SendAsync("DocumentUpdated", result);

        return result!;
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting document {DocumentId}", id);

        var entity = await _context.Documents.FindAsync(id);
        if (entity == null)
            throw new DocumentNotFoundException(id);

        var barCode = entity.BarCode.ToString();
        var documentName = entity.Name;

        _context.Documents.Remove(entity);
        await _context.SaveChangesAsync();

        // Log the delete action to audit trail
        await _auditTrailService.LogAsync(
            AuditAction.Delete,
            barCode,
            $"Document deleted: {documentName}"
        );

        // Notify all clients
        await _hubContext.Clients.All.SendAsync("DocumentDeleted", id);
    }

    /// <summary>
    /// Manual mapping from Entity to DTO
    /// </summary>
    private DocumentDto MapToDto(Document entity)
    {
        return new DocumentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            BarCode = entity.BarCode,

            // Related entities
            DocumentTypeId = entity.DtId,
            DocumentTypeName = entity.Dt?.DtName,
            CounterPartyId = entity.CounterPartyId,
            CounterPartyName = entity.CounterParty?.Name,
            DocumentNameId = entity.DocumentNameId,
            DocumentNameText = entity.DocumentName?.Name,
            FileId = entity.FileId,

            // Dates
            DateOfContract = entity.DateOfContract,
            ReceivingDate = entity.ReceivingDate,
            DispatchDate = entity.DispatchDate,
            ActionDate = entity.ActionDate,
            ValidUntil = entity.ValidUntil,
            SendingOutDate = entity.SendingOutDate,
            ForwardedToSignatoriesDate = entity.ForwardedToSignatoriesDate,

            // Text fields
            Comment = entity.Comment,
            ActionDescription = entity.ActionDescription,
            ReminderGroup = entity.ReminderGroup,
            DocumentNo = entity.DocumentNo,
            AssociatedToPua = entity.AssociatedToPua,
            VersionNo = entity.VersionNo,
            AssociatedToAppendix = entity.AssociatedToAppendix,
            Authorisation = entity.Authorisation,
            ThirdParty = entity.ThirdParty,
            ThirdPartyId = entity.ThirdPartyId,

            // Financial
            CurrencyCode = entity.CurrencyCode,
            Amount = entity.Amount,

            // Boolean flags
            Fax = entity.Fax,
            OriginalReceived = entity.OriginalReceived,
            BankConfirmation = entity.BankConfirmation,
            TranslatedVersionReceived = entity.TranslatedVersionReceived,
            Confidential = entity.Confidential,

            // Audit fields
            CreatedOn = entity.CreatedOn,
            CreatedBy = entity.CreatedBy,
            ModifiedOn = entity.ModifiedOn,
            ModifiedBy = entity.ModifiedBy
        };
    }

    private string GetCurrentUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    }

    /// <summary>
    /// Generate next available BarCode
    /// </summary>
    private async Task<int> GenerateNextBarCode()
    {
        var maxBarCode = await _context.Documents.MaxAsync(d => (int?)d.BarCode) ?? 0;
        return maxBarCode + 1;
    }
}
