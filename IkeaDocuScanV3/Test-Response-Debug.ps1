param(
    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "https://localhost:44101"
)

# Force all TLS/SSL protocols for compatibility
if ($PSVersionTable.PSVersion.Major -lt 6) {
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12 -bor [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls -bor [System.Net.SecurityProtocolType]::Ssl3
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
}

$testUrl = "$BaseUrl/api/configuration/email-templates/diagnostic/DocumentAttachment"

Write-Host "Fetching diagnostic data..." -ForegroundColor Cyan
Write-Host "URL: $testUrl" -ForegroundColor Gray
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri $testUrl -Method GET -UseDefaultCredentials

    Write-Host "=== RAW RESPONSE ===" -ForegroundColor Yellow
    $response | ConvertTo-Json -Depth 10 | Write-Host
    Write-Host ""

    Write-Host "=== RESPONSE TYPE ===" -ForegroundColor Yellow
    Write-Host "Type: $($response.GetType().FullName)" -ForegroundColor White
    Write-Host ""

    Write-Host "=== TOP-LEVEL PROPERTIES ===" -ForegroundColor Yellow
    $response | Get-Member -MemberType Properties | Format-Table -AutoSize
    Write-Host ""

    if ($response.diagnostic) {
        Write-Host "=== DIAGNOSTIC PROPERTIES ===" -ForegroundColor Yellow
        $response.diagnostic | Get-Member -MemberType Properties | Format-Table -AutoSize
    }

    if ($response.summary) {
        Write-Host "=== SUMMARY PROPERTIES ===" -ForegroundColor Yellow
        $response.summary | Get-Member -MemberType Properties | Format-Table -AutoSize
    }

} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
