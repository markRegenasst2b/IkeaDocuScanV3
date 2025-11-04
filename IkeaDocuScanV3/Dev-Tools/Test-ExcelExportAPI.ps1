# ============================================================================
# Excel Export API Testing Script
# ============================================================================
# This script tests the Excel export functionality via API endpoints
# Requires: PowerShell 5.1 or higher
# ============================================================================

# Configuration
$baseUrl = "https://localhost:44101"  # Adjust to your application URL
$apiBase = "$baseUrl/api/excel"

# Create output directory for downloaded Excel files
$outputDir = Join-Path $PSScriptRoot "ExcelExports"
if (-not (Test-Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory | Out-Null
}

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "Excel Export API Test Suite" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# Test 1: Get Export Metadata
# ============================================================================
Write-Host "Test 1: Get Export Metadata" -ForegroundColor Yellow
Write-Host "Endpoint: GET $apiBase/metadata/documents" -ForegroundColor Gray
Write-Host ""

try {
    $metadata = Invoke-RestMethod `
        -Uri "$apiBase/metadata/documents" `
        -Method Get `
        -UseDefaultCredentials `
        -ContentType "application/json"

    Write-Host "SUCCESS: Retrieved metadata for $($metadata.Count) columns" -ForegroundColor Green
    Write-Host ""
    Write-Host "Columns that will be exported:" -ForegroundColor Cyan
    foreach ($column in $metadata) {
        Write-Host "  - $($column.DisplayName) ($($column.DataType))" -ForegroundColor White
    }
    Write-Host ""
}
catch {
    Write-Host "ERROR: Failed to retrieve metadata" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ============================================================================
# Test 2: Validate Export Size (Small Dataset)
# ============================================================================
Write-Host "Test 2: Validate Export Size (Small Dataset)" -ForegroundColor Yellow
Write-Host "Endpoint: POST $apiBase/validate/documents" -ForegroundColor Gray
Write-Host ""

$validateRequest = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 100
        counterPartyId = $null
        documentTypeId = $null
    }
} | ConvertTo-Json

try {
    $validation = Invoke-RestMethod `
        -Uri "$apiBase/validate/documents" `
        -Method Post `
        -UseDefaultCredentials `
        -ContentType "application/json" `
        -Body $validateRequest

    Write-Host "SUCCESS: Validation complete" -ForegroundColor Green
    Write-Host "  Row Count: $($validation.RowCount)" -ForegroundColor White
    Write-Host "  Is Valid: $($validation.IsValid)" -ForegroundColor White
    Write-Host "  Has Warning: $($validation.HasWarning)" -ForegroundColor White
    if ($validation.Message) {
        Write-Host "  Message: $($validation.Message)" -ForegroundColor Yellow
    }
    Write-Host ""
}
catch {
    Write-Host "ERROR: Validation failed" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ============================================================================
# Test 3: Export Documents to Excel (Small Dataset)
# ============================================================================
Write-Host "Test 3: Export Documents to Excel (Small Dataset)" -ForegroundColor Yellow
Write-Host "Endpoint: POST $apiBase/export/documents" -ForegroundColor Gray
Write-Host ""

$exportRequest = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 25
        counterPartyId = $null
        documentTypeId = $null
    }
    filterContext = @{
        "Exported By" = "PowerShell Test Script"
        "Date" = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    }
} | ConvertTo-Json

try {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $outputFile = Join-Path $outputDir "Documents_Export_$timestamp.xlsx"

    Write-Host "Requesting Excel export..." -ForegroundColor Gray

    Invoke-WebRequest `
        -Uri "$apiBase/export/documents" `
        -Method Post `
        -UseDefaultCredentials `
        -ContentType "application/json" `
        -Body $exportRequest `
        -OutFile $outputFile

    $fileInfo = Get-Item $outputFile

    Write-Host "SUCCESS: Excel file exported" -ForegroundColor Green
    Write-Host "  File: $($fileInfo.Name)" -ForegroundColor White
    Write-Host "  Size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor White
    Write-Host "  Path: $($fileInfo.FullName)" -ForegroundColor Cyan
    Write-Host ""

    # Optionally open the file
    $openFile = Read-Host "Open the Excel file? (Y/N)"
    if ($openFile -eq 'Y' -or $openFile -eq 'y') {
        Start-Process $outputFile
    }
}
catch {
    Write-Host "ERROR: Export failed" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
    Write-Host ""
}

# ============================================================================
# Test 4: Export with Filters (Counter Party)
# ============================================================================
Write-Host "Test 4: Export with Filters (Counter Party ID = 1)" -ForegroundColor Yellow
Write-Host "Endpoint: POST $apiBase/export/documents" -ForegroundColor Gray
Write-Host ""

$filteredRequest = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 100
        counterPartyId = 1
        documentTypeId = $null
    }
    filterContext = @{
        "Filter" = "Counter Party ID = 1"
        "Exported By" = "PowerShell Test Script"
    }
} | ConvertTo-Json

try {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $outputFile = Join-Path $outputDir "Documents_Filtered_$timestamp.xlsx"

    Write-Host "Requesting filtered Excel export..." -ForegroundColor Gray

    Invoke-WebRequest `
        -Uri "$apiBase/export/documents" `
        -Method Post `
        -UseDefaultCredentials `
        -ContentType "application/json" `
        -Body $filteredRequest `
        -OutFile $outputFile

    $fileInfo = Get-Item $outputFile

    Write-Host "SUCCESS: Filtered Excel file exported" -ForegroundColor Green
    Write-Host "  File: $($fileInfo.Name)" -ForegroundColor White
    Write-Host "  Size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor White
    Write-Host "  Path: $($fileInfo.FullName)" -ForegroundColor Cyan
    Write-Host ""
}
catch {
    Write-Host "ERROR: Filtered export failed" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ============================================================================
# Test 5: Test Export Size Limits (Warning Threshold)
# ============================================================================
Write-Host "Test 5: Test Export Size Limits (Request 15,000 rows)" -ForegroundColor Yellow
Write-Host "Endpoint: POST $apiBase/validate/documents" -ForegroundColor Gray
Write-Host ""

$largeRequest = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = 15000  # Above warning threshold (10,000)
        counterPartyId = $null
        documentTypeId = $null
    }
} | ConvertTo-Json

try {
    $validation = Invoke-RestMethod `
        -Uri "$apiBase/validate/documents" `
        -Method Post `
        -UseDefaultCredentials `
        -ContentType "application/json" `
        -Body $largeRequest

    Write-Host "Validation Result:" -ForegroundColor Cyan
    Write-Host "  Row Count: $($validation.RowCount)" -ForegroundColor White
    Write-Host "  Is Valid: $($validation.IsValid)" -ForegroundColor White
    Write-Host "  Has Warning: $($validation.HasWarning)" -ForegroundColor $(if ($validation.HasWarning) { "Yellow" } else { "White" })
    if ($validation.Message) {
        Write-Host "  Message: $($validation.Message)" -ForegroundColor Yellow
    }
    Write-Host ""
}
catch {
    Write-Host "ERROR: Large dataset validation failed" -ForegroundColor Red
    Write-Host "Details: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ============================================================================
# Test Summary
# ============================================================================
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Exported files location: $outputDir" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Open the exported Excel files to verify formatting" -ForegroundColor White
Write-Host "  2. Check the IkeaDocuScan database AuditTrail table for export logs" -ForegroundColor White
Write-Host "  3. Verify column headers match the metadata" -ForegroundColor White
Write-Host "  4. Test with different search criteria and filters" -ForegroundColor White
Write-Host ""
Write-Host "Configuration defaults (from appsettings.json):" -ForegroundColor Yellow
Write-Host "  Warning Row Count: 10,000 rows" -ForegroundColor White
Write-Host "  Maximum Row Count: 50,000 rows" -ForegroundColor White
Write-Host "  Header Color: #0051BA (IKEA Blue)" -ForegroundColor White
Write-Host ""
