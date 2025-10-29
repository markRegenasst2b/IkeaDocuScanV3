<#
.SYNOPSIS
Creates multiple copies of a source PDF file in a specified folder,
renaming them with an incrementing integer basename.

.PARAMETER DestinationFolder
The path to the folder where the new PDF files will be created.
The folder must exist.

.PARAMETER SourcePdfPath
The full path to the source PDF file to be duplicated.

.PARAMETER InitialInteger
The starting integer to use for the basename of the first PDF copy.
The file extension '.pdf' will be appended automatically.

.PARAMETER Count
The total number of PDF files (including the first one) to create.

.EXAMPLE
.\Create-NumberedPdfs.ps1 -DestinationFolder "C:\Reports\Monthly" `
                         -SourcePdfPath "C:\Templates\BlankReport.pdf" `
                         -InitialInteger 100 `
                         -Count 5

This will create:
C:\Reports\Monthly\100.pdf
C:\Reports\Monthly\101.pdf
C:\Reports\Monthly\102.pdf
C:\Reports\Monthly\103.pdf
C:\Reports\Monthly\104.pdf
#>
param(
    [Parameter(Mandatory=$true)]
    [string]$DestinationFolder,

    [Parameter(Mandatory=$true)]
    [string]$SourcePdfPath,

    [Parameter(Mandatory=$true)]
    [int]$InitialInteger,

    [Parameter(Mandatory=$true)]
    [int]$Count
)

# --- Input Validation ---
if (-not (Test-Path -Path $DestinationFolder -PathType Container)) {
    Write-Error "The destination folder '$DestinationFolder' does not exist."
    exit 1
}

if (-not (Test-Path -Path $SourcePdfPath -PathType Leaf)) {
    Write-Error "The source PDF file '$SourcePdfPath' does not exist."
    exit 1
}

if ([System.IO.Path]::GetExtension($SourcePdfPath) -ne ".pdf") {
    Write-Warning "The source file '$SourcePdfPath' does not have a '.pdf' extension. Proceeding anyway."
}

if ($Count -le 0) {
    Write-Error "The count must be a positive integer."
    exit 1
}
# --- End of Input Validation ---

Write-Host "Starting to create $Count PDF copies in '$DestinationFolder'..."

# Loop from 0 up to (Count - 1)
for ($i = 0; $i -lt $Count; $i++) {
    # Calculate the current integer for the filename
    $currentInteger = $InitialInteger + $i

    # Construct the new filename path
    $newFileName = "$currentInteger.pdf"
    $destinationPath = Join-Path -Path $DestinationFolder -ChildPath $newFileName

    try {
        # Copy the file
        Copy-Item -Path $SourcePdfPath -Destination $destinationPath -Force

        Write-Host "Created: $newFileName" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to create file for integer $currentInteger. Error: $($_.Exception.Message)"
        # You might want to 'break' here if a failure is critical,
        # otherwise, the script will try to continue with the next file.
    }
}

Write-Host "---"
Write-Host "Operation complete. $Count files were processed starting from integer $InitialInteger."