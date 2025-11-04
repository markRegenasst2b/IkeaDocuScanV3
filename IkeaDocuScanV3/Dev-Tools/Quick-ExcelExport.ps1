# ============================================================================
# Quick Excel Export Test
# ============================================================================
# Simple one-liner tests for Excel export API
# Usage: .\Quick-ExcelExport.ps1
# ============================================================================

param(
    [string]$BaseUrl = "https://localhost:44101",
    [int]$PageSize = 25
)

$apiUrl = "$BaseUrl/api/excel/export/documents"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outputFile = "Documents_Export_$timestamp.xlsx"

Write-Host "Exporting documents to Excel..." -ForegroundColor Cyan
Write-Host "  API: $apiUrl" -ForegroundColor Gray
Write-Host "  Output: $outputFile" -ForegroundColor Gray
Write-Host ""

$request = @{
    searchCriteria = @{
        searchTerm = ""
        pageNumber = 1
        pageSize = $PageSize
        counterPartyId = $null
        documentTypeId = $null
    }
} | ConvertTo-Json

try {
    Invoke-WebRequest `
        -Uri $apiUrl `
        -Method Post `
        -UseDefaultCredentials `
        -ContentType "application/json" `
        -Body $request `
        -OutFile $outputFile

    $file = Get-Item $outputFile
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "  File: $($file.FullName)" -ForegroundColor White
    Write-Host "  Size: $([math]::Round($file.Length / 1KB, 2)) KB" -ForegroundColor White
    Write-Host ""

    Start-Process $outputFile
}
catch {
    Write-Host "ERROR!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
