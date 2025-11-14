# IkeaDocuScan Deployment Guide

## Overview

This guide covers deploying the IkeaDocuScan application to an on-premise Windows Server with IIS.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration Setup](#configuration-setup)
3. [Database Setup](#database-setup)
4. [Application Deployment](#application-deployment)
5. [IIS Configuration](#iis-configuration)
6. [Security Configuration](#security-configuration)
7. [Testing](#testing)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Server Requirements
- **OS**: Windows Server 2019 or later
- **IIS**: Version 10.0 or later with ASP.NET Core hosting bundle
- **.NET**: .NET 9.0 Runtime (ASP.NET Core)
- **SQL Server**: SQL Server 2016 or later (or access to remote SQL Server)
- **Storage**: Network share or local folder for scanned documents

### Required Software
```powershell
# Check if IIS is installed
Get-WindowsFeature -Name Web-Server

# Check .NET version
dotnet --list-runtimes

# Install .NET 9.0 Hosting Bundle if needed
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0
```

---

## Configuration Setup

### Step 1: Create Folder for Scanned Documents

```powershell
# Create folder (adjust path as needed)
New-Item -Path "C:\ScannedDocuments" -ItemType Directory -Force

# Or use network share
# Example: \\FileServer\ScannedDocuments
```

### Step 2: Run Configuration Encryption Tool

```powershell
# Build the encryption tool
cd ConfigEncryptionTool
dotnet build -c Release

# Run the tool
cd bin\Release\net9.0
.\ConfigEncryptionTool.exe
```

**Tool Prompts:**
```
SQL Server: PROD-SQL-01
Database Name: IkeaDocuScan
Use Windows Authentication? y
Scanned Files Path: C:\ScannedDocuments
```

**Output:**
- Creates `secrets.encrypted.json` with DPAPI-encrypted connection string

### Step 3: Configure appsettings Files

**Location**: `IkeaDocuScan-Web/`

**appsettings.json** (default values, in source control):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IkeaDocuScan;Integrated Security=true;TrustServerCertificate=True;"
  },
  "IkeaDocuScan": {
    "ScannedFilesPath": "C:\\ScannedDocuments",
    "AllowedFileExtensions": [ ".pdf", ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".bmp" ],
    "MaxFileSizeBytes": 52428800,
    "EnableFileListCaching": true,
    "CacheDurationSeconds": 60
  }
}
```

**appsettings.Production.json** (production overrides):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**appsettings.Local.json** (server-specific, NOT in source control):
```json
{
  "IkeaDocuScan": {
    "ScannedFilesPath": "\\\\FileServer\\ScannedDocuments"
  }
}
```

### Step 4: Configuration Hierarchy

```
Priority (highest to lowest):
1. Environment Variables (IIS App Pool)
2. secrets.encrypted.json (DPAPI encrypted)
3. appsettings.Local.json (server-specific)
4. appsettings.Production.json
5. appsettings.json (defaults)
```

---

## Database Setup

### Step 1: Create Database

```sql
CREATE DATABASE IkeaDocuScan;
GO

USE IkeaDocuScan;
GO
```

### Step 2: Run Migration Scripts

Execute in order:
```powershell
# Navigate to migration scripts
cd DbMigration\db-scripts

# Run each script in order
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -i 00_Create_Database_And_User.sql
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -i 01_Add_FK_Columns.sql
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -i 02_Migrate_FK_Data.sql
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -i 03_Finalize_FK_Constraints.sql
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -i 04_Create_DocuScanUser_Table.sql
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -i 05_Migrate_Users_To_DocuScanUser.sql
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -i 06_Add_FK_Constraint_UserPermissions.sql
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -i 07_Remove_AccountName_From_UserPermissions.sql
```

---

## Application Deployment

### Step 1: Build Application

```powershell
# Navigate to solution root
cd IkeaDocuScanV3

# Build in Release mode
dotnet build -c Release

# Publish the application
dotnet publish IkeaDocuScan-Web\IkeaDocuScan-Web\IkeaDocuScan-Web.csproj `
  -c Release `
  -o C:\Deploy\IkeaDocuScan `
  --self-contained false
```

### Step 2: Deploy to IIS Folder

```powershell
# Create IIS folder
New-Item -Path "C:\inetpub\IkeaDocuScan" -ItemType Directory -Force

# Copy published files
Copy-Item -Path "C:\Deploy\IkeaDocuScan\*" `
  -Destination "C:\inetpub\IkeaDocuScan" `
  -Recurse -Force

# Copy encrypted secrets file
Copy-Item -Path "secrets.encrypted.json" `
  -Destination "C:\inetpub\IkeaDocuScan\"

# Copy appsettings.Local.json if using
Copy-Item -Path "appsettings.Local.json" `
  -Destination "C:\inetpub\IkeaDocuScan\" `
  -ErrorAction SilentlyContinue
```

---

## IIS Configuration

### Step 1: Create Application Pool

```powershell
Import-Module WebAdministration

# Create Application Pool
New-WebAppPool -Name "IkeaDocuScanAppPool"

# Configure Application Pool
Set-ItemProperty IIS:\AppPools\IkeaDocuScanAppPool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\IkeaDocuScanAppPool -Name managedPipelineMode -Value "Integrated"
Set-ItemProperty IIS:\AppPools\IkeaDocuScanAppPool -Name startMode -Value "AlwaysRunning"

# Set identity (use NetworkService or custom account)
Set-ItemProperty IIS:\AppPools\IkeaDocuScanAppPool -Name processModel.identityType -Value "NetworkService"
```

### Step 2: Create IIS Website

```powershell
# Create Website
New-Website -Name "IkeaDocuScan" `
  -ApplicationPool "IkeaDocuScanAppPool" `
  -PhysicalPath "C:\inetpub\IkeaDocuScan" `
  -Port 80 `
  -HostHeader "docuscan.company.local"

# Or bind to specific IP
# -IPAddress "192.168.1.100"

# Enable HTTPS (recommended)
New-WebBinding -Name "IkeaDocuScan" `
  -Protocol "https" `
  -Port 443 `
  -HostHeader "docuscan.company.local" `
  -SslFlags 0

# Import SSL certificate
# $cert = Import-PfxCertificate -FilePath "path\to\cert.pfx" -CertStoreLocation Cert:\LocalMachine\My
# New-Item -Path IIS:\SslBindings\0.0.0.0!443 -Value $cert
```

### Step 3: Configure Windows Authentication (if using)

```powershell
# Enable Windows Authentication
Set-WebConfigurationProperty `
  -Filter "/system.webServer/security/authentication/windowsAuthentication" `
  -Name "enabled" `
  -Value "True" `
  -PSPath "IIS:\" `
  -Location "IkeaDocuScan"

# Disable Anonymous Authentication
Set-WebConfigurationProperty `
  -Filter "/system.webServer/security/authentication/anonymousAuthentication" `
  -Name "enabled" `
  -Value "False" `
  -PSPath "IIS:\" `
  -Location "IkeaDocuScan"
```

---

## Security Configuration

### Step 1: Set File Permissions

```powershell
# Get Application Pool identity
$appPoolName = "IkeaDocuScanAppPool"
$identity = "IIS AppPool\$appPoolName"

# Grant read access to application folder
$acl = Get-Acl "C:\inetpub\IkeaDocuScan"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity, "Read,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule)
Set-Acl "C:\inetpub\IkeaDocuScan" $acl

# Protect secrets.encrypted.json (read-only, no one else)
$secretFile = "C:\inetpub\IkeaDocuScan\secrets.encrypted.json"
$acl = Get-Acl $secretFile
$acl.SetAccessRuleProtection($true, $false)  # Remove inherited permissions
$acl.Access | ForEach-Object { $acl.RemoveAccessRule($_) } | Out-Null

# Add only AppPool read permission
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity, "Read", "Allow")
$acl.AddAccessRule($rule)

# Add Administrators full control
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Administrators", "FullControl", "Allow")
$acl.AddAccessRule($rule)

Set-Acl $secretFile $acl

Write-Host "✓ Secrets file protected" -ForegroundColor Green
```

### Step 2: Configure Scanned Files Folder Access

```powershell
# Grant read access to scanned files folder
$scannedPath = "C:\ScannedDocuments"  # Adjust as needed

$acl = Get-Acl $scannedPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity, "Read,ReadAndExecute,ListDirectory", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule)
Set-Acl $scannedPath $acl

Write-Host "✓ Scanned files folder access granted" -ForegroundColor Green
```

### Step 3: SQL Server Permissions

```sql
-- If using Windows Authentication
USE IkeaDocuScan;
GO

-- Create login for IIS AppPool
CREATE LOGIN [IIS APPPOOL\IkeaDocuScanAppPool] FROM WINDOWS;
GO

-- Create user
CREATE USER [IkeaDocuScanAppPool] FOR LOGIN [IIS APPPOOL\IkeaDocuScanAppPool];
GO

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER [IkeaDocuScanAppPool];
ALTER ROLE db_datawriter ADD MEMBER [IkeaDocuScanAppPool];
GO

PRINT '✓ Database permissions granted';
GO
```

---

## Testing

### Step 1: Test Application Start

```powershell
# Start Application Pool
Start-WebAppPool -Name "IkeaDocuScanAppPool"

# Start Website
Start-Website -Name "IkeaDocuScan"

# Check status
Get-WebAppPoolState -Name "IkeaDocuScanAppPool"
Get-Website -Name "IkeaDocuScan"
```

### Step 2: Test Application

1. **Navigate to application**:
   - http://docuscan.company.local
   - https://docuscan.company.local (if HTTPS configured)

2. **Test pages**:
   - Home page loads
   - Documents page loads data from database
   - Check-in Scanned page lists files from folder

3. **Test functionality**:
   - Create a document
   - Edit a document
   - Check audit trail logs
   - View scanned files list

### Step 3: Check Logs

```powershell
# Application logs (if file logging configured)
Get-Content "C:\inetpub\IkeaDocuScan\logs\*.log" -Tail 50

# IIS logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC*\*.log" -Tail 20

# Event Viewer
Get-EventLog -LogName Application -Source "IIS*" -Newest 20
```

---

## Troubleshooting

### Issue: 500 Internal Server Error

**Check:**
1. .NET Runtime installed: `dotnet --list-runtimes`
2. Application Pool running: `Get-WebAppPoolState`
3. File permissions correct
4. Database connection string valid

**Solution:**
```powershell
# Enable detailed errors (temporarily)
$webConfig = "C:\inetpub\IkeaDocuScan\web.config"
[xml]$config = Get-Content $webConfig
$config.configuration.'system.webServer'.aspNetCore.stdoutLogEnabled = "true"
$config.Save($webConfig)

# Check stdout logs
Get-Content "C:\inetpub\IkeaDocuScan\logs\stdout_*.log"
```

### Issue: Cannot decrypt configuration

**Symptom**: "Cannot decrypt configuration" error

**Cause**: Encrypted file created on different machine or user

**Solution**:
```powershell
# Re-run encryption tool on the production server
cd ConfigEncryptionTool
.\ConfigEncryptionTool.exe
# Copy new secrets.encrypted.json to application folder
```

### Issue: Access denied to scanned files folder

**Check permissions:**
```powershell
$identity = "IIS APPPOOL\IkeaDocuScanAppPool"
$path = "C:\ScannedDocuments"

Get-Acl $path | Format-List

# Fix permissions
$acl = Get-Acl $path
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity, "Read,ListDirectory", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule)
Set-Acl $path $acl
```

### Issue: Database connection fails

**Test connection:**
```powershell
# Test SQL connectivity
Test-NetConnection -ComputerName "PROD-SQL-01" -Port 1433

# Test authentication
sqlcmd -S PROD-SQL-01 -d IkeaDocuScan -E -Q "SELECT @@VERSION"
```

**Check connection string:**
```powershell
# Verify appsettings files are loaded in correct order
# Check IIS App Pool environment variables
```

---

## Maintenance

### Update Application

```powershell
# Stop app pool
Stop-WebAppPool -Name "IkeaDocuScanAppPool"

# Backup current version
Copy-Item "C:\inetpub\IkeaDocuScan" "C:\Backup\IkeaDocuScan_$(Get-Date -Format 'yyyyMMdd_HHmmss')" -Recurse

# Deploy new version
Copy-Item "C:\Deploy\IkeaDocuScan\*" "C:\inetpub\IkeaDocuScan" -Recurse -Force

# Start app pool
Start-WebAppPool -Name "IkeaDocuScanAppPool"
```

### Backup Configuration

```powershell
# Backup encrypted secrets and local config
$backupPath = "C:\Backup\Config_$(Get-Date -Format 'yyyyMMdd')"
New-Item -Path $backupPath -ItemType Directory -Force

Copy-Item "C:\inetpub\IkeaDocuScan\secrets.encrypted.json" $backupPath
Copy-Item "C:\inetpub\IkeaDocuScan\appsettings.Local.json" $backupPath -ErrorAction SilentlyContinue
```

---

## Support

For issues or questions:
- Review application logs
- Check Event Viewer
- Verify configuration files
- Test database connectivity

**Important Files:**
- `secrets.encrypted.json` - DPAPI encrypted connection string
- `appsettings.Local.json` - Server-specific configuration
- `web.config` - IIS configuration
