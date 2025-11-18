# Action Reminder Windows Service - Deployment Guide

**Service Name:** IkeaDocuScan Action Reminder Service
**Target Server:** Windows Server 2022 Standard
**Framework:** .NET 10.0
**Service Type:** Windows Background Service
**Installation Location:** D:\IkeaDocuScan\Tools\ActionReminder\
**Server Network:** No Internet Access (Air-Gapped Environment)
**Last Updated:** 2025-01-18

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Service Architecture](#service-architecture)
4. [Build and Package Service](#build-and-package-service)
5. [Deploy to Server](#deploy-to-server)
6. [Service Configuration](#service-configuration)
7. [Install as Windows Service](#install-as-windows-service)
8. [Testing and Verification](#testing-and-verification)
9. [Service Management](#service-management)
10. [Troubleshooting](#troubleshooting)
11. [Monitoring](#monitoring)

---

## Overview

The **IkeaDocuScan Action Reminder Service** is a .NET 10.0 Windows background service that automatically sends daily email notifications for documents with upcoming action dates.

### Key Features

✅ **Scheduled Daily Execution** - Runs once per day at configured time (default: 08:00)
✅ **Database Integration** - Fetches due actions from SQL Server database
✅ **Email Notifications** - Sends formatted HTML emails to configured recipients
✅ **Database-Driven Configuration** - Supports dynamic configuration without restarts
✅ **Custom Email Templates** - Customizable templates with placeholder support
✅ **Windows Event Log Integration** - Logs to Windows Event Log for centralized monitoring
✅ **Automatic Retry** - Resilient error handling with automatic retry logic

### How It Works

1. Service starts and waits for configured schedule time (e.g., 08:00)
2. At schedule time, queries database for documents with `ActionDate` = today (or today + `DaysAhead`)
3. If reminders found, loads email template from database (or uses default)
4. Sends email to configured recipients with list of due actions
5. Logs execution results to Windows Event Log and application logs
6. Waits until next day to repeat

---

## Prerequisites

### Server Requirements

- [ ] **Windows Server 2022** (or Windows 10/11 for development)
- [ ] **.NET 10.0 Runtime** installed (already installed in main deployment - offline)
- [ ] **SQL Server** access to `IkeaDocuScan` database (internal network)
- [ ] **SMTP Server** access (smtp-gw.ikea.com) (internal network)
- [ ] **Administrator rights** to install Windows Service
- [ ] **NO internet access** - service package must be transferred manually

### Required Permissions

- **File System**: Read/Write access to `D:\IkeaDocuScan\Tools\ActionReminder\`
- **Database**: Read access to `IkeaDocuScan` database (ActionReminder, Document tables)
- **SMTP**: Network connectivity to SMTP server
- **Event Log**: Write access to Windows Application Event Log (automatic with service account)

### Dependencies

- Main IkeaDocuScan application must be deployed first
- Database must be migrated and accessible
- SQL user `docuscanch` must have read access to required tables

---

## Service Architecture

### Service Components

```
IkeaDocuScan.ActionReminderService/
├── IkeaDocuScan.ActionReminderService.dll  # Main service executable
├── ActionReminderWorker.cs                 # Background worker (scheduled task)
├── Services/
│   └── ActionReminderEmailService.cs       # Email composition and sending
├── ActionReminderServiceOptions.cs         # Configuration model
└── appsettings.json                        # Service configuration

Dependencies:
├── IkeaDocuScan.Infrastructure.dll         # Database access (EF Core)
├── IkeaDocuScan.Shared.dll                 # DTOs and interfaces
├── MailKit.dll                             # Email sending
└── Microsoft.EntityFrameworkCore.dll       # Database ORM
```

### Service Flow

```
┌─────────────────────────────────────────────────────────────┐
│                   Service Start                             │
│  1. Load configuration from appsettings.json                │
│  2. Connect to database                                     │
│  3. Test SMTP connection (optional)                         │
│  4. Calculate next run time based on ScheduleTime           │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Wait for Schedule                         │
│  - Check every [CheckIntervalMinutes] (default: 60 min)    │
│  - Wait until current time >= ScheduleTime                  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Execute Reminder Job                      │
│  1. Query database for documents with ActionDate due       │
│  2. Load email template (from DB or use default)           │
│  3. Compose email with document list                        │
│  4. Send email to configured recipients                     │
│  5. Log results to Event Log                                │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Wait for Next Day                         │
│  - Calculate next run time (same time tomorrow)             │
│  - Sleep until next scheduled time                          │
│  - Repeat daily                                             │
└─────────────────────────────────────────────────────────────┘
```

### Database Query

The service queries documents with the following logic:

```sql
-- Default query (DaysAhead = 0)
SELECT BarCode, DocumentNo, ActionDate, ActionDescription, CounterPartyName
FROM Documents d
LEFT JOIN CounterParty cp ON d.CounterPartyId = cp.Id
WHERE ActionDate = CAST(GETDATE() AS DATE)
  AND ActionDate >= ReceivingDate
ORDER BY ActionDate, BarCode;

-- With DaysAhead = 2
SELECT BarCode, DocumentNo, ActionDate, ActionDescription, CounterPartyName
FROM Documents d
LEFT JOIN CounterParty cp ON d.CounterPartyId = cp.Id
WHERE ActionDate >= CAST(GETDATE() AS DATE)
  AND ActionDate <= DATEADD(DAY, 2, CAST(GETDATE() AS DATE))
  AND ActionDate >= ReceivingDate
ORDER BY ActionDate, BarCode;
```

---

## Build and Package Service

### Step 1: Build Service on Development Machine

**Using Visual Studio 2022:**

1. Open solution: `IkeaDocuScanV3.sln`
2. Locate project: `IkeaDocuScan.ActionReminderService`
3. Right-click project → **Properties**
4. Verify **Target Framework**: `net10.0`
5. Close properties
6. Build → Configuration Manager → Set to **Release**
7. Right-click project → **Rebuild**

**Verify build succeeded:**
```
Build started...
1>------ Rebuild All started: Project: IkeaDocuScan.ActionReminderService ------
1>IkeaDocuScan.ActionReminderService -> D:\...\bin\Release\net10.0\IkeaDocuScan.ActionReminderService.dll
========== Rebuild All: 1 succeeded, 0 failed, 0 skipped ==========
```

### Step 2: Publish Service

**Using Visual Studio:**

1. Right-click `IkeaDocuScan.ActionReminderService` project → **Publish**
2. Target: **Folder**
3. Location: `C:\Publish\ActionReminder\`
4. Click **Finish**
5. Click **Show all settings**

**Configure Publish Profile:**

| Setting | Value |
|---------|-------|
| Configuration | Release |
| Target Framework | net10.0 |
| Deployment Mode | Framework-dependent |
| Target Runtime | win-x64 |
| File Publish Options | ☑ Produce single file |

6. Click **Save**
7. Click **Publish**

**Using Command Line (Alternative):**

```powershell
# Navigate to project directory
cd D:\ProjectSource\IkeaDocuScanV3\IkeaDocuScan.ActionReminderService

# Publish for Windows x64
dotnet publish -c Release -r win-x64 --self-contained false -o C:\Publish\ActionReminder\

Write-Host "✅ Service published successfully" -ForegroundColor Green
```

### Step 3: Verify Published Files

```powershell
cd C:\Publish\ActionReminder

# List published files
Get-ChildItem | Select-Object Name, Length

# Expected files:
# IkeaDocuScan.ActionReminderService.exe  (main executable)
# IkeaDocuScan.ActionReminderService.dll
# IkeaDocuScan.Infrastructure.dll
# IkeaDocuScan.Shared.dll
# appsettings.json
# appsettings.Production.json (if exists)
# All dependency DLLs (MailKit, MimeKit, Microsoft.EntityFrameworkCore, etc.)
```

**Verify Service Executable:**

```powershell
# Check if executable exists
if (Test-Path ".\IkeaDocuScan.ActionReminderService.exe") {
    Write-Host "✅ Service executable found" -ForegroundColor Green

    # Check file version
    $version = (Get-Item ".\IkeaDocuScan.ActionReminderService.exe").VersionInfo
    Write-Host "   Version: $($version.FileVersion)" -ForegroundColor Cyan
    Write-Host "   Product: $($version.ProductName)" -ForegroundColor Cyan
} else {
    Write-Host "❌ Service executable NOT found!" -ForegroundColor Red
}
```

### Step 4: Create Deployment Package

```powershell
# Create ZIP file for transfer to server
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$version = "v1.0" # Update as appropriate
$zipName = "ActionReminderService_${version}_${timestamp}.zip"
$zipPath = "C:\Publish\$zipName"

# Create ZIP archive
Compress-Archive -Path "C:\Publish\ActionReminder\*" -DestinationPath $zipPath -Force

Write-Host "✅ Deployment package created:" -ForegroundColor Green
Write-Host "   File: $zipPath" -ForegroundColor Cyan
Write-Host "   Size: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Cyan

# Create MD5 checksum
$md5 = Get-FileHash -Path $zipPath -Algorithm MD5
$md5.Hash | Out-File "$zipPath.md5"

Write-Host "   MD5: $($md5.Hash)" -ForegroundColor Cyan
```

### Step 5: Transfer to Server (Offline Transfer)

**⚠️ IMPORTANT: Server has NO internet access - manual transfer required**

Choose one of the following methods:

**Method 1: USB Flash Drive (Recommended for Air-Gapped)**
```powershell
# On Development Machine:
$zipPath = "C:\Publish\ActionReminderService_v1.0_20250118_150000.zip"
$usbDrive = "E:\" # Adjust drive letter as needed

Copy-Item -Path $zipPath -Destination $usbDrive
Copy-Item -Path "$zipPath.md5" -Destination $usbDrive

Write-Host "✅ Files copied to USB drive" -ForegroundColor Green

# On Server:
$usbDrive = "E:\" # Adjust drive letter as needed
$destPath = "D:\IkeaDocuScan\Deployment\Archives\"

Copy-Item -Path "$usbDrive\ActionReminderService*.zip" -Destination $destPath
Copy-Item -Path "$usbDrive\*.md5" -Destination $destPath

Write-Host "✅ Service package transferred to server" -ForegroundColor Green
```

**Method 2: RDP Clipboard Copy/Paste**
1. Copy ZIP file on development machine (Ctrl+C)
2. RDP to server
3. Navigate to `D:\IkeaDocuScan\Deployment\Archives\`
4. Paste (Ctrl+V)
5. Wait for transfer to complete

**Method 3: Internal Network Share (If Available)**
```powershell
# From development machine (must be on same internal network)
$zipPath = "C:\Publish\ActionReminderService_v1.0_20250118_150000.zip"
$serverShare = "\\SERVER-NAME\D$\IkeaDocuScan\Deployment\Archives\"

# Test accessibility first
if (Test-Path $serverShare) {
    Copy-Item -Path $zipPath -Destination $serverShare
    Copy-Item -Path "$zipPath.md5" -Destination $serverShare
    Write-Host "✅ Files copied via network share" -ForegroundColor Green
} else {
    Write-Host "❌ Network share not accessible - use USB or RDP method" -ForegroundColor Red
}
```

---

## Deploy to Server

### Step 1: Extract Deployment Package

**On the Server (PowerShell as Administrator):**

```powershell
# Find the uploaded ZIP file
$zipFile = Get-ChildItem "D:\IkeaDocuScan\Deployment\Archives" -Filter "ActionReminderService*.zip" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($zipFile) {
    Write-Host "Found deployment package: $($zipFile.Name)" -ForegroundColor Cyan

    # Verify MD5 checksum (if available)
    $md5File = "$($zipFile.FullName).md5"
    if (Test-Path $md5File) {
        $expectedMd5 = Get-Content $md5File
        $actualMd5 = (Get-FileHash -Path $zipFile.FullName -Algorithm MD5).Hash

        if ($expectedMd5 -eq $actualMd5) {
            Write-Host "✅ MD5 checksum verified" -ForegroundColor Green
        } else {
            Write-Host "❌ MD5 checksum mismatch!" -ForegroundColor Red
            exit
        }
    }

    # Extract to ActionReminder directory
    $extractPath = "D:\IkeaDocuScan\Tools\ActionReminder"

    # Backup existing deployment (if exists)
    if (Test-Path "$extractPath\IkeaDocuScan.ActionReminderService.exe") {
        $backupPath = "D:\Backups\IkeaDocuScan\ActionReminder_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
        New-Item -Path $backupPath -ItemType Directory -Force
        Copy-Item -Path "$extractPath\*" -Destination $backupPath -Recurse -Force
        Write-Host "✅ Existing service backed up to: $backupPath" -ForegroundColor Yellow
    }

    # Clear destination
    if (Test-Path $extractPath) {
        Remove-Item "$extractPath\*" -Recurse -Force -ErrorAction SilentlyContinue
    }

    # Extract ZIP
    Expand-Archive -Path $zipFile.FullName -DestinationPath $extractPath -Force

    Write-Host "✅ Service extracted to: $extractPath" -ForegroundColor Green

    # Verify extraction
    Write-Host "`nExtracted files:" -ForegroundColor Cyan
    Get-ChildItem $extractPath | Select-Object Name, Length
} else {
    Write-Host "❌ No ActionReminderService ZIP file found in Archives!" -ForegroundColor Red
}
```

### Step 2: Verify Service Files

```powershell
$servicePath = "D:\IkeaDocuScan\Tools\ActionReminder"

# Check critical files
$criticalFiles = @(
    "IkeaDocuScan.ActionReminderService.exe",
    "IkeaDocuScan.ActionReminderService.dll",
    "IkeaDocuScan.Infrastructure.dll",
    "IkeaDocuScan.Shared.dll",
    "appsettings.json",
    "MailKit.dll",
    "MimeKit.dll"
)

Write-Host "Verifying critical files:" -ForegroundColor Cyan

$allFilesPresent = $true
foreach ($file in $criticalFiles) {
    $filePath = Join-Path $servicePath $file
    if (Test-Path $filePath) {
        Write-Host "  ✅ $file" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $file - MISSING!" -ForegroundColor Red
        $allFilesPresent = $false
    }
}

if ($allFilesPresent) {
    Write-Host "`n✅ All critical files present" -ForegroundColor Green
} else {
    Write-Host "`n❌ Some critical files are missing - deployment may fail!" -ForegroundColor Red
}
```

---

## Service Configuration

### Step 1: Review Default Configuration

```powershell
$configPath = "D:\IkeaDocuScan\Tools\ActionReminder\appsettings.json"

Write-Host "Default configuration:" -ForegroundColor Cyan
Get-Content $configPath | Out-String
```

### Step 2: Create Production Configuration

Create `appsettings.Production.json` with server-specific settings:

```powershell
$servicePath = "D:\IkeaDocuScan\Tools\ActionReminder"
$prodConfig = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=wtseelm-nx20541.ikeadt.com;Database=IkeaDocuScan;User Id=docuscanch;Password=YourSecurePassword123!;TrustServerCertificate=True;"
  },

  "ActionReminderService": {
    "Enabled": true,
    "ScheduleTime": "08:00",
    "CheckIntervalMinutes": 60,
    "RecipientEmails": [
      "docuscan-admin@ikea.com",
      "legal-team@ikea.com"
    ],
    "EmailSubject": "Action Reminders Due Today - {Count} Items",
    "SendEmptyNotifications": false,
    "DaysAhead": 0
  },

  "Email": {
    "SmtpHost": "smtp-gw.ikea.com",
    "SmtpPort": 25,
    "SecurityMode": "None",
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromAddress": "noreply-docuscan@ikea.com",
    "FromDisplayName": "IKEA DocuScan Action Reminder Service",
    "EnableEmailNotifications": true,
    "TimeoutSeconds": 30
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "IkeaDocuScan.ActionReminderService": "Information"
    }
  }
}
"@

# Save production configuration
$prodConfigPath = "$servicePath\appsettings.Production.json"
$prodConfig | Out-File -FilePath $prodConfigPath -Encoding UTF8 -Force

Write-Host "✅ Production configuration created" -ForegroundColor Green

# Backup configuration
Copy-Item $prodConfigPath -Destination "D:\Backups\IkeaDocuScan\Config\ActionReminder_appsettings.Production.json_$(Get-Date -Format 'yyyyMMdd_HHmmss')" -Force

Write-Host "✅ Configuration backed up" -ForegroundColor Green
```

**⚠️ IMPORTANT:** Update the following values:
- `ConnectionStrings:DefaultConnection` - Use correct password for `docuscanch`
- `ActionReminderService:RecipientEmails` - Add actual recipient email addresses
- `ActionReminderService:ScheduleTime` - Set desired daily execution time (24-hour format)

**Edit configuration manually:**

```powershell
notepad "$servicePath\appsettings.Production.json"
```

### Step 3: Configuration Parameters Explained

**ActionReminderService Section:**

| Parameter | Description | Example Values |
|-----------|-------------|----------------|
| `Enabled` | Master switch for the service | `true` / `false` |
| `ScheduleTime` | Time of day to send reminders (HH:mm) | `"08:00"`, `"07:30"`, `"13:00"` |
| `CheckIntervalMinutes` | How often to check if schedule time reached | `60` (check every hour) |
| `RecipientEmails` | Array of email addresses to receive notifications | `["user1@ikea.com", "user2@ikea.com"]` |
| `EmailSubject` | Email subject line (`{Count}` = number of reminders) | `"Action Reminders Due Today - {Count} Items"` |
| `SendEmptyNotifications` | Send email even when no reminders due | `false` (recommended) |
| `DaysAhead` | Look ahead X days (0 = today only) | `0`, `1`, `2` |

**Email Section:**

| Parameter | Description | Example Values |
|-----------|-------------|----------------|
| `SmtpHost` | SMTP server hostname | `"smtp-gw.ikea.com"` |
| `SmtpPort` | SMTP server port | `25`, `587`, `465` |
| `SecurityMode` | SMTP encryption mode | `"None"`, `"StartTls"`, `"SslOnConnect"`, `"Auto"` |
| `SmtpUsername` | SMTP authentication username (if required) | `""` (empty for no auth) |
| `SmtpPassword` | SMTP authentication password (if required) | `""` (empty for no auth) |
| `FromAddress` | Sender email address | `"noreply-docuscan@ikea.com"` |
| `FromDisplayName` | Sender display name | `"IKEA DocuScan Action Reminder Service"` |
| `EnableEmailNotifications` | Master switch for email sending | `true` / `false` |
| `TimeoutSeconds` | SMTP operation timeout | `30` |

**Example Configurations:**

**Daily reminders at 8:00 AM:**
```json
{
  "ScheduleTime": "08:00",
  "DaysAhead": 0
}
```

**Daily reminders at 7:30 AM, look ahead 2 days:**
```json
{
  "ScheduleTime": "07:30",
  "DaysAhead": 2
}
```

**Multiple recipients:**
```json
{
  "RecipientEmails": [
    "legal-team@ikea.com",
    "finance@ikea.com",
    "admin@ikea.com"
  ]
}
```

### Step 4: Set File Permissions

```powershell
$servicePath = "D:\IkeaDocuScan\Tools\ActionReminder"

# Grant read & execute to service directory
# Note: Service will run as LocalService or domain service account

# For LocalService account:
icacls $servicePath /grant "NT AUTHORITY\LocalService:(OI)(CI)(RX)"

# Grant modify to allow service to write logs (if file logging is configured)
# icacls "$servicePath\logs" /grant "NT AUTHORITY\LocalService:(OI)(CI)(M)"

Write-Host "✅ File permissions set for LocalService account" -ForegroundColor Green
Write-Host "   If using domain service account, update permissions accordingly" -ForegroundColor Yellow
```

---

## Install as Windows Service

### Step 1: Choose Service Account

**Option A: LocalService Account (Simple)**
- Pros: Easy to configure, no password management
- Cons: May have limited network access, may require SQL Server configuration

**Option B: Domain Service Account (Recommended for Production)**
- Pros: Better control, easier database access, can access network resources
- Cons: Requires password management, must be configured in Active Directory

**For this deployment, we'll use Option B (Domain Service Account).**

### Step 2: Create or Identify Service Account

**Create dedicated service account in Active Directory:**

1. Open **Active Directory Users and Computers**
2. Navigate to appropriate OU
3. Create new user: `svc_docuscan_reminder`
4. Set password (strong password, never expires)
5. Add to appropriate groups if needed

**Or use existing service account:**
- `IKEADT\svc_docuscan`

### Step 3: Grant Database Access to Service Account

**Connect to SQL Server (SSMS):**

```sql
USE master;
GO

-- Create login for service account (Windows Authentication)
CREATE LOGIN [IKEADT\svc_docuscan_reminder] FROM WINDOWS;
GO

USE IkeaDocuScan;
GO

-- Create database user
CREATE USER [svc_docuscan_reminder] FOR LOGIN [IKEADT\svc_docuscan_reminder];
GO

-- Grant read permissions
ALTER ROLE db_datareader ADD MEMBER [svc_docuscan_reminder];
GO

-- Verify permissions
EXEC sp_helpuser 'svc_docuscan_reminder';
GO

-- Test access
EXECUTE AS USER = 'svc_docuscan_reminder';

SELECT COUNT(*) FROM Document;
SELECT COUNT(*) FROM ActionReminder;

REVERT;
GO
```

**If using SQL Authentication (Alternative):**

Use the existing `docuscanch` user - already has necessary permissions.

### Step 4: Create Windows Service

**Using PowerShell (Recommended):**

```powershell
$serviceName = "IkeaDocuScanActionReminder"
$displayName = "IKEA DocuScan Action Reminder Service"
$description = "Sends daily email notifications for documents with upcoming action dates"
$binaryPath = "D:\IkeaDocuScan\Tools\ActionReminder\IkeaDocuScan.ActionReminderService.exe"
$serviceAccount = "IKEADT\svc_docuscan_reminder"

# Prompt for service account password
$credential = Get-Credential -UserName $serviceAccount -Message "Enter password for service account"

# Create service
New-Service `
    -Name $serviceName `
    -BinaryPathName $binaryPath `
    -DisplayName $displayName `
    -Description $description `
    -StartupType Automatic `
    -Credential $credential

Write-Host "✅ Windows Service created successfully" -ForegroundColor Green
Write-Host "   Service Name: $serviceName" -ForegroundColor Cyan
Write-Host "   Display Name: $displayName" -ForegroundColor Cyan
Write-Host "   Account: $serviceAccount" -ForegroundColor Cyan
```

**Using SC.exe (Alternative):**

```cmd
REM Run as Administrator
sc create IkeaDocuScanActionReminder ^
binPath= "D:\IkeaDocuScan\Tools\ActionReminder\IkeaDocuScan.ActionReminderService.exe" ^
displayname= "IKEA DocuScan Action Reminder Service" ^
start= auto ^
obj= "IKEADT\svc_docuscan_reminder" ^
password= "ServiceAccountPassword"

REM Set description
sc description IkeaDocuScanActionReminder "Sends daily email notifications for documents with upcoming action dates"
```

### Step 5: Configure Service Recovery

Set automatic restart on failure:

```powershell
$serviceName = "IkeaDocuScanActionReminder"

# Configure recovery options
sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/120000/restart/300000

Write-Host "✅ Service recovery options configured" -ForegroundColor Green
Write-Host "   Reset failure count: 24 hours" -ForegroundColor Cyan
Write-Host "   1st failure: Restart after 1 minute" -ForegroundColor Cyan
Write-Host "   2nd failure: Restart after 2 minutes" -ForegroundColor Cyan
Write-Host "   3rd failure: Restart after 5 minutes" -ForegroundColor Cyan
```

### Step 6: Set Service to Run in Production Environment

**Set environment variable for the service:**

```powershell
# Note: This must be done via registry for Windows Services
$serviceName = "IkeaDocuScanActionReminder"
$registryPath = "HKLM:\SYSTEM\CurrentControlSet\Services\$serviceName"

# Set environment variables for service
New-ItemProperty -Path $registryPath -Name "Environment" -Value @("DOTNET_ENVIRONMENT=Production") -PropertyType MultiString -Force

Write-Host "✅ Service environment set to Production" -ForegroundColor Green
```

### Step 7: Verify Service Installation

```powershell
$serviceName = "IkeaDocuScanActionReminder"

# Get service details
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service) {
    Write-Host "✅ Service installed successfully" -ForegroundColor Green
    Write-Host "`nService Details:" -ForegroundColor Cyan
    Get-Service -Name $serviceName | Select-Object Name, DisplayName, Status, StartType | Format-List

    # Get more details using WMI
    $wmiService = Get-WmiObject Win32_Service | Where-Object {$_.Name -eq $serviceName}
    Write-Host "Service Account: $($wmiService.StartName)" -ForegroundColor Cyan
    Write-Host "Binary Path: $($wmiService.PathName)" -ForegroundColor Cyan
    Write-Host "Description: $($wmiService.Description)" -ForegroundColor Cyan
} else {
    Write-Host "❌ Service not found - installation may have failed" -ForegroundColor Red
}
```

---

## Testing and Verification

### Step 1: Test Configuration Before Starting

**Test database connection:**

```powershell
$servicePath = "D:\IkeaDocuScan\Tools\ActionReminder"
cd $servicePath

# Temporarily run service executable to test configuration
# This will start the service, but we'll stop it quickly to just check logs

# Note: Service executable doesn't have a "test" mode, so we'll check Event Viewer after starting

Write-Host "Service is ready to start. We'll start it and check logs in next step." -ForegroundColor Yellow
```

### Step 2: Start Service for First Time

```powershell
$serviceName = "IkeaDocuScanActionReminder"

Write-Host "Starting service for first time..." -ForegroundColor Yellow

# Start service
Start-Service -Name $serviceName

# Wait for initialization
Start-Sleep -Seconds 10

# Check service status
$service = Get-Service -Name $serviceName

Write-Host "`nService Status: $($service.Status)" -ForegroundColor $(if($service.Status -eq "Running"){"Green"}else{"Red"})

if ($service.Status -ne "Running") {
    Write-Host "❌ Service failed to start - check Event Viewer for errors" -ForegroundColor Red
}
```

### Step 3: Check Event Viewer

```powershell
Write-Host "`nChecking Windows Event Log..." -ForegroundColor Cyan

# Check for service-related events
Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 10 -ErrorAction SilentlyContinue |
    Select-Object TimeGenerated, EntryType, Message |
    Format-Table -AutoSize

# If no events found
if ((Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 1 -ErrorAction SilentlyContinue).Count -eq 0) {
    Write-Host "⚠️ No events found from service yet" -ForegroundColor Yellow
    Write-Host "   Check 'Application' log in Event Viewer manually" -ForegroundColor Yellow
}

# Check for any errors
$errors = Get-EventLog -LogName Application -Newest 20 -EntryType Error -ErrorAction SilentlyContinue |
    Where-Object {$_.Source -like "*IkeaDocuScan*" -or $_.Source -like "*ActionReminder*"}

if ($errors) {
    Write-Host "`n❌ Errors found in Event Log:" -ForegroundColor Red
    $errors | Select-Object TimeGenerated, Source, Message | Format-Table -AutoSize
}
```

### Step 4: Test Immediate Execution (Optional)

To test immediately without waiting for schedule time:

**Option A: Temporarily Change Schedule Time**

```powershell
# Stop service
Stop-Service -Name "IkeaDocuScanActionReminder"

# Edit configuration to run in 2 minutes
$configPath = "D:\IkeaDocuScan\Tools\ActionReminder\appsettings.Production.json"
$currentTime = Get-Date
$testTime = $currentTime.AddMinutes(2).ToString("HH:mm")

Write-Host "Current time: $($currentTime.ToString('HH:mm'))" -ForegroundColor Cyan
Write-Host "Setting test schedule time to: $testTime" -ForegroundColor Cyan

# Read config
$config = Get-Content $configPath | ConvertFrom-Json

# Update schedule time
$config.ActionReminderService.ScheduleTime = $testTime
$config.ActionReminderService.CheckIntervalMinutes = 1 # Check every minute

# Save config
$config | ConvertTo-Json -Depth 10 | Out-File $configPath -Encoding UTF8 -Force

Write-Host "✅ Configuration updated for immediate test" -ForegroundColor Green
Write-Host "   Service will run at: $testTime" -ForegroundColor Cyan

# Start service
Start-Service -Name "IkeaDocuScanActionReminder"

Write-Host "`nService started. Wait for scheduled time and check Event Viewer." -ForegroundColor Yellow
Write-Host "Monitoring Event Log for next 5 minutes..." -ForegroundColor Yellow

# Monitor Event Log
$startTime = Get-Date
$endTime = $startTime.AddMinutes(5)

while ((Get-Date) -lt $endTime) {
    $newEvents = Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -After $startTime -ErrorAction SilentlyContinue

    if ($newEvents) {
        Write-Host "`n✅ New events found:" -ForegroundColor Green
        $newEvents | Select-Object TimeGenerated, EntryType, Message | Format-Table -AutoSize
        break
    }

    Start-Sleep -Seconds 30
    Write-Host "." -NoNewline
}

# Restore original schedule time
Write-Host "`n⚠️ Remember to restore original schedule time in configuration!" -ForegroundColor Yellow
```

**Option B: Check Database for Test Data**

```sql
-- Connect to SQL Server
USE IkeaDocuScan;
GO

-- Check if there are any action reminders due today
SELECT
    BarCode,
    DocumentNo,
    ActionDate,
    ActionDescription,
    DATEDIFF(DAY, GETDATE(), ActionDate) AS DaysUntilDue
FROM Document
WHERE ActionDate >= CAST(GETDATE() AS DATE)
  AND ActionDate <= DATEADD(DAY, 2, CAST(GETDATE() AS DATE))
  AND ActionDate >= ReceivingDate
ORDER BY ActionDate, BarCode;

-- If no data, create test record
INSERT INTO Document (BarCode, DocumentNo, ActionDate, ActionDescription, ReceivingDate, CreatedDate)
VALUES ('TEST-001', 'TEST-DOC-001', CAST(GETDATE() AS DATE), 'Test Action Reminder', DATEADD(DAY, -1, GETDATE()), GETDATE());

-- Verify
SELECT * FROM Document WHERE BarCode = 'TEST-001';
```

### Step 5: Verify Email Sending (If Test Data Exists)

If the service runs and finds action reminders:

1. **Check Event Log** for "Successfully sent action reminder emails" message
2. **Check recipient mailboxes** for email
3. **Verify email content** includes document list and is properly formatted

**Expected Email Format:**

```
Subject: Action Reminders Due Today - X Items

Body:
Dear Team,

The following documents have action dates due today (YYYY-MM-DD):

1. Barcode: 123456789
   Document: Document Name Here
   Action: Action description here

2. Barcode: 987654321
   Document: Another Document
   Action: Another action

Please review these documents and take appropriate action.

Best regards,
IKEA DocuScan Action Reminder Service
```

### Step 6: Verify Service Continues Running

```powershell
# Monitor service for 30 minutes
$serviceName = "IkeaDocuScanActionReminder"
$duration = 30 # minutes
$checkInterval = 5 # minutes
$checks = $duration / $checkInterval

Write-Host "Monitoring service stability for $duration minutes..." -ForegroundColor Yellow

for ($i = 1; $i -le $checks; $i++) {
    $service = Get-Service -Name $serviceName
    $timestamp = Get-Date -Format "HH:mm:ss"

    if ($service.Status -eq "Running") {
        Write-Host "[$timestamp] ✅ Service is running (check $i of $checks)" -ForegroundColor Green
    } else {
        Write-Host "[$timestamp] ❌ Service is NOT running! Status: $($service.Status)" -ForegroundColor Red
        break
    }

    if ($i -lt $checks) {
        Start-Sleep -Seconds ($checkInterval * 60)
    }
}

Write-Host "`n✅ Service stability check complete" -ForegroundColor Green
```

---

## Service Management

### Common Commands

**Start Service:**
```powershell
Start-Service -Name "IkeaDocuScanActionReminder"
```

**Stop Service:**
```powershell
Stop-Service -Name "IkeaDocuScanActionReminder"
```

**Restart Service:**
```powershell
Restart-Service -Name "IkeaDocuScanActionReminder"
```

**Check Service Status:**
```powershell
Get-Service -Name "IkeaDocuScanActionReminder" | Select-Object Name, Status, StartType, DisplayName
```

**View Service Details:**
```powershell
Get-WmiObject Win32_Service | Where-Object {$_.Name -eq "IkeaDocuScanActionReminder"} | Format-List *
```

### Updating Configuration

After changing configuration, restart the service:

```powershell
# Edit configuration
notepad "D:\IkeaDocuScan\Tools\ActionReminder\appsettings.Production.json"

# Backup configuration before making changes
Copy-Item "D:\IkeaDocuScan\Tools\ActionReminder\appsettings.Production.json" `
    -Destination "D:\Backups\IkeaDocuScan\Config\ActionReminder_appsettings_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"

# Restart service to apply changes
Restart-Service -Name "IkeaDocuScanActionReminder"

Write-Host "✅ Service restarted with new configuration" -ForegroundColor Green
```

### Viewing Logs

**Event Viewer:**
```powershell
# Open Event Viewer
eventvwr.msc

# Or view via PowerShell
Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 50 |
    Select-Object TimeGenerated, EntryType, Message |
    Format-Table -AutoSize -Wrap
```

**Export logs to file:**
```powershell
$logExportPath = "D:\Logs\IkeaDocuScan\ActionReminder_EventLog_$(Get-Date -Format 'yyyyMMdd_HHmmss').csv"

Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 1000 |
    Select-Object TimeGenerated, EntryType, Source, Message |
    Export-Csv -Path $logExportPath -NoTypeInformation

Write-Host "✅ Event logs exported to: $logExportPath" -ForegroundColor Green
```

### Changing Service Account

```powershell
$serviceName = "IkeaDocuScanActionReminder"
$newAccount = "IKEADT\new_service_account"

# Get credentials
$credential = Get-Credential -UserName $newAccount -Message "Enter password for new service account"

# Stop service
Stop-Service -Name $serviceName

# Change service account
$service = Get-WmiObject Win32_Service -Filter "Name='$serviceName'"
$result = $service.Change($null,$null,$null,$null,$null,$null,$credential.UserName,$credential.GetNetworkCredential().Password,$null,$null,$null)

if ($result.ReturnValue -eq 0) {
    Write-Host "✅ Service account changed successfully" -ForegroundColor Green

    # Grant database access to new account (run on SQL Server)
    Write-Host "`n⚠️ Remember to grant database access to new account!" -ForegroundColor Yellow
    Write-Host "   Run this on SQL Server:" -ForegroundColor Cyan
    Write-Host "   CREATE LOGIN [$newAccount] FROM WINDOWS;" -ForegroundColor Cyan
    Write-Host "   USE IkeaDocuScan; CREATE USER [new_user] FOR LOGIN [$newAccount];" -ForegroundColor Cyan
    Write-Host "   ALTER ROLE db_datareader ADD MEMBER [new_user];" -ForegroundColor Cyan
} else {
    Write-Host "❌ Failed to change service account. Error code: $($result.ReturnValue)" -ForegroundColor Red
}

# Start service
Start-Service -Name $serviceName
```

### Uninstalling Service

```powershell
$serviceName = "IkeaDocuScanActionReminder"

# Stop service
Write-Host "Stopping service..." -ForegroundColor Yellow
Stop-Service -Name $serviceName -Force

# Wait for service to stop
Start-Sleep -Seconds 5

# Remove service
Write-Host "Removing service..." -ForegroundColor Yellow
sc.exe delete $serviceName

# Verify removal
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service) {
    Write-Host "❌ Service still exists - removal may have failed" -ForegroundColor Red
} else {
    Write-Host "✅ Service removed successfully" -ForegroundColor Green
}

# Service files remain at D:\IkeaDocuScan\Tools\ActionReminder\
# Delete manually if needed
```

---

## Troubleshooting

### Issue: Service Won't Start

**Symptoms:**
- Service starts then immediately stops
- Error in Event Viewer: "The service terminated unexpectedly"

**Check Event Viewer:**
```powershell
Get-EventLog -LogName Application -Newest 20 -EntryType Error |
    Where-Object {$_.Source -like "*ActionReminder*" -or $_.Source -like "*IkeaDocuScan*"} |
    Select-Object TimeGenerated, Source, Message |
    Format-List
```

**Common Causes:**

1. **Missing .NET Runtime:**
```powershell
# Verify .NET 10.0 Runtime installed
dotnet --list-runtimes
# Should show: Microsoft.NETCore.App 10.0.x
```

2. **Database Connection Failed:**
```powershell
# Test database connection manually
$serverName = "wtseelm-nx20541.ikeadt.com"
Test-NetConnection -ComputerName $serverName -Port 1433
```

3. **Service Account Lacks Permissions:**
```sql
-- Check service account has database access
USE IkeaDocuScan;
SELECT name, type_desc FROM sys.database_principals WHERE name LIKE '%docuscan%';
```

4. **Configuration File Invalid:**
```powershell
# Verify JSON is valid
$configPath = "D:\IkeaDocuScan\Tools\ActionReminder\appsettings.Production.json"
try {
    $config = Get-Content $configPath | ConvertFrom-Json
    Write-Host "✅ Configuration file is valid JSON" -ForegroundColor Green
} catch {
    Write-Host "❌ Configuration file has invalid JSON: $_" -ForegroundColor Red
}
```

### Issue: Service Runs But No Emails Sent

**Check 1: Verify Schedule Time**

```powershell
$configPath = "D:\IkeaDocuScan\Tools\ActionReminder\appsettings.Production.json"
$config = Get-Content $configPath | ConvertFrom-Json

Write-Host "Service Configuration:" -ForegroundColor Cyan
Write-Host "  Enabled: $($config.ActionReminderService.Enabled)" -ForegroundColor Cyan
Write-Host "  Schedule Time: $($config.ActionReminderService.ScheduleTime)" -ForegroundColor Cyan
Write-Host "  Current Time: $(Get-Date -Format 'HH:mm')" -ForegroundColor Cyan

if (-not $config.ActionReminderService.Enabled) {
    Write-Host "❌ Service is DISABLED in configuration!" -ForegroundColor Red
}
```

**Check 2: Verify Action Reminders Exist**

```sql
-- Check if there are any action reminders due today
USE IkeaDocuScan;
GO

DECLARE @Today DATE = CAST(GETDATE() AS DATE);

SELECT
    COUNT(*) AS RemindersDueToday
FROM Document
WHERE ActionDate = @Today
  AND ActionDate >= ReceivingDate;

-- If 0, no reminders due today - service won't send email
```

**Check 3: Verify SMTP Connection**

```powershell
# Test SMTP connectivity
$smtpHost = "smtp-gw.ikea.com"
$smtpPort = 25

Test-NetConnection -ComputerName $smtpHost -Port $smtpPort

# Expected: TcpTestSucceeded : True
```

**Check 4: Check Event Log for SMTP Errors**

```powershell
Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 50 |
    Where-Object {$_.Message -like "*SMTP*" -or $_.Message -like "*email*"} |
    Select-Object TimeGenerated, EntryType, Message |
    Format-List
```

### Issue: Service Sends Emails Continuously

**Symptoms:**
- Multiple emails received in short time
- Service runs more than once per day

**Possible Causes:**

1. **CheckIntervalMinutes too low:**
```json
// Should be at least 60 minutes
"CheckIntervalMinutes": 60
```

2. **Service restarted multiple times:**
```powershell
# Check if service was restarted
Get-EventLog -LogName System -Source "Service Control Manager" -Newest 50 |
    Where-Object {$_.Message -like "*ActionReminder*"} |
    Select-Object TimeGenerated, Message
```

### Issue: High CPU or Memory Usage

**Monitor Service Performance:**

```powershell
# Find service process
$process = Get-Process | Where-Object {$_.ProcessName -like "*ActionReminder*"}

if ($process) {
    Write-Host "Service Process:" -ForegroundColor Cyan
    Write-Host "  CPU: $($process.CPU)" -ForegroundColor Cyan
    Write-Host "  Memory: $([math]::Round($process.WorkingSet64 / 1MB, 2)) MB" -ForegroundColor Cyan
    Write-Host "  Threads: $($process.Threads.Count)" -ForegroundColor Cyan
} else {
    Write-Host "⚠️ Service process not found - may not be running" -ForegroundColor Yellow
}
```

**If high resource usage:**
- Check Event Log for errors or warnings
- Consider database query performance
- Monitor during scheduled execution time
- Review recent configuration changes

---

## Monitoring

### Daily Health Check

Create a monitoring script:

```powershell
# Save as: Check-ActionReminderService.ps1
param(
    [string]$ServiceName = "IkeaDocuScanActionReminder"
)

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$logFile = "D:\Logs\IkeaDocuScan\ServiceHealthCheck_$(Get-Date -Format 'yyyyMMdd').log"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $logEntry = "[$timestamp] [$Level] $Message"
    Add-Content -Path $logFile -Value $logEntry
    Write-Host $logEntry -ForegroundColor $(if($Level -eq "ERROR"){"Red"}elseif($Level -eq "WARN"){"Yellow"}else{"Green"})
}

# Check if service exists
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($service -eq $null) {
    Write-Log "Service '$ServiceName' not found!" "ERROR"
    exit 1
}

# Check if service is running
if ($service.Status -ne "Running") {
    Write-Log "Service is not running (Status: $($service.Status)). Attempting to start..." "WARN"

    try {
        Start-Service -Name $ServiceName
        Start-Sleep -Seconds 10

        $service = Get-Service -Name $ServiceName
        if ($service.Status -eq "Running") {
            Write-Log "Service started successfully" "INFO"
        } else {
            Write-Log "Failed to start service" "ERROR"
            exit 2
        }
    } catch {
        Write-Log "Error starting service: $_" "ERROR"
        exit 2
    }
} else {
    Write-Log "Service is running normally" "INFO"
}

# Check recent errors in Event Log
$errors = Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -After (Get-Date).AddHours(-24) -EntryType Error -ErrorAction SilentlyContinue

if ($errors) {
    Write-Log "Found $($errors.Count) error(s) in last 24 hours" "WARN"
    foreach ($error in $errors) {
        Write-Log "  Error at $($error.TimeGenerated): $($error.Message)" "ERROR"
    }
} else {
    Write-Log "No errors in last 24 hours" "INFO"
}

Write-Log "Health check completed successfully" "INFO"
exit 0
```

**Schedule this script to run every hour:**

```powershell
# Create scheduled task
$action = New-ScheduledTaskAction -Execute "powershell.exe" `
    -Argument "-ExecutionPolicy Bypass -File `"D:\IkeaDocuScan\Tools\ActionReminder\Check-ActionReminderService.ps1`""

$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Hours 1) -RepetitionDuration ([TimeSpan]::MaxValue)

$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest

Register-ScheduledTask -TaskName "IkeaDocuScan-ActionReminder-HealthCheck" `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Description "Monitors IkeaDocuScan Action Reminder Service health"

Write-Host "✅ Health check scheduled task created" -ForegroundColor Green
```

### Monitor Email Delivery

**Track email sending history:**

```sql
-- If audit trail or email log table exists
USE IkeaDocuScan;
GO

-- Check AuditTrail for email-related actions
SELECT TOP 50
    Timestamp,
    Action,
    Description,
    UserId
FROM AuditTrail
WHERE Description LIKE '%reminder%' OR Description LIKE '%email%'
ORDER BY Timestamp DESC;
```

### Performance Monitoring

**Monitor service resource usage:**

```powershell
# Create performance monitoring script
# Save as: Monitor-ActionReminderPerformance.ps1

$serviceName = "IkeaDocuScanActionReminder"
$duration = 60 # minutes
$sampleInterval = 5 # minutes

$logPath = "D:\Logs\IkeaDocuScan\ServicePerformance_$(Get-Date -Format 'yyyyMMdd_HHmmss').csv"

$results = @()

$samples = $duration / $sampleInterval

for ($i = 1; $i -le $samples; $i++) {
    $timestamp = Get-Date

    # Get service process
    $process = Get-Process | Where-Object {$_.ProcessName -like "*ActionReminder*"}

    if ($process) {
        $cpuPercent = [math]::Round($process.CPU / (Get-Date).Subtract($process.StartTime).TotalSeconds * 100, 2)
        $memoryMB = [math]::Round($process.WorkingSet64 / 1MB, 2)
        $threads = $process.Threads.Count

        $result = [PSCustomObject]@{
            Timestamp = $timestamp
            CPU_Percent = $cpuPercent
            Memory_MB = $memoryMB
            Threads = $threads
        }

        $results += $result

        Write-Host "[$($timestamp.ToString('HH:mm:ss'))] CPU: $cpuPercent% | Memory: $memoryMB MB | Threads: $threads" -ForegroundColor Cyan
    } else {
        Write-Host "[$($timestamp.ToString('HH:mm:ss'))] Service process not found" -ForegroundColor Yellow
    }

    if ($i -lt $samples) {
        Start-Sleep -Seconds ($sampleInterval * 60)
    }
}

# Export to CSV
$results | Export-Csv -Path $logPath -NoTypeInformation

Write-Host "`n✅ Performance monitoring complete. Results saved to: $logPath" -ForegroundColor Green
```

---

## Summary

**Service Successfully Deployed!**

| Item | Value |
|------|-------|
| Service Name | IkeaDocuScanActionReminder |
| Display Name | IKEA DocuScan Action Reminder Service |
| Installation Path | D:\IkeaDocuScan\Tools\ActionReminder\ |
| Configuration | D:\IkeaDocuScan\Tools\ActionReminder\appsettings.Production.json |
| Service Account | IKEADT\svc_docuscan_reminder (or LocalService) |
| Startup Type | Automatic |
| Schedule Time | 08:00 (configurable) |
| Database | IkeaDocuScan @ wtseelm-nx20541.ikeadt.com |
| SMTP Server | smtp-gw.ikea.com:25 |

**Key Management Commands:**

```powershell
# Start/Stop/Restart
Start-Service -Name "IkeaDocuScanActionReminder"
Stop-Service -Name "IkeaDocuScanActionReminder"
Restart-Service -Name "IkeaDocuScanActionReminder"

# Check status
Get-Service -Name "IkeaDocuScanActionReminder"

# View logs
Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 20

# Edit configuration (requires restart)
notepad D:\IkeaDocuScan\Tools\ActionReminder\appsettings.Production.json
Restart-Service -Name "IkeaDocuScanActionReminder"
```

**Next Steps:**

1. **Monitor for 24-48 Hours:** Watch Event Log for any errors
2. **Verify First Scheduled Run:** Check that email is sent at configured time
3. **Confirm Email Delivery:** Verify recipients receive emails
4. **Document Configuration:** Save configuration details in runbook
5. **Set Up Monitoring:** Schedule health check script to run hourly
6. **Backup Configuration:** Regular backups of appsettings.Production.json

**Support Resources:**

- Event Log: Windows Application Log → Source: "IkeaDocuScan Action Reminder"
- Configuration: D:\IkeaDocuScan\Tools\ActionReminder\appsettings.Production.json
- Database: IkeaDocuScan database on wtseelm-nx20541.ikeadt.com
- Health Check Script: Check-ActionReminderService.ps1

---

**Document Version:** 1.0
**Last Updated:** 2025-01-18
**Prepared For:** IKEA DocuScan V3 - Action Reminder Service Deployment
