# ============================================================================
# Configuration
# ============================================================================

$ErrorActionPreference = "Stop"
$sourceFolder = "C:\Users\markr\source\repos\markRegenasst2b\IkeaDocuScan-V3"
$publishFolder = "d:\Pub"
$projectPath = Join-Path $sourceFolder "IkeaDocuScanV3\IkeaDocuScan-Web\IkeaDocuScan-Web"
$csprojFile = Join-Path $projectPath "IkeaDocuScan-Web.csproj"
$outputZip = "d:\IkeaDocuScan-Debug-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip"

# ============================================================================

function Write-Step {
    param([string]$Message)
    Write-Host "`n[$(Get-Date -Format 'HH:mm:ss')] $Message" -ForegroundColor Cyan
}
function Write-Success {
    param([string]$Message)
    Write-Host "$Message" -ForegroundColor Green
}
function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "$Message" -ForegroundColor Red
}
function Remove-PublishFolder {
    Write-Step "Cleaning publish folder..."

    if (Test-Path $publishFolder) {
        try {
            Remove-Item $publishFolder -Recurse -Force
            Write-Success "Deleted $publishFolder"
        }
        catch {
            Write-ErrorMessage "Failed to delete $publishFolder. $($_.Exception.Message)"
            throw
        }
    }
    else {
        Write-Success "Publish folder does not exist (nothing to clean)"
    }
}
function Test-GitPendingChanges {
    Write-Step "Checking for pending git changes..."

    Push-Location $sourceFolder
    try {
        # Check if we're in a git repository
        $gitStatus = git status --porcelain 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Not a git repository or git not available. Skipping git check."
            return
        }

        if ($gitStatus) {
            Write-ErrorMessage "Pending git changes detected:"
            Write-Host $gitStatus -ForegroundColor Yellow
            Write-Host "`nPlease commit or stash your changes before publishing." -ForegroundColor Yellow
            throw "Cannot publish with pending git changes"
        }

        Write-Success "No pending git changes"
    }
    finally {
        Pop-Location
    }
}
function Update-ProjectVersion {
    Write-Step "Updating project version..."

    if (-not (Test-Path $csprojFile)) {
        throw "Project file not found: $csprojFile"
    }

    # FIX 1: Load XML using explicit UTF8 encoding to prevent parsing/saving errors.
    [xml]$csproj = Get-Content $csprojFile -Encoding UTF8

    # Find PropertyGroup with VersionPrefix
    $propertyGroup = $csproj.Project.PropertyGroup | Where-Object { $_.VersionPrefix -ne $null } | Select-Object -First 1

    if (-not $propertyGroup) {
        throw "Could not find VersionPrefix in project file"
    }

    # Parse current version
    $currentVersion = $propertyGroup.VersionPrefix
    Write-Host "  Current version: $currentVersion" -ForegroundColor Gray

    # Split version and increment last component
    $versionParts = $currentVersion -split '\.'
    if ($versionParts.Count -lt 3) {
        throw "Invalid version format: $currentVersion (expected X.Y.Z)"
    }

    $versionParts[-1] = [int]$versionParts[-1] + 1
    $newVersion = $versionParts -join '.'

    # Update VersionPrefix
    $propertyGroup.VersionPrefix = $newVersion

    # Update VersionSuffix with current date
    $dateSuffix = Get-Date -Format "MMdd"
    $newVersionSuffix = "testdeploy$dateSuffix"

    if ($propertyGroup.VersionSuffix -eq $null) {
        # Create VersionSuffix element if it doesn't exist
        $versionSuffixElement = $csproj.CreateElement("VersionSuffix")
        $versionSuffixElement.InnerText = $newVersionSuffix
        $propertyGroup.AppendChild($versionSuffixElement) | Out-Null
    }
    else {
        $propertyGroup.VersionSuffix = $newVersionSuffix
    }

    # Save XML (This saves it with the encoding it was read with, which is now UTF8)
    $csproj.Save($csprojFile)

    Write-Success "Updated version: $newVersion-$newVersionSuffix"

    return "$newVersion-$newVersionSuffix"
}

try {
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host "           IkeaDocuScan Debug Publish Script                       " -ForegroundColor Cyan
    Write-Host "============================================================================" -ForegroundColor Cyan

    # Step 1: Check for pending git changes
    Test-GitPendingChanges

    # Step 2: Remove existing publish folder
    Remove-PublishFolder

    # Step 3: Update version in .csproj
    $newVersion = Update-ProjectVersion

    # Step 4: Clean and publish
    Invoke-DotNetPublish

    # Step 5: Create zip archive
    New-PublishZip -Version $newVersion

    # Show summary
    Show-Summary -Version $newVersion

    Write-Host " Publish completed successfully!" -ForegroundColor Green

    exit 0
}
catch {
    # Added .Exception.Message for clearer error reporting
    $msg = "Publish failed:  $_"
    Write-Host $msg -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
}