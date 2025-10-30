using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.DTOs.Documents;
using IkeaDocuScan.Shared.Interfaces;
using IkeaDocuScan.Shared.Exceptions;
using IkeaDocuScan.Shared.Enums;
using IkeaDocuScan.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
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
    private readonly DocumentSearchOptions _searchOptions;

    public DocumentService(
        AppDbContext context,
        IHubContext<DataUpdateHub> hubContext,
        IHttpContextAccessor httpContextAccessor,
        IAuditTrailService auditTrailService,
        IEmailService emailService,
        ILogger<DocumentService> logger,
        IOptions<DocumentSearchOptions> searchOptions)
    {
        _context = context;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
        _auditTrailService = auditTrailService;
        _emailService = emailService;
        _logger = logger;
        _searchOptions = searchOptions.Value;
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

    public async Task<DocumentSearchResultDto> SearchAsync(DocumentSearchRequestDto request)
    {
        _logger.LogInformation("Searching documents with filters");

        // Start with base query including all necessary navigation properties
        IQueryable<Document> query = _context.Documents
            .Include(d => d.Dt)
            .Include(d => d.DocumentName)
            .Include(d => d.CounterParty)
                .ThenInclude(cp => cp!.CountryNavigation)
            .AsQueryable();

        // Apply filters
        query = ApplySearchFilters(query, request);

        // Apply sorting BEFORE limiting results
        query = ApplySorting(query, request.SortColumn, request.SortDirection);

        // Get max results limit from configuration
        var maxResults = _searchOptions.MaxResults;
        _logger.LogDebug("Applying max results limit: {MaxResults}", maxResults);

        // Count total matches (before pagination, limited by max results)
        var totalQuery = query.Take(maxResults);
        var totalCount = await totalQuery.CountAsync();
        var maxLimitReached = totalCount >= maxResults;

        // Apply pagination
        var pagedQuery = totalQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        // Execute query and map to DTOs
        var documents = await pagedQuery.ToListAsync();
        var items = documents.Select(MapToSearchItemDto).ToList();

        // Calculate pagination metadata
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new DocumentSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            MaxLimitReached = maxLimitReached,
            MaxLimit = maxResults
        };
    }

    private IQueryable<Document> ApplySearchFilters(IQueryable<Document> query, DocumentSearchRequestDto request)
    {
        // Full-text search in PDF content (iFilter)
        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            // TODO: Implement full-text search using CONTAINS or FREETEXT
            // This requires full-text indexing on DocumentFile.Bytes
            // For now, we'll skip this filter with a warning
            _logger.LogWarning("Full-text search in PDF content not yet implemented");
        }

        // Barcode filter (OR logic)
        var barcodes = request.GetBarcodeList();
        if (barcodes.Any())
        {
            query = query.Where(d => barcodes.Contains(d.BarCode));
        }

        // Document Types (multi-select, OR logic)
        if (request.DocumentTypeIds.Any())
        {
            query = query.Where(d => d.DtId.HasValue && request.DocumentTypeIds.Contains(d.DtId.Value));
        }

        // Document Name
        if (request.DocumentNameId.HasValue)
        {
            query = query.Where(d => d.DocumentNameId == request.DocumentNameId.Value);
        }

        // Document Number (contains)
        if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            query = query.Where(d => d.DocumentNo != null && d.DocumentNo.Contains(request.DocumentNumber));
        }

        // Version No (contains)
        if (!string.IsNullOrWhiteSpace(request.VersionNo))
        {
            query = query.Where(d => d.VersionNo != null && d.VersionNo.Contains(request.VersionNo));
        }

        // Associated to PUA (contains)
        if (!string.IsNullOrWhiteSpace(request.AssociatedToPua))
        {
            query = query.Where(d => d.AssociatedToPua != null && d.AssociatedToPua.Contains(request.AssociatedToPua));
        }

        // Associated to Appendix (contains)
        if (!string.IsNullOrWhiteSpace(request.AssociatedToAppendix))
        {
            query = query.Where(d => d.AssociatedToAppendix != null && d.AssociatedToAppendix.Contains(request.AssociatedToAppendix));
        }

        // Counterparty Name (free-text search in counterparty and third-party names)
        if (!string.IsNullOrWhiteSpace(request.CounterpartyName))
        {
            query = query.Where(d =>
                (d.CounterParty != null && d.CounterParty.Name.Contains(request.CounterpartyName)) ||
                (d.ThirdParty != null && d.ThirdParty.Contains(request.CounterpartyName)));
        }

        // Counterparty No (exact match)
        if (!string.IsNullOrWhiteSpace(request.CounterpartyNo))
        {
            query = query.Where(d => d.CounterParty != null && d.CounterParty.CounterPartyNoAlpha == request.CounterpartyNo);
        }

        // Counterparty Country (exact match on country code or country name)
        if (!string.IsNullOrWhiteSpace(request.CounterpartyCountry))
        {
            query = query.Where(d => d.CounterParty != null &&
                (d.CounterParty.Country == request.CounterpartyCountry ||
                 (d.CounterParty.CountryNavigation != null && d.CounterParty.CountryNavigation.Name == request.CounterpartyCountry)));
        }

        // Counterparty City (contains)
        if (!string.IsNullOrWhiteSpace(request.CounterpartyCity))
        {
            query = query.Where(d => d.CounterParty != null && d.CounterParty.City != null && d.CounterParty.City.Contains(request.CounterpartyCity));
        }

        // Boolean attributes
        if (request.Fax.HasValue)
        {
            query = query.Where(d => d.Fax == request.Fax.Value);
        }

        if (request.OriginalReceived.HasValue)
        {
            query = query.Where(d => d.OriginalReceived == request.OriginalReceived.Value);
        }

        if (request.Confidential.HasValue)
        {
            query = query.Where(d => d.Confidential == request.Confidential.Value);
        }

        if (request.BankConfirmation.HasValue)
        {
            query = query.Where(d => d.BankConfirmation == request.BankConfirmation.Value);
        }

        // Authorisation (contains)
        if (!string.IsNullOrWhiteSpace(request.Authorisation))
        {
            query = query.Where(d => d.Authorisation != null && d.Authorisation.Contains(request.Authorisation));
        }

        // Amount range
        if (request.AmountFrom.HasValue)
        {
            query = query.Where(d => d.Amount >= request.AmountFrom.Value);
        }

        if (request.AmountTo.HasValue)
        {
            query = query.Where(d => d.Amount <= request.AmountTo.Value);
        }

        // Currency (exact match)
        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            query = query.Where(d => d.CurrencyCode == request.CurrencyCode);
        }

        // Date ranges
        if (request.DateOfContractFrom.HasValue)
        {
            query = query.Where(d => d.DateOfContract >= request.DateOfContractFrom.Value);
        }

        if (request.DateOfContractTo.HasValue)
        {
            query = query.Where(d => d.DateOfContract <= request.DateOfContractTo.Value);
        }

        if (request.ReceivingDateFrom.HasValue)
        {
            query = query.Where(d => d.ReceivingDate >= request.ReceivingDateFrom.Value);
        }

        if (request.ReceivingDateTo.HasValue)
        {
            query = query.Where(d => d.ReceivingDate <= request.ReceivingDateTo.Value);
        }

        if (request.SendingOutDateFrom.HasValue)
        {
            query = query.Where(d => d.SendingOutDate >= request.SendingOutDateFrom.Value);
        }

        if (request.SendingOutDateTo.HasValue)
        {
            query = query.Where(d => d.SendingOutDate <= request.SendingOutDateTo.Value);
        }

        if (request.ForwardedToSignatoriesDateFrom.HasValue)
        {
            query = query.Where(d => d.ForwardedToSignatoriesDate >= request.ForwardedToSignatoriesDateFrom.Value);
        }

        if (request.ForwardedToSignatoriesDateTo.HasValue)
        {
            query = query.Where(d => d.ForwardedToSignatoriesDate <= request.ForwardedToSignatoriesDateTo.Value);
        }

        if (request.DispatchDateFrom.HasValue)
        {
            query = query.Where(d => d.DispatchDate >= request.DispatchDateFrom.Value);
        }

        if (request.DispatchDateTo.HasValue)
        {
            query = query.Where(d => d.DispatchDate <= request.DispatchDateTo.Value);
        }

        if (request.ActionDateFrom.HasValue)
        {
            query = query.Where(d => d.ActionDate >= request.ActionDateFrom.Value);
        }

        if (request.ActionDateTo.HasValue)
        {
            query = query.Where(d => d.ActionDate <= request.ActionDateTo.Value);
        }

        return query;
    }

    private IQueryable<Document> ApplySorting(IQueryable<Document> query, string? sortColumn, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            return query; // No sorting

        var isDescending = sortDirection?.ToLower() == "desc";

        // Map column names to entity properties
        query = sortColumn.ToLower() switch
        {
            "barcode" => isDescending ? query.OrderByDescending(d => d.BarCode) : query.OrderBy(d => d.BarCode),
            "documenttype" => isDescending ? query.OrderByDescending(d => d.Dt!.DtName) : query.OrderBy(d => d.Dt!.DtName),
            "documentname" => isDescending ? query.OrderByDescending(d => d.DocumentName!.Name) : query.OrderBy(d => d.DocumentName!.Name),
            "counterparty" => isDescending ? query.OrderByDescending(d => d.CounterParty!.Name) : query.OrderBy(d => d.CounterParty!.Name),
            "counterpartyno" => isDescending ? query.OrderByDescending(d => d.CounterParty!.CounterPartyNoAlpha) : query.OrderBy(d => d.CounterParty!.CounterPartyNoAlpha),
            "country" => isDescending ? query.OrderByDescending(d => d.CounterParty!.CountryNavigation!.Name) : query.OrderBy(d => d.CounterParty!.CountryNavigation!.Name),
            "dateofcontract" => isDescending ? query.OrderByDescending(d => d.DateOfContract) : query.OrderBy(d => d.DateOfContract),
            "receivingdate" => isDescending ? query.OrderByDescending(d => d.ReceivingDate) : query.OrderBy(d => d.ReceivingDate),
            "sendingoutdate" => isDescending ? query.OrderByDescending(d => d.SendingOutDate) : query.OrderBy(d => d.SendingOutDate),
            "forwardedtosignatoriesdate" => isDescending ? query.OrderByDescending(d => d.ForwardedToSignatoriesDate) : query.OrderBy(d => d.ForwardedToSignatoriesDate),
            "dispatchdate" => isDescending ? query.OrderByDescending(d => d.DispatchDate) : query.OrderBy(d => d.DispatchDate),
            "actiondate" => isDescending ? query.OrderByDescending(d => d.ActionDate) : query.OrderBy(d => d.ActionDate),
            "comment" => isDescending ? query.OrderByDescending(d => d.Comment) : query.OrderBy(d => d.Comment),
            "documentno" => isDescending ? query.OrderByDescending(d => d.DocumentNo) : query.OrderBy(d => d.DocumentNo),
            "versionno" => isDescending ? query.OrderByDescending(d => d.VersionNo) : query.OrderBy(d => d.VersionNo),
            "associatedtopua" => isDescending ? query.OrderByDescending(d => d.AssociatedToPua) : query.OrderBy(d => d.AssociatedToPua),
            "amount" => isDescending ? query.OrderByDescending(d => d.Amount) : query.OrderBy(d => d.Amount),
            "currency" => isDescending ? query.OrderByDescending(d => d.CurrencyCode) : query.OrderBy(d => d.CurrencyCode),
            _ => query // Unknown column, no sorting
        };

        return query;
    }

    private DocumentSearchItemDto MapToSearchItemDto(Document entity)
    {
        return new DocumentSearchItemDto
        {
            Id = entity.Id,
            BarCode = entity.BarCode,
            DocumentType = entity.Dt?.DtName,
            DocumentName = entity.DocumentName?.Name,
            Counterparty = entity.CounterParty?.Name,
            CounterpartyNo = entity.CounterParty?.CounterPartyNoAlpha,
            Country = entity.CounterParty?.CountryNavigation?.Name,
            ThirdParty = entity.ThirdParty, // Already comma-separated in DB
            DateOfContract = entity.DateOfContract,
            ReceivingDate = entity.ReceivingDate,
            SendingOutDate = entity.SendingOutDate,
            ForwardedToSignatoriesDate = entity.ForwardedToSignatoriesDate,
            DispatchDate = entity.DispatchDate,
            ActionDate = entity.ActionDate,
            Comment = entity.Comment,
            Fax = entity.Fax,
            OriginalReceived = entity.OriginalReceived,
            TranslationReceived = entity.TranslatedVersionReceived,
            Confidential = entity.Confidential,
            DocumentNo = entity.DocumentNo,
            AssociatedToPua = entity.AssociatedToPua,
            AssociatedToAppendix = entity.AssociatedToAppendix,
            VersionNo = entity.VersionNo,
            ValidUntil = entity.ValidUntil,
            CurrencyCode = entity.CurrencyCode,
            Amount = entity.Amount,
            Authorisation = entity.Authorisation,
            BankConfirmation = entity.BankConfirmation,
            City = entity.CounterParty?.City,
            AffiliatedTo = entity.CounterParty?.AffiliatedTo,
            ActionDescription = entity.ActionDescription,
            HasFile = entity.FileId.HasValue,
            FileId = entity.FileId,
            Name = entity.Name
        };
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

    /// <summary>
    /// Gets the document file (PDF) for download
    /// </summary>
    public async Task<DocumentFileDto?> GetDocumentFileAsync(int id)
    {
        _logger.LogInformation("Getting document file for document ID: {Id}", id);

        var document = await _context.Documents
            .Include(d => d.File)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document?.File == null)
        {
            _logger.LogWarning("Document file not found for document ID: {Id}", id);
            return null;
        }

        _logger.LogInformation("Document file found: {FileName}, Size: {Size} bytes",
            document.File.FileName, document.File.Bytes?.Length ?? 0);

        return new DocumentFileDto
        {
            FileBytes = document.File.Bytes ?? Array.Empty<byte>(),
            FileName = document.File.FileName,
            ContentType = DetermineContentType(document.File.FileName)
        };
    }

    /// <summary>
    /// Determines content type based on file extension
    /// </summary>
    private string DetermineContentType(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "application/octet-stream";

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".tif" or ".tiff" => "image/tiff",
            _ => "application/octet-stream"
        };
    }
}
