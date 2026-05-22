param(
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

$Root = Split-Path -Parent $PSScriptRoot
$ServerDir = Join-Path $Root 'necrotic_server-item_attributes'
$ClientDir = Join-Path $Root 'necrotic_client-item_attributes'

$processes = Get-CimInstance Win32_Process -Filter "name='java.exe' or name='javaw.exe'" |
    Where-Object {
        ($_.CommandLine -match [regex]::Escape($ServerDir) -and $_.CommandLine -match 'com\.ruse\.GameServer|GradleDaemon|gradle-launcher') -or
        ($_.CommandLine -match [regex]::Escape($ClientDir) -and $_.CommandLine -match 'Psycho-gamepack|org\.necrotic\.client')
    }

if (!$processes) {
    Write-Host "No Psycho server/client Java processes found."
    exit 0
}

$processes | Select-Object ProcessId,Name,CommandLine

if ($DryRun) {
    Write-Host "Dry run: would stop the processes listed above."
    exit 0
}

$processes | ForEach-Object {
    Write-Host "Stopping process $($_.ProcessId)"
    Stop-Process -Id $_.ProcessId -Force
}
