param(
    [string]$JdkPath = "$env:LOCALAPPDATA\Codex\jdks\jdk-12.0.2+10",
    [switch]$Unobfuscated,
    [switch]$SyncCache,
    [switch]$DiscordRpc,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$ClientDir = Join-Path $Root 'necrotic_client-item_attributes'
$LogDir = Join-Path $Root 'run-logs'
$ProjectCache = Join-Path $ClientDir 'cache'
$UserCache = Join-Path $env:USERPROFILE 'PsychoCache'

function Use-ProjectJdk {
    param([string]$Path)

    $java = Join-Path $Path 'bin\java.exe'
    if (!(Test-Path -LiteralPath $java)) {
        throw "JDK not found at '$Path'. Install or extract JDK 12, then rerun with -JdkPath."
    }

    $env:JAVA_HOME = $Path
    $env:Path = "$Path\bin;$env:Path"
}

Use-ProjectJdk -Path $JdkPath
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

if ($SyncCache -or !(Test-Path -LiteralPath (Join-Path $UserCache 'main_file_cache.dat'))) {
    Write-Host "Syncing client cache to $UserCache"
    New-Item -ItemType Directory -Force -Path $UserCache | Out-Null
    Copy-Item -Path (Join-Path $ProjectCache '*') -Destination $UserCache -Recurse -Force
}

Push-Location $ClientDir
try {
    & .\gradlew.bat --no-daemon --console=plain buildjar
    if ($LASTEXITCODE -ne 0) { throw "Client build failed with exit code $LASTEXITCODE." }
} finally {
    Pop-Location
}

$jarName = if ($Unobfuscated) { 'Psycho-gamepack-unobfuscated.jar' } else { 'Psycho-gamepack.jar' }
$jar = Join-Path $ClientDir "build\libs\$jarName"
$arguments = @('-jar', $jar)
if ($DiscordRpc) {
    $arguments += '--discord-rpc'
}

if (!(Test-Path -LiteralPath $jar)) {
    throw "Client jar not found: $jar"
}

$out = Join-Path $LogDir 'client.out.log'
$err = Join-Path $LogDir 'client.err.log'

if ($DryRun) {
    Write-Host "Dry run: would start '$jar' in '$ClientDir' with arguments: $($arguments -join ' ')"
    Write-Host "Logs: $out / $err"
    exit 0
}

Remove-Item -LiteralPath $out,$err -ErrorAction SilentlyContinue
$process = Start-Process -FilePath (Join-Path $env:JAVA_HOME 'bin\java.exe') -ArgumentList $arguments -WorkingDirectory $ClientDir -RedirectStandardOutput $out -RedirectStandardError $err -PassThru

Start-Sleep -Seconds 8
$summary = [pscustomobject]@{
    ProcessId = $process.Id
    HasExited = $process.HasExited
    StdOut = $out
    StdErr = $err
    Jar = $jar
}

$summary | Format-List | Out-String | Write-Host
