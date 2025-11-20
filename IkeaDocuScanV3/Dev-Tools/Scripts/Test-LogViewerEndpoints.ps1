#Requires -Version 5.1

<#
.SYNOPSIS
    Tests LogViewerEndpoints migration to dynamic authorization

.DESCRIPTION
    Tests all 5 LogViewerEndpoints with all 4 roles:
    - POST /api/logs/search
    - GET /api/logs/export
    - GET /api/logs/dates
    - GET /api/logs/sources
    - GET /api/logs/statistics

    Expected: ADAdmin and SuperUser get 200 OK, Reader and Publisher get 403 Forbidden

.PARAMETER BaseUrl
    Base URL of the application. Default: https://localhost:44101

.PARAMETER SkipCertificateCheck
    Skip SSL certificate validation (useful for self-signed certs in dev)

.EXAMPLE
    .\Test-LogViewerEndpoints.ps1

.EXAMPLE
    .\Test-LogViewerEndpoints.ps1 -BaseUrl "https://localhost:7001" -SkipCertificateCheck
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$BaseUrl = "https://localhost:44101",

    [Parameter()]
    [switch]$SkipCertificateCheck
)

$ErrorActionPreference = "Continue"

# Test profiles
$testProfiles = @(
    @{
        Id = "reader"
        Name = "Reader"
        ExpectedStatus = 403
    },
    @{
        Id = "publisher"
        Name = "Publisher"
        ExpectedStatus = 403
    },
    @{
        Id = "adadmin"
        Name = "ADAdmin"
        ExpectedStatus = 200
    },
    @{
        Id = "superuser"
        Name = "SuperUser"
        ExpectedStatus = 200
    }
)

# Endpoints to test
$endpoints = @(
    @{
        Method = "POST"
        Path = "/api/logs/search"
        Name = "SearchLogs"
        Body = @{
            fromDate = (Get-Date).AddDays(-7).ToString("yyyy-MM-dd")
            toDate = (Get-Date).ToString("yyyy-MM-dd")
            level = "Information"
            searchText = ""
        } | ConvertTo-Json
    },
    @{
        Method = "GET"
        Path = "/api/logs/export?format=json&fromDate=$((Get-Date).AddDays(-1).ToString('yyyy-MM-dd'))"
        Name = "ExportLogs"
        Body = $null
    },
    @{
        Method = "GET"
        Path = "/api/logs/dates"
        Name = "GetLogDates"
        Body = $null
    },
    @{
        Method = "GET"
        Path = "/api/logs/sources"
        Name = "GetLogSources"
        Body = $null
    },
    @{
        Method = "GET"
        Path = "/api/logs/statistics"
        Name = "GetLogStatistics"
        Body = $null
    }
)

# Helper functions
function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host ("=" * 80) -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host ("=" * 80) -ForegroundColor Cyan
}

function Write-SubHeader {
    param([string]$Text)
    Write-Host ""
    Write-Host $Text -ForegroundColor Yellow
    Write-Host ("-" * 80) -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "  [PASS] $Message" -ForegroundColor Green
}

function Write-Failure {
    param([string]$Message)
    Write-Host "  [FAIL] $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "  $Message" -ForegroundColor Cyan
}

# SSL Certificate bypass
if ($SkipCertificateCheck) {
    Write-Warning "Skipping SSL certificate validation"
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        # PowerShell 6+ handles this via parameter
    } else {
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

# Main execution
Write-Header "LogViewerEndpoints Migration Test"

Write-SubHeader "Pre-flight Checks"

Write-Info "Base URL: $BaseUrl"
Write-Info "Endpoints to test: $($endpoints.Count)"
Write-Info "Profiles to test: $($testProfiles.Count)"
Write-Info "Total test cases: $($endpoints.Count * $testProfiles.Count)"

# Check application is running
try {
    $statusUrl = "$BaseUrl/api/test-identity/status"
    $statusParams = @{
        Uri = $statusUrl
        Method = "GET"
        UseBasicParsing = $true
        TimeoutSec = 5
    }
    if ($SkipCertificateCheck -and $PSVersionTable.PSVersion.Major -ge 6) {
        $statusParams['SkipCertificateCheck'] = $true
    }
    Invoke-WebRequest @statusParams -ErrorAction Stop | Out-Null
    Write-Success "Application is running"
} catch {
    Write-Failure "Cannot connect to application at $BaseUrl"
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Run tests
Write-SubHeader "Running Authorization Tests"

$totalTests = 0
$passedTests = 0
$failedTests = 0
$results = @()

foreach ($profile in $testProfiles) {
    Write-Host ""
    Write-Host ("=" * 80) -ForegroundColor DarkGray
    Write-Host "Testing Profile: $($profile.Name) (Expected: $($profile.ExpectedStatus))" -ForegroundColor White
    Write-Host ("=" * 80) -ForegroundColor DarkGray

    # Activate profile
    try {
        $activateUrl = "$BaseUrl/api/test-identity/activate/$($profile.Id)"
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
        Invoke-WebRequest @activateParams -ErrorAction Stop | Out-Null
        Write-Info "Profile activated: $($profile.Name)"
        Start-Sleep -Milliseconds 500
    } catch {
        Write-Failure "Failed to activate profile: $($_.Exception.Message)"
        continue
    }

    # Test each endpoint
    foreach ($endpoint in $endpoints) {
        $totalTests++
        Write-Host ""
        Write-Host "  Endpoint: $($endpoint.Method) $($endpoint.Path)" -ForegroundColor Gray

        try {
            $testUrl = "$BaseUrl$($endpoint.Path)"
            $testParams = @{
                Uri = $testUrl
                Method = $endpoint.Method
                UseBasicParsing = $true
                TimeoutSec = 10
                WebSession = $session
            }

            if ($SkipCertificateCheck -and $PSVersionTable.PSVersion.Major -ge 6) {
                $testParams['SkipCertificateCheck'] = $true
            }

            if ($endpoint.Body) {
                $testParams['Body'] = $endpoint.Body
                $testParams['ContentType'] = 'application/json'
            }

            try {
                $response = Invoke-WebRequest @testParams -ErrorAction Stop
                $actualStatus = $response.StatusCode
            } catch {
                if ($_.Exception.Response.StatusCode.Value__ -eq 403) {
                    $actualStatus = 403
                } else {
                    throw
                }
            }

            $success = ($actualStatus -eq $profile.ExpectedStatus)

            if ($success) {
                Write-Success "$($endpoint.Name): $actualStatus (Expected: $($profile.ExpectedStatus))"
                $passedTests++
            } else {
                Write-Failure "$($endpoint.Name): $actualStatus (Expected: $($profile.ExpectedStatus))"
                $failedTests++
            }

            $results += @{
                Profile = $profile.Name
                Endpoint = "$($endpoint.Method) $($endpoint.Path)"
                Expected = $profile.ExpectedStatus
                Actual = $actualStatus
                Success = $success
            }

        } catch {
            Write-Failure "$($endpoint.Name): Error - $($_.Exception.Message)"
            $failedTests++
            $results += @{
                Profile = $profile.Name
                Endpoint = "$($endpoint.Method) $($endpoint.Path)"
                Expected = $profile.ExpectedStatus
                Actual = "Error"
                Success = $false
            }
        }
    }
}

# Reset identity
Write-SubHeader "Cleanup"
try {
    $resetUrl = "$BaseUrl/api/test-identity/reset"
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
    Write-Warning "Could not reset identity"
}

# Summary
Write-Header "Test Results Summary"

Write-Host ""
Write-Host "  Total Tests:  $totalTests" -ForegroundColor White
Write-Host "  Passed:       $passedTests" -ForegroundColor Green
Write-Host "  Failed:       $failedTests" -ForegroundColor $(if($failedTests -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($failedTests -eq 0) {
    Write-Host ("=" * 80) -ForegroundColor Green
    Write-Host "  ALL TESTS PASSED - LogViewerEndpoints Migration Successful!" -ForegroundColor Green
    Write-Host ("=" * 80) -ForegroundColor Green
    exit 0
} else {
    Write-Host ("=" * 80) -ForegroundColor Red
    Write-Host "  TESTS FAILED - Review errors above" -ForegroundColor Red
    Write-Host ("=" * 80) -ForegroundColor Red

    Write-Host ""
    Write-Host "Failed Tests:" -ForegroundColor Yellow
    foreach ($result in $results | Where-Object { -not $_.Success }) {
        Write-Host "  - $($result.Profile): $($result.Endpoint) (Expected: $($result.Expected), Got: $($result.Actual))" -ForegroundColor Red
    }

    exit 1
}
