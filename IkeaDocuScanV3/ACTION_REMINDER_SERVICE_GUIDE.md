# IkeaDocuScan Action Reminder Service

## Overview

The **IkeaDocuScan Action Reminder Service** is a Windows Service that runs in the background to automatically send daily email notifications for documents with actions that are due. This service fetches documents from the IkeaDocuScan database and sends formatted email reminders to configured recipients.

---

## Features

- **Automated Daily Execution** - Runs at a configured time each day
- **Database Integration** - Fetches action reminders directly from the IkeaDocuScan database using Entity Framework Core
- **Email Notifications** - Sends professional HTML-formatted email notifications with action details
- **Configurable Scheduling** - Set the exact time of day to run the service
- **Error Handling** - Robust error handling with logging to Windows Event Log
- **Environment-Specific Configuration** - Separate settings for Development and Production
- **Windows Service Integration** - Runs as a native Windows Service with automatic startup

---

## Architecture

```
Windows Service Host
    ↓
ActionReminderWorker (BackgroundService)
    ↓
ActionReminderEmailService
    ├── Fetches due actions from database (EF Core)
    └── Sends emails via EmailSenderService (MailKit)
```

### Project Structure

```
IkeaDocuScan.ActionReminderService/
├── Program.cs                              # Service host configuration
├── ActionReminderWorker.cs                 # Background worker with scheduling
├── ActionReminderServiceOptions.cs         # Configuration options
├── Services/
│   ├── IActionReminderEmailService.cs      # Service interface
│   ├── ActionReminderEmailService.cs       # Business logic implementation
│   ├── IEmailSender.cs                     # Email sender interface
│   └── EmailSenderService.cs               # MailKit email implementation
├── appsettings.json                        # Base configuration
├── appsettings.Development.json            # Development overrides
└── appsettings.Production.json             # Production overrides
```

---

## Configuration

### appsettings.json

The service requires configuration in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=IkeaDocuScan;Integrated Security=true;TrustServerCertificate=true;"
  },
  "ActionReminderService": {
    "Enabled": true,
    "ScheduleTime": "08:00",
    "CheckIntervalMinutes": 60,
    "RecipientEmails": [
      "docuscan-admin@company.com",
      "operations@company.com"
    ],
    "EmailSubject": "Action Reminders Due Today - {Count} Items",
    "SendEmptyNotifications": false,
    "DaysAhead": 0
  },
  "Email": {
    "SmtpHost": "smtp.company.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "SmtpUsername": "docuscan@company.com",
    "SmtpPassword": "",
    "FromAddress": "noreply-docuscan@company.com",
    "FromDisplayName": "IKEA DocuScan Action Reminder Service",
    "EnableEmailNotifications": true,
    "TimeoutSeconds": 30
  }
}
```

### Configuration Options

#### ActionReminderService Section

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Enabled` | bool | true | Master switch to enable/disable the service |
| `ScheduleTime` | string | "08:00" | Time of day to run (24-hour format HH:mm) |
| `CheckIntervalMinutes` | int | 60 | How often to check if it's time to run (in minutes) |
| `RecipientEmails` | string[] | [] | Email addresses to send action reminders to |
| `EmailSubject` | string | "Action Reminders Due Today - {Count} Items" | Subject line template (use {Count} placeholder) |
| `SendEmptyNotifications` | bool | false | Send notification even when no actions are due |
| `DaysAhead` | int | 0 | Number of days ahead to include (0 = today only) |

#### Email Section

Reuses the same email configuration as the main web application. See [EMAIL_SERVICE_GUIDE.md](EMAIL_SERVICE_GUIDE.md) for details.

---

## Installation

### Prerequisites

- Windows Server 2016 or later (or Windows 10/11 for development)
- .NET 9.0 Runtime (or SDK for building)
- SQL Server with IkeaDocuScan database accessible
- SMTP server access for sending emails

### Step 1: Build the Service

```bash
cd IkeaDocuScan.ActionReminderService
dotnet publish -c Release -o C:\Services\IkeaDocuScanActionReminder
```

This creates a published version of the service in `C:\Services\IkeaDocuScanActionReminder`.

### Step 2: Configure the Service

1. Navigate to the published directory:
   ```bash
   cd C:\Services\IkeaDocuScanActionReminder
   ```

2. Edit `appsettings.json`:
   - Update `ConnectionStrings:DefaultConnection` with your database connection
   - Set `ActionReminderService:RecipientEmails` with your admin email addresses
   - Update `Email` section with your SMTP server details

3. **For Production**: Store sensitive values (SMTP password) in environment variables or use DPAPI encryption:
   ```bash
   # Set environment variable for SMTP password
   setx Email__SmtpPassword "your-secure-password" /M
   ```

### Step 3: Install as Windows Service

Run PowerShell **as Administrator** and execute:

```powershell
# Create Windows Service
New-Service -Name "IkeaDocuScanActionReminder" `
    -BinaryPathName "C:\Services\IkeaDocuScanActionReminder\IkeaDocuScan.ActionReminderService.exe" `
    -DisplayName "IKEA DocuScan Action Reminder Service" `
    -Description "Sends daily email notifications for documents with actions due in IKEA DocuScan" `
    -StartupType Automatic

# Start the service
Start-Service IkeaDocuScanActionReminder

# Verify service is running
Get-Service IkeaDocuScanActionReminder
```

### Step 4: Configure Service Account (Optional but Recommended)

For production, run the service under a dedicated service account with appropriate database permissions:

```powershell
# Stop the service
Stop-Service IkeaDocuScanActionReminder

# Configure service to run under specific account
sc.exe config IkeaDocuScanActionReminder obj= "DOMAIN\ServiceAccount" password= "ServiceAccountPassword"

# Start the service
Start-Service IkeaDocuScanActionReminder
```

**Service Account Permissions Required:**
- Read access to IkeaDocuScan database
- Ability to write to Windows Event Log
- Network access to SMTP server

---

## Monitoring

### Windows Event Log

The service logs all events to Windows Event Log under the source name **"IkeaDocuScan Action Reminder"**.

To view logs:

1. Open **Event Viewer** (eventvwr.msc)
2. Navigate to **Windows Logs → Application**
3. Filter by source: **IkeaDocuScan Action Reminder**

### Log Levels

- **Information**: Normal operation events (service start/stop, email sent)
- **Warning**: Configuration issues, no recipients configured
- **Error**: Email send failures, database connection errors
- **Critical**: Service termination due to unhandled exception

### Common Log Messages

| Level | Message | Description |
|-------|---------|-------------|
| Information | "Action Reminder Worker started at: {Time}" | Service started successfully |
| Information | "Found {Count} action reminder(s) due" | Actions found and processing |
| Information | "Successfully sent action reminder emails" | Emails sent successfully |
| Warning | "Action Reminder Service is DISABLED in configuration" | Service is disabled in config |
| Warning | "No recipient emails configured" | RecipientEmails array is empty |
| Error | "Failed to send email" | SMTP or email sending error |
| Error | "Error in Action Reminder Worker main loop" | Unexpected error during processing |

### Checking Service Status

```powershell
# Check service status
Get-Service IkeaDocuScanActionReminder

# View service details
Get-Service IkeaDocuScanActionReminder | Format-List *

# View recent event logs
Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 20
```

---

## Operation

### Daily Execution Flow

1. **Service Start**: The service starts automatically with Windows (if configured)
2. **Schedule Check**: Every `CheckIntervalMinutes`, the worker checks if it's time to run
3. **Time Match**: When current time passes `ScheduleTime` and hasn't run today yet:
   - Fetches documents from database with `ActionDate = today` (or `<= today + DaysAhead`)
   - Filters documents where `ActionDate >= ReceivingDate` and `ActionDate` is not null
   - Builds HTML and plain text email with action details
   - Sends email to all configured recipients
   - Records last run date to prevent duplicate runs
4. **Wait**: Returns to schedule check loop

### Email Format

The service sends professional HTML emails with:

- **Header**: "IKEA DocuScan - Action Reminders Due Today"
- **Table**: All due actions with columns:
  - Barcode
  - Document Type
  - Document Name
  - Document No
  - Counterparty
  - Action Date
  - Receiving Date
  - Action Description
- **Styling**: IKEA brand colors (Blue #0051BA, Yellow #FFDA1A)
- **Plain Text**: Alternative plain text version for email clients that don't support HTML

---

## Management Commands

### Starting and Stopping

```powershell
# Start service
Start-Service IkeaDocuScanActionReminder

# Stop service
Stop-Service IkeaDocuScanActionReminder

# Restart service
Restart-Service IkeaDocuScanActionReminder
```

### Uninstalling

```powershell
# Stop service
Stop-Service IkeaDocuScanActionReminder

# Remove service
Remove-Service IkeaDocuScanActionReminder

# Or using sc.exe
sc.exe delete IkeaDocuScanActionReminder
```

### Updating the Service

To update the service after code changes:

```bash
# 1. Build new version
cd IkeaDocuScan.ActionReminderService
dotnet publish -c Release -o C:\Services\IkeaDocuScanActionReminder_NEW

# 2. Stop service (PowerShell as Administrator)
Stop-Service IkeaDocuScanActionReminder

# 3. Backup old version
Move-Item C:\Services\IkeaDocuScanActionReminder C:\Services\IkeaDocuScanActionReminder_BACKUP

# 4. Deploy new version
Move-Item C:\Services\IkeaDocuScanActionReminder_NEW C:\Services\IkeaDocuScanActionReminder

# 5. Restore configuration
Copy-Item C:\Services\IkeaDocuScanActionReminder_BACKUP\appsettings.json C:\Services\IkeaDocuScanActionReminder\

# 6. Start service
Start-Service IkeaDocuScanActionReminder
```

---

## Troubleshooting

### Service Won't Start

**Check Event Log**:
```powershell
Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 10
```

**Common Issues**:
- **Database connection failed**: Verify connection string in `appsettings.json`
- **Service account permissions**: Ensure account has DB read access
- **Missing .NET runtime**: Install .NET 9.0 Runtime
- **Configuration error**: Check JSON syntax in appsettings.json

### Emails Not Sending

**Check Configuration**:
- Verify `Email:EnableEmailNotifications` is `true`
- Verify `Email:SmtpHost`, `Email:SmtpPort` are correct
- Verify `ActionReminderService:RecipientEmails` contains valid addresses
- Test SMTP credentials manually

**Verify SMTP Settings**:
```powershell
# Test SMTP connection (PowerShell)
$smtpServer = "smtp.company.com"
$smtpPort = 587
Test-NetConnection -ComputerName $smtpServer -Port $smtpPort
```

### No Actions Found When Expected

**Check Database**:
```sql
-- Verify documents with actions due today
SELECT
    BarCode,
    ActionDate,
    ActionDescription
FROM Document
WHERE ActionDate = CAST(GETDATE() AS DATE)
    AND ActionDate >= ReceivingDate
    AND ActionDate IS NOT NULL
```

**Check Configuration**:
- Verify `ActionReminderService:DaysAhead` setting (0 = today only)
- Check if service has run today already (only runs once per day)

### Service Runs at Wrong Time

**Check Configuration**:
- Verify `ActionReminderService:ScheduleTime` format (HH:mm, 24-hour)
- Check server time zone settings
- Review `CheckIntervalMinutes` setting

---

## Development

### Running in Development

For development and testing, you can run the service as a console application:

```bash
cd IkeaDocuScan.ActionReminderService
dotnet run --environment Development
```

**Development Configuration** (`appsettings.Development.json`):
- More frequent checks (every 5 minutes)
- Email notifications disabled by default
- Verbose logging enabled
- Sends empty notifications for testing

### Testing

To test the service without waiting for the scheduled time:

1. Set `ScheduleTime` to a few minutes in the future
2. Set `CheckIntervalMinutes` to 1
3. Set `SendEmptyNotifications` to true
4. Run the service and monitor logs

### Debugging

1. Run Visual Studio as Administrator
2. Open IkeaDocuScanV3.sln
3. Set `IkeaDocuScan.ActionReminderService` as startup project
4. Set breakpoints in `ActionReminderWorker.cs` or `ActionReminderEmailService.cs`
5. Press F5 to start debugging

---

## Security Considerations

### Sensitive Configuration

- **Never commit SMTP passwords** to source control
- Use environment variables for production secrets
- Consider using Windows DPAPI for encryption
- Restrict file system permissions on published directory

### Service Account Security

- Use a dedicated service account with minimum required permissions
- Grant only SELECT permission on required database tables
- Regularly rotate service account passwords
- Monitor service account activity

### Email Security

- Use TLS/SSL for SMTP connections (`UseSsl: true`)
- Authenticate with SMTP server (`SmtpUsername`/`SmtpPassword`)
- Validate recipient email addresses
- Include unsubscribe option if required by policy

---

## Performance

### Resource Usage

- **Memory**: ~50-100 MB typical
- **CPU**: Minimal (only active during scheduled run, typically < 5 seconds)
- **Network**: Depends on email size and recipient count
- **Database**: Read-only queries, minimal impact

### Scalability

- Can handle 1000+ action reminders per run
- Email sending is sequential (not parallelized)
- Consider batch size limits for SMTP server

---

## Support

### Log Files Location

- Windows Event Log: `Application` log, source `IkeaDocuScan Action Reminder`
- Service binaries: `C:\Services\IkeaDocuScanActionReminder`
- Configuration: `C:\Services\IkeaDocuScanActionReminder\appsettings.json`

### Contact

For issues or questions:
- Check Event Viewer logs first
- Review this documentation
- Contact IT Support with Event Log details

---

## Version History

### Version 1.0 (2024)
- Initial release
- Daily scheduled email notifications
- HTML formatted emails with action details
- Windows Service integration
- Entity Framework Core database access
- MailKit SMTP email sending
