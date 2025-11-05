param(
    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "https://localhost:44101"
)

# Force all TLS/SSL protocols for compatibility (fixes "underlying connection was closed" error)
if ($PSVersionTable.PSVersion.Major -lt 6) {
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12 -bor [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls -bor [System.Net.SecurityProtocolType]::Ssl3
}

$ErrorActionPreference = "SilentlyContinue"

Write-Host ""
Write-Host "Checking if IkeaDocuScan application is running..." -ForegroundColor Cyan
Write-Host ""

# Test if port 44101 is listening
$port44101 = Get-NetTCPConnection -LocalPort 44101 -State Listen 2>$null

if ($port44101) {
    Write-Host "[OK] Application is listening on port 44101" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "[ERROR] No application listening on port 44101" -ForegroundColor Red
    Write-Host ""
    Write-Host "To start the application:" -ForegroundColor Yellow
    Write-Host "  1. Open Visual Studio" -ForegroundColor White
    Write-Host "  2. Open IkeaDocuScanV3.sln" -ForegroundColor White
    Write-Host "  3. Set 'IkeaDocuScan-Web' as startup project" -ForegroundColor White
    Write-Host "  4. Press F5 or click 'IIS Express' to run" -ForegroundColor White
    Write-Host ""
    Write-Host "Or from command line (may have auth issues):" -ForegroundColor Yellow
    Write-Host "  cd IkeaDocuScan-Web\IkeaDocuScan-Web" -ForegroundColor White
    Write-Host "  dotnet run" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Try to connect to the API
Write-Host "Testing connection to $BaseUrl..." -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $params = @{
            UseDefaultCredentials = $true
            SkipCertificateCheck = $true
            TimeoutSec = 5
        }
    } else {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
        $params = @{
            UseDefaultCredentials = $true
            TimeoutSec = 5
        }
    }

    # Try a simple endpoint first
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/configuration/email-templates" -Method GET @params

    Write-Host "[OK] Successfully connected to application!" -ForegroundColor Green
    Write-Host "  Status: $($response.StatusCode) $($response.StatusDescription)" -ForegroundColor White
    Write-Host ""
    Write-Host "You can now run the diagnostic:" -ForegroundColor Yellow
    Write-Host "  .\Test-DocumentAttachmentTemplate.ps1" -ForegroundColor White
    Write-Host ""
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__

    if ($statusCode -eq 401) {
        Write-Host "[ERROR] Authentication failed (401 Unauthorized)" -ForegroundColor Red
        Write-Host ""
        Write-Host "Possible causes:" -ForegroundColor Yellow
        Write-Host "  1. You are not logged in with a Windows account that exists in the database" -ForegroundColor White
        Write-Host "  2. Your user account does not have SuperUser permission" -ForegroundColor White
        Write-Host "  3. Windows Authentication is not configured correctly" -ForegroundColor White
        Write-Host ""
        Write-Host "To fix:" -ForegroundColor Yellow
        Write-Host "  - Ensure you're logged into Windows as a user in the DocuScanUser table" -ForegroundColor White
        Write-Host "  - Verify IsSuperUser = true for your account in the database" -ForegroundColor White
        Write-Host "  - Check appsettings.json for authentication configuration" -ForegroundColor White
    } elseif ($statusCode -eq 404) {
        Write-Host "[WARNING] Application is running but endpoint not found (404)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "This might mean:" -ForegroundColor Yellow
        Write-Host "  - The diagnostic endpoint hasn't been registered yet" -ForegroundColor White
        Write-Host "  - Application needs to be rebuilt" -ForegroundColor White
        Write-Host ""
        Write-Host "Try rebuilding:" -ForegroundColor Yellow
        Write-Host "  dotnet build" -ForegroundColor White
        Write-Host "  Then restart the application" -ForegroundColor White
    } else {
        Write-Host "[ERROR] Connection failed" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "Details:" -ForegroundColor Yellow
        if ($_.Exception.InnerException) {
            Write-Host "  Inner Exception: $($_.Exception.InnerException.Message)" -ForegroundColor White
        }
    }
    Write-Host ""
}

Write-Host ""
