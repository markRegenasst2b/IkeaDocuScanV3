# Test-ConfigurationManager.ps1
# PowerShell script to manually test ConfigurationManagerService endpoints

param(
    [string]$BaseUrl = "https://localhost:44101",
    [switch]$SkipCertificateCheck,
    [switch]$UseDefaultCredentials = $true
)

# Setup
$ErrorActionPreference = "Stop"
Write-Host "=== IkeaDocuScan Configuration Manager Test Script ===" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host ""

# Skip certificate validation for self-signed certs in development
if ($SkipCertificateCheck) {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        # PowerShell Core
        $PSDefaultParameterValues['Invoke-RestMethod:SkipCertificateCheck'] = $true
        $PSDefaultParameterValues['Invoke-WebRequest:SkipCertificateCheck'] = $true
    } else {
        # Windows PowerShell
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
    }
    Write-Host "Certificate validation disabled for testing" -ForegroundColor Yellow
    Write-Host ""
}

# Helper function to make API calls
function Invoke-ConfigApi {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [string]$Description
    )

    Write-Host "Testing: $Description" -ForegroundColor Yellow
    Write-Host "  Endpoint: $Method $Endpoint" -ForegroundColor Gray

    try {
        $params = @{
            Uri = "$BaseUrl$Endpoint"
            Method = $Method
            UseDefaultCredentials = $UseDefaultCredentials
            ContentType = "application/json"
        }

        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            Write-Host "  Request Body:" -ForegroundColor Gray
            Write-Host ($params.Body | ConvertFrom-Json | ConvertTo-Json -Depth 10) -ForegroundColor DarkGray
        }

        $response = Invoke-RestMethod @params

        Write-Host "  Status: SUCCESS" -ForegroundColor Green
        Write-Host "  Response:" -ForegroundColor Gray
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor DarkGray
        Write-Host ""

        return $response
    }
    catch {
        Write-Host "  Status: FAILED" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "  Response Body: $responseBody" -ForegroundColor Red
        }
        Write-Host ""
        return $null
    }
}

# Test 1: Get Configuration Sections
Write-Host "=== Test 1: Get Configuration Sections ===" -ForegroundColor Cyan
Invoke-ConfigApi -Method "GET" -Endpoint "/api/configuration/sections" -Description "Retrieve all configuration sections"

# Test 2: Get Specific Configuration Key
Write-Host "=== Test 2: Get Specific Configuration Key ===" -ForegroundColor Cyan
Invoke-ConfigApi -Method "GET" -Endpoint "/api/configuration/Email/SmtpHost" -Description "Get SMTP Host configuration"

# Test 3: Get Email Recipients
Write-Host "=== Test 3: Get Email Recipients ===" -ForegroundColor Cyan
Invoke-ConfigApi -Method "GET" -Endpoint "/api/configuration/email-recipients" -Description "Retrieve all email recipient groups"

# Test 4: Get Email Templates
Write-Host "=== Test 4: Get Email Templates ===" -ForegroundColor Cyan
$templates = Invoke-ConfigApi -Method "GET" -Endpoint "/api/configuration/email-templates" -Description "Retrieve all email templates"

# Test 5: Test SMTP Connection
Write-Host "=== Test 5: Test SMTP Connection ===" -ForegroundColor Cyan
Write-Host "NOTE: This will test using current SMTP settings in the database" -ForegroundColor Yellow
$testSmtp = Invoke-ConfigApi -Method "POST" -Endpoint "/api/configuration/test-smtp" -Description "Test SMTP connection"

if ($testSmtp -and $testSmtp.success -eq $false) {
    Write-Host "SMTP test failed as expected if not configured yet" -ForegroundColor Yellow
}

# Test 6: Update SMTP Configuration (dry run - commented out by default)
Write-Host "=== Test 6: Update SMTP Configuration (COMMENTED OUT) ===" -ForegroundColor Cyan
Write-Host "Uncomment this section to actually update SMTP settings" -ForegroundColor Yellow
Write-Host "Note: All SMTP settings are updated atomically and tested together" -ForegroundColor Yellow
<#
# Bulk SMTP update - all settings saved and tested together
$smtpConfig = @{
    smtpHost = "smtp.office365.com"
    smtpPort = 587
    useSsl = $true
    smtpUsername = "noreply@company.com"
    smtpPassword = "YourPasswordHere"
    fromAddress = "noreply@company.com"
    fromName = "IkeaDocuScan System"
}

Invoke-ConfigApi -Method "POST" -Endpoint "/api/configuration/smtp" -Body $smtpConfig -Description "Update all SMTP settings (with automatic test)"

# IMPORTANT: If the SMTP test fails, ALL changes are rolled back automatically
# This ensures you never have an inconsistent SMTP configuration
#>
Write-Host ""

# Test 7: Update Email Recipient Group (dry run - commented out by default)
Write-Host "=== Test 7: Update Email Recipient Group (COMMENTED OUT) ===" -ForegroundColor Cyan
Write-Host "Uncomment to update a recipient group" -ForegroundColor Yellow
<#
# Note: Use POST /api/configuration/email-recipients/{groupKey} to update recipients
$recipientRequest = @{
    emailAddresses = @(
        "test1@company.com",
        "test2@company.com"
    )
    reason = "Testing recipient group update"
}

Invoke-ConfigApi -Method "POST" -Endpoint "/api/configuration/email-recipients/TestGroup" -Body $recipientRequest -Description "Update TestGroup email recipients"
#>
Write-Host ""

# Test 8: Run Configuration Migration
Write-Host "=== Test 8: Run Configuration Migration ===" -ForegroundColor Cyan
Write-Host "This will migrate settings from appsettings.json to database" -ForegroundColor Yellow
Write-Host "Do you want to run the migration? (Y/N)" -ForegroundColor Yellow
$confirm = Read-Host

if ($confirm -eq "Y" -or $confirm -eq "y") {
    $migrationRequest = @{
        overwriteExisting = $false
    }

    Invoke-ConfigApi -Method "POST" -Endpoint "/api/configuration/migrate" -Body $migrationRequest -Description "Migrate configuration to database"
} else {
    Write-Host "Migration skipped" -ForegroundColor Gray
    Write-Host ""
}

# Test 9: Reload Configuration Cache
Write-Host "=== Test 9: Reload Configuration Cache ===" -ForegroundColor Cyan
Invoke-ConfigApi -Method "POST" -Endpoint "/api/configuration/reload" -Description "Reload configuration cache"

# Test 10: Create Email Template (dry run - commented out)
Write-Host "=== Test 10: Create Email Template (COMMENTED OUT) ===" -ForegroundColor Cyan
Write-Host "Uncomment to create a test email template" -ForegroundColor Yellow
<#
$emailTemplate = @{
    templateName = "Test Template"
    templateKey = "TestTemplate"
    subject = "Test Email: {{Username}}"
    htmlBody = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .header { background-color: #0066cc; color: white; padding: 20px; }
        .content { padding: 20px; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Test Email</h1>
    </div>
    <div class="content">
        <p>Hello {{Username}},</p>
        <p>This is a test email sent on {{Date}}.</p>
        <p>Best regards,<br>IkeaDocuScan System</p>
    </div>
</body>
</html>
"@
    plainTextBody = "Hello {{Username}}, This is a test email sent on {{Date}}."
    category = "System"
    isActive = $true
    isDefault = $false
}

Invoke-ConfigApi -Method "POST" -Endpoint "/api/configuration/email-templates" -Body $emailTemplate -Description "Create email template"
#>
Write-Host ""

# Test 11: Get Available Placeholders
Write-Host "=== Test 11: Get Available Placeholders ===" -ForegroundColor Cyan
Invoke-ConfigApi -Method "GET" -Endpoint "/api/configuration/email-templates/placeholders" -Description "Get available placeholders and loops for templates"

# Test 12: Get Specific Email Template by Key (if any exist)
if ($templates -and $templates.Count -gt 0) {
    Write-Host "=== Test 12: Get Specific Email Template by Key ===" -ForegroundColor Cyan
    $templateKey = $templates[0].templateKey
    if ($templateKey) {
        Invoke-ConfigApi -Method "GET" -Endpoint "/api/configuration/email-templates/$templateKey" -Description "Get template by key: $templateKey"
    }
}

# Summary
Write-Host ""
Write-Host "=== Test Summary ===" -ForegroundColor Cyan
Write-Host "All tests completed. Review the output above for results." -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Review any failed tests and check server logs" -ForegroundColor Gray
Write-Host "2. Uncomment sections to test configuration updates (Test 6, 7, 10)" -ForegroundColor Gray
Write-Host "3. Navigate to https://localhost:44101/configuration-management to test UI" -ForegroundColor Gray
Write-Host "4. Run Verify-ConfigurationDatabase.sql to check database state" -ForegroundColor Gray
Write-Host "5. Check database tables:" -ForegroundColor Gray
Write-Host "   - SystemConfigurations (Email settings)" -ForegroundColor DarkGray
Write-Host "   - SystemConfigurationAudits (Change history)" -ForegroundColor DarkGray
Write-Host "   - EmailTemplates (5 default templates)" -ForegroundColor DarkGray
Write-Host "   - EmailRecipientGroups (AdminNotifications, AccessRequests)" -ForegroundColor DarkGray
Write-Host "   - EmailRecipients (Recipients by group)" -ForegroundColor DarkGray
Write-Host ""
