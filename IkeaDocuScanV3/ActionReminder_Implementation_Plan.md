# Action Reminder Feature - Implementation Plan

**Project:** IkeaDocuScan-V3
**Feature:** Action Reminder Excel Report & Background Email Service
**Created:** November 4, 2025
**Status:** Planning Phase

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture Alignment](#architecture-alignment)
3. [Part 1: Excel Report Page](#part-1-excel-report-page)
4. [Part 2: Background Process Project](#part-2-background-process-project)
5. [Configuration](#configuration)
6. [Testing Strategy](#testing-strategy)
7. [Deployment Plan](#deployment-plan)
8. [Future Enhancements](#future-enhancements)

---

## Overview

### Business Requirements

The Action Reminder feature enables users to:
1. View all documents with due actions in an Excel-style grid
2. Export the action list to Excel format
3. Receive automated daily email notifications for actions due today

### Technical Scope

**Two Components:**
1. **Web UI:** Blazor page with Excel preview/export functionality
2. **Background Service:** Windows Service for automated email notifications

### Data Model

Actions are stored directly on the `Document` entity:
- `Document.ActionDate` (DateTime?) - The date when action is due
- `Document.ActionDescription` (string?) - Description of the action required

### Business Rules

1. **Due Actions Definition:** An action is considered due when:
   - `ActionDate` is not null
   - `ActionDate` is greater than or equal to `ReceivingDate`
   - `ActionDate` is today or in the past

2. **Display Order:** Actions sorted by:
   - Primary: `ActionDate` (ascending)
   - Secondary: `BarCode` (ascending)

---

## Architecture Alignment

### Layered Architecture

```
┌─────────────────────────────────────────────────┐
│  Blazor Client (ActionReminders.razor)          │
│  - InteractiveWebAssemblyRenderMode             │
│  - Uses ExcelPreview component                  │
└─────────────────┬───────────────────────────────┘
                  │ HTTP API
┌─────────────────▼───────────────────────────────┐
│  ASP.NET Core Server                            │
│  - ActionReminderEndpoints.cs                   │
│  - Maps /api/action-reminders/*                 │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│  Services Layer                                 │
│  - IActionReminderService                       │
│  - ActionReminderService                        │
│  - Uses IDocumentService, IEmailService         │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│  Infrastructure (EF Core)                       │
│  - AppDbContext                                 │
│  - Document, DocumentType, etc.                 │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│  Background Service (Separate Project)          │
│  - IkeaDocuScan.ActionReminderService           │
│  - Windows Service/Worker                       │
│  - Uses shared Infrastructure & Services        │
└─────────────────────────────────────────────────┘
```

### Design Patterns

Following IkeaDocuScan conventions:
- **Service-Oriented Design:** Interface-based services with DI
- **DTO Pattern:** Separate DTOs for data transfer (ActionReminderDto)
- **Repository Pattern:** Via EF Core DbContext
- **Minimal API:** Endpoint definitions
- **Excel Export Pattern:** Reuse existing ExcelPreview Pate and implicitly ExcelReporting library. do not imlement excel export. use existing ExcelPreview component and its export functionality. 

---

## Part 1: Excel Report Page

### 1.1 Data Transfer Objects (DTOs)

#### Location: `IkeaDocuScan.Shared/DTOs/ActionReminders/`

#### ActionReminderDto.cs
```
Purpose: Read DTO for displaying action reminder data
Properties:
  - BarCode (string) - Document barcode
  - DocumentType (string) - Document type name
  - DocumentName (string) - Document name
  - DocumentNo (string?) - Document number
  - CounterParty (string) - Counter party name
  - CounterPartyNo (int?) - Counter party ID/number
  - ActionDate (DateTime?) - Due date (formatted as dd/MM/yyyy)
  - ReceivingDate (DateTime?) - Document receiving date
  - ActionDescription (string?) - Action description
  - Comment (string?) - Document comment

Base Class: ExportableBase (from ExcelReporting)
Attributes: [ExcelExport] attributes for each property with:
  - Display names
  - Data types (String, Date)
  - Formats (date format: "dd/MM/yyyy")
  - Column order
```

#### ActionReminderSearchRequestDto.cs
```
Purpose: DTO for filtering action reminders
Properties:
  - DateFrom (DateTime?) - Filter by ActionDate >= DateFrom
  - DateTo (DateTime?) - Filter by ActionDate <= DateTo
  - DocumentTypeIds (List<int>?) - Filter by document types
  - CounterPartyIds (List<int>?) - Filter by counter parties
  - SearchString (string?) - Search in multiple fields
  - IncludeFutureActions (bool) - Include actions with future dates
  - IncludeOverdueOnly (bool) - Only show overdue actions
```

### 1.2 Service Layer

#### Location: `IkeaDocuScan.Shared/Interfaces/`

#### IActionReminderService.cs
```
Interface Definition:
  - Task<List<ActionReminderDto>> GetDueActionsAsync(ActionReminderSearchRequestDto? request = null)
    Purpose: Get all due actions with optional filtering
    Returns: List of action reminders mapped to DTOs

  - Task<List<ActionReminderDto>> GetActionsDueOnDateAsync(DateTime date)
    Purpose: Get actions due on a specific date (for email notifications)
    Returns: List of action reminders for specified date

  - Task<int> GetDueActionsCountAsync()
    Purpose: Get count of currently due actions (for dashboard/badges)
    Returns: Count of due actions
```

#### Location: `IkeaDocuScan-Web/IkeaDocuScan-Web/Services/`

#### ActionReminderService.cs
```
Implementation:
  Dependencies:
    - IDbContextFactory<AppDbContext> _contextFactory
    - ILogger<ActionReminderService> _logger

  GetDueActionsAsync Implementation:
    1. Create DbContext via factory
    2. Build EF Core query:
       - Join: Document → DocumentType (LEFT JOIN)
       - Join: Document → DocumentName (LEFT JOIN)
       - Join: Document → CounterParty (LEFT JOIN)
       - Where: ActionDate IS NOT NULL
       - Where: ActionDate >= ReceivingDate
       - Apply filters from ActionReminderSearchRequestDto:
         * Date range filters
         * Document type filter
         * Counter party filter
         * Search string (BarCode, DocumentName, Comment)
       - Order by: ActionDate ASC, BarCode ASC
    3. Use AsNoTracking() for read-only queries
    4. Project to ActionReminderDto via Select()
    5. Return list

  GetActionsDueOnDateAsync Implementation:
    1. Call GetDueActionsAsync with filter:
       - DateFrom = date.Date
       - DateTo = date.Date
    2. Return filtered list

  GetDueActionsCountAsync Implementation:
    1. Build same query as GetDueActionsAsync
    2. Return CountAsync() instead of ToListAsync()

  Error Handling:
    - Log all database access
    - Catch and log exceptions
    - Re-throw as custom exceptions if needed

  Performance Considerations:
    - Use AsNoTracking() for read-only queries
    - Consider adding database index on:
      * Document.ActionDate
      * Document.ReceivingDate
    - Use projection to avoid loading full entities
```

### 1.3 API Endpoints

#### Location: `IkeaDocuScan-Web/IkeaDocuScan-Web/Endpoints/`

#### ActionReminderEndpoints.cs
```
Purpose: REST API endpoints for action reminders

Endpoints:
  1. GET /api/action-reminders
     Handler: GetDueActions
     Parameters:
       - [FromQuery] ActionReminderSearchRequestDto? request
     Returns: Ok(List<ActionReminderDto>)
     Authorization: [Authorize(Policy = "HasAccess")]
     Logging: Log request and result count

  2. GET /api/action-reminders/count
     Handler: GetDueActionsCount
     Parameters: None
     Returns: Ok(int count)
     Authorization: [Authorize(Policy = "HasAccess")]

  3. GET /api/action-reminders/date/{date}
     Handler: GetActionsDueOnDate
     Parameters:
       - [FromRoute] DateTime date
     Returns: Ok(List<ActionReminderDto>)
     Authorization: [Authorize(Policy = "HasAccess")]

Extension Method:
  public static void MapActionReminderEndpoints(this IEndpointRouteBuilder app)
  - Maps all endpoints with route prefix "/api/action-reminders"
  - Applies authorization policies
  - Includes OpenAPI tags for documentation
```

#### Location: `IkeaDocuScan-Web/IkeaDocuScan-Web/Program.cs`

```
Registration:
  Line ~140: app.MapActionReminderEndpoints();

  Service registration:
  Line ~75: builder.Services.AddScoped<IActionReminderService, ActionReminderService>();
```

### 1.4 Client-Side HTTP Service

#### Location: `IkeaDocuScan-Web/IkeaDocuScan-Web.Client/Services/`

#### ActionReminderHttpService.cs
```
Purpose: Client-side HTTP API wrapper for action reminders

Implementation:
  Dependencies:
    - HttpClient _httpClient (injected)
    - ILogger<ActionReminderHttpService> _logger

  Interface: IActionReminderService (same as server-side)

  Methods:
    1. GetDueActionsAsync(ActionReminderSearchRequestDto? request)
       - Build query string from request properties
       - GET /api/action-reminders?{queryString}
       - Deserialize response to List<ActionReminderDto>
       - Handle HttpRequestException and log errors

    2. GetActionsDueOnDateAsync(DateTime date)
       - GET /api/action-reminders/date/{date:yyyy-MM-dd}
       - Deserialize response

    3. GetDueActionsCountAsync()
       - GET /api/action-reminders/count
       - Deserialize response to int

  Error Handling:
    - Catch HttpRequestException
    - Log error details
    - Return empty list or 0 for count
    - Consider showing user-friendly error messages
```

#### Location: `IkeaDocuScan-Web/IkeaDocuScan-Web.Client/Program.cs`

```
Registration:
  builder.Services.AddScoped<IActionReminderService, ActionReminderHttpService>();
```

### 1.5 Blazor Page Component

#### Location: `IkeaDocuScan-Web/IkeaDocuScan-Web.Client/Pages/`

#### ActionReminders.razor
```
Page Route: @page "/action-reminders"
Render Mode: @rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))
Authorization: @attribute [Authorize(Policy = "HasAccess")]

Component Structure:

  Header Section:
    - Page title: "Action Reminders"
    - Description: "View and export documents with due actions"
    - Badge showing total count of due actions

  Filter Section (Collapsible Card):
    - Date Range Filters:
      * From Date (DatePicker)
      * To Date (DatePicker)
      * Quick filters: "Today", "This Week", "This Month", "Overdue"
    - Document Type Filter (Multi-select dropdown)
    - Counter Party Filter (Multi-select dropdown)
    - Search Box (searches BarCode, DocumentName, Comment)
    - Buttons:
      * "Apply Filters" (primary button)
      * "Clear Filters" (secondary button)
      * "Refresh" (icon button)

  Excel Preview Section:
    - Use existing ExcelPreview component
    - Pass ActionReminderDto list as data
    - Component handles:
      * Grid display with sorting
      * Column formatting
      * Excel download button
      * Row selection
    - Context information:
      * Total records count
      * Filter criteria summary
      * Last updated timestamp

  Empty State:
    - Show when no due actions found
    - Message: "No action reminders found"
    - Icon: Calendar check icon
    - Helpful text based on filters applied

  Loading State:
    - Spinner with message "Loading action reminders..."
    - Displayed during initial load and filter application

  Error State:
    - Alert box for errors
    - Retry button
    - Error message from exception
```

#### ActionReminders.razor.cs
```
Code-Behind Implementation:

  Dependencies:
    - IActionReminderService _actionReminderService (injected)
    - IDocumentTypeService _documentTypeService (injected)
    - ICounterPartyService _counterPartyService (injected)
    - ILogger<ActionReminders> _logger (injected)
    - NavigationManager _navigationManager (injected)

  State Properties:
    - List<ActionReminderDto>? actionReminders
    - ActionReminderSearchRequestDto searchRequest (initialized with defaults)
    - List<DocumentTypeDto>? documentTypes
    - List<CounterPartyDto>? counterParties
    - int totalCount
    - bool isLoading
    - bool isLoadingFilters
    - string? errorMessage
    - DateTime lastUpdated

  Lifecycle Methods:
    OnInitializedAsync():
      1. Set isLoading = true
      2. Load filter data in parallel:
         - documentTypes = await _documentTypeService.GetAllAsync()
         - counterParties = await _counterPartyService.GetAllAsync()
      3. Initialize default filters (show overdue + today)
      4. await LoadActionRemindersAsync()
      5. Set isLoading = false

    OnAfterRenderAsync(bool firstRender):
      - If firstRender, log page load

  Private Methods:
    LoadActionRemindersAsync():
      1. Set isLoading = true
      2. Clear errorMessage
      3. Try:
         - actionReminders = await _actionReminderService.GetDueActionsAsync(searchRequest)
         - totalCount = actionReminders.Count
         - lastUpdated = DateTime.Now
         - Log success with count
      4. Catch (Exception ex):
         - Log error
         - errorMessage = user-friendly message
         - actionReminders = empty list
      5. Finally:
         - isLoading = false
         - StateHasChanged()

    ApplyFiltersAsync():
      1. Validate date range (from <= to)
      2. await LoadActionRemindersAsync()

    ClearFiltersAsync():
      1. Reset searchRequest to defaults
      2. await LoadActionRemindersAsync()

    RefreshAsync():
      1. await LoadActionRemindersAsync()

    SetQuickFilter(string filterType):
      1. Based on filterType ("Today", "Week", "Month", "Overdue"):
         - Set appropriate DateFrom/DateTo
      2. await LoadActionRemindersAsync()

    NavigateToExcelPreview():
      1. Build query string from search criteria
      2. Navigate to /excel-preview?{filters}

  UI Event Handlers:
    - OnDateFromChanged(DateTime? date)
    - OnDateToChanged(DateTime? date)
    - OnDocumentTypeChanged(List<int> selectedIds)
    - OnCounterPartyChanged(List<int> selectedIds)
    - OnSearchStringChanged(string value)
```

### 1.6 Integration with ExcelPreview Component

#### Approach

**Reuse Existing Component:**
The application already has an ExcelPreview component that:
- Displays data in Excel-style grid
- Supports sorting and filtering
- Provides Excel download functionality
- Works with DTOs decorated with [ExcelExport] attributes

**Integration Strategy:**

```
Option 1: Embed ExcelPreview in ActionReminders Page
  - Pass actionReminders list directly to ExcelPreview
  - ExcelPreview reads [ExcelExport] attributes from ActionReminderDto
  - Automatic column generation and formatting

  Implementation:
    <ExcelPreview Data="@actionReminders"
                  Options="@excelOptions"
                  OnDownload="@HandleExcelDownload" />

Option 2: Navigate to Standalone ExcelPreview Page
  - Navigate to /excel-preview with query parameters
  - ExcelPreview page fetches data via API
  - Maintains existing pattern for selection-based exports

  Implementation:
    NavigationManager.NavigateTo($"/excel-preview?type=action-reminders&{filters}");

Recommended: Option 1 (Embedded)
  Rationale:
    - Simpler user experience (no navigation)
    - Filters apply immediately to grid
    - Better performance (no roundtrip)
    - Consistent with Excel export pattern
```

### 1.7 Excel Export Configuration

#### ActionReminderDto Excel Attributes

```
Property Decorations:
  [ExcelExport("Barcode", ExcelDataType.String, Order = 1)]
  public string BarCode { get; set; }

  [ExcelExport("Document Type", ExcelDataType.String, Order = 2)]
  public string DocumentType { get; set; }

  [ExcelExport("Document Name", ExcelDataType.String, Order = 3)]
  public string DocumentName { get; set; }

  [ExcelExport("Document No", ExcelDataType.String, Order = 4)]
  public string? DocumentNo { get; set; }

  [ExcelExport("Counterparty", ExcelDataType.String, Order = 5)]
  public string CounterParty { get; set; }

  [ExcelExport("Counterparty No", ExcelDataType.Number, "#,##0", Order = 6)]
  public int? CounterPartyNo { get; set; }

  [ExcelExport("Action Date", ExcelDataType.Date, "dd/MM/yyyy", Order = 7)]
  public DateTime? ActionDate { get; set; }

  [ExcelExport("Receiving Date", ExcelDataType.Date, "dd/MM/yyyy", Order = 8)]
  public DateTime? ReceivingDate { get; set; }

  [ExcelExport("Action Description", ExcelDataType.String, Order = 9)]
  public string? ActionDescription { get; set; }

  [ExcelExport("Comment", ExcelDataType.String, Order = 10)]
  public string? Comment { get; set; }
```

#### Excel File Configuration

```
Filename Pattern: "ActionReminders_{yyyy-MM-dd_HHmmss}.xlsx"
Sheet Name: "Action Reminders"
Styling:
  - Header: IKEA Blue (#0051BA), White text, Bold
  - Freeze panes: First row
  - Auto-filter: Enabled
  - Column widths: Auto-fit with max 50 characters
Date Format: dd/MM/yyyy (matches database format)
```

### 1.8 Navigation and Menu Integration

#### Location: Navigation Menu Component

```
Add Menu Item:
  Section: Main Navigation (after "Documents")
  Label: "Action Reminders"
  Icon: bi-calendar-check (Bootstrap Icons)
  Route: /action-reminders
  Badge: Show count of due actions (if > 0)
    - Color: Red/Warning for overdue
    - Color: Blue/Info for today's actions
    - Update badge count via API polling or SignalR

Authorization: Display only for users with "HasAccess" policy
```

---

## Part 2: Background Process Project

### 2.1 Project Structure

#### New Project: `IkeaDocuScan.ActionReminderService`

```
Project Type: Worker Service (.NET 9)
Target Framework: net9.0
Project Template: Worker Service

Location: Solution Root
  IkeaDocuScanV3/
  ├── IkeaDocuScan.ActionReminderService/    ← NEW
  ├── IkeaDocuScan-Web/
  ├── IkeaDocuScan-Web.Client/
  ├── IkeaDocuScan.Infrastructure/
  └── IkeaDocuScan.Shared/

Project References:
  - IkeaDocuScan.Infrastructure (Data access)
  - IkeaDocuScan.Shared (DTOs, Interfaces, Configuration)

NuGet Packages:
  - Microsoft.Extensions.Hosting (Worker host)
  - Microsoft.Extensions.Hosting.WindowsServices (Windows Service support)
  - Microsoft.EntityFrameworkCore.SqlServer (Database access)
  - Microsoft.Extensions.Configuration (appsettings support)
  - Microsoft.Extensions.Configuration.Json
  - Microsoft.Extensions.Logging (Logging)
  - MailKit (Email via SMTP - reuse existing)
```

### 2.2 Configuration

#### Location: `IkeaDocuScan.ActionReminderService/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IkeaDocuScan;Integrated Security=true;TrustServerCertificate=True;"
  },
  "ActionReminderService": {
    "RunTime": "08:00:00",
    "EnableEmailNotifications": true,
    "DefaultRecipients": [
      "legal@ikea.com"
    ],
    "EmailSubject": "IKEA DocuScan - Action Reminders Due Today",
    "IncludeWeekends": false,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 60
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": 25,
    "UseSsl": false,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromAddress": "noreply-docuscan@company.com",
    "FromDisplayName": "IKEA DocuScan - Action Reminder Service",
    "TimeoutSeconds": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "Console": {
      "FormatterName": "simple",
      "FormatterOptions": {
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
      }
    },
    "EventLog": {
      "SourceName": "IkeaDocuScan.ActionReminderService",
      "LogName": "Application",
      "LogLevel": {
        "Default": "Warning"
      }
    }
  }
}
```

#### Configuration Options Class

```
Location: IkeaDocuScan.Shared/Configuration/ActionReminderServiceOptions.cs

public class ActionReminderServiceOptions
{
    public TimeSpan RunTime { get; set; } = TimeSpan.FromHours(8); // 8:00 AM
    public bool EnableEmailNotifications { get; set; } = true;
    public List<string> DefaultRecipients { get; set; } = new();
    public string EmailSubject { get; set; } = "Action Reminders Due Today";
    public bool IncludeWeekends { get; set; } = false;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
}
```

### 2.3 Worker Service Implementation

#### Location: `IkeaDocuScan.ActionReminderService/Worker.cs`

```
Purpose: Background worker that runs on schedule

Implementation:
  Base Class: BackgroundService

  Dependencies:
    - ILogger<Worker> _logger
    - IServiceProvider _serviceProvider
    - IConfiguration _configuration
    - ActionReminderServiceOptions _options

  Fields:
    - Timer? _timer
    - DateTime _lastRunDate

  ExecuteAsync(CancellationToken stoppingToken):
    1. Log service startup
    2. Load configuration
    3. Calculate next run time based on _options.RunTime
    4. Schedule timer to run at configured time daily
    5. While (!stoppingToken.IsCancellationRequested):
       - Wait for next scheduled time
       - If weekend and !IncludeWeekends: Skip
       - await ProcessActionRemindersAsync()
       - Calculate next run time (+24 hours)
       - Update _lastRunDate

  ProcessActionRemindersAsync():
    1. Log start of processing
    2. Using (var scope = _serviceProvider.CreateScope()):
       - Resolve scoped services:
         * IActionReminderService
         * IEmailService
       - Try:
         a. Get today's due actions:
            actions = await _actionReminderService.GetActionsDueOnDateAsync(DateTime.Today)
         b. If actions.Count > 0:
            - Log count
            - await SendEmailNotificationsAsync(actions)
         c. Else:
            - Log "No actions due today"
       - Catch (Exception ex):
         a. Log error with full details
         b. Retry logic (up to MaxRetryAttempts):
            - Wait RetryDelaySeconds
            - Retry operation
         c. If all retries fail:
            - Log critical error
            - Consider sending alert email to admin

  SendEmailNotificationsAsync(List<ActionReminderDto> actions):
    1. Generate email body from template
    2. Create HTML table of actions
    3. For each recipient in DefaultRecipients:
       - Try:
         a. await _emailService.SendEmailAsync(
              to: recipient,
              subject: _options.EmailSubject,
              body: emailBody,
              isHtml: true
            )
         b. Log success
       - Catch (Exception ex):
         a. Log error for specific recipient
         b. Continue with next recipient

  StartAsync(CancellationToken cancellationToken):
    1. Call base.StartAsync()
    2. Log service started
    3. Log next scheduled run time

  StopAsync(CancellationToken cancellationToken):
    1. Log service stopping
    2. Dispose timer
    3. Call base.StopAsync()
```

### 2.4 Email Template

#### Location: `IkeaDocuScan.ActionReminderService/Templates/ActionReminderEmailTemplate.cs`

```
Purpose: Generate HTML email body for action reminders

Static Class: ActionReminderEmailTemplate

Methods:
  GenerateEmailBody(List<ActionReminderDto> actions, DateTime date):
    Returns: string (HTML)

    Template Structure:
      - Header: IKEA branding, DocuScan logo
      - Introduction paragraph:
        "You have {count} action reminder(s) due on {date:dd/MM/yyyy}"
      - Summary section:
        * Total actions
        * Earliest action date (if overdue included)
        * Document types involved
      - Actions table (HTML):
        Columns:
          1. Barcode (link to document preview if possible)
          2. Document Type
          3. Document Name
          4. Counterparty
          5. Action Date
          6. Action Description
        Styling:
          - IKEA blue (#0051BA) header
          - Alternating row colors
          - Mobile-responsive
          - Overdue rows highlighted in red
      - Footer:
        * Link to Action Reminders page
        * Contact information
        * Unsubscribe note (if applicable)

  HTML Template Features:
    - Inline CSS (email clients compatibility)
    - Mobile responsive (@media queries)
    - Table layout (better email client support)
    - Plain text fallback version
    - Action links to web application (with auth consideration)
```

### 2.5 Service Registration and DI

#### Location: `IkeaDocuScan.ActionReminderService/Program.cs`

```
Host Builder Configuration:

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Shared.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "IkeaDocuScan Action Reminder Service";
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Configuration
        services.Configure<ActionReminderServiceOptions>(
            hostContext.Configuration.GetSection("ActionReminderService"));
        services.Configure<EmailOptions>(
            hostContext.Configuration.GetSection("Email"));

        // Database
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(
                hostContext.Configuration.GetConnectionString("DefaultConnection")));

        // Services (Scoped - will be resolved in worker scope)
        services.AddScoped<IActionReminderService, ActionReminderService>();
        services.AddScoped<IEmailService, EmailService>();

        // Worker
        services.AddHostedService<Worker>();

        // Logging
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddEventLog(); // Windows Event Log
        });
    })
    .Build();

await host.RunAsync();
```

### 2.6 Service Implementation (Background)

#### Location: `IkeaDocuScan.ActionReminderService/Services/ActionReminderBackgroundService.cs`

```
Purpose: Provides action reminder functionality specific to background service

Note: Reuses server-side ActionReminderService from IkeaDocuScan-Web
      This ensures business logic consistency between web UI and background service

Implementation Strategy:
  - Reference IkeaDocuScan-Web project assembly
  - OR: Move ActionReminderService to IkeaDocuScan.Shared for sharing
  - OR: Duplicate service implementation (not recommended)

Recommended: Move to Shared
  1. Move IActionReminderService to IkeaDocuScan.Shared/Interfaces/
  2. Move ActionReminderService to IkeaDocuScan.Shared/Services/
  3. Both web and background projects reference Shared
  4. Register service in both DI containers

Benefits:
  - Single source of truth for business logic
  - Easier testing
  - Consistent behavior across components
  - Shared DTOs and interfaces
```

### 2.7 Logging Strategy

#### Logging Targets

```
1. Console Output:
   - All log levels during development
   - Info and above in production
   - Formatted with timestamps

2. Windows Event Log:
   - Warning and above
   - Source: "IkeaDocuScan.ActionReminderService"
   - Application log

3. File Logging (Optional):
   - Use Serilog or NLog
   - Rolling file with date
   - Location: C:\Logs\IkeaDocuScan\ActionReminder\
   - Retention: 30 days

Log Events:
  - Service Start/Stop
  - Scheduled run time
  - Action count retrieved
  - Email send success/failure
  - Errors and exceptions (with stack trace)
  - Configuration changes
  - Retry attempts
```

### 2.8 Error Handling

#### Error Scenarios

```
1. Database Connection Failure:
   - Log error
   - Retry with exponential backoff
   - Alert admin after max retries
   - Service continues to next scheduled run

2. Email Send Failure:
   - Log error with recipient details
   - Continue with next recipient
   - Don't fail entire batch
   - Send summary of failures to admin

3. Service Crash:
   - Windows Service Manager auto-restart
   - Configure recovery options:
     * First failure: Restart service (1 minute delay)
     * Second failure: Restart service (5 minute delay)
     * Subsequent failures: Restart service (10 minute delay)
     * Reset fail count after: 1 day

4. Configuration Error:
   - Log critical error
   - Service stops
   - Admin must fix configuration
   - Event log entry created

5. No Recipients Configured:
   - Log warning
   - Skip email sending
   - Continue with next run
```

### 2.9 Windows Service Installation

#### Installation Script

```
Location: IkeaDocuScan.ActionReminderService/install-service.ps1

PowerShell Script:
  # Must run as Administrator
  $serviceName = "IkeaDocuScanActionReminder"
  $displayName = "IKEA DocuScan Action Reminder Service"
  $description = "Sends daily email notifications for document action reminders"
  $binaryPath = "$PSScriptRoot\IkeaDocuScan.ActionReminderService.exe"

  # Check if service exists
  $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

  if ($service) {
      Write-Host "Stopping existing service..."
      Stop-Service -Name $serviceName -Force

      Write-Host "Removing existing service..."
      sc.exe delete $serviceName
      Start-Sleep -Seconds 2
  }

  # Create service
  Write-Host "Installing service..."
  New-Service -Name $serviceName `
              -BinaryPathName $binaryPath `
              -DisplayName $displayName `
              -Description $description `
              -StartupType Automatic

  # Configure recovery options
  sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/300000/restart/600000

  # Start service
  Write-Host "Starting service..."
  Start-Service -Name $serviceName

  Write-Host "Service installed and started successfully!"
  Write-Host "Check Windows Services (services.msc) to verify."
```

#### Uninstallation Script

```
Location: IkeaDocuScan.ActionReminderService/uninstall-service.ps1

PowerShell Script:
  $serviceName = "IkeaDocuScanActionReminder"

  $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

  if ($service) {
      Write-Host "Stopping service..."
      Stop-Service -Name $serviceName -Force

      Write-Host "Removing service..."
      sc.exe delete $serviceName

      Write-Host "Service uninstalled successfully!"
  } else {
      Write-Host "Service not found."
  }
```

### 2.10 Deployment Configuration

#### Publishing Profile

```
Build Configuration:
  Configuration: Release
  Target Framework: net9.0
  Deployment Mode: Self-contained
  Target Runtime: win-x64

Output:
  bin\Release\net9.0\win-x64\publish\

Files:
  - IkeaDocuScan.ActionReminderService.exe
  - appsettings.json
  - appsettings.Production.json
  - All dependencies (DLLs)
  - install-service.ps1
  - uninstall-service.ps1
  - README.md (deployment instructions)
```

#### Deployment Steps

```
1. Pre-deployment:
   - Update appsettings.Production.json with production values
   - Test service locally in console mode
   - Verify database connection
   - Test email sending

2. Build:
   - dotnet publish -c Release -r win-x64 --self-contained

3. Server Preparation:
   - Create installation directory: C:\Program Files\IkeaDocuScan\ActionReminderService\
   - Copy published files to directory
   - Update appsettings.json with server-specific values
   - Verify .NET 9 Runtime installed (or use self-contained)

4. Installation:
   - Run PowerShell as Administrator
   - Navigate to installation directory
   - Execute: .\install-service.ps1
   - Verify service installed: Get-Service IkeaDocuScanActionReminder

5. Configuration:
   - Open Services (services.msc)
   - Configure service properties:
     * Log on as: Network Service or specific account
     * Recovery options (already set by script)

6. Testing:
   - Start service manually
   - Check Windows Event Log for startup messages
   - Verify next scheduled run time logged
   - Manually trigger test email (if possible)

7. Monitoring:
   - Set up Windows Event Log monitoring
   - Create alert for service failures
   - Monitor email delivery
   - Check database performance
```

---

## Configuration

### 3.1 Shared Configuration

#### Email Configuration

```
Location: IkeaDocuScan.Shared/Configuration/EmailOptions.cs

Shared by:
  - Web application (IkeaDocuScan-Web)
  - Background service (IkeaDocuScan.ActionReminderService)

Properties:
  - SmtpHost
  - SmtpPort
  - UseSsl
  - SmtpUsername (encrypted)
  - SmtpPassword (encrypted)
  - FromAddress
  - FromDisplayName
  - TimeoutSeconds

Configuration in appsettings.json for both projects
```

### 3.2 Database Indexes

#### Recommended Indexes

```
Purpose: Improve performance of action reminder queries

Migration: Add_ActionReminder_Indexes

Indexes to Create:
  1. IX_Document_ActionDate
     Table: Document
     Column: ActionDate
     Include: ReceivingDate, ActionDescription
     Filter: WHERE ActionDate IS NOT NULL

  2. IX_Document_ActionDate_ReceivingDate
     Table: Document
     Columns: ActionDate, ReceivingDate
     Include: BarCode, DT_ID, DocumentNameId, CounterPartyId, DocumentNo, Comment
     Filter: WHERE ActionDate IS NOT NULL AND ActionDate >= ReceivingDate

Benefits:
  - Faster filtering by ActionDate
  - Reduced table scans
  - Better performance for daily background job
  - Improved web page load times
```

---

## Testing Strategy

### 4.1 Unit Testing

#### Test Projects

```
New Test Project: IkeaDocuScan.ActionReminderService.Tests
Framework: xUnit
Packages:
  - xunit
  - Moq (mocking)
  - Microsoft.EntityFrameworkCore.InMemory (in-memory database)

Test Classes:
  1. ActionReminderServiceTests
     - GetDueActionsAsync_ReturnsCorrectData
     - GetDueActionsAsync_FiltersCorrectly
     - GetDueActionsAsync_OrdersCorrectly
     - GetActionsDueOnDateAsync_FiltersToSingleDate
     - GetDueActionsCountAsync_ReturnsCorrectCount

  2. WorkerTests
     - Worker_RunsAtScheduledTime
     - Worker_SkipsWeekendsWhenConfigured
     - Worker_HandlesNoActionsGracefully
     - Worker_RetriesOnFailure
     - Worker_StopsGracefully

  3. EmailTemplateTests
     - GenerateEmailBody_CreatesValidHtml
     - GenerateEmailBody_IncludesAllActions
     - GenerateEmailBody_HighlightsOverdueActions
```

### 4.2 Integration Testing

#### Test Scenarios

```
Web UI Tests:
  1. Page Load
     - Page renders without errors
     - Filters load correctly
     - Initial data loads

  2. Filtering
     - Date range filtering works
     - Document type filtering works
     - Counter party filtering works
     - Search text filtering works
     - Combined filters work correctly

  3. Excel Export
     - Export button generates file
     - File contains correct data
     - File formatting is correct
     - File opens in Excel without errors

Background Service Tests:
  1. Service Installation
     - Service installs successfully
     - Service starts without errors
     - Service configuration is correct

  2. Scheduled Execution
     - Service runs at configured time
     - Service processes correct data
     - Service sends emails successfully

  3. Error Handling
     - Service handles database errors
     - Service handles email errors
     - Service retries appropriately
     - Service logs errors correctly
```

### 4.3 Manual Testing Checklist

```
Web UI:
  [ ] Navigate to /action-reminders page
  [ ] Verify page loads without errors
  [ ] Verify filters display correctly
  [ ] Apply date range filter
  [ ] Apply document type filter
  [ ] Apply counter party filter
  [ ] Enter search text
  [ ] Verify data updates after filter
  [ ] Clear filters
  [ ] Verify data resets
  [ ] Click export to Excel
  [ ] Verify Excel file downloads
  [ ] Open Excel file
  [ ] Verify data is correct
  [ ] Verify formatting is correct
  [ ] Test with no results
  [ ] Verify empty state displays

Background Service:
  [ ] Install service
  [ ] Verify service in Services panel
  [ ] Start service
  [ ] Check Event Log for startup
  [ ] Wait for scheduled run OR trigger manually
  [ ] Verify email sent
  [ ] Check email content
  [ ] Verify data accuracy in email
  [ ] Stop service
  [ ] Verify graceful shutdown
  [ ] Uninstall service
  [ ] Verify complete removal
```

---

## Deployment Plan

### 5.1 Deployment Phases

#### Phase 1: Web UI (Week 1)

```
Tasks:
  1. Implement DTOs and service layer
  2. Create API endpoints
  3. Build Blazor page component
  4. Integrate with ExcelPreview
  5. Add navigation menu item
  6. Unit testing
  7. Integration testing
  8. Deploy to staging environment
  9. User acceptance testing
  10. Deploy to production

Deliverables:
  - Action Reminders page accessible at /action-reminders
  - Excel export functionality working
  - Filters functional
  - Unit tests passing
  - Documentation updated
```

#### Phase 2: Background Service (Week 2)

```
Tasks:
  1. Create Worker Service project
  2. Implement Worker class
  3. Create email template
  4. Configure DI and logging
  5. Create installation scripts
  6. Unit testing
  7. Integration testing
  8. Install on staging server
  9. Monitor for one week
  10. Install on production server

Deliverables:
  - Windows Service installed and running
  - Daily emails sending successfully
  - Error handling working
  - Monitoring in place
  - Documentation complete
```

#### Phase 3: Monitoring & Optimization (Week 3)

```
Tasks:
  1. Set up monitoring dashboards
  2. Configure alerts
  3. Analyze performance
  4. Add database indexes if needed
  5. Optimize queries
  6. Fine-tune email templates
  7. Gather user feedback
  8. Make adjustments

Deliverables:
  - Monitoring dashboard
  - Alerts configured
  - Performance baseline established
  - Optimizations implemented
```

### 5.2 Rollback Plan

#### Web UI Rollback

```
Scenario: Critical bug in web UI

Steps:
  1. Disable /action-reminders route in Program.cs
  2. Comment out endpoint registration
  3. Redeploy previous version
  4. Remove menu item temporarily
  5. Notify users via email

Recovery Time: < 30 minutes
```

#### Background Service Rollback

```
Scenario: Service causing issues

Steps:
  1. Stop service: Stop-Service IkeaDocuScanActionReminder
  2. Disable service: Set-Service -StartupType Disabled
  3. Verify service stopped
  4. Resume manual email reminders (temporary)
  5. Fix issues offline
  6. Redeploy when ready

Recovery Time: < 5 minutes
```

### 5.3 Monitoring and Alerts

#### Metrics to Monitor

```
Web UI:
  - Page load time
  - API response time
  - Excel export success rate
  - Error rate
  - User access count

Background Service:
  - Service uptime
  - Email send success rate
  - Email send failure count
  - Processing time
  - Action count trend
  - Error count

Alerts:
  - Service stopped unexpectedly
  - Email send failure (consecutive)
  - Database connection failure
  - Processing time > threshold
  - Error rate > threshold
```

#### Monitoring Tools

```
1. Windows Services
   - Service status monitoring
   - Automatic restart on failure

2. Windows Event Log
   - Service events
   - Error events
   - Warning events

3. Application Insights (if available)
   - Performance metrics
   - Error tracking
   - Custom events

4. Email Monitoring
   - SMTP server logs
   - Delivery reports
   - Bounce tracking

5. Database Monitoring
   - Query performance
   - Index usage
   - Connection pool
```

---

## Future Enhancements

### 6.1 Short-term (3-6 months)

```
1. Custom Email Recipients
   - Allow specifying recipients per document type
   - User preferences for email notifications
   - Unsubscribe functionality

2. Reminder Frequency Options
   - Weekly digest instead of daily
   - Configurable reminder schedule
   - Multiple reminders per action

3. Dashboard Widget
   - Show due actions count on dashboard
   - Quick access to action list
   - Upcoming actions preview

4. Mobile Notifications
   - Push notifications via mobile app
   - SMS notifications (if needed)

5. Action Snooze/Postpone
   - Allow postponing action date
   - Snooze reminder for specific duration
```

### 6.2 Long-term (6-12 months)

```
1. Advanced Scheduling
   - Different schedules for different document types
   - Escalation rules (send to manager if not addressed)
   - Automatic follow-up reminders

2. Action Workflow
   - Assign actions to specific users
   - Track action completion
   - Workflow states (Pending, In Progress, Completed)
   - Comments and updates on actions

3. Reporting
   - Action completion reports
   - Overdue action reports
   - Performance metrics
   - Trend analysis

4. Integration
   - Calendar integration (Outlook, Google)
   - Task management integration (Planner, Jira)
   - Teams/Slack notifications

5. AI-Powered Features
   - Predict action completion time
   - Suggest action priorities
   - Automatic categorization
```

### 6.3 Scalability Considerations

```
Current Implementation:
  - Direct database access from background service
  - Synchronous email sending
  - Single-server deployment

Future Scalability:
  1. Message Queue (RabbitMQ/Azure Service Bus)
     - Decouple email sending from processing
     - Handle high volume of emails
     - Retry failed messages

  2. Distributed Processing
     - Multiple worker instances
     - Load balancing
     - Horizontal scaling

  3. Caching Layer
     - Cache action list for quick access
     - Reduce database load
     - Improve page performance

  4. Background Job Framework
     - Hangfire or Quartz.NET
     - Web-based job monitoring
     - Job retry and failure handling
     - Scheduled job management UI
```

---

## Appendix

### A. File Checklist

#### New Files to Create

```
Shared Layer:
  ☐ IkeaDocuScan.Shared/DTOs/ActionReminders/ActionReminderDto.cs
  ☐ IkeaDocuScan.Shared/DTOs/ActionReminders/ActionReminderSearchRequestDto.cs
  ☐ IkeaDocuScan.Shared/Interfaces/IActionReminderService.cs
  ☐ IkeaDocuScan.Shared/Configuration/ActionReminderServiceOptions.cs

Server Layer:
  ☐ IkeaDocuScan-Web/Services/ActionReminderService.cs
  ☐ IkeaDocuScan-Web/Endpoints/ActionReminderEndpoints.cs

Client Layer:
  ☐ IkeaDocuScan-Web.Client/Services/ActionReminderHttpService.cs
  ☐ IkeaDocuScan-Web.Client/Pages/ActionReminders.razor
  ☐ IkeaDocuScan-Web.Client/Pages/ActionReminders.razor.cs

Background Service:
  ☐ IkeaDocuScan.ActionReminderService/ (entire project)
  ☐ IkeaDocuScan.ActionReminderService/Program.cs
  ☐ IkeaDocuScan.ActionReminderService/Worker.cs
  ☐ IkeaDocuScan.ActionReminderService/Templates/ActionReminderEmailTemplate.cs
  ☐ IkeaDocuScan.ActionReminderService/appsettings.json
  ☐ IkeaDocuScan.ActionReminderService/install-service.ps1
  ☐ IkeaDocuScan.ActionReminderService/uninstall-service.ps1
  ☐ IkeaDocuScan.ActionReminderService/README.md

Database:
  ☐ IkeaDocuScan.Infrastructure/Migrations/Add_ActionReminder_Indexes.cs

Tests:
  ☐ IkeaDocuScan.ActionReminderService.Tests/ (test project)
```

#### Files to Modify

```
Configuration:
  ☐ IkeaDocuScan-Web/Program.cs (register services and endpoints)
  ☐ IkeaDocuScan-Web.Client/Program.cs (register HTTP service)
  ☐ Navigation menu component (add menu item)

Database:
  ☐ Add indexes to Document table (migration)
```

### B. Dependencies Matrix

```
Project Dependencies:

IkeaDocuScan.Shared
  └── (No dependencies)

IkeaDocuScan.Infrastructure
  └── IkeaDocuScan.Shared

IkeaDocuScan-Web
  ├── IkeaDocuScan.Infrastructure
  ├── IkeaDocuScan.Shared
  └── ExcelReporting (existing)

IkeaDocuScan-Web.Client
  └── IkeaDocuScan.Shared

IkeaDocuScan.ActionReminderService
  ├── IkeaDocuScan.Infrastructure
  ├── IkeaDocuScan.Shared
  └── IkeaDocuScan-Web (for shared services) OR move services to Shared
```

### C. Naming Conventions

```
Follow IkeaDocuScan conventions:

Services:
  - Interface: IActionReminderService
  - Implementation: ActionReminderService
  - Client HTTP: ActionReminderHttpService

DTOs:
  - Read: ActionReminderDto
  - Request: ActionReminderSearchRequestDto
  - Suffix pattern: Dto

Endpoints:
  - File: ActionReminderEndpoints.cs
  - Extension Method: MapActionReminderEndpoints

Pages:
  - File: ActionReminders.razor
  - Route: /action-reminders
  - Code-behind: ActionReminders.razor.cs

Configuration:
  - Options class: ActionReminderServiceOptions
  - Section name: "ActionReminderService"
```

### D. SQL Query Reference

```sql
-- Original SQL query for reference (DO NOT USE directly)
-- Use EF Core LINQ equivalent

WITH docs AS (
    SELECT
        d.BarCode,
        dt.DT_Name AS [Document type],
        dn.Name AS [Document name],
        d.DocumentNo AS [Document No],
        cp.Name AS [Counterparty],
        cp.CounterPartyId AS [Counterparty No],
        FORMAT(d.ActionDate, 'dd/MM/yyyy') AS ActionDate,
        FORMAT(d.ReceivingDate, 'dd/MM/yyyy') AS ReceivingDate,
        d.ActionDescription,
        d.Comment
    FROM dbo.Document d
    LEFT JOIN dbo.DocumentType dt ON dt.DT_ID = d.DT_ID
    LEFT JOIN dbo.DocumentName dn ON dn.ID = d.DocumentNameId
    LEFT JOIN dbo.CounterParty cp ON cp.CounterPartyId = d.CounterPartyId
)
SELECT * FROM docs
WHERE docs.ActionDate IS NOT NULL
  AND ActionDate >= ReceivingDate
ORDER BY ActionDate, BarCode
```

---

## Summary

This implementation plan provides a comprehensive blueprint for building the Action Reminder feature following IkeaDocuScan's established architecture and conventions.

**Key Deliverables:**
1. Blazor page for viewing and exporting action reminders
2. Windows Service for automated daily email notifications
3. Shared services for consistent business logic
4. Complete testing coverage
5. Deployment scripts and documentation

**Timeline Estimate:**
- Web UI: 1 week (including testing)
- Background Service: 1 week (including testing)
- Monitoring & Optimization: 1 week
- **Total: 3 weeks**

**Team Requirements:**
- 1 Full-stack .NET Developer
- 1 QA Engineer (for testing)
- DevOps support (for service deployment)

**Success Criteria:**
- Users can view and export due actions
- Daily emails sent reliably at scheduled time
- Performance meets requirements (< 2 second page load)
- Zero data loss or missed notifications
- Comprehensive error handling and logging

---

**Document Version:** 1.0
**Last Updated:** November 4, 2025
**Next Review:** After Phase 1 completion
