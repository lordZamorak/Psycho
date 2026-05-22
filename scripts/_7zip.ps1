Set-StrictMode -Version Latest

function Resolve-7ZipPath {
    $commandNames = @("7z.exe", "7zz.exe", "7zr.exe")

    foreach ($name in $commandNames) {
        $command = Get-Command $name -ErrorAction SilentlyContinue
        if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
            return $command.Source
        }
    }

    $knownPaths = @(
        "C:\Program Files\7-Zip\7z.exe",
        "C:\Program Files (x86)\7-Zip\7z.exe"
    )

    foreach ($path in $knownPaths) {
        if (Test-Path -LiteralPath $path) {
            return $path
        }
    }

    throw "7-Zip was not found. Install it with: winget install --id 7zip.7zip -e"
}
