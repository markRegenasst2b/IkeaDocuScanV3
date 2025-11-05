param(
    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "https://localhost:44101"
)

Write-Host "Testing different connection methods..." -ForegroundColor Cyan
Write-Host ""

# Force TLS 1.2
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

$testUrl = "$BaseUrl/api/configuration/email-templates/diagnostic/DocumentAttachment"

Write-Host "Test URL: $testUrl" -ForegroundColor Yellow
Write-Host ""

# Test 1: Basic web request with no credentials
Write-Host "Test 1: Basic request (no credentials)..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri $testUrl -Method GET -UseBasicParsing
    Write-Host "  SUCCESS: Status $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "  Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
    }
}
Write-Host ""

# Test 2: With UseDefaultCredentials
Write-Host "Test 2: With UseDefaultCredentials..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri $testUrl -Method GET -UseDefaultCredentials -UseBasicParsing
    Write-Host "  SUCCESS: Status $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "  Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
    }
}
Write-Host ""

# Test 3: With explicit credentials
Write-Host "Test 3: With explicit current user credentials..." -ForegroundColor Cyan
try {
    $cred = [System.Net.CredentialCache]::DefaultNetworkCredentials
    $response = Invoke-WebRequest -Uri $testUrl -Method GET -Credential $cred -UseBasicParsing
    Write-Host "  SUCCESS: Status $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "  Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
    }
}
Write-Host ""

# Test 4: Using WebClient
Write-Host "Test 4: Using WebClient..." -ForegroundColor Cyan
try {
    $webClient = New-Object System.Net.WebClient
    $webClient.UseDefaultCredentials = $true
    $result = $webClient.DownloadString($testUrl)
    Write-Host "  SUCCESS: Downloaded $($result.Length) characters" -ForegroundColor Green
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.InnerException) {
        Write-Host "  Inner: $($_.Exception.InnerException.Message)" -ForegroundColor Yellow
    }
}
Write-Host ""

# Test 5: Check what protocols are enabled
Write-Host "Test 5: Current TLS protocols enabled..." -ForegroundColor Cyan
Write-Host "  SecurityProtocol: $([System.Net.ServicePointManager]::SecurityProtocol)" -ForegroundColor White
Write-Host ""

# Test 6: Try with explicit TLS protocols
Write-Host "Test 6: With all TLS protocols enabled..." -ForegroundColor Cyan
try {
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12 -bor [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls -bor [System.Net.SecurityProtocolType]::Ssl3

    $response = Invoke-RestMethod -Uri $testUrl -Method GET -UseDefaultCredentials
    Write-Host "  SUCCESS: Got response" -ForegroundColor Green
    Write-Host "  Summary: $($response.summary.recommendation)" -ForegroundColor White
} catch {
    Write-Host "  FAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.InnerException) {
        Write-Host "  Inner: $($_.Exception.InnerException.Message)" -ForegroundColor Yellow
    }
}
Write-Host ""

Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Gray
Write-Host ".NET Framework Version: $([System.Environment]::Version)" -ForegroundColor Gray
Write-Host ""
