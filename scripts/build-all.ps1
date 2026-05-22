param(
    [string]$JdkPath = "$env:LOCALAPPDATA\Codex\jdks\jdk-12.0.2+10"
)

$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$ServerDir = Join-Path $Root 'necrotic_server-item_attributes'
$ClientDir = Join-Path $Root 'necrotic_client-item_attributes'

function Use-ProjectJdk {
    param([string]$Path)

    $java = Join-Path $Path 'bin\java.exe'
    $javac = Join-Path $Path 'bin\javac.exe'

    if (!(Test-Path -LiteralPath $java) -or !(Test-Path -LiteralPath $javac)) {
        throw "JDK not found at '$Path'. Install or extract JDK 12, then rerun with -JdkPath."
    }

    $env:JAVA_HOME = $Path
    $env:Path = "$Path\bin;$env:Path"
}

Use-ProjectJdk -Path $JdkPath

Write-Host "Using JAVA_HOME=$env:JAVA_HOME"
& (Join-Path $env:JAVA_HOME 'bin\java.exe') -version

Write-Host "`nBuilding server..."
Push-Location $ServerDir
try {
    & .\gradlew.bat --no-daemon --console=plain build
    if ($LASTEXITCODE -ne 0) { throw "Server build failed with exit code $LASTEXITCODE." }
} finally {
    Pop-Location
}

Write-Host "`nBuilding client..."
Push-Location $ClientDir
try {
    & .\gradlew.bat --no-daemon --console=plain buildjar
    if ($LASTEXITCODE -ne 0) { throw "Client build failed with exit code $LASTEXITCODE." }
} finally {
    Pop-Location
}

Write-Host "`nBuilds completed successfully."
