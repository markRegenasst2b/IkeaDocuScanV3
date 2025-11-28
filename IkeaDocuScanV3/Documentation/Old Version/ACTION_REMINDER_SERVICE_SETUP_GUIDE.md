# Action Reminder Service - Windows Service Setup Guide

**Version:** 1.0
**Date:** 2025-11-14
**Target Framework:** .NET 10.0
**Service Type:** Windows Background Service

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Publishing the Service](#publishing-the-service)
4. [Installation as Windows Service](#installation-as-windows-service)
5. [Configuration](#configuration)
6. [Service Management](#service-management)
7. [Troubleshooting](#troubleshooting)
8. [Monitoring](#monitoring)

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

1. Service starts and waits for configured schedule time
2. Checks database for documents with `ActionDate` = today (or today + `DaysAhead`)
3. If reminders found, loads email template from database (or uses default)
4. Sends email to configured recipients with list of due actions
5. Waits until next day to repeat

---

## Prerequisites

### Server Requirements

- **Windows Server 2019 or later** (or Windows 10/11 for development)
- **.NET 10.0 Runtime** installed
- **SQL Server** (2017 or later) accessible from the server
- **SMTP Server** access (for sending emails)

### Required Permissions

- **File System**: Write permissions in `C:\IkeaDocuScan\ActionReminder\`
- **Database**: Read access to `IkeaDocuScan` database
- **SMTP**: Ability to connect to SMTP server
- **Event Log**: Permission to write to Windows Event Log (automatic with Local Service account)
- **Windows Service**: Administrator rights to install/manage services

### Software Requirements on Deployment Machine

- Visual Studio 2022 or .NET SDK 10.0
- SQL Server Management Studio (for database verification)
- Administrator access to target server

---

## Publishing the Service

### Step 1: Open Project in Visual Studio 2022

1. Open the solution: `IkeaDocuScanV3.sln`
2. Locate project: `IkeaDocuScan.ActionReminderService`
3. Set build configuration to **Release**

### Step 2: Publish the Project

**Option A: Using Visual Studio**

1. Right-click `IkeaDocuScan.ActionReminderService` project → **Publish**
2. Choose **Folder** as target
3. Set folder path: `C:\Publish\ActionReminder\`
4. Click **Publish**

**Option B: Using Command Line**

```powershell
# Navigate to project directory
cd IkeaDocuScanV3\IkeaDocuScan.ActionReminderService

# Publish for Windows x64
dotnet publish -c Release -r win-x64 --self-contained false -o C:\Publish\ActionReminder\
```

### Step 3: Verify Published Files

Check that the following files exist in the publish folder:

- ✅ `IkeaDocuScan.ActionReminderService.exe` (main executable)
- ✅ `IkeaDocuScan.ActionReminderService.dll`
- ✅ `IkeaDocuScan.Infrastructure.dll`
- ✅ `IkeaDocuScan.Shared.dll`
- ✅ `appsettings.json` (configuration file)
- ✅ All dependency DLLs (MailKit, Microsoft.EntityFrameworkCore, etc.)

---

## Installation as Windows Service

### Step 1: Copy Files to Target Location

1. Create target directory on server:
   ```powershell
   New-Item -Path "C:\IkeaDocuScan\ActionReminder" -ItemType Directory -Force
   ```

2. Copy all published files to `C:\IkeaDocuScan\ActionReminder\`

3. Verify files copied successfully:
   ```powershell
   Get-ChildItem "C:\IkeaDocuScan\ActionReminder\" | Select-Object Name, Length
   ```

### Step 2: Configure appsettings.json

Before installing the service, update the configuration file:

1. Navigate to: `C:\IkeaDocuScan\ActionReminder\appsettings.json`

2. Edit with Notepad or preferred editor:
   ```powershell
   notepad "C:\IkeaDocuScan\ActionReminder\appsettings.json"
   ```

3. Update critical settings (see [Configuration](#configuration) section for details):
   - `ConnectionStrings:DefaultConnection` - SQL Server connection string
   - `ActionReminderService:RecipientEmails` - Email recipients
   - `Email:SmtpHost` - SMTP server address
   - `Email:FromAddress` - Sender email address

**Example Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SQLSERVER01;Database=IkeaDocuScan;Integrated Security=true;TrustServerCertificate=true;"
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
    "SmtpHost": "smtp.ikea.com",
    "SmtpPort": 587,
    "SecurityMode": "StartTls",
    "SmtpUsername": "docuscan@ikea.com",
    "SmtpPassword": "YourPassword",
    "FromAddress": "noreply-docuscan@ikea.com",
    "FromDisplayName": "IKEA DocuScan Action Reminder Service",
    "EnableEmailNotifications": true,
    "TimeoutSeconds": 30
  }
}
```

### Step 3: Create Windows Service

**Using PowerShell (Recommended):**

```powershell
# Run as Administrator
New-Service `
    -Name "IkeaDocuScanActionReminder" `
    -BinaryPathName "C:\IkeaDocuScan\ActionReminder\IkeaDocuScan.ActionReminderService.exe" `
    -DisplayName "IKEA DocuScan Action Reminder Service" `
    -Description "Sends daily email notifications for documents with upcoming action dates" `
    -StartupType Automatic `
    -Credential (Get-Credential)
```

**Using SC.exe:**

```cmd
REM Run as Administrator
sc create IkeaDocuScanActionReminder ^
binPath= "C:\IkeaDocuScan\ActionReminder\IkeaDocuScan.ActionReminderService.exe" ^
displayname= "IKEA DocuScan Action Reminder Service" ^
start= auto ^
obj= "NT AUTHORITY\LocalService"
```

### Step 4: Configure Service Account (Important)

The service account needs:
- ✅ Read access to SQL Server database
- ✅ Network access to SMTP server
- ✅ Read/Write access to `C:\IkeaDocuScan\ActionReminder\`

**Option A: Use Local Service Account (Recommended for simplicity)**
```powershell
# Service runs as NT AUTHORITY\LocalService
# Configure SQL Server to allow this account
```

**Option B: Use Domain Service Account (Recommended for production)**
```powershell
# Configure service to run as domain service account
sc config IkeaDocuScanActionReminder obj= "DOMAIN\svc_docuscan" password= "ServicePassword"

# Grant database access to this account in SQL Server
```

### Step 5: Configure Service Recovery

Set automatic restart on failure:

```powershell
sc failure IkeaDocuScanActionReminder reset= 86400 actions= restart/60000/restart/120000/restart/300000
```

This configures:
- Reset failure count after 24 hours (86400 seconds)
- 1st failure: Restart after 60 seconds
- 2nd failure: Restart after 2 minutes
- 3rd failure: Restart after 5 minutes

### Step 6: Start the Service

```powershell
Start-Service -Name "IkeaDocuScanActionReminder"

# Verify service is running
Get-Service -Name "IkeaDocuScanActionReminder" | Select-Object Status, StartType, DisplayName
```

Expected output:
```
Status  StartType DisplayName
------  --------- -----------
Running Automatic IKEA DocuScan Action Reminder Service
```

---

## Configuration

### Configuration File Structure

The service uses `appsettings.json` located in the service directory: `C:\IkeaDocuScan\ActionReminder\appsettings.json`

### Configuration Sections

#### 1. Connection Strings

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=IkeaDocuScan;Integrated Security=true;TrustServerCertificate=true;"
}
```

**Parameters:**
- `Server` - SQL Server hostname or IP address
- `Database` - Database name (should be `IkeaDocuScan`)
- `Integrated Security=true` - Use Windows Authentication
- OR use `User Id=sa;Password=YourPassword` for SQL Authentication
- `TrustServerCertificate=true` - Required for self-signed certificates

#### 2. Action Reminder Service Configuration

```json
"ActionReminderService": {
  "Enabled": true,
  "ScheduleTime": "08:00",
  "CheckIntervalMinutes": 60,
  "RecipientEmails": [
    "admin1@company.com",
    "admin2@company.com"
  ],
  "EmailSubject": "Action Reminders Due Today - {Count} Items",
  "SendEmptyNotifications": false,
  "DaysAhead": 0
}
```

**Parameter Details:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Enabled` | bool | true | Master switch - set to `false` to disable the service |
| `ScheduleTime` | string | "08:00" | Time of day to send reminders (24-hour format HH:mm) |
| `CheckIntervalMinutes` | int | 60 | How often to check if it's time to run (in minutes) |
| `RecipientEmails` | string[] | [] | Array of email addresses to receive notifications |
| `EmailSubject` | string | "Action Reminders Due Today - {Count} Items" | Email subject line. Use `{Count}` placeholder for number of reminders |
| `SendEmptyNotifications` | bool | false | Send email even when no reminders are due |
| `DaysAhead` | int | 0 | Look ahead X days (0 = today only, 1 = today + tomorrow, etc.) |

**Examples:**

```json
// Send reminders at 7:30 AM
"ScheduleTime": "07:30"

// Look ahead 2 days (today, tomorrow, day after)
"DaysAhead": 2

// Send to multiple recipients
"RecipientEmails": [
  "legal@ikea.com",
  "finance@ikea.com",
  "admin@ikea.com"
]
```

#### 3. Email (SMTP) Configuration

```json
"Email": {
  "SmtpHost": "smtp.company.com",
  "SmtpPort": 587,
  "SecurityMode": "StartTls",
  "SmtpUsername": "docuscan@company.com",
  "SmtpPassword": "YourPassword",
  "FromAddress": "noreply-docuscan@company.com",
  "FromDisplayName": "IKEA DocuScan Action Reminder Service",
  "EnableEmailNotifications": true,
  "TimeoutSeconds": 30
}
```

**Parameter Details:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SmtpHost` | string | Required | SMTP server hostname |
| `SmtpPort` | int | 25 | SMTP server port (25=plain, 587=TLS, 465=SSL) |
| `SecurityMode` | string | "Auto" | Options: `Auto`, `None`, `StartTls`, `SslOnConnect` |
| `SmtpUsername` | string | "" | SMTP authentication username (if required) |
| `SmtpPassword` | string | "" | SMTP authentication password (if required) |
| `FromAddress` | string | Required | Sender email address |
| `FromDisplayName` | string | "" | Sender display name |
| `EnableEmailNotifications` | bool | true | Master switch for email sending |
| `TimeoutSeconds` | int | 30 | SMTP operation timeout |

**Security Mode Options:**
- `Auto` (Recommended) - Automatically selects based on port: Port 25→None, Port 587→StartTls, Port 465→SslOnConnect
- `None` - No encryption (use only for internal servers)
- `StartTls` - Upgrade to TLS after connecting (common for port 587)
- `SslOnConnect` - SSL from connection start (common for port 465)

**Common SMTP Configurations:**

**Microsoft 365 / Outlook:**
```json
{
  "SmtpHost": "smtp.office365.com",
  "SmtpPort": 587,
  "SecurityMode": "StartTls",
  "SmtpUsername": "docuscan@yourcompany.com",
  "SmtpPassword": "YourPassword"
}
```

**Gmail (requires App Password):**
```json
{
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SecurityMode": "StartTls",
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "YourAppPassword"
}
```

**Internal Exchange Server:**
```json
{
  "SmtpHost": "mail.yourcompany.local",
  "SmtpPort": 25,
  "SecurityMode": "None",
  "SmtpUsername": "",
  "SmtpPassword": ""
}
```

#### 4. Logging Configuration

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft": "Warning",
    "IkeaDocuScan.ActionReminderService": "Information"
  }
}
```

**Log Levels:**
- `Trace` - Very detailed logs (for debugging)
- `Debug` - Debugging information
- `Information` - General informational messages (recommended)
- `Warning` - Warning messages
- `Error` - Error messages only
- `Critical` - Critical failures only
- `None` - No logging

### Updating Configuration

**After changing appsettings.json, you MUST restart the service:**

```powershell
Restart-Service -Name "IkeaDocuScanActionReminder"
```

---

## Service Management

### Common Service Commands

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
Get-Service -Name "IkeaDocuScanActionReminder" | Select-Object Status, StartType, DisplayName
```

**View Service Properties:**
```powershell
Get-WmiObject Win32_Service | Where-Object {$_.Name -eq "IkeaDocuScanActionReminder"} | Format-List *
```

### Changing Service Account

```powershell
# Using PowerShell
$cred = Get-Credential
$service = Get-WmiObject Win32_Service -Filter "Name='IkeaDocuScanActionReminder'"
$service.Change($null,$null,$null,$null,$null,$null,$cred.UserName,$cred.GetNetworkCredential().Password,$null,$null,$null)

# Then restart service
Restart-Service -Name "IkeaDocuScanActionReminder"
```

### Uninstalling the Service

```powershell
# Stop the service first
Stop-Service -Name "IkeaDocuScanActionReminder"

# Remove the service
Remove-Service -Name "IkeaDocuScanActionReminder"

# Or using SC.exe
sc delete IkeaDocuScanActionReminder
```

---

## Troubleshooting

### Service Won't Start

**Check Event Viewer:**
1. Open Event Viewer (`eventvwr.msc`)
2. Navigate to: **Windows Logs** → **Application**
3. Look for errors from source: **IkeaDocuScan Action Reminder**

**Common Issues:**

**Issue: "Service did not start due to logon failure"**
- **Cause**: Service account doesn't have correct password or permissions
- **Solution**: Verify service account credentials and permissions

**Issue: "Could not connect to database"**
- **Cause**: Connection string incorrect or service account lacks database access
- **Solution**: Test connection string and grant database permissions

**Issue: "File not found"**
- **Cause**: Missing DLL files or incorrect path
- **Solution**: Verify all published files are present in service directory

### Service Starts but No Emails Sent

**Check 1: Verify Configuration**
```powershell
# View current configuration
Get-Content "C:\IkeaDocuScan\ActionReminder\appsettings.json"
```

Verify:
- ✅ `Enabled` is set to `true`
- ✅ `RecipientEmails` contains valid email addresses
- ✅ `EnableEmailNotifications` is `true`
- ✅ SMTP settings are correct

**Check 2: View Service Logs**
```powershell
# View recent Application event log entries
Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 50 | Format-Table TimeGenerated, EntryType, Message -AutoSize
```

**Check 3: Verify Schedule Time**

The service runs once per day at the configured `ScheduleTime`. If it's not time yet, it won't send emails.

```json
// Runs at 8:00 AM
"ScheduleTime": "08:00"
```

To test immediately, temporarily change to current time + 1 minute, restart service.

**Check 4: Test SMTP Connection**

From the server, test SMTP connectivity:

```powershell
# Test SMTP port connectivity
Test-NetConnection -ComputerName smtp.company.com -Port 587

# Should show: TcpTestSucceeded: True
```

### No Action Reminders Found

**Check Database:**

Run this query in SQL Server Management Studio:

```sql
-- Check for documents with action dates due today
SELECT
    BarCode,
    DocumentNo,
    ActionDate,
    ActionDescription
FROM Documents
WHERE ActionDate = CAST(GETDATE() AS DATE)
  AND ActionDate >= ReceivingDate
ORDER BY ActionDate, BarCode;
```

If no results, there are no action reminders due today.

### Testing the Service

**Method 1: Reduce Check Interval**

Temporarily set check interval to 1 minute for testing:

```json
"CheckIntervalMinutes": 1
```

Restart service and monitor Event Viewer.

**Method 2: Set Schedule Time to Near Future**

Set schedule time to 2 minutes from now:

```json
// If current time is 14:30, set to 14:32
"ScheduleTime": "14:32"
```

Restart service and wait.

**Method 3: Enable Debug Logging**

```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "IkeaDocuScan.ActionReminderService": "Debug"
  }
}
```

Restart service and check Event Viewer for detailed logs.

---

## Monitoring

### Windows Event Log

The service logs to Windows Event Log under source: **IkeaDocuScan Action Reminder**

**View Logs:**
1. Open Event Viewer (`eventvwr.msc`)
2. Navigate to: **Windows Logs** → **Application**
3. Filter by source: **IkeaDocuScan Action Reminder**

**Key Log Messages:**

| Message | Type | Meaning |
|---------|------|---------|
| "Action Reminder Worker started" | Information | Service started successfully |
| "Found X action reminder(s) due" | Information | Reminders found and will be sent |
| "No action reminders due today" | Information | No reminders found |
| "Successfully sent action reminder emails" | Information | Emails sent successfully |
| "Error processing action reminders" | Error | Something went wrong |
| "SMTP connection test failed" | Error | Cannot connect to SMTP server |

### Daily Health Check

Create a scheduled task to verify service is running:

```powershell
# Save as: Check-ActionReminderService.ps1
$serviceName = "IkeaDocuScanActionReminder"
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service -eq $null) {
    Write-EventLog -LogName Application -Source "HealthCheck" -EventId 1001 -EntryType Error -Message "Action Reminder Service not found"
    exit 1
}

if ($service.Status -ne "Running") {
    Write-EventLog -LogName Application -Source "HealthCheck" -EventId 1002 -EntryType Warning -Message "Action Reminder Service is not running. Attempting to start..."
    Start-Service -Name $serviceName
    exit 2
}

Write-EventLog -LogName Application -Source "HealthCheck" -EventId 1000 -EntryType Information -Message "Action Reminder Service is running normally"
exit 0
```

Schedule this script to run every hour.

---

## Summary

✅ **Installation**: Service installed at `C:\IkeaDocuScan\ActionReminder\`
✅ **Configuration**: Edit `appsettings.json` and restart service
✅ **Service Name**: `IkeaDocuScanActionReminder`
✅ **Runs**: Once daily at configured `ScheduleTime`
✅ **Logging**: Windows Event Log → Application → Source: "IkeaDocuScan Action Reminder"
✅ **Management**: Use standard Windows service commands

For configuration and email template customization, see: **ACTION_REMINDER_CONFIGURATION_GUIDE.md**

---

## Quick Reference

**Service Installation:**
```powershell
New-Service -Name "IkeaDocuScanActionReminder" -BinaryPathName "C:\IkeaDocuScan\ActionReminder\IkeaDocuScan.ActionReminderService.exe" -DisplayName "IKEA DocuScan Action Reminder Service" -StartupType Automatic
```

**Start/Stop/Restart:**
```powershell
Start-Service -Name "IkeaDocuScanActionReminder"
Stop-Service -Name "IkeaDocuScanActionReminder"
Restart-Service -Name "IkeaDocuScanActionReminder"
```

**Check Status:**
```powershell
Get-Service -Name "IkeaDocuScanActionReminder"
```

**View Logs:**
```powershell
Get-EventLog -LogName Application -Source "IkeaDocuScan Action Reminder" -Newest 20
```

**Test SMTP:**
```powershell
Test-NetConnection -ComputerName smtp.company.com -Port 587
```

---

**For issues or questions, check Event Viewer logs first. Most issues are related to configuration, permissions, or SMTP connectivity.**
