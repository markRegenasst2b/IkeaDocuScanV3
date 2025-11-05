<#
.SYNOPSIS
    Retrieves the list of available countries from the IkeaDocuScan system.

.DESCRIPTION
    Queries the /api/countries endpoint to get all valid country codes
    that can be used when creating or updating counter parties.

.PARAMETER BaseUrl
    The base URL of the IkeaDocuScan API. Defaults to https://localhost:44101

.PARAMETER Format
    Output format: 'Table' (default), 'List', or 'Codes'
    - Table: Formatted table with CountryCode and Name
    - List: Simple list format
    - Codes: Only country codes (for scripting)

.EXAMPLE
    .\Get-AvailableCountries.ps1
    Retrieves all countries and displays them in a table

.EXAMPLE
    .\Get-AvailableCountries.ps1 -Format Codes
    Retrieves only the country codes

.EXAMPLE
    .\Get-AvailableCountries.ps1 -BaseUrl "https://docuscan.company.com"
    Retrieves countries from a specific server
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "https://localhost:44101",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Table", "List", "Codes")]
    [string]$Format = "Table"
)

# Suppress SSL certificate validation for development
if ($PSVersionTable.PSVersion.Major -ge 6) {
    # PowerShell Core
    $params = @{
        Uri                     = "$BaseUrl/api/countries"
        Method                  = "GET"
        UseDefaultCredentials   = $true
        SkipCertificateCheck    = $true
    }
} else {
    # Windows PowerShell
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    $params = @{
        Uri                     = "$BaseUrl/api/countries"
        Method                  = "GET"
        UseDefaultCredentials   = $true
    }
}

try {
    Write-Host "Retrieving countries from: $BaseUrl/api/countries" -ForegroundColor Cyan

    $countries = Invoke-RestMethod @params

    if ($null -eq $countries -or $countries.Count -eq 0) {
        Write-Warning "No countries found in the system."
        Write-Host "Please ensure the Country table is populated with reference data."
        exit 1
    }

    Write-Host "`nFound $($countries.Count) countries:" -ForegroundColor Green
    Write-Host ""

    switch ($Format) {
        "Table" {
            # Display as formatted table
            $countries | Select-Object @{
                Name = "Country Code"
                Expression = { $_.countryCode }
            }, @{
                Name = "Country Name"
                Expression = { $_.name }
            } | Format-Table -AutoSize
        }

        "List" {
            # Display as simple list
            $countries | ForEach-Object {
                Write-Host "$($_.countryCode): $($_.name)"
            }
        }

        "Codes" {
            # Display only codes (useful for scripting)
            $countries | ForEach-Object {
                Write-Host $_.countryCode
            }
        }
    }

    Write-Host "`nTotal: $($countries.Count) countries" -ForegroundColor Cyan

} catch {
    Write-Error "Failed to retrieve countries: $_"
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "  1. Server not running or incorrect URL"
    Write-Host "  2. Authentication failed (Windows credentials required)"
    Write-Host "  3. User doesn't have 'HasAccess' permission"
    Write-Host "  4. API endpoint not available"
    Write-Host ""
    Write-Host "Stack trace:" -ForegroundColor Gray
    Write-Host $_.Exception.ToString() -ForegroundColor Gray
    exit 1
}
