# AuditTrail Service Implementation

## Overview

The `AuditTrailService` provides comprehensive audit logging capabilities for the IkeaDocuScan application. It captures user actions and writes them to the `AuditTrail` database table, enabling complete traceability of document operations.

## Features

- **Action Tracking**: Captures 8 different user actions
- **Automatic User Detection**: Automatically captures the current user from HTTP context
- **Batch Operations**: Support for logging multiple actions efficiently
- **Flexible Querying**: Retrieve audit logs by document, user, date range, or action type
- **Detailed Logging**: Optional details field for additional context
- **Exception Handling**: Robust error handling with logging

## Supported Actions

The service tracks the following user actions (defined in `AuditAction` enum):

1. **Edit** - User modified a document
2. **Register** - User registered a new document
3. **CheckIn** - User checked in a document
4. **Delete** - User deleted a document
5. **SendLink** - User sent a link to a document
6. **SendAttachment** - User sent an attachment
7. **SendLinks** - User sent multiple links
8. **SendAttachments** - User sent multiple attachments

## Database Schema

The `AuditTrail` table has the following structure:

```sql
CREATE TABLE [AuditTrail]
(
    [ID] INT IDENTITY NOT NULL PRIMARY KEY,
    [Timestamp] DATETIME NOT NULL,
    [User] VARCHAR(128) NOT NULL,
    [Action] VARCHAR(128) NOT NULL,
    [Details] VARCHAR(2500) NULL,
    [BarCode] VARCHAR(10) NOT NULL
)
```

## Installation

### 1. Add the Service to Dependency Injection

The service is already registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
```

### 2. Inject into Your Services

```csharp
public class MyService
{
    private readonly IAuditTrailService _auditTrailService;

    public MyService(IAuditTrailService auditTrailService)
    {
        _auditTrailService = auditTrailService;
    }
}
```

## Usage Examples

### Basic Logging

```csharp
// Log an edit action
await _auditTrailService.LogAsync(
    AuditAction.Edit,
    barCode: "12345",
    details: "Updated document name and date"
);

// Log a registration
await _auditTrailService.LogAsync(
    AuditAction.Register,
    barCode: "12346",
    details: "New contract document"
);

// Log a deletion
await _auditTrailService.LogAsync(
    AuditAction.Delete,
    barCode: "12345",
    details: "Duplicate document removed"
);
```

### Using Document ID Instead of BarCode

```csharp
// Service will automatically look up the barcode
await _auditTrailService.LogByDocumentIdAsync(
    AuditAction.Edit,
    documentId: 42,
    details: "Updated via document ID"
);
```

### Batch Operations

```csharp
// Log multiple actions efficiently
var barCodes = new List<string> { "12345", "12346", "12347" };

await _auditTrailService.LogBatchAsync(
    AuditAction.SendLinks,
    barCodes,
    details: "Sent to customer@example.com"
);
```

### Querying Audit Logs

```csharp
// Get audit history for a specific document
var history = await _auditTrailService.GetByBarCodeAsync("12345", limit: 50);

// Get all actions by a user
var userActions = await _auditTrailService.GetByUserAsync("john.doe@company.com", limit: 100);

// Get recent activity (activity feed)
var recent = await _auditTrailService.GetRecentAsync(limit: 20);

// Get actions within a date range
var report = await _auditTrailService.GetByDateRangeAsync(
    startDate: DateTime.Now.AddMonths(-1),
    endDate: DateTime.Now
);

// Filter by action type
var deleteActions = await _auditTrailService.GetByDateRangeAsync(
    startDate: DateTime.Now.AddMonths(-1),
    endDate: DateTime.Now,
    action: AuditAction.Delete
);
```

## Integration with DocumentService

Here's how to integrate audit logging into your existing services:

```csharp
public class DocumentService : IDocumentService
{
    private readonly AppDbContext _context;
    private readonly IAuditTrailService _auditTrailService;

    public async Task<DocumentDto> CreateAsync(CreateDocumentDto dto)
    {
        // Create the document
        var entity = new Document { /* ... */ };
        _context.Documents.Add(entity);
        await _context.SaveChangesAsync();

        // Log the registration
        await _auditTrailService.LogAsync(
            AuditAction.Register,
            entity.BarCode.ToString(),
            $"Document registered: {entity.Name}"
        );

        return MapToDto(entity);
    }

    public async Task<DocumentDto> UpdateAsync(UpdateDocumentDto dto)
    {
        var entity = await _context.Documents.FindAsync(dto.Id);

        // Track what changed
        var changes = new List<string>();
        if (entity.Name != dto.Name)
            changes.Add($"Name: '{entity.Name}' -> '{dto.Name}'");

        // Update the entity
        entity.Name = dto.Name;
        await _context.SaveChangesAsync();

        // Log the edit with details
        await _auditTrailService.LogAsync(
            AuditAction.Edit,
            entity.BarCode.ToString(),
            string.Join(", ", changes)
        );

        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Documents.FindAsync(id);
        var barCode = entity.BarCode.ToString();

        _context.Documents.Remove(entity);
        await _context.SaveChangesAsync();

        // Log deletion
        await _auditTrailService.LogAsync(
            AuditAction.Delete,
            barCode,
            $"Deleted: {entity.Name}"
        );
    }
}
```

## Email/Link Sending Integration

```csharp
public class EmailService
{
    private readonly IAuditTrailService _auditTrailService;

    public async Task SendDocumentLink(string barCode, string recipient)
    {
        try
        {
            // Send the email
            await SendEmailAsync(recipient, GenerateLink(barCode));

            // Log the action
            await _auditTrailService.LogAsync(
                AuditAction.SendLink,
                barCode,
                $"Link sent to: {recipient}"
            );
        }
        catch (Exception ex)
        {
            // Log the failure
            await _auditTrailService.LogAsync(
                AuditAction.SendLink,
                barCode,
                $"FAILED to send to: {recipient} - Error: {ex.Message}"
            );
            throw;
        }
    }

    public async Task SendMultipleAttachments(List<string> barCodes, string recipient)
    {
        // Send attachments
        await SendAttachmentsAsync(barCodes, recipient);

        // Log batch operation
        await _auditTrailService.LogBatchAsync(
            AuditAction.SendAttachments,
            barCodes,
            $"Attachments sent to: {recipient}"
        );
    }
}
```

## Reporting and Analytics

```csharp
public class AuditReportService
{
    private readonly IAuditTrailService _auditTrailService;

    public async Task<AuditReport> GenerateMonthlyReport(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var allActions = await _auditTrailService.GetByDateRangeAsync(startDate, endDate);

        return new AuditReport
        {
            TotalActions = allActions.Count,
            UniqueUsers = allActions.Select(a => a.User).Distinct().Count(),
            UniqueDocuments = allActions.Select(a => a.BarCode).Distinct().Count(),
            ActionBreakdown = allActions
                .GroupBy(a => a.Action)
                .ToDictionary(g => g.Key, g => g.Count()),
            MostActiveUsers = allActions
                .GroupBy(a => a.User)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new { User = g.Key, ActionCount = g.Count() })
                .ToList()
        };
    }

    public async Task<List<string>> FindDocumentsDeletedByUser(string username, DateTime since)
    {
        var actions = await _auditTrailService.GetByUserAsync(username, limit: 1000);

        return actions
            .Where(a => a.Action == "Delete" && a.Timestamp >= since)
            .Select(a => a.BarCode)
            .ToList();
    }
}
```

## API Endpoints Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditTrailService _auditTrailService;

    public AuditController(IAuditTrailService auditTrailService)
    {
        _auditTrailService = auditTrailService;
    }

    [HttpGet("document/{barCode}")]
    public async Task<ActionResult<List<AuditTrailDto>>> GetDocumentHistory(string barCode)
    {
        var history = await _auditTrailService.GetByBarCodeAsync(barCode);
        return Ok(history);
    }

    [HttpGet("recent")]
    public async Task<ActionResult<List<AuditTrailDto>>> GetRecentActivity([FromQuery] int limit = 50)
    {
        var recent = await _auditTrailService.GetRecentAsync(limit);
        return Ok(recent);
    }

    [HttpGet("user/{username}")]
    public async Task<ActionResult<List<AuditTrailDto>>> GetUserActivity(string username)
    {
        var actions = await _auditTrailService.GetByUserAsync(username);
        return Ok(actions);
    }

    [HttpGet("report")]
    public async Task<ActionResult<List<AuditTrailDto>>> GetAuditReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? action = null)
    {
        AuditAction? actionEnum = null;
        if (!string.IsNullOrEmpty(action) && Enum.TryParse<AuditAction>(action, out var parsed))
        {
            actionEnum = parsed;
        }

        var report = await _auditTrailService.GetByDateRangeAsync(startDate, endDate, actionEnum);
        return Ok(report);
    }
}
```

## Best Practices

### 1. Always Log Critical Operations

```csharp
// DO: Log all CRUD operations
await _auditTrailService.LogAsync(AuditAction.Edit, barCode, details);

// DO: Log security-sensitive operations
await _auditTrailService.LogAsync(AuditAction.Delete, barCode, $"Deleted by admin: {reason}");
```

### 2. Provide Meaningful Details

```csharp
// GOOD: Specific details
await _auditTrailService.LogAsync(
    AuditAction.Edit,
    barCode,
    "Changed CounterParty from 'ACME Corp' to 'ACME International'"
);

// AVOID: Generic details
await _auditTrailService.LogAsync(AuditAction.Edit, barCode, "Updated");
```

### 3. Use Batch Operations for Bulk Actions

```csharp
// GOOD: Single batch operation
await _auditTrailService.LogBatchAsync(AuditAction.SendLinks, barCodes, details);

// AVOID: Loop with individual logs (slower)
foreach (var barCode in barCodes)
{
    await _auditTrailService.LogAsync(AuditAction.SendLinks, barCode, details);
}
```

### 4. Handle Exceptions Gracefully

```csharp
try
{
    await PerformCriticalOperation();
    await _auditTrailService.LogAsync(AuditAction.Edit, barCode, "Success");
}
catch (Exception ex)
{
    // Log the failure
    await _auditTrailService.LogAsync(
        AuditAction.Edit,
        barCode,
        $"FAILED: {ex.Message}"
    );
    throw;
}
```

## Performance Considerations

1. **Batch Operations**: Use `LogBatchAsync` for multiple documents
2. **Indexing**: Consider adding database indexes on frequently queried columns:
   - `BarCode` (for document history)
   - `User` (for user activity)
   - `Timestamp` (for date range queries)
   - `Action` (for action-specific reports)

```sql
CREATE INDEX IX_AuditTrail_BarCode ON AuditTrail(BarCode);
CREATE INDEX IX_AuditTrail_User ON AuditTrail([User]);
CREATE INDEX IX_AuditTrail_Timestamp ON AuditTrail(Timestamp);
CREATE INDEX IX_AuditTrail_Action ON AuditTrail(Action);
```

3. **Limit Results**: Always specify reasonable limits when querying
4. **Async Operations**: All operations are async for better scalability

## Troubleshooting

### Issue: Audit logs not being created

**Check:**
- Service is registered in `Program.cs`
- Database connection is working
- `AuditTrail` table exists
- Current user is being detected correctly

### Issue: "System" user instead of actual username

**Solution:**
Ensure your authentication middleware is running before the audit service tries to log:

```csharp
app.UseAuthentication();
app.UseMiddleware<WindowsIdentityMiddleware>();
app.UseAuthorization();
```

### Issue: Performance degradation

**Solutions:**
- Add database indexes (see Performance Considerations)
- Use batch operations for bulk logging
- Implement archiving for old audit records
- Consider async logging with background queue

## Migration from Legacy Code

If you have existing audit logging code, migrate it as follows:

```csharp
// OLD CODE
_context.AuditTrails.Add(new AuditTrail
{
    Timestamp = DateTime.Now,
    User = username,
    Action = "Edit",
    Details = details,
    BarCode = barCode
});
await _context.SaveChangesAsync();

// NEW CODE
await _auditTrailService.LogAsync(AuditAction.Edit, barCode, details);
```

## Files Created

1. **`IkeaDocuScan.Shared/Enums/AuditAction.cs`** - Enum defining audit actions
2. **`IkeaDocuScan.Shared/Interfaces/IAuditTrailService.cs`** - Service interface and DTO
3. **`IkeaDocuScan-Web/Services/AuditTrailService.cs`** - Service implementation
4. **`IkeaDocuScan-Web/Services/DocumentServiceWithAudit.cs`** - Example integration
5. **`IkeaDocuScan-Web/Services/AuditTrailUsageExamples.cs`** - Comprehensive examples
6. **`Program.cs`** - Updated with service registration

## Support

For questions or issues:
- Review the usage examples in `AuditTrailUsageExamples.cs`
- Check the integration example in `DocumentServiceWithAudit.cs`
- Consult the logs for detailed error messages

## Version History

- **v1.0** - Initial implementation
  - Core audit trail logging
  - Support for 8 action types
  - Query capabilities by document, user, date range
  - Batch operation support
