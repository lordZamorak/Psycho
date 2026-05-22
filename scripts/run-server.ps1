param(
    [string]$JdkPath = "$env:LOCALAPPDATA\Codex\jdks\jdk-12.0.2+10",
    [int]$Port = 13377,
    [string]$ConfigPath,
    [switch]$Restart,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$ServerDir = Join-Path $Root 'necrotic_server-item_attributes'
$LogDir = Join-Path $Root 'run-logs'
if (!$ConfigPath) {
    $ConfigPath = Join-Path $ServerDir 'data\config.yaml'
}

function Use-ProjectJdk {
    param([string]$Path)

    $java = Join-Path $Path 'bin\java.exe'
    if (!(Test-Path -LiteralPath $java)) {
        throw "JDK not found at '$Path'. Install or extract JDK 12, then rerun with -JdkPath."
    }

    $env:JAVA_HOME = $Path
    $env:Path = "$Path\bin;$env:Path"
}

function Get-ServerProcesses {
    Get-CimInstance Win32_Process -Filter "name='java.exe' or name='javaw.exe'" |
        Where-Object { $_.CommandLine -match [regex]::Escape($ServerDir) -and $_.CommandLine -match 'com\.ruse\.GameServer|GradleDaemon|gradle-launcher' }
}

Use-ProjectJdk -Path $JdkPath
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

$listeners = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Where-Object State -eq 'Listen'
if ($listeners -and !$Restart) {
    $listeners | Select-Object LocalAddress,LocalPort,State,OwningProcess
    if (!$DryRun) {
        throw "Port $Port is already listening. Use -Restart to stop this project's existing server first."
    }
    Write-Host "Dry run: port $Port is already listening; a real launch would require -Restart or a free port."
}

if ($Restart) {
    Get-ServerProcesses | ForEach-Object {
        Write-Host "Stopping server-related process $($_.ProcessId)"
        Stop-Process -Id $_.ProcessId -Force
    }
    Start-Sleep -Seconds 2
}

Write-Host "Preparing server distribution..."
Push-Location $ServerDir
try {
    & .\gradlew.bat --no-daemon --console=plain installDist
    if ($LASTEXITCODE -ne 0) { throw "installDist failed with exit code $LASTEXITCODE." }
} finally {
    Pop-Location
}

$runner = Get-ChildItem -LiteralPath (Join-Path $ServerDir 'build\install') -Recurse -Filter '*.bat' |
    Where-Object { $_.FullName -match '\\bin\\' } |
    Select-Object -First 1

if (!$runner) {
    throw "Could not find generated server runner under build\install."
}

$out = Join-Path $LogDir 'server.out.log'
$err = Join-Path $LogDir 'server.err.log'
$env:PSYCHO_CONFIG = $ConfigPath

if ($DryRun) {
    Write-Host "Dry run: server runner resolved to '$($runner.FullName)' in '$ServerDir'."
    Write-Host "Config: $ConfigPath"
    Write-Host "Logs: $out / $err"
    exit 0
}

Remove-Item -LiteralPath $out,$err -ErrorAction SilentlyContinue
$process = Start-Process -FilePath $runner.FullName -WorkingDirectory $ServerDir -RedirectStandardOutput $out -RedirectStandardError $err -WindowStyle Hidden -PassThru

Start-Sleep -Seconds 8
$listen = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Where-Object State -eq 'Listen'

$summary = [pscustomobject]@{
    ProcessId = $process.Id
    HasExited = $process.HasExited
    PortListening = [bool]$listen
    StdOut = $out
    StdErr = $err
}

$summary | Format-List | Out-String | Write-Host
