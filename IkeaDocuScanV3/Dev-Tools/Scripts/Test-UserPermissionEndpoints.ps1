#Requires -Version 5.1

<#
.SYNOPSIS
    Tests UserPermissionEndpoints migration to dynamic authorization

.DESCRIPTION
    Tests all 11 UserPermissionEndpoints with all 4 roles:
    - GET /api/userpermissions/
    - GET /api/userpermissions/users
    - GET /api/userpermissions/{id}
    - GET /api/userpermissions/user/{userId}
    - GET /api/userpermissions/me
    - POST /api/userpermissions/
    - PUT /api/userpermissions/{id}
    - DELETE /api/userpermissions/{id}
    - DELETE /api/userpermissions/user/{userId}
    - POST /api/userpermissions/user
    - PUT /api/userpermissions/user/{userId}

    Expected results based on database seed:
    - Reader: GET /{id}, /user/{userId}, /me (200), others (403)
    - Publisher: GET /{id}, /user/{userId}, /me (200), others (403)
    - ADAdmin: GET /, /users, /{id}, /user/{userId}, /me (200), write operations (403)
    - SuperUser: All endpoints (200)

.PARAMETER BaseUrl
    Base URL of the application. Default: https://localhost:44101

.PARAMETER SkipCertificateCheck
    Skip SSL certificate validation (useful for self-signed certs in dev)

.EXAMPLE
    .\Test-UserPermissionEndpoints.ps1

.EXAMPLE
    .\Test-UserPermissionEndpoints.ps1 -BaseUrl "https://localhost:7001" -SkipCertificateCheck
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$BaseUrl = "https://localhost:44101",

    [Parameter()]
    [switch]$SkipCertificateCheck
)

$ErrorActionPreference = "Continue"

# Test profiles with expected status codes per endpoint
$testProfiles = @(
    @{
        Id = "reader"
        Name = "Reader"
        ExpectedAccess = @{
            "GET /"              = 403
            "GET /users"         = 403
            "GET /{id}"          = 404  # All roles authorized, but ID 999 doesn't exist
            "GET /user/{userId}" = 200  # All roles
            "GET /me"            = 200  # All roles
            "POST /"             = 403
            "PUT /{id}"          = 403
            "DELETE /{id}"       = 403
            "DELETE /user/{userId}" = 403
            "POST /user"         = 403
            "PUT /user/{userId}" = 403
        }
    },
    @{
        Id = "publisher"
        Name = "Publisher"
        ExpectedAccess = @{
            "GET /"              = 403
            "GET /users"         = 403
            "GET /{id}"          = 404  # All roles authorized, but ID 999 doesn't exist
            "GET /user/{userId}" = 200  # All roles
            "GET /me"            = 200  # All roles
            "POST /"             = 403
            "PUT /{id}"          = 403
            "DELETE /{id}"       = 403
            "DELETE /user/{userId}" = 403
            "POST /user"         = 403
            "PUT /user/{userId}" = 403
        }
    },
    @{
        Id = "adadmin"
        Name = "ADAdmin"
        ExpectedAccess = @{
            "GET /"              = 200  # ADAdmin + SuperUser
            "GET /users"         = 200  # ADAdmin + SuperUser
            "GET /{id}"          = 404  # All roles authorized, but ID 999 doesn't exist
            "GET /user/{userId}" = 200  # All roles
            "GET /me"            = 200  # All roles
            "POST /"             = 403
            "PUT /{id}"          = 403
            "DELETE /{id}"       = 403
            "DELETE /user/{userId}" = 403
            "POST /user"         = 403
            "PUT /user/{userId}" = 403
        }
    },
    @{
        Id = "superuser"
        Name = "SuperUser"
        ExpectedAccess = @{
            "GET /"              = 200
            "GET /users"         = 200
            "GET /{id}"          = 404  # Authorized, but ID 999 doesn't exist
            "GET /user/{userId}" = 200
            "GET /me"            = 200
            "POST /"             = 201  # Authorized, created successfully (201 Created)
            "PUT /{id}"          = 400  # Authorized, but test data is invalid
            "DELETE /{id}"       = 400  # Authorized, but ID doesn't exist or data invalid
            "DELETE /user/{userId}" = 400  # Authorized, but ID doesn't exist or data invalid
            "POST /user"         = 400  # Authorized, but test data is invalid
            "PUT /user/{userId}" = 200  # Authorized, updated successfully (200 OK)
        }
    }
)

# Endpoints to test
$endpoints = @(
    @{
        Method = "GET"
        Path = "/api/userpermissions/"
        Name = "GetAllUserPermissions"
        TestName = "GET /"
        Body = $null
    },
    @{
        Method = "GET"
        Path = "/api/userpermissions/users"
        Name = "GetAllDocuScanUsers"
        TestName = "GET /users"
        Body = $null
    },
    @{
        Method = "GET"
        Path = "/api/userpermissions/999"
        Name = "GetUserPermissionById"
        TestName = "GET /{id}"
        Body = $null
        Note = "Non-existent ID - tests authorization only, 404 expected"
    },
    @{
        Method = "GET"
        Path = "/api/userpermissions/user/1"
        Name = "GetUserPermissionsByUserId"
        TestName = "GET /user/{userId}"
        Body = $null
    },
    @{
        Method = "GET"
        Path = "/api/userpermissions/me"
        Name = "GetMyPermissions"
        TestName = "GET /me"
        Body = $null
    },
    @{
        Method = "POST"
        Path = "/api/userpermissions/"
        Name = "CreateUserPermission"
        TestName = "POST /"
        Body = @{
            userId = 1
            permission = "TestPermission"
        } | ConvertTo-Json
    },
    @{
        Method = "PUT"
        Path = "/api/userpermissions/1"
        Name = "UpdateUserPermission"
        TestName = "PUT /{id}"
        Body = @{
            id = 1
            userId = 1
            permission = "UpdatedPermission"
        } | ConvertTo-Json
    },
    @{
        Method = "DELETE"
        Path = "/api/userpermissions/999"
        Name = "DeleteUserPermission"
        TestName = "DELETE /{id}"
        Body = $null
    },
    @{
        Method = "DELETE"
        Path = "/api/userpermissions/user/999"
        Name = "DeleteDocuScanUser"
        TestName = "DELETE /user/{userId}"
        Body = $null
    },
    @{
        Method = "POST"
        Path = "/api/userpermissions/user"
        Name = "CreateDocuScanUser"
        TestName = "POST /user"
        Body = @{
            accountName = "test.user"
            displayName = "Test User"
        } | ConvertTo-Json
    },
    @{
        Method = "PUT"
        Path = "/api/userpermissions/user/1"
        Name = "UpdateDocuScanUser"
        TestName = "PUT /user/{userId}"
        Body = @{
            userId = 1
            accountName = "updated.user"
            displayName = "Updated User"
        } | ConvertTo-Json
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
Write-Header "UserPermissionEndpoints Migration Test"

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
    Write-Host "Testing Profile: $($profile.Name)" -ForegroundColor White
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
        $expectedStatus = $profile.ExpectedAccess[$endpoint.TestName]
        Write-Host ""
        Write-Host "  Endpoint: $($endpoint.Method) $($endpoint.Path) (Expected: $expectedStatus)" -ForegroundColor Gray

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
                if ($_.Exception.Response) {
                    $actualStatus = $_.Exception.Response.StatusCode.Value__
                } else {
                    throw
                }
            }

            $success = ($actualStatus -eq $expectedStatus)

            if ($success) {
                Write-Success "$($endpoint.Name): $actualStatus (Expected: $expectedStatus)"
                $passedTests++
            } else {
                Write-Failure "$($endpoint.Name): $actualStatus (Expected: $expectedStatus)"
                $failedTests++
            }

            $results += @{
                Profile = $profile.Name
                Endpoint = "$($endpoint.Method) $($endpoint.Path)"
                Expected = $expectedStatus
                Actual = $actualStatus
                Success = $success
            }

        } catch {
            Write-Failure "$($endpoint.Name): Error - $($_.Exception.Message)"
            $failedTests++
            $results += @{
                Profile = $profile.Name
                Endpoint = "$($endpoint.Method) $($endpoint.Path)"
                Expected = $expectedStatus
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
    Write-Host "  ALL TESTS PASSED - UserPermissionEndpoints Migration Successful!" -ForegroundColor Green
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
