# Excel Export API Testing Guide

This guide provides PowerShell scripts and examples for testing the Excel export functionality.

## Prerequisites

- PowerShell 5.1 or higher
- IkeaDocuScan application running (default: https://localhost:44101)
- Valid Windows authentication credentials
- Access to the IkeaDocuScan system

## Available Scripts

### 1. Test-ExcelExportAPI.ps1 (Comprehensive Test Suite)

**Description:** Complete test suite covering all endpoints with multiple scenarios

**Usage:**
```powershell
cd Dev-Tools
.\Test-ExcelExportAPI.ps1
```

**What it tests:**
- ✅ Get export metadata (column definitions)
- ✅ Validate export size (small dataset)
- ✅ Export documents to Excel
- ✅ Export with filters (counter party)
- ✅ Test size limit warnings

**Output:** Excel files saved to `Dev-Tools/ExcelExports/`

---

### 2. Quick-ExcelExport.ps1 (Quick Test)

**Description:** Fast single-command export test

**Usage:**
```powershell
.\Quick-ExcelExport.ps1
```

**With custom parameters:**
```powershell
.\Quick-ExcelExport.ps1 -BaseUrl "https://localhost:44101" -PageSize 50
```

**Output:** Excel file in current directory, automatically opens

---

## API Endpoints

### 1. Get Export Metadata
**Endpoint:** `GET /api/excel/metadata/documents`

**Description:** Returns column definitions for the export

**PowerShell Example:**
```powershell
$baseUrl = "https://localhost:44101"
$metadata = Invoke-RestMethod `
    -Uri "$baseUrl/api/excel/metadata/documents" `
    -Method Get `
    -UseDefaultCredentials

# Display columns
$metadata | ForEach-Object {
    Write-Host "$($_.DisplayName) - $($_.DataType)"
}
```

**Response:**
```json
[
  {
    "displayName": "Document ID",
    "dataType": "Number",
    "format": "#,##0",
    "order": 1
  },
  {
    "displayName": "Document Name",
    "dataType": "String",
    "format": "",
    "order": 2
  }
  // ... more columns
]
```

---

### 2. Validate Export Size
**Endpoint:** `POST /api/excel/validate/documents`

**Description:** Validates the export size before generating the file

**PowerShell Example:**
```powershell
$baseUrl = "https://localhost:44101"

$request = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 5000
        counterPartyId = $null
        documentTypeId = $null
    }
} | ConvertTo-Json

$validation = Invoke-RestMethod `
    -Uri "$baseUrl/api/excel/validate/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request

Write-Host "Row Count: $($validation.RowCount)"
Write-Host "Is Valid: $($validation.IsValid)"
Write-Host "Has Warning: $($validation.HasWarning)"
Write-Host "Message: $($validation.Message)"
```

**Response:**
```json
{
  "isValid": true,
  "hasWarning": false,
  "rowCount": 247,
  "message": null
}
```

**Warning Response (>10,000 rows):**
```json
{
  "isValid": true,
  "hasWarning": true,
  "rowCount": 15000,
  "message": "Large export detected (15,000 rows). This may take several seconds to generate. Continue?"
}
```

**Error Response (>50,000 rows):**
```json
{
  "isValid": false,
  "hasWarning": false,
  "rowCount": 75000,
  "message": "Export exceeds maximum allowed rows (50,000). Current row count: 75,000. Please apply filters to reduce the data size."
}
```

---

### 3. Export Documents to Excel
**Endpoint:** `POST /api/excel/export/documents`

**Description:** Generates and downloads an Excel file

**PowerShell Example (Basic):**
```powershell
$baseUrl = "https://localhost:44101"
$outputFile = "Documents_Export.xlsx"

$request = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 100
    }
} | ConvertTo-Json

Invoke-WebRequest `
    -Uri "$baseUrl/api/excel/export/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request `
    -OutFile $outputFile

Write-Host "Export complete: $outputFile"
Start-Process $outputFile
```

**PowerShell Example (With Filters):**
```powershell
$request = @{
    searchCriteria = @{
        searchTerm = "contract"
        pageNumber = 1
        pageSize = 500
        counterPartyId = 5
        documentTypeId = 3
        dateFrom = "2024-01-01"
        dateTo = "2024-12-31"
    }
    filterContext = @{
        "Search Term" = "contract"
        "Counter Party" = "Acme Corp"
        "Document Type" = "Agreements"
        "Date Range" = "2024-01-01 to 2024-12-31"
    }
} | ConvertTo-Json

Invoke-WebRequest `
    -Uri "$baseUrl/api/excel/export/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request `
    -OutFile "Documents_Filtered.xlsx"
```

**Response:** Binary Excel file (XLSX format)

---

## Request Body Structure

### SearchCriteria Object
```json
{
  "searchTerm": "string",           // Optional: text search
  "pageNumber": 1,                   // Page number (1-based)
  "pageSize": 100,                   // Number of results
  "counterPartyId": null,            // Optional: filter by counter party
  "documentTypeId": null,            // Optional: filter by document type
  "dateFrom": "2024-01-01",         // Optional: date range start
  "dateTo": "2024-12-31"            // Optional: date range end
}
```

### FilterContext Object (Optional)
```json
{
  "Filter Name": "Filter Value",
  "Another Filter": "Another Value"
}
```

This will be displayed in the preview UI to show what filters were applied.

---

## Common Scenarios

### Scenario 1: Export All Documents (Up to 25)
```powershell
$request = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 25
    }
} | ConvertTo-Json

Invoke-WebRequest `
    -Uri "https://localhost:44101/api/excel/export/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request `
    -OutFile "AllDocuments.xlsx"
```

### Scenario 2: Export by Counter Party
```powershell
$request = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 1000
        counterPartyId = 10
    }
} | ConvertTo-Json

Invoke-WebRequest `
    -Uri "https://localhost:44101/api/excel/export/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request `
    -OutFile "CounterParty_10.xlsx"
```

### Scenario 3: Export by Document Type and Date Range
```powershell
$request = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 5000
        documentTypeId = 2
        dateFrom = "2024-01-01"
        dateTo = "2024-12-31"
    }
} | ConvertTo-Json

Invoke-WebRequest `
    -Uri "https://localhost:44101/api/excel/export/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request `
    -OutFile "Contracts_2024.xlsx"
```

### Scenario 4: Search and Export
```powershell
$request = @{
    searchCriteria = @{
        searchTerm = "confidential"
        pageNumber = 1
        pageSize = 500
    }
} | ConvertTo-Json

Invoke-WebRequest `
    -Uri "https://localhost:44101/api/excel/export/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request `
    -OutFile "Confidential_Documents.xlsx"
```

---

## Error Handling

### Handle HTTP Errors
```powershell
try {
    Invoke-WebRequest `
        -Uri "$baseUrl/api/excel/export/documents" `
        -Method Post `
        -UseDefaultCredentials `
        -ContentType "application/json" `
        -Body $request `
        -OutFile $outputFile

    Write-Host "Success!" -ForegroundColor Green
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "Status Code: $statusCode" -ForegroundColor Red

        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}
```

### Common Error Responses

**400 Bad Request - Export Too Large:**
```json
{
  "error": "Export exceeds maximum allowed rows (50,000). Current row count: 75,000. Please apply filters to reduce the data size.",
  "rowCount": 75000
}
```

**401 Unauthorized:**
```
User not authenticated. Ensure Windows authentication is enabled.
```

**403 Forbidden:**
```
User does not have 'HasAccess' permission.
```

**500 Internal Server Error:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Excel Export Failed",
  "status": 500,
  "detail": "Error message details"
}
```

---

## Configuration

Export settings are configured in `appsettings.json`:

```json
{
  "ExcelExport": {
    "WarningRowCount": 10000,        // Show warning above this count
    "MaximumRowCount": 50000,        // Hard limit
    "HeaderBackgroundColor": "#0051BA",  // IKEA Blue
    "HeaderFontColor": "#FFFFFF",
    "SheetName": "Export",
    "FreezeHeaderRow": true,
    "EnableFilters": true
  }
}
```

---

## Audit Trail

All Excel exports are logged to the `AuditTrail` table with:
- **Action:** `ExportExcel`
- **BarCode:** `BULK_EXPORT`
- **Details:** `Exported X documents to Excel`
- **Username:** Current authenticated user
- **Timestamp:** Export date/time

**Check Audit Log:**
```sql
SELECT TOP 10
    Action,
    BarCode,
    Details,
    Username,
    ActionDate
FROM AuditTrail
WHERE Action = 'ExportExcel'
ORDER BY ActionDate DESC
```

---

## Troubleshooting

### Issue: "Unable to connect to the remote server"
**Solution:** Ensure the application is running and the URL is correct.

```powershell
# Test if the application is running
Invoke-WebRequest -Uri "https://localhost:44101" -UseBasicParsing
```

### Issue: "401 Unauthorized"
**Solution:** Use `-UseDefaultCredentials` to pass Windows authentication.

```powershell
# Correct way (with authentication)
Invoke-WebRequest -Uri $url -UseDefaultCredentials

# Wrong way (will fail)
Invoke-WebRequest -Uri $url
```

### Issue: "The underlying connection was closed: Could not establish trust relationship"
**Solution:** Add `-SkipCertificateCheck` for self-signed certificates (PowerShell 7+) or:

```powershell
# PowerShell 5.1
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
```

### Issue: "Export returns empty or corrupted file"
**Solution:** Check that you have data matching the search criteria.

```powershell
# First, validate the export
$validation = Invoke-RestMethod `
    -Uri "$baseUrl/api/excel/validate/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request

if ($validation.RowCount -eq 0) {
    Write-Host "No data matches your search criteria"
}
```

---

## Advanced Usage

### Batch Export Multiple Filters
```powershell
$counterParties = @(1, 2, 3, 5, 10)

foreach ($cpId in $counterParties) {
    $request = @{
        searchCriteria = @{
            searchTerm = ""
            pageNumber = 1
            pageSize = 1000
            counterPartyId = $cpId
        }
    } | ConvertTo-Json

    $outputFile = "CounterParty_$cpId.xlsx"

    Invoke-WebRequest `
        -Uri "$baseUrl/api/excel/export/documents" `
        -Method Post `
        -UseDefaultCredentials `
        -ContentType "application/json" `
        -Body $request `
        -OutFile $outputFile

    Write-Host "Exported: $outputFile" -ForegroundColor Green
}
```

### Monitor Export Progress (Large Files)
```powershell
$request = @{
    searchCriteria = @{
        pageSize = 10000
    }
} | ConvertTo-Json

Write-Host "Starting large export..." -ForegroundColor Yellow

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

Invoke-WebRequest `
    -Uri "$baseUrl/api/excel/export/documents" `
    -Method Post `
    -UseDefaultCredentials `
    -ContentType "application/json" `
    -Body $request `
    -OutFile "Large_Export.xlsx"

$stopwatch.Stop()

Write-Host "Export completed in $($stopwatch.Elapsed.TotalSeconds) seconds" -ForegroundColor Green
```

---

## Next Steps

1. Run `.\Test-ExcelExportAPI.ps1` for a complete test
2. Open the generated Excel files to verify formatting
3. Check the audit trail in the database
4. Customize the scripts for your specific use cases
5. Integrate with your existing PowerShell automation

---

**Last Updated:** 2025-01-27
