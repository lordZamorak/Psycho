[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Archive,

    [string] $Destination,

    [switch] $Overwrite
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\_7zip.ps1"

$archivePath = (Resolve-Path -LiteralPath $Archive).Path
$extension = [System.IO.Path]::GetExtension($archivePath).ToLowerInvariant()
$supportedExtensions = @(".zip", ".rar", ".7z")

if ($supportedExtensions -notcontains $extension) {
    throw "Unsupported archive type '$extension'. Supported extraction formats are: $($supportedExtensions -join ', ')."
}

if ([string]::IsNullOrWhiteSpace($Destination)) {
    $Destination = Join-Path (Split-Path -Parent $archivePath) ([System.IO.Path]::GetFileNameWithoutExtension($archivePath))
}

if (-not (Test-Path -LiteralPath $Destination)) {
    New-Item -ItemType Directory -Path $Destination | Out-Null
}

$destinationPath = (Resolve-Path -LiteralPath $Destination).Path
$sevenZip = Resolve-7ZipPath
$overwriteMode = if ($Overwrite) { "-aoa" } else { "-aos" }
$arguments = @("x", $archivePath, "-o$destinationPath", $overwriteMode, "-y")

Write-Host "Extracting with 7-Zip: $archivePath"
Write-Host "Destination: $destinationPath"

& $sevenZip @arguments
if ($LASTEXITCODE -ne 0) {
    throw "7-Zip extraction failed with exit code $LASTEXITCODE."
}

Write-Host "Archive extracted successfully."
