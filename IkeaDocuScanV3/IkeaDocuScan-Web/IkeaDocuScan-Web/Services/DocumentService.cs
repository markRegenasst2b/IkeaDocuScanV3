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
    private readonly IEmailService _emailService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        AppDbContext context,
        IHubContext<DataUpdateHub> hubContext,
        IHttpContextAccessor httpContextAccessor,
        IAuditTrailService auditTrailService,
        IEmailService emailService,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
        _auditTrailService = auditTrailService;
        _emailService = emailService;
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

    public async Task<DocumentDto?> GetByBarCodeAsync(string barCode)
    {
        _logger.LogInformation("Fetching document by BarCode {BarCode}", barCode);

        if (!int.TryParse(barCode, out int barCodeInt))
        {
            _logger.LogWarning("Invalid BarCode format: {BarCode}", barCode);
            return null;
        }

        var entity = await _context.Documents
            .Include(d => d.DocumentName)
            .Include(d => d.Dt)
            .Include(d => d.CounterParty)
            .FirstOrDefaultAsync(d => d.BarCode == barCodeInt);

        if (entity == null)
        {
            _logger.LogWarning("Document not found with BarCode {BarCode}", barCode);
            return null;
        }

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

        // Determine BarCode: use provided BarCode if available, otherwise generate next available
        int barCode;
        if (!string.IsNullOrWhiteSpace(dto.BarCode) && int.TryParse(dto.BarCode, out int providedBarCode))
        {
            // Use provided BarCode (from Check-in mode where it comes from filename)
            barCode = providedBarCode;
            _logger.LogInformation("Using provided BarCode: {BarCode}", barCode);

            // Validate that this BarCode doesn't already exist
            var existingDocument = await _context.Documents.AnyAsync(d => d.BarCode == barCode);
            if (existingDocument)
            {
                throw new ValidationException($"Document with BarCode {barCode} already exists");
            }
        }
        else
        {
            // Generate next BarCode (for Register mode)
            barCode = await GenerateNextBarCode();
            _logger.LogInformation("Generated new BarCode: {BarCode}", barCode);
        }

        // Create DocumentFile if file bytes are provided (Check-in mode)
        int? fileId = dto.FileId;
        if (dto.FileBytes != null && dto.FileBytes.Length > 0 && !string.IsNullOrEmpty(dto.FileName))
        {
            _logger.LogInformation("Creating DocumentFile for {FileName} with {FileSize} bytes", dto.FileName, dto.FileBytes.Length);

            var documentFile = new DocumentFile
            {
                FileName = dto.FileName,
                FileType = dto.FileType ?? Path.GetExtension(dto.FileName).TrimStart('.'),
                Bytes = dto.FileBytes
            };

            _context.DocumentFiles.Add(documentFile);
            await _context.SaveChangesAsync(); // Save to get the FileId

            fileId = documentFile.Id;
            _logger.LogInformation("DocumentFile created with ID {FileId}", fileId);
        }

        var entity = new Document
        {
            Name = dto.Name,
            BarCode = barCode,
            DtId = dto.DocumentTypeId,
            CounterPartyId = dto.CounterPartyId,
            DocumentNameId = dto.DocumentNameId,
            FileId = fileId,
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

        // Create DocumentFile if file bytes are provided (for attaching files to existing documents)
        if (dto.FileBytes != null && dto.FileBytes.Length > 0 && !string.IsNullOrEmpty(dto.FileName))
        {
            // Only allow file attachment if document doesn't already have a file
            if (entity.FileId.HasValue)
            {
                throw new ValidationException($"Document already has a file attached (FileId: {entity.FileId}). Cannot attach another file.");
            }

            _logger.LogInformation("Creating DocumentFile for {FileName} with {FileSize} bytes", dto.FileName, dto.FileBytes.Length);

            var documentFile = new DocumentFile
            {
                FileName = dto.FileName,
                FileType = dto.FileType ?? Path.GetExtension(dto.FileName).TrimStart('.'),
                Bytes = dto.FileBytes
            };

            _context.DocumentFiles.Add(documentFile);
            await _context.SaveChangesAsync(); // Save to get the FileId

            entity.FileId = documentFile.Id;
            _logger.LogInformation("DocumentFile created with ID {FileId} and attached to document {DocumentId}", documentFile.Id, entity.Id);
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
        if (dto.FileBytes != null && dto.FileBytes.Length > 0)
            changes.Add($"File attached: {dto.FileName}");

        entity.Name = dto.Name;
        entity.DtId = dto.DocumentTypeId;
        entity.CounterPartyId = dto.CounterPartyId;
        entity.DocumentNameId = dto.DocumentNameId;
        // FileId is set above when creating DocumentFile from FileBytes, or can be set explicitly via dto.FileId
        if (dto.FileId.HasValue && dto.FileBytes == null)
        {
            entity.FileId = dto.FileId;
        }
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

    /// <summary>
    /// Send document link to recipient via email
    /// </summary>
    /// <param name="barCode">Document bar code</param>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="message">Optional message to include</param>
    public async Task SendDocumentLinkAsync(string barCode, string recipientEmail, string? message = null)
    {
        _logger.LogInformation("Sending document link for {BarCode} to {RecipientEmail}", barCode, recipientEmail);

        var document = await GetByBarCodeAsync(barCode);
        if (document == null)
        {
            throw new DocumentNotFoundException($"Document with bar code {barCode} not found");
        }

        // Generate document link (adjust URL as needed)
        var documentLink = $"{GetBaseUrl()}/documents/{barCode}";

        await _emailService.SendDocumentLinkAsync(recipientEmail, barCode, documentLink, message);
        await _auditTrailService.LogAsync(AuditAction.SendLink, barCode, $"Link sent to {recipientEmail}");

        _logger.LogInformation("Document link sent successfully for {BarCode}", barCode);
    }

    /// <summary>
    /// Send document as attachment via email
    /// </summary>
    /// <param name="barCode">Document bar code</param>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="fileData">Document file data</param>
    /// <param name="fileName">File name</param>
    /// <param name="message">Optional message to include</param>
    public async Task SendDocumentAttachmentAsync(
        string barCode,
        string recipientEmail,
        byte[] fileData,
        string fileName,
        string? message = null)
    {
        _logger.LogInformation("Sending document attachment for {BarCode} to {RecipientEmail}", barCode, recipientEmail);

        var document = await GetByBarCodeAsync(barCode);
        if (document == null)
        {
            throw new DocumentNotFoundException($"Document with bar code {barCode} not found");
        }

        await _emailService.SendDocumentAttachmentAsync(
            recipientEmail,
            barCode,
            fileData,
            fileName,
            message);

        await _auditTrailService.LogAsync(AuditAction.SendAttachment, barCode, $"Attachment sent to {recipientEmail}");

        _logger.LogInformation("Document attachment sent successfully for {BarCode}", barCode);
    }

    /// <summary>
    /// Send multiple document links via email
    /// </summary>
    /// <param name="barCodes">Collection of document bar codes</param>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="message">Optional message to include</param>
    public async Task SendDocumentLinksAsync(
        IEnumerable<string> barCodes,
        string recipientEmail,
        string? message = null)
    {
        var barCodeList = barCodes.ToList();
        _logger.LogInformation("Sending {Count} document links to {RecipientEmail}", barCodeList.Count, recipientEmail);

        var documents = new List<(string BarCode, string Link)>();
        var baseUrl = GetBaseUrl();

        foreach (var barCode in barCodeList)
        {
            var document = await GetByBarCodeAsync(barCode);
            if (document != null)
            {
                var link = $"{baseUrl}/documents/{barCode}";
                documents.Add((barCode, link));
            }
        }

        if (documents.Count == 0)
        {
            throw new DocumentNotFoundException("No valid documents found for the provided bar codes");
        }

        await _emailService.SendDocumentLinksAsync(recipientEmail, documents, message);
        await _auditTrailService.LogBatchAsync(
            AuditAction.SendLinks,
            documents.Select(d => d.BarCode),
            $"Links sent to {recipientEmail}");

        _logger.LogInformation("{Count} document links sent successfully", documents.Count);
    }

    /// <summary>
    /// Send multiple documents as attachments via email
    /// </summary>
    /// <param name="documentsToSend">Collection of documents with bar codes, file data, and file names</param>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="message">Optional message to include</param>
    public async Task SendDocumentAttachmentsAsync(
        IEnumerable<(string BarCode, byte[] FileData, string FileName)> documentsToSend,
        string recipientEmail,
        string? message = null)
    {
        var documentList = documentsToSend.ToList();
        _logger.LogInformation("Sending {Count} document attachments to {RecipientEmail}",
            documentList.Count, recipientEmail);

        var validDocuments = new List<(string BarCode, byte[] Data, string FileName)>();

        foreach (var (barCode, fileData, fileName) in documentList)
        {
            var document = await GetByBarCodeAsync(barCode);
            if (document != null && fileData?.Length > 0)
            {
                validDocuments.Add((barCode, fileData, fileName));
            }
        }

        if (validDocuments.Count == 0)
        {
            throw new DocumentNotFoundException("No valid documents found for the provided bar codes");
        }

        await _emailService.SendDocumentAttachmentsAsync(recipientEmail, validDocuments, message);
        await _auditTrailService.LogBatchAsync(
            AuditAction.SendAttachments,
            validDocuments.Select(d => d.BarCode),
            $"Attachments sent to {recipientEmail}");

        _logger.LogInformation("{Count} document attachments sent successfully", validDocuments.Count);
    }

    /// <summary>
    /// Get base URL for generating document links
    /// </summary>
    private string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            return $"{request.Scheme}://{request.Host}";
        }

        // Fallback - should be configured in production
        return "https://docuscan.company.com";
    }
}
