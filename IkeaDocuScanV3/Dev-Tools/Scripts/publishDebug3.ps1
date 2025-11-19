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