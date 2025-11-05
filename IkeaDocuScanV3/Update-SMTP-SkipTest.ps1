# Update-SMTP-SkipTest.ps1
# Emergency script to update SMTP configuration WITHOUT testing
# Use this when SMTP server is unreachable but you need to save the configuration

param(
    [string]$BaseUrl = "https://localhost:44101",
    [switch]$SkipCertificateCheck,
    [switch]$UseDefaultCredentials = $true
)

$ErrorActionPreference = "Stop"

Write-Host "=== SMTP Configuration Update (Skip Test Mode) ===" -ForegroundColor Yellow
Write-Host "WARNING: This will save SMTP settings WITHOUT testing connectivity" -ForegroundColor Red
Write-Host ""

# Skip certificate validation if requested
if ($SkipCertificateCheck) {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $PSDefaultParameterValues['Invoke-RestMethod:SkipCertificateCheck'] = $true
    } else {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
    }
}

# Collect SMTP settings
Write-Host "Enter SMTP Configuration:" -ForegroundColor Cyan
Write-Host ""

$smtpHost = Read-Host "SMTP Host (e.g., smtp.office365.com)"
$smtpPort = Read-Host "SMTP Port (default: 587)"
if ([string]::IsNullOrWhiteSpace($smtpPort)) { $smtpPort = "587" }

$useSslInput = Read-Host "Use SSL/TLS? (Y/N, default: Y)"
$useSsl = if ($useSslInput -eq "N" -or $useSslInput -eq "n") { $false } else { $true }

$smtpUsername = Read-Host "SMTP Username (optional, press Enter to skip)"
$smtpPassword = Read-Host "SMTP Password (optional, press Enter to skip)" -AsSecureString
$smtpPasswordPlain = if ($smtpPassword.Length -gt 0) {
    [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($smtpPassword))
} else { "" }

$fromAddress = Read-Host "From Address (e.g., noreply@company.com)"
$fromName = Read-Host "From Display Name (optional, e.g., DocuScan System)"

Write-Host ""
Write-Host "Configuration Summary:" -ForegroundColor Yellow
Write-Host "  SMTP Host: $smtpHost"
Write-Host "  SMTP Port: $smtpPort"
Write-Host "  Use SSL: $useSsl"
Write-Host "  Username: $smtpUsername"
Write-Host "  From Address: $fromAddress"
Write-Host "  From Name: $fromName"
Write-Host ""

$confirm = Read-Host "Save this configuration WITHOUT testing? (Y/N)"
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "Cancelled" -ForegroundColor Gray
    exit
}

# Build request body (just the SMTP config)
$config = @{
    smtpHost = $smtpHost
    smtpPort = [int]$smtpPort
    useSsl = $useSsl
    smtpUsername = $smtpUsername
    smtpPassword = $smtpPasswordPlain
    fromAddress = $fromAddress
    fromName = $fromName
}

$body = $config | ConvertTo-Json -Depth 10

Write-Host ""
Write-Host "Sending request..." -ForegroundColor Yellow

try {
    # Use query parameter for skipTest
    $params = @{
        Uri = "$BaseUrl/api/configuration/smtp?skipTest=true"
        Method = "POST"
        Body = $body
        ContentType = "application/json"
        UseDefaultCredentials = $UseDefaultCredentials
    }

    $response = Invoke-RestMethod @params

    Write-Host ""
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Message: $($response.message)" -ForegroundColor Green
    Write-Host "Tested: $($response.tested)" -ForegroundColor $(if ($response.tested) { "Green" } else { "Yellow" })
    Write-Host ""

    if (-not $response.tested) {
        Write-Host "IMPORTANT: SMTP settings were saved WITHOUT testing!" -ForegroundColor Yellow
        Write-Host "You should test the connection manually:" -ForegroundColor Yellow
        Write-Host "  POST $BaseUrl/api/configuration/test-smtp" -ForegroundColor Gray
    }
}
catch {
    Write-Host ""
    Write-Host "FAILED!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Ensure the application is running" -ForegroundColor Gray
    Write-Host "2. Check your authentication" -ForegroundColor Gray
    Write-Host "3. Verify the URL: $BaseUrl" -ForegroundColor Gray
    Write-Host "4. See TROUBLESHOOTING_SMTP.md for more help" -ForegroundColor Gray
}

Write-Host ""
