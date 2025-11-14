# IkeaDocuScan Action Reminder Service

A Windows Service that sends daily email notifications for documents with actions due in the IkeaDocuScan system.

## Quick Start

### Build

```bash
dotnet publish -c Release -o C:\Services\IkeaDocuScanActionReminder
```

### Configure

Edit `appsettings.json` in the published directory:

1. Update database connection string or provide secrets.encrypted.json from the appsettings encryption tool
2. Set recipient email addresses
3. Configure SMTP settings
4. Set schedule time (default: 08:00)

### Install

Run as Administrator:

```powershell
New-Service -Name "IkeaDocuScanActionReminder" `
    -BinaryPathName "C:\Services\IkeaDocuScanActionReminder\IkeaDocuScan.ActionReminderService.exe" `
    -DisplayName "IKEA DocuScan Action Reminder Service" `
    -StartupType Automatic

Start-Service IkeaDocuScanActionReminder
```

## Features

- ✅ Automated daily execution
- ✅ Email notifications with HTML formatting
- ✅ Database integration via Entity Framework Core
- ✅ Windows Event Log logging
- ✅ Configurable scheduling
- ✅ Error handling and retry logic

## Configuration

Key settings in `appsettings.json`:

```json
{
  "ActionReminderService": {
    "Enabled": true,
    "ScheduleTime": "08:00",
    "RecipientEmails": ["admin@company.com"]
  }
}
```

## Documentation

See [ACTION_REMINDER_SERVICE_GUIDE.md](../ACTION_REMINDER_SERVICE_GUIDE.md) for complete documentation including:

- Detailed installation instructions
- Configuration options
- Monitoring and troubleshooting
- Security considerations
- Management commands

## Development

Run locally for testing:

```bash
dotnet run --environment Development
```

## Architecture

- **Worker**: `ActionReminderWorker.cs` - Background service with scheduling logic
- **Service**: `ActionReminderEmailService.cs` - Fetches due actions and sends emails
- **Email**: `EmailSenderService.cs` - SMTP email sending via MailKit

## Requirements

- .NET 9.0
- SQL Server (IkeaDocuScan database)
- SMTP server access
- Windows Server 2016+ or Windows 10/11

## License

Internal use only - IKEA DocuScan Project
