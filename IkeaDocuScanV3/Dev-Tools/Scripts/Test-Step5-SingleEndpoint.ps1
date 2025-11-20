#Requires -Version 5.1

<#
.SYNOPSIS
    Tests Step 5: Single Endpoint Dynamic Authorization

.DESCRIPTION
    Tests the GET /api/userpermissions/users endpoint with all 4 role profiles:
    - Reader (should get 403 Forbidden)
    - Publisher (should get 403 Forbidden)
    - ADAdmin (should get 200 OK)
    - SuperUser (should get 200 OK)

    Uses the Test Identity API to switch between profiles and test authorization.

.PARAMETER BaseUrl
    Base URL of the application. Default: https://localhost:44101

.PARAMETER SkipCertificateCheck
    Skip SSL certificate validation (useful for self-signed certs in dev)

.EXAMPLE
    .\Test-Step5-SingleEndpoint.ps1

.EXAMPLE
    .\Test-Step5-SingleEndpoint.ps1 -BaseUrl "https://localhost:7001" -SkipCertificateCheck

.NOTES
    Author: IkeaDocuScan Development Team
    Date: 2025-11-19
    Requires: Application running in DEBUG mode with Test Identity endpoints enabled
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$BaseUrl = "https://localhost:44101",

    [Parameter()]
    [switch]$SkipCertificateCheck
)

# ============================================================================
# Configuration
# ============================================================================

$ErrorActionPreference = "Continue"

# Test profiles to test
$testProfiles = @(
    @{
        Id = "reader"
        Name = "Reader 1"
        ExpectedStatus = 403
        ExpectedResult = "Forbidden"
    },
    @{
        Id = "publisher"
        Name = "Publisher 1"
        ExpectedStatus = 403
        ExpectedResult = "Forbidden"
    },
    @{
        Id = "adadmin"
        Name = "ADAdmin (Read-Only Admin)"
        ExpectedStatus = 200
        ExpectedResult = "OK"
    },
    @{
        Id = "superuser"
        Name = "SuperUser 1"
        ExpectedStatus = 200
        ExpectedResult = "OK"
    }
)

# Endpoint to test
$testEndpoint = "/api/userpermissions/users"
$testEndpointFull = "$BaseUrl$testEndpoint"

# ============================================================================
# Helper Functions
# ============================================================================

function Write-Header {
    param([string]$Text)
    Write-Host " " -NoNewline
    Write-Host ("=" * 80) -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host ("=" * 80) -ForegroundColor Cyan
}

function Write-SubHeader {
    param([string]$Text)
    Write-Host " $Text" -ForegroundColor Yellow
    Write-Host ("-" * 80) -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "   $Message" -ForegroundColor Green
}

function Write-Failure {
    param([string]$Message)
    Write-Host "   $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "  $Message" -ForegroundColor Cyan
}

function Write-TestResult {
    param(
        [string]$ProfileName,
        [int]$ExpectedStatus,
        [int]$ActualStatus,
        [string]$ResponseBody,
        [bool]$Success
    )

    Write-Host "   Profile: " -NoNewline
    Write-Host $ProfileName -ForegroundColor White
    Write-Host "  Expected: " -NoNewline
    Write-Host "$ExpectedStatus" -ForegroundColor $(if($ExpectedStatus -eq 200) { "Green" } else { "Yellow" })
    Write-Host "  Actual:   " -NoNewline
    Write-Host "$ActualStatus" -ForegroundColor $(if($ActualStatus -eq $ExpectedStatus) { "Green" } else { "Red" })

    if ($Success) {
        Write-Success "PASS - Got expected status code"
    } else {
        Write-Failure "FAIL - Status code mismatch"
    }

    if ($ActualStatus -eq 200 -and $ResponseBody) {
        try {
            $json = $ResponseBody | ConvertFrom-Json
            if ($json -is [Array]) {
                Write-Info "Response: Array with $($json.Count) items"
            } else {
                Write-Info "Response: $($json | ConvertTo-Json -Compress)"
            }
        } catch {
            Write-Info "Response: $($ResponseBody.Substring(0, [Math]::Min(100, $ResponseBody.Length)))..."
        }
    }
}

# ============================================================================
# SSL Certificate Bypass (for dev environments)
# ============================================================================

if ($SkipCertificateCheck) {
    Write-Warning "Skipping SSL certificate validation (dev mode)"

    # For PowerShell 6+
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        # Already supported via -SkipCertificateCheck parameter
    }
    # For PowerShell 5.1
    else {
        if (-not ([System.Management.Automation.PSTypeName]'TrustAllCertsPolicy').Type) {
            Add-Type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(
        ServicePoint svcPoint, X509Certificate certificate,
        WebRequest request, int certificateProblem) {
        return true;
    }
}
"@
        }
        [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
    }
}

# ============================================================================
# Pre-flight Checks
# ============================================================================

Write-Header "Step 5: Single Endpoint Dynamic Authorization Test"

Write-SubHeader "Pre-flight Checks"

Write-Info "Base URL: $BaseUrl"
Write-Info "Test Endpoint: $testEndpoint"
Write-Info "Full URL: $testEndpointFull"

# Check if application is running
try {
    $statusUrl = "$BaseUrl/api/test-identity/status"
    Write-Info "Checking if application is running..."

    $statusParams = @{
        Uri = $statusUrl
        Method = "GET"
        UseBasicParsing = $true
        TimeoutSec = 5
    }

    if ($SkipCertificateCheck -and $PSVersionTable.PSVersion.Major -ge 6) {
        $statusParams['SkipCertificateCheck'] = $true
    }

    $statusResponse = Invoke-WebRequest @statusParams -ErrorAction Stop
    Write-Success "Application is running and responding"
} catch {
    Write-Failure "Cannot connect to application at $BaseUrl"
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host " Make sure the application is running with:" -ForegroundColor Yellow
    Write-Host "  cd IkeaDocuScan-Web/IkeaDocuScan-Web" -ForegroundColor White
    Write-Host "  dotnet run" -ForegroundColor White
    exit 1
}

# Check if Test Identity endpoints are available
try {
    $profilesUrl = "$BaseUrl/api/test-identity/profiles"
    Write-Info "Checking Test Identity endpoints..."

    $profilesParams = @{
        Uri = $profilesUrl
        Method = "GET"
        UseBasicParsing = $true
        TimeoutSec = 5
    }

    if ($SkipCertificateCheck -and $PSVersionTable.PSVersion.Major -ge 6) {
        $profilesParams['SkipCertificateCheck'] = $true
    }

    $profilesResponse = Invoke-WebRequest @profilesParams -ErrorAction Stop
    Write-Success "Test Identity endpoints are available (DEBUG mode confirmed)"
} catch {
    Write-Failure "Test Identity endpoints not available"
    Write-Host "  Make sure application is running in DEBUG mode" -ForegroundColor Red
    exit 1
}

# ============================================================================
# Main Test Loop
# ============================================================================

Write-SubHeader "Running Authorization Tests"

$results = @()
$passCount = 0
$failCount = 0

foreach ($profile in $testProfiles) {
    Write-Host " " 
    Write-Host ("=" * 80) -ForegroundColor DarkGray
    Write-Host "Testing: $($profile.Name) (Profile ID: $($profile.Id))" -ForegroundColor White
    Write-Host ("=" * 80) -ForegroundColor DarkGray

    # Step 1: Activate test profile
    try {
        $activateUrl = "$BaseUrl/api/test-identity/activate/$($profile.Id)"
        Write-Info "Activating test profile..."

        $activateParams = @{
            Uri = $activateUrl
            Method = "POST"
            UseBasicParsing = $true
            TimeoutSec = 10
            SessionVariable = "session"
        }

        if ($SkipCertificateCheck -and $PSVersionTable.PSVersion.Major -ge 6) {
            $activateParams['SkipCertificateCheck'] = $true
        }

        $activateResponse = Invoke-WebRequest @activateParams -ErrorAction Stop
        Write-Success "Profile activated: $($profile.Name)"

        # Small delay to ensure session is established
        Start-Sleep -Milliseconds 500
    } catch {
        Write-Failure "Failed to activate profile: $($_.Exception.Message)"
        $failCount++
        continue
    }

    # Step 2: Test the endpoint
    try {
        Write-Info "Testing endpoint access..."

        $testParams = @{
            Uri = $testEndpointFull
            Method = "GET"
            UseBasicParsing = $true
            TimeoutSec = 10
            WebSession = $session
        }

        if ($SkipCertificateCheck -and $PSVersionTable.PSVersion.Major -ge 6) {
            $testParams['SkipCertificateCheck'] = $true
        }

        try {
            $testResponse = Invoke-WebRequest @testParams -ErrorAction Stop
            $actualStatus = $testResponse.StatusCode
            $responseBody = $testResponse.Content
        } catch {
            # Handle 403 Forbidden (expected for Reader/Publisher)
            if ($_.Exception.Response.StatusCode.Value__ -eq 403) {
                $actualStatus = 403
                $responseBody = "Forbidden"
            } else {
                throw
            }
        }

        $success = ($actualStatus -eq $profile.ExpectedStatus)

        Write-TestResult -ProfileName $profile.Name `
                        -ExpectedStatus $profile.ExpectedStatus `
                        -ActualStatus $actualStatus `
                        -ResponseBody $responseBody `
                        -Success $success

        if ($success) {
            $passCount++
        } else {
            $failCount++
        }

        $results += @{
            Profile = $profile.Name
            ProfileId = $profile.Id
            Expected = $profile.ExpectedStatus
            Actual = $actualStatus
            Success = $success
        }

    } catch {
        Write-Failure "Test failed with error: $($_.Exception.Message)"
        $failCount++

        $results += @{
            Profile = $profile.Name
            ProfileId = $profile.Id
            Expected = $profile.ExpectedStatus
            Actual = "Error"
            Success = $false
        }
    }
}

# ============================================================================
# Reset to default identity
# ============================================================================

Write-SubHeader "Cleanup"

try {
    $resetUrl = "$BaseUrl/api/test-identity/reset"
    Write-Info "Resetting to default identity..."

    $resetParams = @{
        Uri = $resetUrl
        Method = "POST"
        UseBasicParsing = $true
        TimeoutSec = 5
    }

    if ($SkipCertificateCheck -and $PSVersionTable.PSVersion.Major -ge 6) {
        $resetParams['SkipCertificateCheck'] = $true
    }

    Invoke-WebRequest @resetParams -ErrorAction Stop | Out-Null
    Write-Success "Identity reset to default"
} catch {
    Write-Warning "Could not reset identity: $($_.Exception.Message)"
}

# ============================================================================
# Summary Report
# ============================================================================

Write-Header "Test Results Summary"

Write-Host "  Total Tests: " -NoNewline
Write-Host "$($testProfiles.Count)" -ForegroundColor White

Write-Host "  Passed:      " -NoNewline
Write-Host "$passCount" -ForegroundColor Green

Write-Host "  Failed:      " -NoNewline
Write-Host "$failCount" -ForegroundColor $(if($failCount -eq 0) { "Green" } else { "Red" })

Write-Host "   Results:" -ForegroundColor White
foreach ($result in $results) {
    $statusColor = if($result.Success) { "Green" } else { "Red" }
    $statusSymbol = if($result.Success) { "v" } else { "x" }

    Write-Host "    $statusSymbol " -NoNewline -ForegroundColor $statusColor
    Write-Host "$($result.Profile): " -NoNewline
    Write-Host "Expected $($result.Expected), Got $($result.Actual)" -ForegroundColor $statusColor
}

# ============================================================================
# Pass/Fail Determination
# ============================================================================

Write-Host "" 
Write-Host ("=" * 80) -ForegroundColor Cyan

if ($failCount -eq 0) {
    Write-Host "ALL TESTS PASSED - Step 5 Complete!" -ForegroundColor Green
    Write-Host ("=" * 80) -ForegroundColor Cyan

    Write-Host " Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Verify cache behavior by running tests twice" -ForegroundColor White
    Write-Host "  2. Check application logs for cache hit/miss messages" -ForegroundColor White
    Write-Host "  3. Proceed to Step 6 (already complete)" -ForegroundColor White
    Write-Host "  4. Move on to Step 7: Endpoint Migration" -ForegroundColor White

    exit 0
} else {
    Write-Host "  ✗ TESTS FAILED - Review errors above" -ForegroundColor Red
    Write-Host ("=" * 80) -ForegroundColor Cyan

    Write-Host " Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Check database has seed data:" -ForegroundColor White
    Write-Host "     SELECT * FROM EndpointRegistry WHERE Route = '/api/userpermissions/users'" -ForegroundColor Gray
    Write-Host "     SELECT * FROM EndpointRolePermission" -ForegroundColor Gray
    Write-Host "   2. Check application logs for authorization errors" -ForegroundColor White
    Write-Host "   3. Verify DynamicAuthorizationPolicyProvider is registered" -ForegroundColor White
    Write-Host "   4. Test endpoint directly in browser with Dev Identity Switcher" -ForegroundColor White

    exit 1
}
