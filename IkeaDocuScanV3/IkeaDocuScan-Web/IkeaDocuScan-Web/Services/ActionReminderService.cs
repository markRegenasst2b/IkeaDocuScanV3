using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.DTOs.ActionReminders;
using IkeaDocuScan.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Service for managing action reminders
/// </summary>
public class ActionReminderService : IActionReminderService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<ActionReminderService> _logger;

    public ActionReminderService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<ActionReminderService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<ActionReminderDto>> GetDueActionsAsync(ActionReminderSearchRequestDto? request = null)
    {
        _logger.LogInformation("Fetching due actions with filters: {@Request}", request);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Build base query with joins
            var query = context.Documents
                .AsNoTracking()
                .Where(d => d.ActionDate != null && d.ActionDate >= d.ReceivingDate)
                .Select(d => new
                {
                    DocumentId = d.Id,
                    BarCode = d.BarCode.ToString(),
                    DocumentType = d.Dt != null ? d.Dt.DtName : "",
                    DocumentName = d.DocumentName != null ? d.DocumentName.Name : "",
                    d.DocumentNo,
                    CounterParty = d.CounterParty != null ? d.CounterParty.Name ?? "" : "",
                    CounterPartyNo = d.CounterParty != null ? (int?)d.CounterParty.CounterPartyId : null,
                    d.ActionDate,
                    d.ReceivingDate,
                    d.ActionDescription,
                    d.Comment,
                    DocumentTypeId = (int?)d.DtId,
                    CounterPartyId = d.CounterPartyId
                });

            // Apply filters if provided
            if (request != null)
            {
                // Date range filters
                if (request.DateFrom.HasValue)
                {
                    var dateFrom = request.DateFrom.Value.Date;
                    query = query.Where(d => d.ActionDate >= dateFrom);
                }

                if (request.DateTo.HasValue)
                {
                    var dateTo = request.DateTo.Value.Date;
                    query = query.Where(d => d.ActionDate <= dateTo);
                }

                // Overdue only filter
                if (request.IncludeOverdueOnly)
                {
                    var today = DateTime.Today;
                    query = query.Where(d => d.ActionDate < today);
                }
                // Future actions filter
                else if (!request.IncludeFutureActions)
                {
                    var today = DateTime.Today;
                    query = query.Where(d => d.ActionDate <= today);
                }

                // Document type filter
                if (request.DocumentTypeIds != null && request.DocumentTypeIds.Any())
                {
                    query = query.Where(d => d.DocumentTypeId.HasValue && request.DocumentTypeIds.Contains(d.DocumentTypeId.Value));
                }

                // Counter party filter
                if (request.CounterPartyIds != null && request.CounterPartyIds.Any())
                {
                    query = query.Where(d => d.CounterPartyId.HasValue && request.CounterPartyIds.Contains(d.CounterPartyId.Value));
                }

                // Counter party name search
                if (!string.IsNullOrWhiteSpace(request.CounterPartySearch))
                {
                    var counterPartySearchLower = request.CounterPartySearch.ToLower();
                    query = query.Where(d => d.CounterParty != null && d.CounterParty.Contains(counterPartySearchLower));
                }

                // Search string filter
                if (!string.IsNullOrWhiteSpace(request.SearchString))
                {
                    var searchLower = request.SearchString.ToLower();
                    query = query.Where(d =>
                        d.BarCode.Contains(searchLower) ||
                        (d.DocumentName != null && d.DocumentName.Contains(searchLower)) ||
                        (d.Comment != null && d.Comment.Contains(searchLower)) ||
                        (d.ActionDescription != null && d.ActionDescription.Contains(searchLower)));
                }
            }

            // Order by ActionDate and BarCode
            query = query.OrderBy(d => d.ActionDate).ThenBy(d => d.BarCode);

            // Project to DTO
            var results = await query.Select(d => new ActionReminderDto
            {
                DocumentId = d.DocumentId,
                BarCode = d.BarCode,
                DocumentType = d.DocumentType,
                DocumentName = d.DocumentName,
                DocumentNo = d.DocumentNo,
                CounterParty = d.CounterParty,
                CounterPartyNo = d.CounterPartyNo,
                ActionDate = d.ActionDate,
                ReceivingDate = d.ReceivingDate,
                ActionDescription = d.ActionDescription,
                Comment = d.Comment
            }).ToListAsync();

            _logger.LogInformation("Retrieved {Count} due actions", results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching due actions");
            throw;
        }
    }

    public async Task<List<ActionReminderDto>> GetActionsDueOnDateAsync(DateTime date)
    {
        _logger.LogInformation("Fetching actions due on {Date}", date.Date);

        var request = new ActionReminderSearchRequestDto
        {
            DateFrom = date.Date,
            DateTo = date.Date,
            IncludeFutureActions = true
        };

        return await GetDueActionsAsync(request);
    }

    public async Task<int> GetDueActionsCountAsync()
    {
        _logger.LogInformation("Fetching due actions count");

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var count = await context.Documents
                .AsNoTracking()
                .Where(d => d.ActionDate != null && d.ActionDate >= d.ReceivingDate)
                .CountAsync();

            _logger.LogInformation("Due actions count: {Count}", count);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching due actions count");
            throw;
        }
    }
}
