# Check-in Scanned Documents Feature

## Overview

This feature allows users to view scanned documents from a server folder and check them into the document management system.

---

## Features Implemented

### ✅ Configuration Management
- **DPAPI Encryption** - Windows Data Protection API for secure configuration
- **Multi-layered Configuration** - appsettings.json → appsettings.Production.json → appsettings.Local.json → secrets.encrypted.json → Environment Variables
- **Validation** - Automatic validation on application startup
- **No Azure Required** - Works entirely on-premise with Windows Server

### ✅ File Service
- **Security** - Path traversal attack prevention
- **File Type Filtering** - Configurable allowed extensions
- **File Size Limits** - Configurable maximum file size
- **Caching** - In-memory caching for performance
- **Comprehensive Logging** - All operations logged

### ✅ User Interface
- **File List Page** - Table with search, sort, and pagination
- **File Detail Page** - View file metadata and perform actions
- **Responsive Design** - Works on desktop and mobile
- **Error Handling** - User-friendly error messages
- **Loading States** - Spinners during async operations

### ✅ Audit Trail Integration
- **Check-in Logging** - Automatically logs when files are checked in
- **User Tracking** - Captures current user from authentication
- **Detailed Context** - Includes file name and size in audit details

### ✅ Tools
- **Configuration Encryption Tool** - CLI tool to create encrypted configuration files
- **Deployment Scripts** - PowerShell scripts for IIS deployment

---

## Files Created

### Configuration Infrastructure
```
IkeaDocuScan.Shared/Configuration/
├── DpapiConfigurationHelper.cs          - DPAPI encryption/decryption
├── EncryptedJsonConfigurationProvider.cs - Custom config provider
└── IkeaDocuScanOptions.cs               - Strongly-typed options

IkeaDocuScan-Web/
├── appsettings.json                      - Default configuration
├── appsettings.Local.json.example        - Example local config
└── .gitignore                            - Excludes sensitive files
```

### Services & DTOs
```
IkeaDocuScan.Shared/
├── DTOs/ScannedFiles/
│   └── ScannedFileDto.cs                 - File DTO with formatting
└── Interfaces/
    └── IScannedFileService.cs            - Service interface

IkeaDocuScan-Web/Services/
└── ScannedFileService.cs                 - Service implementation
```

### Blazor Pages
```
IkeaDocuScan-Web.Client/Pages/
├── CheckinScanned.razor                  - File list with table
└── CheckinFileDetail.razor               - File detail page

IkeaDocuScan-Web.Client/Layout/
└── NavMenu.razor                         - Updated with new menu item
```

### Tools & Documentation
```
ConfigEncryptionTool/
├── Program.cs                            - CLI encryption tool
└── ConfigEncryptionTool.csproj          - Project file

Documentation/
├── BLAZORISE_SETUP_INSTRUCTIONS.md      - Blazorise installation
├── DEPLOYMENT_GUIDE.md                  - Complete deployment guide
└── README_CHECKIN_SCANNED.md           - This file
```

---

## Configuration

### appsettings.json
```json
{
  "IkeaDocuScan": {
    "ScannedFilesPath": "C:\\ScannedDocuments",
    "AllowedFileExtensions": [".pdf", ".jpg", ".jpeg", ".png", ".tif", ".tiff"],
    "MaxFileSizeBytes": 52428800,
    "EnableFileListCaching": true,
    "CacheDurationSeconds": 60
  }
}
```

### appsettings.Local.json (server-specific)
```json
{
  "IkeaDocuScan": {
    "ScannedFilesPath": "\\\\FileServer\\ScannedDocuments"
  }
}
```

### secrets.encrypted.json (DPAPI encrypted)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "AQAAANCMnd8BFdERjHoAwE..."  // Encrypted
  }
}
```

---

## Setup Instructions

### 1. Install Blazorise (Manual Step Required)

See `BLAZORISE_SETUP_INSTRUCTIONS.md`

```bash
cd IkeaDocuScan-Web.Client
dotnet add package Blazorise --version 1.6.1
dotnet add package Blazorise.Bootstrap5 --version 1.6.1
dotnet add package Blazorise.Icons.FontAwesome --version 1.6.1
dotnet add package Blazorise.DataGrid --version 1.6.1
```

### 2. Configure Scanned Files Folder

**Development:**
```powershell
# Create folder
New-Item -Path "C:\ScannedDocuments" -ItemType Directory

# Add some test files
Copy-Item "C:\TestFiles\*.pdf" "C:\ScannedDocuments\"
```

**Production:**
```powershell
# Use network share
# Configure in appsettings.Local.json:
# "ScannedFilesPath": "\\\\FileServer\\Share\\ScannedDocuments"
```

### 3. Create Encrypted Configuration (Production Only)

```powershell
cd ConfigEncryptionTool
dotnet run

# Follow prompts to create secrets.encrypted.json
# Copy to application directory
```

### 4. Build and Run

```bash
dotnet build
dotnet run --project IkeaDocuScan-Web
```

### 5. Test

Navigate to:
- http://localhost:5000/checkin-scanned

---

## Usage

### Check-in Scanned Page

1. **View Files**
   - Lists all files from configured folder
   - Shows file icon, name, size, modified date
   - Search and filter capabilities
   - Pagination for large lists

2. **Select File**
   - Click on row to select
   - Click "View" to see details

3. **File Details**
   - View file metadata
   - Preview (PDF/Image preview coming soon)
   - Check-in to create document
   - Download file

4. **Check-in**
   - Click "Check-in Document"
   - System creates document record
   - Logs audit trail
   - Redirects to documents page

---

## Security Features

### Path Traversal Protection
```csharp
// Blocks attempts like: "../../etc/passwd"
if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
{
    return false; // Blocked
}

// Ensures file is within allowed directory
var normalizedPath = Path.GetFullPath(fullPath);
var basePath = Path.GetFullPath(_options.ScannedFilesPath);

if (!normalizedPath.StartsWith(basePath))
{
    return false; // Blocked
}
```

### File Extension Whitelist
```csharp
// Only configured extensions allowed
AllowedFileExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".tif", ".tiff"]
```

### File Size Limits
```csharp
// Prevents loading huge files
MaxFileSizeBytes = 52428800  // 50MB default
```

### DPAPI Encryption
```csharp
// Connection strings encrypted with Windows credentials
// Only same machine + user can decrypt
var encrypted = DpapiConfigurationHelper.Encrypt(connectionString);
```

---

## Architecture

### Configuration Layers
```
┌─────────────────────────────────────────┐
│ Environment Variables (IIS App Pool)    │  ← Highest Priority
├─────────────────────────────────────────┤
│ secrets.encrypted.json (DPAPI)          │
├─────────────────────────────────────────┤
│ appsettings.Local.json                  │
├─────────────────────────────────────────┤
│ appsettings.Production.json             │
├─────────────────────────────────────────┤
│ appsettings.json (defaults)             │  ← Lowest Priority
└─────────────────────────────────────────┘
```

### Service Architecture
```
CheckinScanned.razor
    ↓ calls
IScannedFileService (interface)
    ↓ implements
ScannedFileService
    ↓ uses
IkeaDocuScanOptions (configuration)
    ↓ validates path
File System (C:\ScannedDocuments)
```

### Security Flow
```
User Request
    ↓
ValidateFileName()
    ↓
CheckPathSafe() → Prevent traversal
    ↓
CheckExtension() → Whitelist only
    ↓
CheckFileSize() → Size limits
    ↓
ReadFile() → Log access
    ↓
Return to User
```

---

## Future Enhancements

### Coming Soon
- [ ] PDF preview with PDF.js
- [ ] Image preview
- [ ] File download via API endpoint
- [ ] Bulk check-in (multiple files)
- [ ] File move vs. copy options
- [ ] OCR integration
- [ ] Barcode generation during check-in
- [ ] Auto-populate document fields from file metadata

### Under Consideration
- [ ] Drag-and-drop file upload
- [ ] Direct scan integration
- [ ] File versioning
- [ ] Thumbnail generation
- [ ] Full-text search in PDFs

---

## Troubleshooting

### Files not showing

**Check:**
1. Folder path correct in configuration
2. Folder permissions (IIS AppPool needs Read access)
3. Files have allowed extensions
4. Application logs for errors

**Debug:**
```powershell
# Check folder access
Get-ChildItem "C:\ScannedDocuments"

# Check IIS AppPool identity
$identity = "IIS APPPOOL\IkeaDocuScanAppPool"
(Get-Acl "C:\ScannedDocuments").Access | Where-Object { $_.IdentityReference -eq $identity }
```

### Configuration errors

**Check:**
1. Validate appsettings.json syntax
2. Run validation: `options?.Validate()`
3. Check application startup logs

**Fix:**
```json
// Ensure all required fields present
{
  "IkeaDocuScan": {
    "ScannedFilesPath": "C:\\ScannedDocuments"  // Required
  }
}
```

### Decryption errors

**Symptom:** "Cannot decrypt configuration"

**Cause:** Encrypted on different machine

**Fix:**
```powershell
# Re-run encryption tool on THIS server
cd ConfigEncryptionTool
.\ConfigEncryptionTool.exe
```

---

## Performance

### Caching
- File list cached for 60 seconds (configurable)
- Reduces I/O on network shares
- Cache invalidated on refresh

### Pagination
- Default 10 items per page
- Prevents loading large lists
- Client-side pagination (already loaded)

### Lazy Loading
- Files loaded on page load only
- Metadata cached
- Content loaded on demand

---

## API Reference

### IScannedFileService

```csharp
Task<List<ScannedFileDto>> GetScannedFilesAsync();
Task<ScannedFileDto?> GetFileByNameAsync(string fileName);
Task<byte[]?> GetFileContentAsync(string fileName);
Task<bool> FileExistsAsync(string fileName);
Task<Stream?> GetFileStreamAsync(string fileName);
```

### IkeaDocuScanOptions

```csharp
public class IkeaDocuScanOptions
{
    public string ScannedFilesPath { get; set; }
    public string[] AllowedFileExtensions { get; set; }
    public long MaxFileSizeBytes { get; set; }
    public bool EnableFileListCaching { get; set; }
    public int CacheDurationSeconds { get; set; }
}
```

---

## Testing

### Unit Tests (TODO)
```csharp
[Fact]
public void PathTraversalBlocked()
{
    var service = new ScannedFileService(...);
    var result = service.IsPathSafe("../../etc/passwd");
    Assert.False(result);
}
```

### Integration Tests (TODO)
```csharp
[Fact]
public async Task GetFiles_ReturnsOnlyAllowedExtensions()
{
    var files = await service.GetScannedFilesAsync();
    Assert.All(files, f =>
        allowedExtensions.Contains(f.Extension));
}
```

---

## Support

For questions or issues:
- Check logs in `IkeaDocuScan-Web/logs/`
- Review `DEPLOYMENT_GUIDE.md`
- Check `BLAZORISE_SETUP_INSTRUCTIONS.md`
- Examine audit trail for check-in operations

**Key Configuration Files:**
- `appsettings.json` - Defaults
- `appsettings.Local.json` - Server-specific
- `secrets.encrypted.json` - Encrypted secrets

**Key Services:**
- `ScannedFileService` - File operations
- `AuditTrailService` - Activity logging
- `DocumentService` - Document management
