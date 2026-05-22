[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Archive,

    [Parameter(Mandatory = $true)]
    [string[]] $Path,

    [switch] $Overwrite
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\_7zip.ps1"

$archivePath = if ([System.IO.Path]::IsPathRooted($Archive)) {
    $Archive
} else {
    Join-Path (Get-Location).Path $Archive
}

$extension = [System.IO.Path]::GetExtension($archivePath).ToLowerInvariant()
$archiveType = switch ($extension) {
    ".zip" { "zip"; break }
    ".7z" { "7z"; break }
    ".rar" { throw "7-Zip can extract .rar files, but it cannot create .rar archives. Use .zip or .7z for new archives." }
    default { throw "Unsupported archive type '$extension'. Supported creation formats are: .zip, .7z." }
}

if ((Test-Path -LiteralPath $archivePath) -and -not $Overwrite) {
    throw "Archive already exists: $archivePath. Pass -Overwrite to replace it."
}

if ((Test-Path -LiteralPath $archivePath) -and $Overwrite) {
    Remove-Item -LiteralPath $archivePath -Force
}

$archiveDirectory = Split-Path -Parent $archivePath
if (-not [string]::IsNullOrWhiteSpace($archiveDirectory) -and -not (Test-Path -LiteralPath $archiveDirectory)) {
    New-Item -ItemType Directory -Path $archiveDirectory | Out-Null
}

$inputPaths = foreach ($entry in $Path) {
    (Resolve-Path -LiteralPath $entry).Path
}

$sevenZip = Resolve-7ZipPath
$arguments = @("a", "-t$archiveType", $archivePath) + $inputPaths

Write-Host "Creating $extension archive with 7-Zip: $archivePath"

& $sevenZip @arguments
if ($LASTEXITCODE -ne 0) {
    throw "7-Zip archive creation failed with exit code $LASTEXITCODE."
}

Write-Host "Archive created successfully."
