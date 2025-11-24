# IkeaDocuScan Installation Guide

**Target:** Windows Server 2022, IIS, SQL Server
**Runtime:** ASP.NET Core 10.0

---

## 1. Directory Structure

Create on D:\

```
D:\IkeaDocuScan\
├── Deployment\
│   ├── Archives\
│   ├── Current\
│   ├── Scripts\
│   └── Installers\
├── wwwroot\IkeaDocuScan\
│   └── logs\
├── Tools\
│   ├── ConfigEncryptionTool\
│   └── ActionReminder\
├── Database\MigrationScripts\
└── ScannedFiles\checkin\

D:\Logs\IkeaDocuScan\
D:\Backups\IkeaDocuScan\
├── Database\
└── Config\
```

---

## 2. Server Prerequisites

```powershell
# Verify Windows Server 2022, domain-joined, 10GB+ free on D:\, 8GB+ RAM
Get-ComputerInfo | Select-Object WindowsProductName, CsDomain
Get-PSDrive D | Select-Object @{N="FreeGB";E={[math]::Round($_.Free/1GB,2)}}
```

---

## 3. Install IIS Features

```powershell
$features = @(
    'Web-WebServer','Web-Common-Http','Web-Default-Doc','Web-Dir-Browsing',
    'Web-Http-Errors','Web-Static-Content','Web-Http-Redirect','Web-Health',
    'Web-Http-Logging','Web-Log-Libraries','Web-Request-Monitor','Web-Performance',
    'Web-Stat-Compression','Web-Dyn-Compression','Web-Security','Web-Filtering',
    'Web-Windows-Auth','Web-App-Dev','Web-Net-Ext45','Web-Asp-Net45',
    'Web-ISAPI-Ext','Web-ISAPI-Filter','Web-WebSockets','Web-Mgmt-Tools','Web-Mgmt-Console'
)
Install-WindowsFeature -Name $features
```

Verify: http://localhost shows IIS welcome page

---

## 4. Install .NET 10.0 Runtime

Download and install **ASP.NET Core 10.0 Hosting Bundle** from Microsoft.

```powershell
# Restart PowerShell, then verify
dotnet --info
# Expected: .NET 10.x
```

---

## 5. Database Setup (SSMS)

```sql
-- Set compatibility level
ALTER DATABASE [PPDOCUSCAN] SET COMPATIBILITY_LEVEL = 150;

-- Create application login
USE master;
CREATE LOGIN docuscanch
WITH PASSWORD = 'docuscanch25',
     DEFAULT_DATABASE = PPDOCUSCAN,
     CHECK_EXPIRATION = OFF,
     CHECK_POLICY = OFF;

USE PPDOCUSCAN;
CREATE USER docuscanch FOR LOGIN docuscanch;
ALTER ROLE db_owner ADD MEMBER docuscanch;
```

Run migration scripts in order:
```
D:\IkeaDocuScan\Database\MigrationScripts\02_Migrate_FK_Data.sql
D:\IkeaDocuScan\Database\MigrationScripts\03_Finalize_FK_Constraints.sql
D:\IkeaDocuScan\Database\MigrationScripts\04_Create_DocuScanUser_Table.sql
D:\IkeaDocuScan\Database\MigrationScripts\05_Migrate_Users_To_DocuScanUser.sql
D:\IkeaDocuScan\Database\MigrationScripts\06_Add_FK_Constraint_UserPermissions.sql
D:\IkeaDocuScan\Database\MigrationScripts\07_Remove_AccountName_From_UserPermissions.sql
```

Delete unused tables after migration.
All tables except the folloing should be removed:
    EmailTemplate
    EmailRecipientGroup
    EmailRecipient
    AuditTrail
    CounterParty
    DocuScanUser
    Country
    Currency
    Document
    DocumentFile
    DocumentName
    DocumentType
    UserPermissions
    EndpointRegistry
    EndpointRolePermission
    PermissionChangeAuditLog
    SystemConfiguration
    SystemConfigurationAudit

Script to identify unused tables:
```
use <PPDOCUSCAN>;
DECLARE @SchemaName sysname = 'dbo'; -- Replace 'dbo' with the actual schema/user name if different

SELECT
    t.name AS TableName
FROM
    sys.tables t
INNER JOIN
    sys.schemas s ON t.schema_id = s.schema_id
WHERE
    s.name = @SchemaName -- Filter by the provided schema/user name
    AND t.name NOT IN (
        'EmailTemplate',
        'EmailRecipientGroup',
        'EmailRecipient',
        'AuditTrail',
        'CounterParty',
        'DocuScanUser',
        'Country',
        'Currency',
        'Document',
        'DocumentFile',
        'DocumentName',
        'DocumentType',
        'UserPermissions',
        'EndpointRegistry',
        'EndpointRolePermission',
        'PermissionChangeAuditLog',
        'SystemConfiguration',
        'SystemConfigurationAudit'
    )
ORDER BY
    TableName;
```

---

## 6. Deploy Application

**On Dev Machine:**
```powershell
# Update version in .csproj: <VersionPrefix>3.x.x</VersionPrefix>
cd path\to\IkeaDocuScan-Web
dotnet clean
dotnet publish -c Debug -o d:/Pub
```

Copy published files to server: `D:\IkeaDocuScan\Deployment\Current`
    use 'D:\IkeaDocuScan\Deployment\Archives\' to store the zip files.
    check if  script to unzip and follwing script to deploy to www exist in `D:\IkeaDocuScan\Deployment\Scripts\`


**On Server:**
```powershell
# Stop web app first in IIS Manager

$sourcePath = "D:\IkeaDocuScan\Deployment\Current"
$destPath = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan"

# Backup existing (if exists)
if (Test-Path "$destPath\IkeaDocuScan-Web.dll") {
    $backupPath = "D:\Backups\IkeaDocuScan\Deployment_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    New-Item -Path $backupPath -ItemType Directory -Force
    Copy-Item -Path "$destPath\*" -Destination $backupPath -Recurse -Force
}

# Clear destination (preserve config)
Get-ChildItem $destPath |
    Where-Object {$_.Name -notin @('appsettings.Local.json','secrets.encrypted.json','logs')} |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# Copy new files
Copy-Item -Path "$sourcePath\*" -Destination $destPath -Recurse -Force

# Ensure logs directory
New-Item -Path "$destPath\logs" -ItemType Directory -Force -ErrorAction SilentlyContinue
```

---

## 7. Configuration

Create `D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVERNAME;Database=PPDOCUSCAN;User Id=docuscanch;Password=<<password>>>;TrustServerCertificate=True;"
  },
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
      { "Name": "Console" },
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
```

Backup config:
```powershell
Copy-Item "D:\IkeaDocuScan\wwwroot\IkeaDocuScan\appsettings.Local.json" `
    -Destination "D:\Backups\IkeaDocuScan\Config\appsettings.Local.json_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
```

---

## 8. IIS Configuration

### Application Pool (IIS Manager)

| Setting | Value |
|---------|-------|
| Name | IkeaDocuScan |
| .NET CLR Version | No Managed Code |
| Pipeline Mode | Integrated |
| Start Mode | AlwaysRunning |
| Idle Timeout | 0 |
| Load User Profile | True |
| Recycle Time | 1740 minutes |

### Web Site (IIS Manager)

| Setting | Value |
|---------|-------|
| Site Name | IkeaDocuScan |
| Physical Path | D:\IkeaDocuScan\wwwroot\IkeaDocuScan |
| Host Header | testdocuscan.ikeadt.com |
| Application Pool | IkeaDocuScan |
| HTTPS Binding | Add with certificate |

### Authentication (Site > Authentication)

- Anonymous Authentication: **Disabled**
- Windows Authentication: **Enabled**
  - useKernelMode: Enabled
  - Extended Protection: Accept

---

## 9. File Permissions

```powershell
$appRoot = "D:\IkeaDocuScan\wwwroot\IkeaDocuScan"
$appPoolIdentity = "IIS APPPOOL\IkeaDocuScan"

# Application root: Read & Execute
icacls $appRoot /grant "${appPoolIdentity}:(OI)(CI)(RX)" /T

# App logs: Modify
icacls "$appRoot\logs" /grant "${appPoolIdentity}:(OI)(CI)(M)"

# Config files: Restricted
icacls "$appRoot\appsettings.Local.json" /inheritance:r
icacls "$appRoot\appsettings.Local.json" /grant "Administrators:(F)"
icacls "$appRoot\appsettings.Local.json" /grant "${appPoolIdentity}:(R)"

icacls "$appRoot\secrets.encrypted.json" /inheritance:r
icacls "$appRoot\secrets.encrypted.json" /grant "Administrators:(F)"
icacls "$appRoot\secrets.encrypted.json" /grant "${appPoolIdentity}:(R)"

# Scanned files: Read & Execute
icacls "D:\IkeaDocuScan\ScannedFiles\checkin" /grant "${appPoolIdentity}:(OI)(CI)(RX)"

# Serilog logs: Modify
icacls "D:\Logs\IkeaDocuScan" /grant "${appPoolIdentity}:(OI)(CI)(M)"
```

---

## 10. Verify Deployment

```powershell
# AppPool status
$state = (Get-WebAppPoolState -Name "IkeaDocuScan").Value
Write-Host "AppPool: $state"

# Start site
Start-WebAppPool -Name "IkeaDocuScan"
Start-Website -Name "IkeaDocuScan"
```

Browse to: https://testdocuscan.ikeadt.com

---

## Quick Reference - Update Deployment

```powershell
# 1. Stop site
Stop-Website -Name "IkeaDocuScan"
Stop-WebAppPool -Name "IkeaDocuScan"

# 2. Backup & deploy (section 6)

# 3. Start site
Start-WebAppPool -Name "IkeaDocuScan"
Start-Website -Name "IkeaDocuScan"
```
