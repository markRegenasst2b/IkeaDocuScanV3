param(
    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "https://localhost:44101"
)

# Force all TLS/SSL protocols
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12 -bor [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls -bor [System.Net.SecurityProtocolType]::Ssl3
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

Write-Host "Fetching DocumentAttachment template..." -ForegroundColor Cyan

$template = Invoke-RestMethod -Uri "$BaseUrl/api/configuration/email-templates/DocumentAttachment" -Method GET -UseDefaultCredentials

Write-Host ""
Write-Host "Template Name: $($template.templateName)" -ForegroundColor Yellow
Write-Host "Template Key: $($template.templateKey)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Subject:" -ForegroundColor Cyan
Write-Host "  $($template.subject)"
Write-Host ""
Write-Host "Placeholders in Subject:" -ForegroundColor Cyan
$subjectPlaceholders = [regex]::Matches($template.subject, '\{\{(\w+)\}\}') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
if ($subjectPlaceholders) {
    foreach ($p in $subjectPlaceholders) {
        Write-Host "  - {{$p}}" -ForegroundColor White
    }
} else {
    Write-Host "  No placeholders found" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Placeholders in HTML Body:" -ForegroundColor Cyan
$htmlPlaceholders = [regex]::Matches($template.htmlBody, '\{\{(\w+)\}\}') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
if ($htmlPlaceholders) {
    foreach ($p in $htmlPlaceholders) {
        Write-Host "  - {{$p}}" -ForegroundColor White
    }
} else {
    Write-Host "  No placeholders found" -ForegroundColor Red
}
Write-Host ""
Write-Host "Loop sections:" -ForegroundColor Cyan
$loopStarts = [regex]::Matches($template.htmlBody, '\{\{#(\w+)\}\}') | ForEach-Object { $_.Groups[1].Value }
if ($loopStarts) {
    foreach ($loop in $loopStarts) {
        Write-Host "  - {{#$loop}}...{{/$loop}}" -ForegroundColor White
    }
} else {
    Write-Host "  No loops found" -ForegroundColor Gray
}
Write-Host ""
