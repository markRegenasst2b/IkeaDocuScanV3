# IkeaDocuScan V3 - Windows Server 2022 Deployment Guide

**Target Server:** Windows Server 2022 Standard
**Framework:** .NET 10.0
**Database Server:** wtseelm-nx20541.ikeadt.com (SQL Server 15.0.4440.1)
**Installation Drive:** D:\
**Production URL:** https://testdocuscan.ikeadt.com
**Server Network:** No Internet Access (Air-Gapped Environment)
**Last Updated:** 2025-01-18

---

## Table of Contents

1. [Overview](#overview)
2. [Server File Structure](#server-file-structure)
3. [Prerequisites](#prerequisites)
4. [Phase 1: Server Preparation](#phase-1-server-preparation)
5. [Phase 2: IIS Installation and Configuration](#phase-2-iis-installation-and-configuration)
6. [Phase 3: SSL Certificate Setup](#phase-3-ssl-certificate-setup)
7. [Phase 4: Database Migration](#phase-4-database-migration)
8. [Phase 5: Application Build and Deployment](#phase-5-application-build-and-deployment)
9. [Phase 6: Application Configuration](#phase-6-application-configuration)
10. [Phase 7: Final IIS Configuration](#phase-7-final-iis-configuration)
11. [Phase 8: Smoke Testing](#phase-8-smoke-testing)
12. [Troubleshooting](#troubleshooting)
13. [Rollback Procedures](#rollback-procedures)

---

## Overview

This guide provides step-by-step instructions for deploying the IkeaDocuScan V3 Blazor application to a Windows Server 2022 Standard server **with no internet access** (air-gapped environment). The deployment includes:

- ✅ Complete file structure setup on D:\ drive
- ✅ IIS installation and configuration from scratch (using Windows Features)
- ✅ Self-signed SSL certificate creation for testing
- ✅ SQL Server database migration
- ✅ Application build, packaging, and deployment
- ✅ Configuration for IKEA Active Directory integration
- ✅ Comprehensive smoke testing procedures
- ✅ Offline .NET 10.0 Runtime installation

**⚠️ IMPORTANT: Air-Gapped Environment**
This server has **NO internet access**. All required software, installers, and deployment packages must be:
- Downloaded on a machine with internet access
- Transferred to the server via USB drive, RDP clipboard, or network share
- See "Required Downloads" section below for complete list

**Estimated Deployment Time:** 3-4 hours (first-time deployment)

---

## Server File Structure

The following directory structure will be created on the D:\ drive:

```
D:\
├── IkeaDocuScan\                          # Main application root
│   ├── Deployment\                        # Deployment working directory
│   │   ├── Archives\                      # Historical deployment packages
│   │   ├── Current\                       # Current deployment package (unzipped)
│   │   ├── Scripts\                       # Deployment helper scripts
│   │   └── Installers\                    # .NET Runtime and other installers (offline)
│   │
│   ├── wwwroot\                           # IIS application root
│   │   └── IkeaDocuScan\                  # Application files (deployed here)
│   │       ├── IkeaDocuScan-Web.dll
│   │       ├── web.config
│   │       ├── wwwroot\
│   │       ├── appsettings.json
│   │       ├── appsettings.Production.json
│   │       ├── appsettings.Local.json     # Server-specific config (create manually)
│   │       ├── secrets.encrypted.json     # Encrypted secrets (create manually)
│   │       └── logs\                      # Application stdout logs
│   │
│   ├── Tools\                             # Utility tools
│   │   ├── ConfigEncryptionTool\          # DPAPI encryption utility
│   │   └── ActionReminder\                # Action Reminder Service (separate deployment)
│   │
│   ├── Database\                          # Database migration scripts
│   │   └── MigrationScripts\              # SQL scripts for database upgrade
│   │
│   └── ScannedFiles\                      # Scanned document storage
│       └── checkin\                       # Check-in directory
│
├── Logs\                                  # Application logs root
│   └── IkeaDocuScan\                      # IkeaDocuScan application logs
│       └── log-YYYYMMDD.json              # Daily rolling log files
│
└── Backups\                               # Backup storage
    └── IkeaDocuScan\                      # Application backups
        ├── Database\                      # Database backups
        └── Config\                        # Configuration file backups
```

**Key Directories:**

| Path | Purpose | Permissions Required |
|------|---------|---------------------|
| `D:\IkeaDocuScan\wwwroot\IkeaDocuScan\` | IIS application root | IIS AppPool: Read & Execute |
| `D:\IkeaDocuScan\ScannedFiles\checkin\` | Scanned documents | IIS AppPool: Read |
| `D:\Logs\IkeaDocuScan\` | Serilog file logs | IIS AppPool: Modify |
| `D:\IkeaDocuScan\Deployment\` | Deployment working folder | Administrators: Full Control |
| `D:\IkeaDocuScan\Database\` | SQL migration scripts | Administrators: Read |
| `D:\Backups\IkeaDocuScan\` | Backup storage | Administrators: Full Control |

---

## Prerequisites

### Required Software on Development Machine

- [ ] **Visual Studio 2022** (v17.8 or later)
- [ ] **.NET 10.0 SDK** installed
- [ ] **SQL Server Management Studio (SSMS)** (latest version)
- [ ] **Git** (for source control)
- [ ] **7-Zip or WinRAR** (for creating deployment package)
- [ ] **Internet access** (for downloading .NET Runtime and other components)

### Required Software on Target Server

- [ ] **Windows Server 2022 Standard** (fully patched)
- [ ] **.NET 10.0 Runtime** (will be installed offline from transferred installer)
- [ ] **ASP.NET Core 10.0 Hosting Bundle** (will be installed offline from transferred installer)
- [ ] **SQL Server Client Tools** or SSMS (if not installed, transfer installer)
- [ ] **Administrator access** to the server

### Network Requirements

- [ ] Server can reach SQL Server: `wtseelm-nx20541.ikeadt.com:1433` (internal network)
- [ ] Server can reach IKEA Active Directory domain controllers (internal network)
- [ ] Server can reach IKEA SMTP gateway: `smtp-gw.ikea.com:25` (internal network)
- [ ] **NO internet access** - all software must be transferred manually
- [ ] Firewall allows inbound HTTPS (port 443) and HTTP (port 80) from internal network

### Required Downloads (On Machine with Internet Access)

**⚠️ CRITICAL: Download these on a machine with internet access BEFORE starting deployment:**

1. **ASP.NET Core 10.0 Hosting Bundle (Windows)**
   - URL: https://dotnet.microsoft.com/download/dotnet/10.0
   - File: `dotnet-hosting-10.0.x-win.exe` (approx. 200MB)
   - Select: **Hosting Bundle** (includes Runtime + IIS integration)

2. **SQL Server Management Studio (Optional - if not on server)**
   - URL: https://aka.ms/ssmsfullsetup
   - File: `SSMS-Setup-ENU.exe` (approx. 600MB)
   - Only needed if not already installed on server

3. **IkeaDocuScan Application Package**
   - Built and packaged from source code (see Phase 5)
   - File: `IkeaDocuScan_vX.X.X_YYYYMMDD_HHMMSS.zip`

**Transfer Method Options:**
- USB flash drive (minimum 2GB)
- RDP clipboard copy/paste
- Internal network file share
- Secure file transfer via internal network

### Pre-Deployment Checklist

**Complete this checklist BEFORE starting deployment to avoid delays:**

**On Machine with Internet Access (Development Machine):**

- [ ] **Download ASP.NET Core 10.0 Hosting Bundle**
  - File: `dotnet-hosting-10.0.x-win.exe` (approx. 200MB)
  - URL: https://dotnet.microsoft.com/download/dotnet/10.0
  - Verify file size is approximately 200MB

- [ ] **Build and package IkeaDocuScan application**
  - Version updated in .csproj file
  - Solution built in Release configuration
  - Application published to folder
  - Database migration scripts included in package
  - ConfigEncryptionTool included in package
  - Deployment ZIP created with timestamp
  - MD5 checksum file created

- [ ] **Prepare Transfer Media**
  - USB flash drive formatted (FAT32 or NTFS)
  - At least 2GB free space on USB drive
  - Or RDP session configured with clipboard enabled
  - Or internal network share path identified and accessible

**Files to Transfer (Total approx. 500MB):**

```
USB Drive or Transfer Location:
├── dotnet-hosting-10.0.x-win.exe           (~200 MB)
├── IkeaDocuScan_vX.X.X_YYYYMMDD.zip       (~100-300 MB)
├── IkeaDocuScan_vX.X.X_YYYYMMDD.zip.md5   (1 KB)
└── SSMS-Setup-ENU.exe (optional)           (~600 MB) - if SSMS not on server
```

**Documentation to Have Available:**
- [ ] This deployment guide (WINDOWS_SERVER_2022_DEPLOYMENT_GUIDE.md)
- [ ] Database migration scripts (if not in deployment package)
- [ ] SQL Server credentials (sqladmin password)
- [ ] Desired `docuscanch` password for application database user
- [ ] IKEA AD group names confirmed
- [ ] Email addresses for service recipients

**Server Access Verified:**
- [ ] RDP access to Windows Server 2022 server working
- [ ] Administrator credentials available
- [ ] Server hostname/IP address noted
- [ ] Server can reach SQL Server (wtseelm-nx20541.ikeadt.com)
- [ ] Server is domain-joined to IKEA Active Directory

**⚠️ STOP**: Do not proceed until ALL items above are checked!

### Access Requirements

- [ ] **Windows Server**: Administrator rights
- [ ] **SQL Server**: `sqladmin` account credentials
- [ ] **Active Directory**: Read access to verify AD groups exist
- [ ] **IKEA SMTP**: Access to SMTP gateway (no authentication required)

### Database Requirements

- [ ] SQL Server 2017 or later (confirmed: 15.0.4440.1)
- [ ] SQL Server running and accessible
- [ ] Database migration SQL scripts available
- [ ] Backup of current production database (if upgrading)

---

## Phase 1: Server Preparation

### Step 1.1: Verify Server Specifications

Run these commands in PowerShell (as Administrator):

```powershell
# Verify Windows version
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, OsHardwareAbstractionLayer

# Expected: Windows Server 2022 Standard

# Check available disk space on D:\ (need at least 10GB)
Get-PSDrive D | Select-Object Name, Used, Free, @{Name="FreeGB";Expression={[math]::Round($_.Free/1GB,2)}}

# Check RAM (recommended 8GB+)
Get-CimInstance Win32_ComputerSystem | Select-Object TotalPhysicalMemory, @{Name="TotalRAM_GB";Expression={[math]::Round($_.TotalPhysicalMemory/1GB,2)}}

# Check if server is domain-joined
Get-ComputerInfo | Select-Object CsDomain, CsDomainRole

# Expected: CsDomain should show ikeadt.com or similar

# Verify NO internet access (expected to fail)
Write-Host "`nTesting internet connectivity (should fail):" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "https://www.microsoft.com" -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
    Write-Host "⚠️ WARNING: Server has internet access! This deployment is designed for air-gapped environment." -ForegroundColor Yellow
} catch {
    Write-Host "✅ Confirmed: No internet access (as expected for air-gapped environment)" -ForegroundColor Green
}
```

**Minimum Requirements:**
- 10GB free space on D:\
- 8GB RAM
- 2+ CPU cores
- Domain-joined to IKEA Active Directory
- **NO internet access** (air-gapped environment)

### Step 1.2: Create Directory Structure

Run in PowerShell (as Administrator):

```powershell
# Create main directory structure
$basePath = "D:\IkeaDocuScan"

$directories = @(
    "$basePath\Deployment\Archives",
    "$basePath\Deployment\Current",
    "$basePath\Deployment\Scripts",
    "$basePath\Deployment\Installers",
    "$basePath\wwwroot\IkeaDocuScan",
    "$basePath\Tools\ConfigEncryptionTool",
    "$basePath\Tools\ActionReminder",
    "$basePath\Database\MigrationScripts",
    "$basePath\ScannedFiles\checkin",
    "D:\Logs\IkeaDocuScan",
    "D:\Backups\IkeaDocuScan\Database",
    "D:\Backups\IkeaDocuScan\Config"
)

foreach ($dir in $directories) {
    New-Item -Path $dir -ItemType Directory -Force
    Write-Host "Created: $dir" -ForegroundColor Green
}

# Verify structure created
Write-Host "`nDirectory structure created successfully:" -ForegroundColor Cyan
Get-ChildItem "D:\IkeaDocuScan" -Recurse -Directory | Select-Object FullName
Get-ChildItem "D:\Logs" -Recurse -Directory | Select-Object FullName
Get-ChildItem "D:\Backups" -Recurse -Directory | Select-Object FullName
```

**Expected Output:**
```
Created: D:\IkeaDocuScan\Deployment\Archives
Created: D:\IkeaDocuScan\Deployment\Current
...
Created: D:\Backups\IkeaDocuScan\Config

Directory structure created successfully:
D:\IkeaDocuScan\Deployment
D:\IkeaDocuScan\Deployment\Archives
...
```

### Step 1.3: Install .NET 10.0 Runtime and Hosting Bundle (Offline)

**⚠️ PREREQUISITE: Transfer the installer to the server first**

The ASP.NET Core 10.0 Hosting Bundle installer must be downloaded on a machine with internet access and transferred to the server.

**On Machine with Internet Access:**

1. Open browser
2. Navigate to: https://dotnet.microsoft.com/download/dotnet/10.0
3. Click **Download Hosting Bundle** (Windows)
4. Save file: `dotnet-hosting-10.0.x-win.exe` (approx. 200MB)
5. Transfer to server using one of these methods:
   - Copy to USB drive
   - Use RDP clipboard (copy file on local machine, paste on RDP session)
   - Copy to internal network share accessible from server

**On the Server:**

1. Copy installer to: `D:\IkeaDocuScan\Deployment\Installers\`

```powershell
# Verify installer is present
$installerPath = "D:\IkeaDocuScan\Deployment\Installers"
$installer = Get-ChildItem $installerPath -Filter "dotnet-hosting-*.exe"

if ($installer) {
    Write-Host "✅ Installer found: $($installer.Name)" -ForegroundColor Green
    Write-Host "   Size: $([math]::Round($installer.Length / 1MB, 2)) MB" -ForegroundColor Cyan
} else {
    Write-Host "❌ Installer not found in $installerPath" -ForegroundColor Red
    Write-Host "   Please transfer dotnet-hosting-10.0.x-win.exe to this location" -ForegroundColor Yellow
    exit
}
```

**Install Hosting Bundle:**

```powershell
# Navigate to installer location
cd D:\IkeaDocuScan\Deployment\Installers

# Find the installer
$installer = Get-ChildItem -Filter "dotnet-hosting-*.exe" | Select-Object -First 1

if ($installer) {
    Write-Host "Installing ASP.NET Core Hosting Bundle..." -ForegroundColor Yellow
    Write-Host "This may take 5-10 minutes..." -ForegroundColor Yellow

    # Run installer silently
    Start-Process -FilePath $installer.FullName -ArgumentList "/quiet", "/norestart" -Wait

    Write-Host "✅ ASP.NET Core Hosting Bundle installed successfully" -ForegroundColor Green

    # Restart IIS to load new .NET runtime
    Write-Host "Restarting IIS..." -ForegroundColor Yellow
    net stop was /y
    net start w3svc

    Write-Host "✅ IIS restarted" -ForegroundColor Green
} else {
    Write-Host "❌ Installer not found!" -ForegroundColor Red
    exit
}
```

**Verify Installation:**

```powershell
# Check installed .NET runtimes
dotnet --list-runtimes

# Expected output should include:
# Microsoft.AspNetCore.App 10.0.x [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
# Microsoft.NETCore.App 10.0.x [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
```

**If `dotnet` command not found after installation:**

```powershell
# Add to system PATH
$env:Path += ";C:\Program Files\dotnet"
[Environment]::SetEnvironmentVariable("Path", $env:Path, [EnvironmentVariableTarget]::Machine)

# Close and reopen PowerShell, then verify again
dotnet --version

# If still not working, reboot server
# Restart-Computer -Force
```

**Alternative: Verify installation via Programs and Features:**

```powershell
# Check installed programs for .NET
Get-ItemProperty HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\* |
    Where-Object {$_.DisplayName -like "*ASP.NET Core*" -or $_.DisplayName -like "*.NET*"} |
    Select-Object DisplayName, DisplayVersion, InstallDate |
    Format-Table -AutoSize

# Expected: ASP.NET Core 10.0.x Runtime and Hosting Bundle
```

### Step 1.4: Test SQL Server Connectivity

```powershell
# Test network connectivity to SQL Server
Test-NetConnection -ComputerName "wtseelm-nx20541.ikeadt.com" -Port 1433

# Expected: TcpTestSucceeded : True
```

**If connection fails:**
- Check firewall rules
- Verify SQL Server is running
- Confirm SQL Server allows remote connections

**Test SQL Authentication (if available):**

```powershell
# Install SqlServer module if not present
Install-Module -Name SqlServer -Scope CurrentUser -Force

# Test connection with sqladmin
$serverName = "wtseelm-nx20541.ikeadt.com"
$credential = Get-Credential -Message "Enter sqladmin credentials"

try {
    Invoke-Sqlcmd -ServerInstance $serverName -Database "master" -Credential $credential -Query "SELECT @@VERSION AS 'SQL Server Version'"
    Write-Host "SQL Server connection successful!" -ForegroundColor Green
} catch {
    Write-Host "SQL Server connection failed: $_" -ForegroundColor Red
}
```

### Step 1.5: Verify Active Directory Access

```powershell
# Check if server can reach AD
$domain = $env:USERDNSDOMAIN
Write-Host "Domain: $domain"

# Test LDAP connectivity
$ldapPath = "LDAP://DC=ikeadt,DC=com"
try {
    $searcher = New-Object System.DirectoryServices.DirectorySearcher
    $searcher.SearchRoot = New-Object System.DirectoryServices.DirectoryEntry($ldapPath)
    $searcher.Filter = "(objectClass=domain)"
    $result = $searcher.FindOne()

    if ($result) {
        Write-Host "Active Directory connection successful!" -ForegroundColor Green
    }
} catch {
    Write-Host "Active Directory connection failed: $_" -ForegroundColor Red
}

# Verify IKEA AD groups exist (optional - may require specific permissions)
$adGroups = @(
    "UG-DocScanningReaders-CG@WAL-FIN-CH-GEL",
    "UG-DocScanningPublishers-CG@WAL-FIN-CH-GEL",
    "UG-DocScanningSuperUsers-CG@WAL-FIN-CH-GEL"
)

foreach ($group in $adGroups) {
    try {
        $adGroup = Get-ADGroup -Filter "Name -eq '$group'" -ErrorAction Stop
        Write-Host "Found AD Group: $group" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not verify AD Group: $group" -ForegroundColor Yellow
    }
}
```

**Checklist:**

- [ ] D:\ drive has 10GB+ free space
- [ ] Directory structure created successfully
- [ ] ASP.NET Core 10.0 Hosting Bundle installer transferred to server
- [ ] .NET 10.0 Hosting Bundle installed (offline installer)
- [ ] `dotnet --list-runtimes` shows ASP.NET Core 10.0.x
- [ ] SQL Server connectivity verified (port 1433) - internal network
- [ ] Active Directory connectivity verified - internal network
- [ ] Server is domain-joined
- [ ] Server confirmed to have NO internet access (expected)

---

## Phase 2: IIS Installation and Configuration

### Step 2.1: Install IIS with Required Features

Run in PowerShell (as Administrator):

```powershell
# Install IIS with all required features
Install-WindowsFeature -Name Web-Server -IncludeManagementTools

# Install additional required features
$features = @(
    'Web-WebServer',
    'Web-Common-Http',
    'Web-Default-Doc',
    'Web-Dir-Browsing',
    'Web-Http-Errors',
    'Web-Static-Content',
    'Web-Http-Redirect',
    'Web-Health',
    'Web-Http-Logging',
    'Web-Log-Libraries',
    'Web-Request-Monitor',
    'Web-Performance',
    'Web-Stat-Compression',
    'Web-Dyn-Compression',
    'Web-Security',
    'Web-Filtering',
    'Web-Windows-Auth',
    'Web-App-Dev',
    'Web-Net-Ext45',
    'Web-Asp-Net45',
    'Web-ISAPI-Ext',
    'Web-ISAPI-Filter',
    'Web-WebSockets',
    'Web-Mgmt-Tools',
    'Web-Mgmt-Console'
)

Install-WindowsFeature -Name $features -IncludeManagementTools

Write-Host "`nIIS Installation Complete!" -ForegroundColor Green
```

**Verify Installation:**

```powershell
# Check IIS is running
Get-Service -Name W3SVC | Select-Object Name, Status, StartType

# Expected: Status = Running, StartType = Automatic

# Open IIS Manager
inetmgr
```

### Step 2.2: Test IIS Installation

**Create Test Page:**

```powershell
# Create simple test HTML file
$testHtml = @"
<!DOCTYPE html>
<html>
<head>
    <title>IIS Test Page</title>
</head>
<body>
    <h1>IIS is Working!</h1>
    <p>Server: $env:COMPUTERNAME</p>
    <p>Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
</body>
</html>
"@

Set-Content -Path "C:\inetpub\wwwroot\test.html" -Value $testHtml

Write-Host "Test page created at: http://localhost/test.html" -ForegroundColor Green
```

**Test in Browser:**

```powershell
# Open default browser to test page
Start-Process "http://localhost/test.html"

# Expected: Browser shows "IIS is Working!" page
```

**Verify from Remote Machine (optional):**
- From another computer on the network, browse to: `http://[SERVER-IP]/test.html`
- If accessible, IIS is working correctly

### Step 2.3: Configure IIS for ASP.NET Core

**Enable IIS features required for ASP.NET Core:**

Already included in Step 2.1, but verify:

```powershell
# Verify ASP.NET Core Module is installed
Get-WebConfigurationProperty -Filter system.webServer/globalModules -PSPath 'IIS:\' -Name Collection |
    Where-Object {$_.name -like "*AspNetCore*"} |
    Select-Object name, image

# Expected: AspNetCoreModuleV2 should be listed
```

**If not found, reinstall Hosting Bundle:**

```powershell
# Repair ASP.NET Core Hosting Bundle
cd D:\IkeaDocuScan\Deployment
Start-Process -FilePath ".\dotnet-hosting-10.0.x-win.exe" -ArgumentList "/repair", "/quiet", "/norestart" -Wait

# Restart IIS
iisreset
```

**Checklist:**

- [ ] IIS installed successfully
- [ ] IIS Manager opens without errors
- [ ] Test page displays correctly at http://localhost/test.html
- [ ] Windows Authentication feature installed
- [ ] WebSocket Protocol feature installed
- [ ] ASP.NET Core Module V2 is registered

---

## Phase 3: SSL Certificate Setup

Since no SSL certificate is available yet, we'll create a self-signed certificate for testing purposes.

### Step 3.1: Create Self-Signed SSL Certificate

Run in PowerShell (as Administrator):

```powershell
# Create self-signed certificate for testing
$certSubject = "testdocuscan.ikeadt.com"
$certName = "IkeaDocuScan Test Certificate"

$cert = New-SelfSignedCertificate `
    -Subject "CN=$certSubject" `
    -DnsName $certSubject, "localhost", $env:COMPUTERNAME `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyUsage DigitalSignature, KeyEncipherment `
    -NotAfter (Get-Date).AddYears(2) `
    -FriendlyName $certName `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") `
    -KeyExportPolicy Exportable

Write-Host "Certificate created successfully!" -ForegroundColor Green
Write-Host "Thumbprint: $($cert.Thumbprint)" -ForegroundColor Cyan
Write-Host "Subject: $($cert.Subject)" -ForegroundColor Cyan
Write-Host "Expiry: $($cert.NotAfter)" -ForegroundColor Cyan

# Save thumbprint for later use
$certThumbprint = $cert.Thumbprint
$certThumbprint | Out-File "D:\IkeaDocuScan\Deployment\certificate-thumbprint.txt"

Write-Host "`nCertificate thumbprint saved to: D:\IkeaDocuScan\Deployment\certificate-thumbprint.txt" -ForegroundColor Green
```

### Step 3.2: Add Certificate to Trusted Root (for Testing)

**⚠️ WARNING:** This is only for testing. In production, use a certificate from a trusted CA.

```powershell
# Export certificate
$certPassword = ConvertTo-SecureString -String "TempPassword123!" -Force -AsPlainText
$certPath = "D:\IkeaDocuScan\Deployment\testdocuscan-cert.pfx"

Export-PfxCertificate -Cert "Cert:\LocalMachine\My\$certThumbprint" `
    -FilePath $certPath `
    -Password $certPassword

Write-Host "Certificate exported to: $certPath" -ForegroundColor Green

# Import to Trusted Root Certificates (makes it trusted on this server)
Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\Root

Write-Host "Certificate added to Trusted Root - browser warnings will not appear on this server" -ForegroundColor Yellow
Write-Host "⚠️ CLIENT MACHINES WILL STILL SHOW WARNINGS - This is expected for self-signed certificates" -ForegroundColor Yellow
```

### Step 3.3: Verify Certificate Installation

```powershell
# Verify certificate in Personal store
Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.Subject -like "*testdocuscan*"} |
    Select-Object Subject, Thumbprint, NotAfter, FriendlyName | Format-List

# Verify certificate in Trusted Root store
Get-ChildItem Cert:\LocalMachine\Root | Where-Object {$_.Subject -like "*testdocuscan*"} |
    Select-Object Subject, Thumbprint, NotAfter, FriendlyName | Format-List

Write-Host "`nCertificate is ready for IIS binding" -ForegroundColor Green
```

### Step 3.4: Configure DNS (Hosts File for Testing)

For testing purposes, add entry to hosts file:

```powershell
# Add entry to hosts file (run as Administrator)
$hostsPath = "C:\Windows\System32\drivers\etc\hosts"
$hostsEntry = "`n# IkeaDocuScan Test`n127.0.0.1    testdocuscan.ikeadt.com"

# Backup original hosts file
Copy-Item $hostsPath "$hostsPath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

# Add entry
Add-Content -Path $hostsPath -Value $hostsEntry

Write-Host "Hosts file updated. Entry added:" -ForegroundColor Green
Write-Host $hostsEntry -ForegroundColor Cyan

# Verify
Write-Host "`nVerifying DNS resolution:" -ForegroundColor Cyan
Resolve-DnsName -Name "testdocuscan.ikeadt.com" -Type A -DnsOnly -ErrorAction SilentlyContinue
nslookup testdocuscan.ikeadt.com
```

**For Production:**
- Remove hosts file entry
- Create proper DNS A record pointing to server IP
- Use certificate from trusted CA (e.g., Let's Encrypt, internal CA, or commercial CA)

**Checklist:**

- [ ] Self-signed certificate created successfully
- [ ] Certificate thumbprint saved to file
- [ ] Certificate exported to .pfx file
- [ ] Certificate added to Trusted Root store
- [ ] Hosts file updated with testdocuscan.ikeadt.com entry
- [ ] DNS resolution test successful

---

## Phase 4: Database Migration

### Step 4.1: Prepare SQL Server Connection

**Connect to SQL Server using SSMS:**

1. Open SQL Server Management Studio (SSMS)
2. Server name: `wtseelm-nx20541.ikeadt.com`
3. Authentication: SQL Server Authentication
4. Login: `sqladmin`
5. Password: [Enter sqladmin password]
6. Click **Connect**

**Verify Connection:**

```sql
-- Check SQL Server version
SELECT @@VERSION AS 'SQL Server Version';

-- Expected: Microsoft SQL Server 2019 or later (15.0.4440.1)

-- Check available databases
SELECT name, database_id, create_date
FROM sys.databases
WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')
ORDER BY name;
```

### Step 4.2: Create Database and Application User

**Important:** The SQL migration scripts should handle this, but verify the database user creation.

```sql
-- 1. Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IkeaDocuScan')
BEGIN
    CREATE DATABASE IkeaDocuScan;
    PRINT 'Database IkeaDocuScan created successfully';
END
ELSE
BEGIN
    PRINT 'Database IkeaDocuScan already exists';
END
GO

-- 2. Switch to new database
USE IkeaDocuScan;
GO

-- 3. Create SQL login for application (if not exists)
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'docuscanch')
BEGIN
    -- Replace 'YourSecurePassword123!' with a strong password
    CREATE LOGIN docuscanch WITH PASSWORD = 'YourSecurePassword123!', CHECK_POLICY = OFF;
    PRINT 'Login docuscanch created successfully';
END
ELSE
BEGIN
    PRINT 'Login docuscanch already exists';
END
GO

-- 4. Create database user and grant permissions
USE IkeaDocuScan;
GO

IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'docuscanch')
BEGIN
    CREATE USER docuscanch FOR LOGIN docuscanch;
    PRINT 'User docuscanch created successfully';
END
GO

-- 5. Grant necessary permissions
ALTER ROLE db_datareader ADD MEMBER docuscanch;
ALTER ROLE db_datawriter ADD MEMBER docuscanch;
ALTER ROLE db_ddladmin ADD MEMBER docuscanch; -- Required for migrations
GO

-- 6. Verify user creation
SELECT
    dp.name AS DatabaseUser,
    dp.type_desc AS UserType,
    dp.create_date AS CreatedDate,
    STRING_AGG(dr.name, ', ') AS DatabaseRoles
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
LEFT JOIN sys.database_principals dr ON drm.role_principal_id = dr.principal_id
WHERE dp.name = 'IKEA\L-DSCAN-A-FINCHPRA'
GROUP BY dp.name, dp.type_desc, dp.create_date;
GO
```

**⚠️ IMPORTANT:** Save the password for `docuscanch` user - you'll need it for application configuration.

### Step 4.3: Copy Migration Scripts to Server

**Transfer SQL scripts from development machine:**

Method 1: Copy via RDP clipboard
Method 2: Copy via network share
Method 3: Upload to server

```powershell
# On the server, verify scripts are in place
$scriptsPath = "D:\IkeaDocuScan\Database\MigrationScripts"

# List all SQL scripts
Get-ChildItem $scriptsPath -Filter "*.sql" | Select-Object Name, Length, LastWriteTime

# Expected: Multiple .sql files in numbered order
```

**Migration scripts should be executed in this order:**

The exact scripts will be provided separately, but typical migration order:

3. `02_Migrate_Data.sql` - Data migration (if upgrading)
4. `03_Create_Stored_Procedures.sql` - Stored procedures (if any)
5. `04_Seed_Reference_Data.sql` - Reference data (document types, countries, etc.)
6. `05_Create_Indexes.sql` - Additional indexes for performance
7. `06_Update_Schema.sql` - Schema updates (if upgrading from older version)

### Step 4.4: Execute Migration Scripts

**⚠️ CRITICAL:** Execute scripts one at a time, in order, and verify success before proceeding.

**Create Execution Log:**

```powershell
# Create log file
$logPath = "D:\IkeaDocuScan\Database\Migration_Log_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
$logHeader = @"
IkeaDocuScan Database Migration Log
====================================
Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Server: wtseelm-nx20541.ikeadt.com
Database: IkeaDocuScan
Executed By: $env:USERNAME

"@

$logHeader | Out-File $logPath
Write-Host "Migration log created: $logPath" -ForegroundColor Green
```

**Execute Each Script:**

For each SQL script file:

1. Open script in SSMS
2. Verify database is set to **IkeaDocuScan** (dropdown at top)
3. Review script contents (do NOT modify)
4. Click **Execute** (F5)
5. Wait for completion
6. **Check Messages tab:**
   - ✅ **Green** = Success
   - ❌ **Red** = Error - **STOP AND INVESTIGATE**
7. Record results in log file

**Example execution for each script:**

```sql
-- In SSMS Query Window:
-- 1. Ensure correct database is selected
USE IkeaDocuScan;
GO

-- 2. Execute the script (click Execute or press F5)
-- 3. Check Messages tab for results

-- Example success message:
-- (XX rows affected)
-- Command(s) completed successfully.
```

**Document execution results:**

```powershell
# Add to log file after each script execution
$scriptResult = @"

Script: 01_Create_Schema.sql
Execution Time: $(Get-Date -Format 'HH:mm:ss')
Status: SUCCESS / FAILED
Rows Affected: XX
Duration: X seconds
Notes: [Any warnings or special observations]
----------------------------------------

"@

Add-Content -Path $logPath -Value $scriptResult
```

### Step 4.5: Verify Database Migration

After all scripts execute successfully:

```sql
-- 1. Verify all expected tables exist
USE IkeaDocuScan;
GO

SELECT
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Expected tables (approximate):
-- ActionReminder, AuditTrail, CounterParty, CounterPartyRelation
-- Country, Currency, Document, DocumentFile, DocumentName, DocumentType
-- DocuScanUser, UserPermission

-- 2. Verify row counts
SELECT 'ActionReminder' AS TableName, COUNT(*) AS RowCount FROM ActionReminder
UNION ALL
SELECT 'AuditTrail', COUNT(*) FROM AuditTrail
UNION ALL
SELECT 'CounterParty', COUNT(*) FROM CounterParty
UNION ALL
SELECT 'Country', COUNT(*) FROM Country
UNION ALL
SELECT 'Currency', COUNT(*) FROM Currency
UNION ALL
SELECT 'Document', COUNT(*) FROM Document
UNION ALL
SELECT 'DocumentFile', COUNT(*) FROM DocumentFile
UNION ALL
SELECT 'DocumentName', COUNT(*) FROM DocumentName
UNION ALL
SELECT 'DocumentType', COUNT(*) FROM DocumentType
UNION ALL
SELECT 'DocuScanUser', COUNT(*) FROM DocuScanUser
UNION ALL
SELECT 'UserPermission', COUNT(*) FROM UserPermission;

-- 3. Verify foreign key constraints
SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
ORDER BY TableName, ForeignKeyName;

-- 4. Test connection with application user
EXECUTE AS USER = 'docuscanch';

-- Should succeed (read access)
SELECT COUNT(*) AS TableCount
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';

-- Should succeed (write access test)
-- (Don't actually insert - just verify permission)
SELECT HAS_PERMS_BY_NAME('dbo.Document', 'OBJECT', 'INSERT') AS CanInsert;
-- Expected: 1 (true)

REVERT; -- Switch back to admin user
GO
```

### Step 4.6: Test Database Connection from Server

```powershell
# Test connection from PowerShell on the server
$serverName = "wtseelm-nx20541.ikeadt.com"
$databaseName = "IkeaDocuScan"
$username = "docuscanch"
$password = "YourSecurePassword123!" # Use actual password

$connectionString = "Server=$serverName;Database=$databaseName;User Id=$username;Password=$password;TrustServerCertificate=True;"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
    $tableCount = $command.ExecuteScalar()

    Write-Host "✅ Database connection successful!" -ForegroundColor Green
    Write-Host "   Table count: $tableCount" -ForegroundColor Cyan

    $connection.Close()
} catch {
    Write-Host "❌ Database connection failed: $_" -ForegroundColor Red
}
```

**Checklist:**

- [ ] SQL Server connection established
- [ ] Database `IkeaDocuScan` created
- [ ] SQL login `docuscanch` created
- [ ] Database user `docuscanch` created with proper roles
- [ ] All migration scripts executed successfully (logged)
- [ ] All expected tables exist
- [ ] Foreign key constraints verified
- [ ] Connection test with `docuscanch` user successful
- [ ] Migration log file saved to D:\IkeaDocuScan\Database\

---

## Phase 5: Application Build and Deployment

### Step 5.1: Update Version Number

**On Development Machine:**

1. Open solution: `IkeaDocuScanV3.sln` in Visual Studio 2022
2. Open file: `IkeaDocuScan-Web\IkeaDocuScan-Web.csproj`
3. Locate version properties:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <VersionPrefix>3.1.0</VersionPrefix>
  <VersionSuffix>testdeploy</VersionSuffix>
  <!-- Version will be: 3.1.0-testdeploy -->
</PropertyGroup>
```

4. Update version as appropriate:
   - Production release: `<VersionPrefix>3.1.0</VersionPrefix>` and `<VersionSuffix></VersionSuffix>`
   - Test deployment: `<VersionPrefix>3.1.0</VersionPrefix>` and `<VersionSuffix>test</VersionSuffix>`

5. Save file

### Step 5.2: Clean and Rebuild Solution

**In Visual Studio:**

1. Right-click solution in Solution Explorer → **Clean Solution**
2. Wait for completion (check Output window)
3. Right-click solution → **Rebuild Solution**
4. Verify no errors (check Error List window)
5. Check Output window shows: `Build succeeded`

**Expected Output:**
```
Build started...
1>------ Rebuild All started: Project: IkeaDocuScan.Shared ------
1>IkeaDocuScan.Shared -> D:\...\bin\Release\net10.0\IkeaDocuScan.Shared.dll
...
========== Rebuild All: 5 succeeded, 0 failed, 0 skipped ==========
```

### Step 5.3: Publish Application

**Using Visual Studio:**

1. Right-click `IkeaDocuScan-Web` project → **Publish**
2. Click **New** (or select existing profile)
3. Target: **Folder**
4. Location: `C:\Publish\IkeaDocuScan` (or your preferred location)
5. Click **Finish**
6. Click **Show all settings**

**Configure Publish Profile:**

| Setting | Value |
|---------|-------|
| Configuration | Release |
| Target Framework | net10.0 |
| Deployment Mode | Framework-dependent |
| Target Runtime | Portable |
| File Publish Options | ☑ Delete existing files prior to publish |

7. Click **Save**
8. Click **Publish**
9. Wait for completion (check Output window)

**Expected Output:**
```
Publish started...
IkeaDocuScan-Web -> C:\Publish\IkeaDocuScan\
Publish succeeded.
========== Publish: 1 succeeded, 0 failed ==========
```

**Verify Published Files:**

```powershell
# On development machine
cd C:\Publish\IkeaDocuScan

# List key files
Get-ChildItem | Select-Object Name, Length

# Expected files:
# IkeaDocuScan-Web.dll
# IkeaDocuScan-Web.deps.json
# IkeaDocuScan-Web.runtimeconfig.json
# web.config
# appsettings.json
# appsettings.Production.json
# wwwroot\ (directory with _framework\)
```

### Step 5.4: Include Database Scripts and Tools

**Copy Database Scripts:**

```powershell
# Copy migration scripts to publish folder
$publishPath = "C:\Publish\IkeaDocuScan"
$scriptsSource = "D:\ProjectSource\IkeaDocuScanV3\Database\MigrationScripts"

Copy-Item -Path $scriptsSource -Destination "$publishPath\Database\MigrationScripts" -Recurse -Force

Write-Host "Migration scripts copied to publish folder" -ForegroundColor Green
```

**Copy ConfigEncryptionTool:**

```powershell
# Build ConfigEncryptionTool project
# In Visual Studio: Right-click ConfigEncryptionTool project → Publish (or Build)

# Copy to publish folder
$toolSource = "D:\ProjectSource\IkeaDocuScanV3\ConfigEncryptionTool\bin\Release\net10.0"
$toolDest = "$publishPath\Tools\ConfigEncryptionTool"

Copy-Item -Path "$toolSource\*" -Destination $toolDest -Recurse -Force

Write-Host "ConfigEncryptionTool copied to publish folder" -ForegroundColor Green
```

### Step 5.5: Create Deployment Package

```powershell
# Create ZIP file with timestamp
$publishPath = "C:\Publish\IkeaDocuScan"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$version = "v3.1.0-test" # Match your version
$zipName = "IkeaDocuScan_${version}_${timestamp}.zip"
$zipPath = "C:\Publish\$zipName"

# Create ZIP archive
Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -Force

Write-Host "`nDeployment package created:" -ForegroundColor Green
Write-Host "  File: $zipPath" -ForegroundColor Cyan
Write-Host "  Size: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Cyan

# Create MD5 checksum for verification
$md5 = Get-FileHash -Path $zipPath -Algorithm MD5
$md5.Hash | Out-File "$zipPath.md5"

Write-Host "  MD5: $($md5.Hash)" -ForegroundColor Cyan
Write-Host "`nChecksum saved to: $zipPath.md5" -ForegroundColor Green
```

### Step 5.6: Transfer Deployment Package to Server (Offline Transfer)

**⚠️ IMPORTANT: Server has NO internet access - manual transfer required**

Choose one of the following methods to transfer the deployment package:

**Method 1: USB Flash Drive (Recommended for Air-Gapped)**
```powershell
# On Development Machine:
# 1. Copy deployment ZIP to USB drive
$zipPath = "C:\Publish\IkeaDocuScan_v3.1.0-test_20250118_140530.zip"
$usbDrive = "E:\" # Adjust drive letter as needed

Copy-Item -Path $zipPath -Destination $usbDrive
Copy-Item -Path "$zipPath.md5" -Destination $usbDrive

Write-Host "✅ Files copied to USB drive" -ForegroundColor Green
Write-Host "   Safely remove USB and connect to server" -ForegroundColor Yellow

# On Server:
# 1. Connect USB drive to server
# 2. Copy from USB to deployment directory
$usbDrive = "E:\" # Adjust drive letter as needed
$destPath = "D:\IkeaDocuScan\Deployment\Archives\"

Copy-Item -Path "$usbDrive\IkeaDocuScan*.zip" -Destination $destPath
Copy-Item -Path "$usbDrive\*.md5" -Destination $destPath

Write-Host "✅ Deployment package transferred to server" -ForegroundColor Green
```

**Method 2: RDP Clipboard Copy/Paste**
```powershell
# On Development Machine:
# 1. Locate ZIP file in Windows Explorer
# 2. Right-click → Copy (Ctrl+C)

# On Server (in RDP session):
# 3. Navigate to D:\IkeaDocuScan\Deployment\Archives\
# 4. Right-click → Paste (Ctrl+V)
# 5. Wait for file transfer to complete (may take several minutes for large files)

# Verify transfer completed
$destPath = "D:\IkeaDocuScan\Deployment\Archives\"
Get-ChildItem $destPath -Filter "IkeaDocuScan*.zip" | Select-Object Name, Length, LastWriteTime
```

**Method 3: Internal Network Share (If Available)**
```powershell
# From development machine (must be on same internal network)
$zipPath = "C:\Publish\IkeaDocuScan_v3.1.0-test_20250118_140530.zip"
$serverShare = "\\SERVER-NAME\D$\IkeaDocuScan\Deployment\Archives\"

# Test network path accessibility first
if (Test-Path $serverShare) {
    Copy-Item -Path $zipPath -Destination $serverShare
    Copy-Item -Path "$zipPath.md5" -Destination $serverShare
    Write-Host "✅ Files copied via network share" -ForegroundColor Green
} else {
    Write-Host "❌ Network share not accessible" -ForegroundColor Red
    Write-Host "   Use USB drive or RDP clipboard method instead" -ForegroundColor Yellow
}
```

**Method 4: Secure File Transfer via Jump Server (If Available)**
```powershell
# If organization has a jump server or bastion host:
# 1. Transfer files to jump server first
# 2. From jump server, transfer to target server
# Consult with network/security team for approved transfer method
```

**Verify Transfer Integrity:**
```powershell
# On Server - verify file transferred completely
$zipFile = Get-ChildItem "D:\IkeaDocuScan\Deployment\Archives" -Filter "IkeaDocuScan*.zip" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($zipFile) {
    Write-Host "✅ Deployment package found" -ForegroundColor Green
    Write-Host "   File: $($zipFile.Name)" -ForegroundColor Cyan
    Write-Host "   Size: $([math]::Round($zipFile.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    Write-Host "   Date: $($zipFile.LastWriteTime)" -ForegroundColor Cyan
} else {
    Write-Host "❌ Deployment package not found!" -ForegroundColor Red
}
```

### Step 5.7: Extract Deployment Package on Server

**On the Server (PowerShell as Administrator):**

```powershell
# Navigate to deployment directory
cd D:\IkeaDocuScan\Deployment

# Find the uploaded ZIP file
$zipFile = Get-ChildItem "D:\IkeaDocuScan\Deployment\Archives" -Filter "*.zip" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

Write-Host "Found deployment package: $($zipFile.Name)" -ForegroundColor Cyan

# Verify MD5 checksum (if available)
$md5File = "$($zipFile.FullName).md5"
if (Test-Path $md5File) {
    $expectedMd5 = Get-Content $md5File
    $actualMd5 = (Get-FileHash -Path $zipFile.FullName -Algorithm MD5).Hash

    if ($expectedMd5 -eq $actualMd5) {
        Write-Host "✅ MD5 checksum verified - file integrity confirmed" -ForegroundColor Green
    } else {
        Write-Host "❌ MD5 checksum mismatch - file may be corrupted!" -ForegroundColor Red
        Write-Host "   Expected: $expectedMd5" -ForegroundColor Red
        Write-Host "   Actual:   $actualMd5" -ForegroundColor Red
        exit
    }
}

# Extract to Current folder
$extractPath = "D:\IkeaDocuScan\Deployment\Current"

# Clear existing contents
if (Test-Path $extractPath) {
    Remove-Item "$extractPath\*" -Recurse -Force
}

# Extract ZIP
Expand-Archive -Path $zipFile.FullName -DestinationPath $extractPath -Force

Write-Host "✅ Deployment package extracted to: $extractPath" -ForegroundColor Green

# Verify extraction
Write-Host "`nExtracted contents:" -ForegroundColor Cyan
Get-ChildItem $extractPath | Select-Object Name, Length, LastWriteTime
```

### Step 5.8: Copy Application Files to wwwroot

```powershell
# Copy from Current to wwwroot
$sourcePath = "D:\IkeaDocuScan\Deployment\Current"
$destPath = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan"

# Create backup of existing deployment (if exists)
if (Test-Path "$destPath\IkeaDocuScan-Web.dll") {
    $backupPath = "D:\Backups\IkeaDocuScan\Deployment_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    New-Item -Path $backupPath -ItemType Directory -Force
    Copy-Item -Path "$destPath\*" -Destination $backupPath -Recurse -Force
    Write-Host "✅ Existing deployment backed up to: $backupPath" -ForegroundColor Yellow
}

# Clear destination (except config files)
Get-ChildItem $destPath |
    Where-Object {$_.Name -notin @('appsettings.Local.json', 'secrets.encrypted.json', 'logs')} |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# Copy new deployment
Copy-Item -Path "$sourcePath\*" -Destination $destPath -Recurse -Force

Write-Host "✅ Application files copied to: $destPath" -ForegroundColor Green

# Create logs directory if it doesn't exist
$logsPath = "$destPath\logs"
if (-not (Test-Path $logsPath)) {
    New-Item -Path $logsPath -ItemType Directory -Force
}

# Verify deployment
Write-Host "`nDeployed application files:" -ForegroundColor Cyan
Get-ChildItem $destPath -File | Select-Object Name, Length | Format-Table
```

### Step 5.9: Copy Tools to Tools Directory

```powershell
# Copy ConfigEncryptionTool
$toolSource = "D:\IkeaDocuScan\Deployment\Current\Tools\ConfigEncryptionTool"
$toolDest = "D:\IkeaDocuScan\Tools\ConfigEncryptionTool"

if (Test-Path $toolSource) {
    Copy-Item -Path "$toolSource\*" -Destination $toolDest -Recurse -Force
    Write-Host "✅ ConfigEncryptionTool copied to: $toolDest" -ForegroundColor Green
} else {
    Write-Host "⚠️ Warning: ConfigEncryptionTool not found in deployment package" -ForegroundColor Yellow
}

# Verify tool works
cd $toolDest
if (Test-Path ".\ConfigEncryptionTool.exe") {
    Write-Host "`nConfigEncryptionTool ready for use" -ForegroundColor Green
} else {
    Write-Host "❌ ConfigEncryptionTool.exe not found!" -ForegroundColor Red
}
```

**Checklist:**

- [ ] Version number updated in .csproj file
- [ ] Solution cleaned and rebuilt successfully (no errors)
- [ ] Application published to folder
- [ ] Published files verified (DLL, web.config, wwwroot, etc.)
- [ ] Database scripts included in publish folder
- [ ] ConfigEncryptionTool included in publish folder
- [ ] Deployment package (ZIP) created with timestamp
- [ ] MD5 checksum generated
- [ ] Package transferred to server
- [ ] MD5 checksum verified on server
- [ ] Package extracted to D:\IkeaDocuScan\Deployment\Current\
- [ ] Existing deployment backed up (if applicable)
- [ ] Application files copied to D:\IkeaDocuScan\wwwroot\IkeaDocuScan\
- [ ] ConfigEncryptionTool copied to D:\IkeaDocuScan\Tools\
- [ ] logs directory created

---

## Phase 6: Application Configuration

### Step 6.1: Review Default Configuration

```powershell
# Review deployed appsettings.json
$appSettings = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.json"
Get-Content $appSettings | Out-String

# Review appsettings.Production.json
$appSettingsProd = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Production.json"
Get-Content $appSettingsProd | Out-String
```

**Do NOT modify these files** - they are deployed with the application.

### Step 6.2: Create Encrypted Connection String

**Run ConfigEncryptionTool as IIS AppPool Identity:**

```powershell
# Navigate to tool directory
cd D:\IkeaDocuScan\Tools\ConfigEncryptionTool

# Run tool (will be run as IIS AppPool identity after AppPool is created)
# For now, run as Administrator to create initial config

.\ConfigEncryptionTool.exe
```

**ConfigEncryptionTool will prompt for:**

```
===========================================
IkeaDocuScan Configuration Encryption Tool
===========================================

Enter SQL Server hostname: wtseelm-nx20541.ikeadt.com
Enter Database name: IkeaDocuScan
Use Windows Authentication? (y/n): n
Enter SQL Username: docuscanch
Enter SQL Password: ********************
Enter Scanned Files Path: D:\IkeaDocuScan\ScannedFiles\checkin

Encrypting configuration using DPAPI...
✓ Connection string encrypted successfully
✓ Configuration file created: secrets.encrypted.json

Testing decryption...
✓ Decryption test successful

Configuration file ready for deployment.
```

**Copy secrets.encrypted.json to application root:**

```powershell
# The tool should create secrets.encrypted.json in current directory
$secretsFile = ".\secrets.encrypted.json"

if (Test-Path $secretsFile) {
    Copy-Item $secretsFile -Destination "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\" -Force
    Write-Host "✅ secrets.encrypted.json copied to application root" -ForegroundColor Green

    # Also backup to config backup folder
    Copy-Item $secretsFile -Destination "D:\Backups\IkeaDocuScan\Config\secrets.encrypted.json_$(Get-Date -Format 'yyyyMMdd_HHmmss')" -Force
} else {
    Write-Host "❌ secrets.encrypted.json not found!" -ForegroundColor Red
}
```

### Step 6.3: Create appsettings.Local.json

Create server-specific configuration file:

```powershell
# Create appsettings.Local.json with IKEA-specific settings
$localConfig = @"
{
  "IkeaDocuScan": {
    "ContactEmail": "docuscan-support@ikea.com",
    "DomainName": "ikeadt.com",

    "UserEmail": {
      "LDAPRoot": "LDAP://DC=ikeadt,DC=com",
      "LDAPFilter": "(sAMAccountName={0})"
    },

    "EmailGroups": {
      "LDAPRoot": "LDAP://OU=Ikea,OU=Collab,DC=ikeadt,DC=com",
      "LDAPFilter": "(name=*Reminder*)"
    },

    "ADGroupReader": "IKEA\\UG-DocScanningReaders-CG@WAL-FIN-CH-GEL",
    "ADGroupPublisher": "IKEA\\UG-DocScanningPublishers-CG@WAL-FIN-CH-GEL",
    "ADGroupSuperUser": "IKEA\\UG-DocScanningSuperUsers-CG@WAL-FIN-CH-GEL"
  },

  "Email": {
    "SmtpHost": "smtp-gw.ikea.com",
    "SmtpPort": 25,
    "UseSsl": false,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromAddress": "noreply-docuscan@ikea.com",
    "FromDisplayName": "IKEA DocuScan System",
    "AdminEmail": "docuscan-admins@ikea.com",
    "ApplicationUrl": "https://testdocuscan.ikeadt.com",
    "EnableEmailNotifications": true
  },

  "ExcelExport": {
    "ApplicationUrl": "https://testdocuscan.ikeadt.com"
  },

  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "D:\\Logs\\IkeaDocuScan\\log-.json",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "formatter": "Serilog.Formatting.Compact.RenderedCompactJsonFormatter, Serilog.Formatting.Compact",
          "shared": true,
          "fileSizeLimitBytes": 104857600,
          "rollOnFileSizeLimit": true
        }
      }
    ]
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
"@

# Save to application root
$localConfigPath = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json"
$localConfig | Out-File -FilePath $localConfigPath -Encoding UTF8 -Force

Write-Host "✅ appsettings.Local.json created" -ForegroundColor Green

# Also backup
Copy-Item $localConfigPath -Destination "D:\Backups\IkeaDocuScan\Config\appsettings.Local.json_$(Get-Date -Format 'yyyyMMdd_HHmmss')" -Force

# Display contents for verification
Write-Host "`nConfiguration contents:" -ForegroundColor Cyan
Get-Content $localConfigPath | Out-String
```

**Review and Adjust:**

Open the file in Notepad and verify/adjust:
- LDAP paths match your Active Directory structure
- AD group names are correct
- SMTP settings are correct for your environment
- Email addresses are correct

```powershell
notepad D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json
```

### Step 6.4: Set File Permissions

```powershell
# Set permissions for application root
$appRoot = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan"

# Grant read & execute to IIS AppPool (will be created in next phase)
# Note: AppPool must exist first - this will be run after AppPool creation
# For now, document the commands:

Write-Host @"

File permissions will be set after IIS Application Pool is created.
Run these commands after Phase 7:

icacls "$appRoot" /grant "IIS APPPOOL\IkeaDocuScan:(OI)(CI)(RX)"
icacls "$appRoot\logs" /grant "IIS APPPOOL\IkeaDocuScan:(OI)(CI)(M)"

icacls "$appRoot\appsettings.Local.json" /inheritance:r
icacls "$appRoot\appsettings.Local.json" /grant "Administrators:(F)"
icacls "$appRoot\appsettings.Local.json" /grant "IIS APPPOOL\IkeaDocuScan:(R)"

icacls "$appRoot\secrets.encrypted.json" /inheritance:r
icacls "$appRoot\secrets.encrypted.json" /grant "Administrators:(F)"
icacls "$appRoot\secrets.encrypted.json" /grant "IIS APPPOOL\IkeaDocuScan:(R)"

"@ -ForegroundColor Yellow
```

### Step 6.5: Set Permissions for Scanned Files Directory

```powershell
# Grant read access to scanned files directory
$scannedFilesPath = "D:\IkeaDocuScan\ScannedFiles\checkin"

# For now, grant Administrators full access
icacls $scannedFilesPath /grant "Administrators:(OI)(CI)(F)"

Write-Host "✅ Scanned files directory permissions set (temporary)" -ForegroundColor Green
Write-Host "   Will be updated after IIS AppPool is created" -ForegroundColor Yellow
```

### Step 6.6: Set Permissions for Logs Directory

```powershell
# Grant write access to Serilog logs directory
$logsPath = "D:\Logs\IkeaDocuScan"

# Grant Administrators full access
icacls $logsPath /grant "Administrators:(OI)(CI)(F)"

Write-Host "✅ Logs directory permissions set (temporary)" -ForegroundColor Green
Write-Host "   Will be updated after IIS AppPool is created" -ForegroundColor Yellow
```

**Checklist:**

- [ ] Reviewed default appsettings.json (not modified)
- [ ] ConfigEncryptionTool executed successfully
- [ ] secrets.encrypted.json created and contains encrypted connection string
- [ ] secrets.encrypted.json copied to application root
- [ ] secrets.encrypted.json backed up to D:\Backups\IkeaDocuScan\Config\
- [ ] appsettings.Local.json created with IKEA-specific settings
- [ ] LDAP paths verified/adjusted for IKEA AD structure
- [ ] AD group names verified
- [ ] SMTP settings verified
- [ ] Application URL set to https://testdocuscan.ikeadt.com
- [ ] Log file path set to D:\Logs\IkeaDocuScan\
- [ ] appsettings.Local.json backed up
- [ ] File permission commands documented (to be executed after AppPool creation)

---

## Phase 7: Final IIS Configuration

### Step 7.1: Create Application Pool

```powershell
# Import IIS module
Import-Module WebAdministration

# Create Application Pool
$appPoolName = "IkeaDocuScan"

if (Test-Path "IIS:\AppPools\$appPoolName") {
    Write-Host "Application Pool '$appPoolName' already exists" -ForegroundColor Yellow
} else {
    New-WebAppPool -Name $appPoolName
    Write-Host "✅ Application Pool '$appPoolName' created" -ForegroundColor Green
}

# Configure Application Pool
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "managedPipelineMode" -Value "Integrated"
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.idleTimeout" -Value ([TimeSpan]::FromMinutes(0))
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.loadUserProfile" -Value $true
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "recycling.periodicRestart.time" -Value ([TimeSpan]::FromMinutes(1740))

Write-Host "✅ Application Pool configured" -ForegroundColor Green

# Display configuration
Get-ItemProperty "IIS:\AppPools\$appPoolName" | Format-List name, state, managedRuntimeVersion, managedPipelineMode, startMode
```

### Step 7.2: Create IIS Website

**Option A: Create as New Website (Recommended for Production)**

```powershell
$siteName = "IkeaDocuScan"
$physicalPath = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan"
$hostHeader = "testdocuscan.ikeadt.com"
$certThumbprint = Get-Content "D:\IkeaDocuScan\Deployment\certificate-thumbprint.txt"

# Remove if exists
if (Test-Path "IIS:\Sites\$siteName") {
    Remove-WebSite -Name $siteName
    Write-Host "Existing site '$siteName' removed" -ForegroundColor Yellow
}

# Create new website
New-WebSite -Name $siteName `
    -PhysicalPath $physicalPath `
    -ApplicationPool $appPoolName `
    -HostHeader $hostHeader `
    -Protocol https `
    -Port 443 `
    -Ssl `
    -SslFlags 0

Write-Host "✅ Website '$siteName' created" -ForegroundColor Green

# Add SSL certificate binding
$binding = Get-WebBinding -Name $siteName -Protocol https
$binding.AddSslCertificate($certThumbprint, "My")

Write-Host "✅ SSL certificate bound to website" -ForegroundColor Green

# Add HTTP binding (for redirect to HTTPS)
New-WebBinding -Name $siteName -Protocol http -Port 80 -HostHeader $hostHeader

Write-Host "✅ HTTP binding added (port 80)" -ForegroundColor Green
```

**Option B: Create as Application under Default Web Site (Alternative)**

```powershell
$siteName = "Default Web Site"
$appName = "docuscan"
$physicalPath = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan"

# Remove if exists
if (Test-Path "IIS:\Sites\$siteName\$appName") {
    Remove-WebApplication -Site $siteName -Name $appName
    Write-Host "Existing application '$appName' removed" -ForegroundColor Yellow
}

# Create application
New-WebApplication -Site $siteName -Name $appName -PhysicalPath $physicalPath -ApplicationPool $appPoolName

Write-Host "✅ Application '$appName' created under '$siteName'" -ForegroundColor Green
Write-Host "   URL: https://localhost/$appName" -ForegroundColor Cyan
```

**For this deployment, use Option A (New Website).**

### Step 7.3: Configure Authentication

```powershell
$siteName = "IkeaDocuScan"

# Disable Anonymous Authentication
Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/anonymousAuthentication" `
    -Name "enabled" `
    -Value $false `
    -PSPath "IIS:\" `
    -Location $siteName

# Enable Windows Authentication
Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/windowsAuthentication" `
    -Name "enabled" `
    -Value $true `
    -PSPath "IIS:\" `
    -Location $siteName

# Configure Windows Authentication settings
Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/windowsAuthentication" `
    -Name "useKernelMode" `
    -Value $true `
    -PSPath "IIS:\" `
    -Location $siteName

Set-WebConfigurationProperty -Filter "/system.webServer/security/authentication/windowsAuthentication/extendedProtection" `
    -Name "tokenChecking" `
    -Value "Allow" `
    -PSPath "IIS:\" `
    -Location $siteName

Write-Host "✅ Authentication configured (Windows Auth enabled, Anonymous disabled)" -ForegroundColor Green

# Verify authentication settings
Write-Host "`nAuthentication configuration:" -ForegroundColor Cyan
Get-WebConfigurationProperty -Filter "/system.webServer/security/authentication/anonymousAuthentication" -Name "enabled" -PSPath "IIS:\" -Location $siteName | Select-Object Value
Get-WebConfigurationProperty -Filter "/system.webServer/security/authentication/windowsAuthentication" -Name "enabled" -PSPath "IIS:\" -Location $siteName | Select-Object Value
```

### Step 7.4: Apply File Permissions (Now that AppPool Exists)

```powershell
$appRoot = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan"
$appPoolIdentity = "IIS APPPOOL\IkeaDocuScan"

# Grant read & execute to application root
icacls $appRoot /grant "${appPoolIdentity}:(OI)(CI)(RX)" /T

# Grant modify to logs directory
icacls "$appRoot\logs" /grant "${appPoolIdentity}:(OI)(CI)(M)"

# Restrict config files
icacls "$appRoot\appsettings.Local.json" /inheritance:r
icacls "$appRoot\appsettings.Local.json" /grant "Administrators:(F)"
icacls "$appRoot\appsettings.Local.json" /grant "${appPoolIdentity}:(R)"

icacls "$appRoot\secrets.encrypted.json" /inheritance:r
icacls "$appRoot\secrets.encrypted.json" /grant "Administrators:(F)"
icacls "$appRoot\secrets.encrypted.json" /grant "${appPoolIdentity}:(R)"

# Grant read to scanned files
icacls "D:\IkeaDocuScan\ScannedFiles\checkin" /grant "${appPoolIdentity}:(OI)(CI)(RX)"

# Grant modify to Serilog logs directory
icacls "D:\Logs\IkeaDocuScan" /grant "${appPoolIdentity}:(OI)(CI)(M)"

Write-Host "✅ File permissions applied" -ForegroundColor Green
```

### Step 7.5: Start Application Pool and Website

```powershell
# Start Application Pool
Start-WebAppPool -Name $appPoolName

# Wait a moment
Start-Sleep -Seconds 2

# Verify AppPool is running
$appPoolState = (Get-WebAppPoolState -Name $appPoolName).Value
Write-Host "Application Pool state: $appPoolState" -ForegroundColor $(if($appPoolState -eq "Started"){"Green"}else{"Red"})

# Start Website
Start-WebSite -Name $siteName

# Verify website is started
$siteState = (Get-WebsiteState -Name $siteName).Value
Write-Host "Website state: $siteState" -ForegroundColor $(if($siteState -eq "Started"){"Green"}else{"Red"})

# Display site bindings
Write-Host "`nWebsite bindings:" -ForegroundColor Cyan
Get-WebBinding -Name $siteName | Select-Object protocol, bindingInformation, certificateHash
```

### Step 7.6: Check Application Logs

```powershell
# Wait for application to initialize
Write-Host "`nWaiting for application to initialize (30 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Check stdout logs
$stdoutLogs = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\logs"

if (Test-Path $stdoutLogs) {
    $latestLog = Get-ChildItem $stdoutLogs -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1

    if ($latestLog) {
        Write-Host "`nLatest stdout log: $($latestLog.Name)" -ForegroundColor Cyan
        Write-Host "Content (last 50 lines):" -ForegroundColor Cyan
        Get-Content $latestLog.FullName -Tail 50
    } else {
        Write-Host "⚠️ No stdout logs found" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️ Stdout logs directory does not exist" -ForegroundColor Yellow
}

# Check Serilog logs
$serilogLogs = "D:\Logs\IkeaDocuScan"

if (Test-Path $serilogLogs) {
    $latestSerilog = Get-ChildItem $serilogLogs -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1

    if ($latestSerilog) {
        Write-Host "`nLatest Serilog file: $($latestSerilog.Name)" -ForegroundColor Cyan
        Write-Host "Content (last 20 lines):" -ForegroundColor Cyan
        Get-Content $latestSerilog.FullName -Tail 20
    } else {
        Write-Host "⚠️ No Serilog files found" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️ Serilog directory does not exist" -ForegroundColor Yellow
}

# Check Windows Event Log
Write-Host "`nChecking Windows Event Log for IIS/ASP.NET Core errors:" -ForegroundColor Cyan
Get-EventLog -LogName Application -Source "IIS*","ASP.NET*" -Newest 10 -ErrorAction SilentlyContinue |
    Select-Object TimeGenerated, EntryType, Source, Message |
    Format-Table -AutoSize
```

**Checklist:**

- [ ] Application Pool "IkeaDocuScan" created
- [ ] Application Pool configured (.NET CLR: No Managed Code, Start Mode: AlwaysRunning, Load User Profile: True)
- [ ] Website "IkeaDocuScan" created
- [ ] HTTPS binding configured (port 443)
- [ ] SSL certificate bound to website
- [ ] HTTP binding added (port 80)
- [ ] Anonymous Authentication disabled
- [ ] Windows Authentication enabled
- [ ] File permissions applied to application root
- [ ] File permissions applied to logs directories
- [ ] File permissions applied to scanned files directory
- [ ] Config file permissions restricted
- [ ] Application Pool started successfully
- [ ] Website started successfully
- [ ] No errors in stdout logs
- [ ] No errors in Serilog logs
- [ ] No errors in Windows Event Log

---

## Phase 8: Smoke Testing

### Step 8.1: Test Basic Connectivity

**From the Server:**

```powershell
# Test HTTPS endpoint
try {
    $response = Invoke-WebRequest -Uri "https://testdocuscan.ikeadt.com" -UseBasicParsing -UseDefaultCredentials
    Write-Host "✅ HTTPS connection successful" -ForegroundColor Green
    Write-Host "   Status Code: $($response.StatusCode)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ HTTPS connection failed: $_" -ForegroundColor Red
}

# Test HTTP endpoint (should redirect or respond)
try {
    $response = Invoke-WebRequest -Uri "http://testdocuscan.ikeadt.com" -UseBasicParsing -UseDefaultCredentials
    Write-Host "✅ HTTP connection successful" -ForegroundColor Green
    Write-Host "   Status Code: $($response.StatusCode)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ HTTP connection failed: $_" -ForegroundColor Red
}
```

### Step 8.2: Test Health Check Endpoints

```powershell
# Test /health endpoint
$healthUrl = "https://testdocuscan.ikeadt.com/health"

try {
    $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -UseDefaultCredentials
    $content = $response.Content

    Write-Host "✅ Health check endpoint accessible" -ForegroundColor Green
    Write-Host "   Response: $content" -ForegroundColor Cyan

    if ($content -match "Healthy") {
        Write-Host "✅ Application reports Healthy status" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Application may not be healthy: $content" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Health check failed: $_" -ForegroundColor Red
}

# Test /health/ready endpoint (JSON response)
$readyUrl = "https://testdocuscan.ikeadt.com/health/ready"

try {
    $response = Invoke-WebRequest -Uri $readyUrl -UseBasicParsing -UseDefaultCredentials
    $content = $response.Content | ConvertFrom-Json

    Write-Host "✅ Readiness check endpoint accessible" -ForegroundColor Green
    Write-Host "   Status: $($content.status)" -ForegroundColor Cyan

    if ($content.checks) {
        Write-Host "   Health Checks:" -ForegroundColor Cyan
        foreach ($check in $content.checks) {
            $color = if ($check.status -eq "Healthy") { "Green" } else { "Red" }
            Write-Host "      - $($check.name): $($check.status)" -ForegroundColor $color
        }
    }
} catch {
    Write-Host "❌ Readiness check failed: $_" -ForegroundColor Red
}
```

### Step 8.3: Open Application in Browser

```powershell
# Open application in default browser
Start-Process "https://testdocuscan.ikeadt.com"

Write-Host @"

Browser opened to: https://testdocuscan.ikeadt.com

Expected behavior:
✅ Browser shows certificate warning (expected for self-signed cert)
✅ Click 'Advanced' → 'Proceed' to continue
✅ Application should load home page
✅ Windows Authentication should authenticate you automatically
✅ You should see your username displayed

"@ -ForegroundColor Yellow
```

**Manual Browser Tests:**

Navigate to these URLs and verify behavior:

| URL | Expected Result |
|-----|-----------------|
| https://testdocuscan.ikeadt.com | Home page loads, Windows Auth automatic |
| https://testdocuscan.ikeadt.com/Identity/Account/Login | Identity/login page (if applicable) |
| https://testdocuscan.ikeadt.com/Documents | Documents page loads |
| https://testdocuscan.ikeadt.com/health | Shows "Healthy" text |

### Step 8.4: Test Windows Authentication

**Verify Current User:**

1. Browse to application: https://testdocuscan.ikeadt.com
2. Look for user information display (typically in header/nav bar)
3. Should show: `IKEADT\[YourUsername]`

**Check Browser Developer Tools:**

1. Press F12 to open Developer Tools
2. Go to **Network** tab
3. Refresh page (F5)
4. Click on first request
5. Check **Request Headers** → Should include: `Authorization: Negotiate ...`

### Step 8.5: Test Database Connection

```powershell
# Check Serilog for database connection messages
$serilogPath = "D:\Logs\IkeaDocuScan"
$latestLog = Get-ChildItem $serilogPath -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($latestLog) {
    $logContent = Get-Content $latestLog.FullName | Out-String

    if ($logContent -match "Entity Framework Core.*initialized") {
        Write-Host "✅ Entity Framework Core initialized" -ForegroundColor Green
    }

    if ($logContent -match "Database connection.*successful") {
        Write-Host "✅ Database connection successful" -ForegroundColor Green
    }

    if ($logContent -match "error|exception" -and $logContent -match -not "Microsoft.AspNetCore.Diagnostics") {
        Write-Host "⚠️ Errors found in log - review manually:" -ForegroundColor Yellow
        Write-Host "   Log file: $($latestLog.FullName)" -ForegroundColor Cyan
    }
}
```

### Step 8.6: Test Document Search

**In Browser:**

1. Navigate to: https://testdocuscan.ikeadt.com/Documents
2. Should see document search interface
3. Try a simple search (e.g., search for `*` or leave empty and click Search)
4. Should return results or "No documents found" message
5. If results returned, click on a document to view details

### Step 8.7: Test Scanned Files Access

**In Browser:**

1. Navigate to: https://testdocuscan.ikeadt.com/CheckinScanned
2. Should list scanned files from `D:\IkeaDocuScan\ScannedFiles\checkin\`
3. If directory is empty, add a test file:

```powershell
# Create a test PDF in scanned files directory
$testFile = "D:\IkeaDocuScan\ScannedFiles\checkin\test-document.txt"
Set-Content -Path $testFile -Value "Test scanned document"

Write-Host "✅ Test file created: $testFile" -ForegroundColor Green
Write-Host "   Refresh the CheckinScanned page in browser" -ForegroundColor Yellow
```

4. Verify file appears in list
5. Try to view/download the file

### Step 8.8: Test Excel Export (If Applicable)

**In Browser:**

1. Navigate to document search page
2. Perform a search that returns results
3. Look for "Export to Excel" button
4. Click export
5. Verify Excel file downloads
6. Open Excel file and verify data is correct

### Step 8.9: Verify Audit Trail

**In Browser:**

1. Perform some operations (search, view document, etc.)
2. Navigate to admin/audit trail section (if accessible)
3. Verify audit entries are being created

**Check Database:**

```sql
-- Connect to SQL Server using SSMS
USE IkeaDocuScan;
GO

-- Check audit trail entries
SELECT TOP 20
    Id,
    Action,
    EntityName,
    Description,
    UserId,
    Timestamp
FROM AuditTrail
ORDER BY Timestamp DESC;

-- Expected: Recent entries from your testing activities
```

### Step 8.10: Test Email Notifications (Optional)

**If SMTP is configured:**

1. Navigate to feature that sends emails (e.g., access request)
2. Trigger email notification
3. Check if email is received
4. If not received, check Serilog for SMTP errors:

```powershell
$serilogPath = "D:\Logs\IkeaDocuScan"
$latestLog = Get-ChildItem $serilogPath -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1

Get-Content $latestLog.FullName | Select-String -Pattern "SMTP|email|MailKit" -Context 2
```

### Step 8.11: Performance Check

```powershell
# Monitor application performance
Write-Host "Monitoring application performance for 2 minutes..." -ForegroundColor Yellow
Write-Host "Please interact with the application in your browser during this time." -ForegroundColor Yellow

# Monitor CPU usage
$appPoolName = "IkeaDocuScan"
$duration = 120 # seconds

$counter = "\Process(w3wp*)\% Processor Time"
$samples = Get-Counter -Counter $counter -SampleInterval 5 -MaxSamples ($duration / 5)

$avgCpu = ($samples.CounterSamples | Measure-Object -Property CookedValue -Average).Average

Write-Host "`nAverage CPU usage: $([math]::Round($avgCpu, 2))%" -ForegroundColor Cyan

if ($avgCpu -lt 50) {
    Write-Host "✅ CPU usage is normal" -ForegroundColor Green
} elseif ($avgCpu -lt 80) {
    Write-Host "⚠️ CPU usage is moderate" -ForegroundColor Yellow
} else {
    Write-Host "❌ CPU usage is high - investigate" -ForegroundColor Red
}

# Check memory usage
$w3wpProcess = Get-Process -Name "w3wp" -ErrorAction SilentlyContinue |
    Where-Object {$_.ProcessName -eq "w3wp"}

if ($w3wpProcess) {
    $memoryMB = [math]::Round($w3wpProcess.WorkingSet64 / 1MB, 2)
    Write-Host "Memory usage: $memoryMB MB" -ForegroundColor Cyan

    if ($memoryMB -lt 500) {
        Write-Host "✅ Memory usage is normal" -ForegroundColor Green
    } elseif ($memoryMB -lt 1000) {
        Write-Host "⚠️ Memory usage is moderate" -ForegroundColor Yellow
    } else {
        Write-Host "⚠️ Memory usage is high - monitor" -ForegroundColor Yellow
    }
}
```

### Step 8.12: Final Verification Checklist

Run through this checklist manually:

```
CONNECTIVITY:
- [ ] https://testdocuscan.ikeadt.com loads successfully
- [ ] Certificate warning appears (expected for self-signed)
- [ ] Can proceed past certificate warning
- [ ] Home page renders correctly
- [ ] No JavaScript errors in browser console (F12)

AUTHENTICATION:
- [ ] Windows Authentication works automatically
- [ ] Username displays correctly (IKEADT\username)
- [ ] Can access protected pages
- [ ] Cannot access pages without proper authorization

FUNCTIONALITY:
- [ ] Document search page loads
- [ ] Can perform document search
- [ ] Search results display correctly
- [ ] Can view document details
- [ ] Scanned files page loads
- [ ] Can see scanned files list
- [ ] Can download scanned files
- [ ] Excel export works (if tested)

PERFORMANCE:
- [ ] Pages load within 3 seconds
- [ ] No significant delays in navigation
- [ ] No memory leaks observed
- [ ] CPU usage remains reasonable

LOGGING:
- [ ] Serilog files being created in D:\Logs\IkeaDocuScan\
- [ ] No error entries in logs
- [ ] Audit trail entries being created
- [ ] Windows Event Log shows no errors

DATABASE:
- [ ] Application connects to database successfully
- [ ] Data operations work (select, insert, update)
- [ ] Foreign key constraints enforced
- [ ] Audit trail records user activities
```

**Final Sign-Off:**

```
Deployment completed successfully on: ___________________
Tested by: ___________________
Server: wtseelm-nx20541.ikeadt.com
Application URL: https://testdocuscan.ikeadt.com
Version: ___________________

Signature: ___________________
```

---

## Troubleshooting

### Issue: Application Pool Stops Immediately

**Symptoms:**
- Pool starts but stops after a few seconds
- 502.5 error in browser
- "HTTP Error 502.5 - Process Failure"

**Resolution:**

1. Check stdout logs:
```powershell
Get-ChildItem "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\logs" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content
```

2. Verify .NET 10.0 Runtime installed:
```powershell
dotnet --list-runtimes
# Should show: Microsoft.AspNetCore.App 10.0.x
```

3. Check Event Viewer:
```powershell
Get-EventLog -LogName Application -Source "IIS*" -Newest 20 | Format-Table TimeGenerated, EntryType, Message -AutoSize
```

4. Verify web.config is correct:
```powershell
Get-Content "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\web.config"
# Should have: <aspNetCore processPath="dotnet" arguments=".\IkeaDocuScan-Web.dll" />
```

### Issue: Database Connection Fails

**Symptoms:**
- 500 error on any database operation
- "Cannot open database" in logs
- "Login failed for user 'docuscanch'"

**Resolution:**

1. Test SQL connection:
```powershell
$serverName = "wtseelm-nx20541.ikeadt.com"
$credential = Get-Credential -Message "Enter docuscanch credentials"

Invoke-Sqlcmd -ServerInstance $serverName -Database "IkeaDocuScan" -Credential $credential -Query "SELECT @@VERSION"
```

2. Verify connection string decryption:
```powershell
# Check if secrets.encrypted.json exists
Test-Path "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\secrets.encrypted.json"

# If issues, recreate with ConfigEncryptionTool (must run as IIS AppPool identity)
```

3. Check SQL Server allows remote connections:
```sql
-- On SQL Server
EXEC sp_configure 'remote access', 1;
RECONFIGURE;
```

4. Verify docuscanch user has access:
```sql
USE IkeaDocuScan;
GO

-- Check user exists
SELECT name, type_desc FROM sys.database_principals WHERE name = 'docuscanch';

-- Check user roles
EXEC sp_helpuser 'docuscanch';
```

### Issue: Windows Authentication Not Working

**Symptoms:**
- Prompted for credentials repeatedly
- Browser shows "401 - Unauthorized"
- Anonymous user shown in logs

**Resolution:**

1. Verify IIS Authentication settings:
```powershell
$siteName = "IkeaDocuScan"

# Check Anonymous Auth (should be disabled)
Get-WebConfigurationProperty -Filter "/system.webServer/security/authentication/anonymousAuthentication" -Name "enabled" -PSPath "IIS:\" -Location $siteName

# Check Windows Auth (should be enabled)
Get-WebConfigurationProperty -Filter "/system.webServer/security/authentication/windowsAuthentication" -Name "enabled" -PSPath "IIS:\" -Location $siteName
```

2. Configure browser for Integrated Windows Authentication:

**Internet Explorer / Edge:**
- Internet Options → Security → Local intranet → Sites → Advanced
- Add: https://testdocuscan.ikeadt.com
- Ensure "Automatic logon with current user name and password" is selected

**Chrome:**
- Uses Windows settings (same as IE)

**Firefox:**
- Navigate to: `about:config`
- Search for: `network.automatic-ntlm-auth.trusted-uris`
- Add: `https://testdocuscan.ikeadt.com`

3. Verify server is domain-joined:
```powershell
Get-ComputerInfo | Select-Object CsDomain, CsDomainRole
```

### Issue: Cannot Access Scanned Files

**Symptoms:**
- Files list is empty
- "Access denied" errors
- "Path not found" errors

**Resolution:**

1. Verify path in configuration:
```powershell
Get-Content "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json" | Select-String -Pattern "ScannedFilesPath"
```

2. Test access as AppPool identity:
```powershell
# Run CMD as IIS AppPool identity (requires PSExec or similar)
# Or check permissions:
icacls "D:\IkeaDocuScan\ScannedFiles\checkin"

# Should show: IIS APPPOOL\IkeaDocuScan:(OI)(CI)(RX)
```

3. Verify path exists and has files:
```powershell
Get-ChildItem "D:\IkeaDocuScan\ScannedFiles\checkin"
```

### Issue: SSL Certificate Errors

**Symptoms:**
- Browser shows "Your connection is not private"
- "NET::ERR_CERT_AUTHORITY_INVALID"
- Certificate warnings don't go away

**Resolution:**

**For Testing (Self-Signed Certificate):**
- This is expected behavior
- Click "Advanced" → "Proceed to testdocuscan.ikeadt.com (unsafe)"
- Consider adding certificate to client machine's Trusted Root

**For Production:**
- Obtain certificate from trusted CA
- Install certificate on server
- Update IIS binding with new certificate

### Issue: Serilog Not Writing Logs

**Symptoms:**
- No files in D:\Logs\IkeaDocuScan\
- Logs directory is empty

**Resolution:**

1. Verify directory exists:
```powershell
Test-Path "D:\Logs\IkeaDocuScan"
```

2. Check permissions:
```powershell
icacls "D:\Logs\IkeaDocuScan"
# Should show: IIS APPPOOL\IkeaDocuScan:(OI)(CI)(M)
```

3. Grant permissions if missing:
```powershell
icacls "D:\Logs\IkeaDocuScan" /grant "IIS APPPOOL\IkeaDocuScan:(OI)(CI)(M)"
```

4. Verify configuration in appsettings.Local.json:
```powershell
Get-Content "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json" | Select-String -Pattern "Serilog" -Context 5
```

### Issue: SignalR WebSocket Connections Failing

**Symptoms:**
- Real-time updates not working
- Browser console shows "WebSocket connection failed"
- Network tab shows 403/404 on `/hubs/` endpoints

**Resolution:**

1. Verify WebSocket Protocol feature installed:
```powershell
Get-WindowsFeature -Name "Web-WebSockets"
# State should be: Installed
```

2. Install if missing:
```powershell
Install-WindowsFeature -Name "Web-WebSockets"
iisreset
```

3. Check if ARR (Application Request Routing) is interfering:
```powershell
# If ARR is installed, disable proxy
# IIS Manager → Server → Application Request Routing → Server Proxy Settings → Enable proxy: OFF
```

### Issue: High CPU or Memory Usage

**Symptoms:**
- Application slow to respond
- Server performance degraded
- w3wp.exe consuming resources

**Resolution:**

1. Check application logs for errors:
```powershell
Get-Content "D:\Logs\IkeaDocuScan\log-$(Get-Date -Format 'yyyyMMdd').json" | Select-String -Pattern "error|exception" -Context 3
```

2. Monitor specific counters:
```powershell
# CPU
Get-Counter "\Process(w3wp*)\% Processor Time" -Continuous

# Memory
Get-Counter "\Process(w3wp*)\Working Set" -Continuous
```

3. Consider:
- Restart Application Pool
- Review recent code changes
- Check for database query performance issues
- Monitor database server performance

---

## Rollback Procedures

If deployment fails critically and you need to rollback:

### Step 1: Stop Application Pool

```powershell
Stop-WebAppPool -Name "IkeaDocuScan"
Stop-WebSite -Name "IkeaDocuScan"
```

### Step 2: Restore Previous Application Files

```powershell
# Find latest backup
$backupPath = Get-ChildItem "D:\Backups\IkeaDocuScan" -Directory |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($backupPath) {
    Write-Host "Restoring from: $($backupPath.FullName)" -ForegroundColor Yellow

    # Clear current deployment
    Remove-Item "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\*" -Recurse -Force -Exclude "appsettings.Local.json","secrets.encrypted.json","logs"

    # Restore backup
    Copy-Item -Path "$($backupPath.FullName)\*" -Destination "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\" -Recurse -Force

    Write-Host "✅ Application files restored from backup" -ForegroundColor Green
} else {
    Write-Host "❌ No backup found!" -ForegroundColor Red
}
```

### Step 3: Rollback Database (If Necessary)

**If database migration was applied:**

```sql
-- Connect to SQL Server using SSMS
USE IkeaDocuScan;
GO

-- Option A: Restore database from backup
USE master;
GO
ALTER DATABASE IkeaDocuScan SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

RESTORE DATABASE IkeaDocuScan
FROM DISK = 'D:\Backups\IkeaDocuScan\Database\IkeaDocuScan_Backup_YYYYMMDD.bak'
WITH REPLACE;
GO

ALTER DATABASE IkeaDocuScan SET MULTI_USER;
GO

-- Option B: Run rollback scripts (if provided)
-- Execute rollback SQL scripts in reverse order
```

### Step 4: Restore Configuration Files

```powershell
# Restore config files from backup
$configBackup = Get-ChildItem "D:\Backups\IkeaDocuScan\Config" -File |
    Where-Object {$_.Name -like "appsettings.Local.json*"} |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($configBackup) {
    Copy-Item -Path $configBackup.FullName -Destination "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json" -Force
    Write-Host "✅ Configuration restored from backup" -ForegroundColor Green
}
```

### Step 5: Start Application Pool

```powershell
Start-WebAppPool -Name "IkeaDocuScan"
Start-WebSite -Name "IkeaDocuScan"

# Wait and verify
Start-Sleep -Seconds 10

$appPoolState = (Get-WebAppPoolState -Name "IkeaDocuScan").Value
Write-Host "Application Pool state: $appPoolState" -ForegroundColor $(if($appPoolState -eq "Started"){"Green"}else{"Red"})
```

### Step 6: Verify Rollback Success

```powershell
# Test health endpoint
try {
    $response = Invoke-WebRequest -Uri "https://testdocuscan.ikeadt.com/health" -UseBasicParsing -UseDefaultCredentials
    Write-Host "✅ Application responding after rollback" -ForegroundColor Green
    Write-Host "   Response: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Application not responding after rollback: $_" -ForegroundColor Red
}
```

---

## Deployment Complete

**Congratulations!** The IkeaDocuScan V3 application has been successfully deployed to Windows Server 2022.

**Key Information:**

| Item | Value |
|------|-------|
| Server | Windows Server 2022 Standard |
| Application URL | https://testdocuscan.ikeadt.com |
| Database Server | wtseelm-nx20541.ikeadt.com |
| Database Name | IkeaDocuScan |
| IIS Application Pool | IkeaDocuScan |
| Application Root | D:\IkeaDocuScan\wwwroot\IkeaDocuScan |
| Scanned Files | D:\IkeaDocuScan\ScannedFiles\checkin |
| Logs Directory | D:\Logs\IkeaDocuScan |
| Tools Directory | D:\IkeaDocuScan\Tools |

**Next Steps:**

1. **Monitor Application:** Check logs and performance for first 24-48 hours
2. **User Acceptance Testing:** Have users test all functionality
3. **SSL Certificate:** Replace self-signed certificate with proper certificate from CA
4. **DNS Update:** Create proper DNS A record (remove hosts file entry)
5. **Backup Schedule:** Set up automated backups for database and configuration
6. **Documentation:** Document any environment-specific configurations
7. **Action Reminder Service:** Deploy Action Reminder Windows Service (see separate guide)

**Support:**

- Application Logs: `D:\Logs\IkeaDocuScan\`
- Stdout Logs: `D:\IkeaDocuScan\wwwroot\IkeaDocuScan\logs\`
- Event Viewer: Application Log, filter by IIS/ASP.NET Core
- Configuration: `D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json`

---

## Appendix A: Offline Deployment Summary

### Why Offline Deployment?

This deployment guide is specifically designed for air-gapped (no internet access) environments where:
- Server cannot reach public internet for security reasons
- All software must be pre-downloaded and transferred manually
- Package integrity must be verified before installation

### Software That Must Be Transferred

| Component | Size | Source | Required |
|-----------|------|--------|----------|
| ASP.NET Core 10.0 Hosting Bundle | ~200MB | https://dotnet.microsoft.com/download/dotnet/10.0 | ✅ Yes |
| IkeaDocuScan Application Package | ~100-300MB | Built from source | ✅ Yes |
| SQL Server Management Studio | ~600MB | https://aka.ms/ssmsfullsetup | ⚠️ If not installed |

### Transfer Methods Comparison

| Method | Speed | Reliability | File Size Limit | Notes |
|--------|-------|-------------|-----------------|-------|
| USB Flash Drive | Fast | High | Unlimited | **Recommended** for air-gapped |
| RDP Clipboard | Moderate | Moderate | ~2GB | Can fail for very large files |
| Network Share | Fast | High | Unlimited | Requires internal network access |
| Jump Server | Moderate | High | Unlimited | Requires security approval |

### Post-Deployment Internet Isolation

After deployment, the server will:
- ✅ Access SQL Server (wtseelm-nx20541.ikeadt.com) - internal network
- ✅ Access Active Directory - internal network
- ✅ Access SMTP gateway (smtp-gw.ikea.com) - internal network
- ❌ **NO access to public internet** - by design

### Updating the Application

Future updates will require:
1. Build new version on development machine (with internet)
2. Create deployment package
3. Transfer to server via USB/RDP/network share
4. Follow update procedure (similar to initial deployment)
5. No direct download on server

---

## Appendix B: Quick Reference Commands

### Essential PowerShell Commands

```powershell
# Check .NET installation
dotnet --list-runtimes

# Check IIS status
Get-Service W3SVC

# Check Application Pool status
Get-WebAppPoolState -Name "IkeaDocuScan"

# Check website status
Get-WebsiteState -Name "IkeaDocuScan"

# View recent application logs
Get-Content "D:\Logs\IkeaDocuScan\log-$(Get-Date -Format 'yyyyMMdd').json" -Tail 50

# View stdout logs
Get-ChildItem "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\logs" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 50

# Check SQL connectivity
Test-NetConnection -ComputerName "wtseelm-nx20541.ikeadt.com" -Port 1433

# Restart application (full reset)
Stop-WebAppPool -Name "IkeaDocuScan"
Stop-WebSite -Name "IkeaDocuScan"
Start-Sleep -Seconds 5
Start-WebAppPool -Name "IkeaDocuScan"
Start-WebSite -Name "IkeaDocuScan"
```

### File Locations Quick Reference

```
Configuration:
D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json
D:\IkeaDocuScan\wwwroot\IkeaDocuScan\secrets.encrypted.json

Logs:
D:\Logs\IkeaDocuScan\log-YYYYMMDD.json
D:\IkeaDocuScan\wwwroot\IkeaDocuScan\logs\stdout-*.log

Backups:
D:\Backups\IkeaDocuScan\Database\
D:\Backups\IkeaDocuScan\Config\

Tools:
D:\IkeaDocuScan\Tools\ConfigEncryptionTool\
D:\IkeaDocuScan\Tools\ActionReminder\

Deployment:
D:\IkeaDocuScan\Deployment\Archives\
D:\IkeaDocuScan\Deployment\Current\
D:\IkeaDocuScan\Deployment\Installers\
```

---

## Appendix C: Offline Troubleshooting

### Cannot Download .NET Runtime Error

**Symptom:** Error messages about unable to download .NET components

**Cause:** Server trying to download components from internet (which is not available)

**Solution:**
1. Verify ASP.NET Core Hosting Bundle is installed:
   ```powershell
   dotnet --list-runtimes
   ```
2. If not installed, verify installer is in `D:\IkeaDocuScan\Deployment\Installers\`
3. Run installer manually as described in Phase 1, Step 1.3

### NuGet Package Restore Fails

**Symptom:** If rebuilding on server, NuGet packages fail to restore

**Cause:** Server cannot reach nuget.org

**Solution:**
- **Do NOT build on the server**
- Build and publish on development machine with internet access
- Transfer complete published package to server

### Windows Update or Feature Installation Fails

**Symptom:** Windows trying to download updates or features

**Cause:** Windows Server features may require source media

**Solution:**
1. For IIS features, use Windows Features on Demand with install media:
   ```powershell
   Install-WindowsFeature -Name Web-Server -Source "D:\sources\sxs"
   ```
2. Or use DISM with Windows Server ISO mounted

---

**Document Version:** 1.1
**Last Updated:** 2025-01-18
**Prepared For:** IKEA DocuScan V3 Deployment (Air-Gapped Environment)
