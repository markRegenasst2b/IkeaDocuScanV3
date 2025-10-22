using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.Enums;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for managing audit trail entries
/// Captures user actions: Edit, Register, CheckIn, Delete, SendLink, SendAttachment, SendLinks, SendAttachments
/// </summary>
public class AuditTrailService : IAuditTrailService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditTrailService> _logger;

    public AuditTrailService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditTrailService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task LogAsync(AuditAction action, string barCode, string? details = null, string? username = null)
    {
        try
        {
            var user = username ?? GetCurrentUsername();
            var actionName = action.ToString();

            var auditEntry = new AuditTrail
            {
                Timestamp = DateTime.Now,
                User = user,
                Action = actionName,
                Details = details,
                BarCode = barCode
            };

            _context.AuditTrails.Add(auditEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: User={User}, Action={Action}, BarCode={BarCode}",
                user, actionName, barCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create audit log: Action={Action}, BarCode={BarCode}",
                action, barCode);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task LogByDocumentIdAsync(AuditAction action, int documentId, string? details = null, string? username = null)
    {
        try
        {
            var document = await _context.Documents
                .Where(d => d.Id == documentId)
                .Select(d => new { d.BarCode })
                .FirstOrDefaultAsync();

            if (document == null)
            {
                _logger.LogWarning(
                    "Cannot create audit log: Document {DocumentId} not found",
                    documentId);
                throw new InvalidOperationException($"Document with ID {documentId} not found");
            }

            await LogAsync(action, document.BarCode.ToString(), details, username);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Failed to create audit log by document ID: DocumentId={DocumentId}, Action={Action}",
                documentId, action);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task LogBatchAsync(AuditAction action, IEnumerable<string> barCodes, string? details = null, string? username = null)
    {
        try
        {
            var user = username ?? GetCurrentUsername();
            var actionName = action.ToString();
            var timestamp = DateTime.Now;

            var auditEntries = barCodes.Select(barCode => new AuditTrail
            {
                Timestamp = timestamp,
                User = user,
                Action = actionName,
                Details = details,
                BarCode = barCode
            }).ToList();

            _context.AuditTrails.AddRange(auditEntries);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Batch audit log created: User={User}, Action={Action}, Count={Count}",
                user, actionName, auditEntries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create batch audit log: Action={Action}, Count={Count}",
                action, barCodes.Count());
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<AuditTrailDto>> GetByBarCodeAsync(string barCode, int limit = 100)
    {
        var entries = await _context.AuditTrails
            .Where(a => a.BarCode == barCode)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new AuditTrailDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                User = a.User,
                Action = a.Action,
                Details = a.Details,
                BarCode = a.BarCode
            })
            .ToListAsync();

        return entries;
    }

    /// <inheritdoc />
    public async Task<List<AuditTrailDto>> GetByUserAsync(string username, int limit = 100)
    {
        var entries = await _context.AuditTrails
            .Where(a => a.User == username)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new AuditTrailDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                User = a.User,
                Action = a.Action,
                Details = a.Details,
                BarCode = a.BarCode
            })
            .ToListAsync();

        return entries;
    }

    /// <inheritdoc />
    public async Task<List<AuditTrailDto>> GetRecentAsync(int limit = 100)
    {
        var entries = await _context.AuditTrails
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new AuditTrailDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                User = a.User,
                Action = a.Action,
                Details = a.Details,
                BarCode = a.BarCode
            })
            .ToListAsync();

        return entries;
    }

    /// <inheritdoc />
    public async Task<List<AuditTrailDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, AuditAction? action = null)
    {
        var query = _context.AuditTrails
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate);

        if (action.HasValue)
        {
            var actionName = action.Value.ToString();
            query = query.Where(a => a.Action == actionName);
        }

        var entries = await query
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditTrailDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                User = a.User,
                Action = a.Action,
                Details = a.Details,
                BarCode = a.BarCode
            })
            .ToListAsync();

        return entries;
    }

    /// <summary>
    /// Get the current username from the HTTP context
    /// </summary>
    private string GetCurrentUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    }
}
