param(
    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "https://localhost:44101"
)

# Force all TLS/SSL protocols for compatibility (fixes "underlying connection was closed" error)
if ($PSVersionTable.PSVersion.Major -lt 6) {
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12 -bor [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls -bor [System.Net.SecurityProtocolType]::Ssl3
}

# Suppress SSL certificate validation for development
if ($PSVersionTable.PSVersion.Major -ge 6) {
    $params = @{
        UseDefaultCredentials = $true
        SkipCertificateCheck = $true
    }
} else {
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    $params = @{
        UseDefaultCredentials = $true
    }
}

$separator = "=" * 80

Write-Host ""
Write-Host $separator -ForegroundColor Cyan
Write-Host "DocumentAttachment Template Diagnostic Tool" -ForegroundColor Cyan
Write-Host $separator -ForegroundColor Cyan
Write-Host ""

try {
    Write-Host "Connecting to: $BaseUrl" -ForegroundColor Yellow
    Write-Host "Running comprehensive diagnostic..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "$BaseUrl/api/configuration/email-templates/diagnostic/DocumentAttachment"
    # Call the diagnostic endpoint
    $diagnostic = Invoke-RestMethod -Uri "$BaseUrl/api/configuration/email-templates/diagnostic/DocumentAttachment" -Method GET  @params

    # Display timestamp
    Write-Host "Diagnostic run at: $($diagnostic.diagnostic.timestamp)" -ForegroundColor Gray
    Write-Host ""

    # Display each check
    foreach ($check in $diagnostic.diagnostic.checks) {
        Write-Host "Check: $($check.checkName)" -ForegroundColor Cyan

        switch ($check.status) {
            "success" {
                Write-Host "  Status: SUCCESS" -ForegroundColor Green

                if ($null -ne $check.foundCount) {
                    Write-Host "  Found $($check.foundCount) template(s)" -ForegroundColor White
                }

                if ($null -ne $check.templates) {
                    foreach ($template in $check.templates) {
                        Write-Host "    - TemplateKey: $($template.templateKey) (Length: $($template.templateKeyLength))" -ForegroundColor White
                        Write-Host "      TemplateName: $($template.templateName)" -ForegroundColor White
                        Write-Host "      IsActive: $($template.isActive)" -ForegroundColor $(if ($template.isActive) { "Green" } else { "Red" })
                        Write-Host "      Category: $($template.category)" -ForegroundColor White
                        Write-Host "      Hex Bytes: $($template.templateKeyBytes -join '-')" -ForegroundColor Gray
                    }
                }

                if ($null -ne $check.template) {
                    Write-Host "    Template ID: $($check.template.templateId)" -ForegroundColor White
                    Write-Host "    Template Name: $($check.template.templateName)" -ForegroundColor White
                    Write-Host "    Is Active: $($check.template.isActive)" -ForegroundColor $(if ($check.template.isActive) { "Green" } else { "Red" })
                    Write-Host "    Category: $($check.template.category)" -ForegroundColor White
                    Write-Host "    Subject: $($check.template.subjectPreview)" -ForegroundColor White
                    Write-Host "    Created: $($check.template.createdDate) by $($check.template.createdBy)" -ForegroundColor Gray
                }

                if ($null -ne $check.found) {
                    Write-Host "    Found: $($check.found)" -ForegroundColor $(if ($check.found) { "Green" } else { "Red" })
                    if ($null -ne $check.templateId) {
                        Write-Host "    Template ID: $($check.templateId)" -ForegroundColor White
                    }
                    if ($null -ne $check.templateName) {
                        Write-Host "    Template Name: $($check.templateName)" -ForegroundColor White
                    }
                }
            }
            "not_found" {
                Write-Host "  Status: NOT FOUND" -ForegroundColor Red
            }
            "info" {
                Write-Host "  Status: INFO" -ForegroundColor Yellow
                Write-Host "    Expected Key: $($check.expectedKey)" -ForegroundColor White
                Write-Host "    Expected Length: $($check.expectedLength)" -ForegroundColor White
                Write-Host "    Expected Hex: $($check.expectedBytesAsString)" -ForegroundColor Gray
            }
            "error" {
                Write-Host "  Status: ERROR" -ForegroundColor Red
                Write-Host "    Message: $($check.message)" -ForegroundColor Red
            }
        }

        Write-Host ""
    }

    # Display summary
    Write-Host $separator -ForegroundColor Cyan
    Write-Host "SUMMARY" -ForegroundColor Cyan
    Write-Host $separator -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Templates in database: $($diagnostic.summary.templatesInDatabase)" -ForegroundColor White
    Write-Host "Exact match found: $($diagnostic.summary.exactMatchFound)" -ForegroundColor $(if ($diagnostic.summary.exactMatchFound) { "Green" } else { "Red" })
    Write-Host "Active match found: $($diagnostic.summary.activeMatchFound)" -ForegroundColor $(if ($diagnostic.summary.activeMatchFound) { "Green" } else { "Red" })
    Write-Host "Service retrieval successful: $($diagnostic.summary.serviceRetrievalSuccessful)" -ForegroundColor $(if ($diagnostic.summary.serviceRetrievalSuccessful) { "Green" } else { "Red" })
    Write-Host ""
    Write-Host "RECOMMENDATION:" -ForegroundColor Yellow
    Write-Host "  $($diagnostic.summary.recommendation)" -ForegroundColor White
    Write-Host ""

} catch {
    Write-Host "[ERROR] Failed to run diagnostic!" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible issues:" -ForegroundColor Yellow
    Write-Host "  1. Server not running at $BaseUrl" -ForegroundColor Gray
    Write-Host "  2. Authentication failed (need Windows auth or valid credentials)" -ForegroundColor Gray
    Write-Host "  3. User does not have SuperUser permission" -ForegroundColor Gray
    Write-Host "  4. Application not started or endpoint not available" -ForegroundColor Gray
    Write-Host ""
}

Write-Host ""
Write-Host $separator -ForegroundColor Cyan
Write-Host ""
