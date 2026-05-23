# Psycho Local Scripts

Run all commands from PowerShell.

## Build both projects

```powershell
.\scripts\build-all.ps1
```

## Start server

```powershell
.\scripts\run-server.ps1
```

If a previous local server is already running:

```powershell
.\scripts\run-server.ps1 -Restart
```

To launch with an explicit server config:

```powershell
.\scripts\run-server.ps1 -ConfigPath "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\necrotic_server-item_attributes\data\config.yaml"
```

## Start client

```powershell
.\scripts\run-client.ps1
```

To refresh `C:\Users\xzero\PsychoCache` from the bundled client cache:

```powershell
.\scripts\run-client.ps1 -SyncCache
```

The client uses the local bundled cache by default through this sync path. Remote cache downloading is disabled in source until a valid hosted cache zip and version file are configured.

Discord rich presence is disabled by default for local test stability. To enable it explicitly:

```powershell
.\scripts\run-client.ps1 -DiscordRpc
```

## Stop local game processes

```powershell
.\scripts\stop-game.ps1
```

## Archive files with 7-Zip

Use these helpers for `.zip`, `.rar`, and `.7z` work. They explicitly use 7-Zip from PATH or `C:\Program Files\7-Zip\7z.exe`; do not use WinZip for project archives.

Extract a `.zip`, `.rar`, or `.7z` file:

```powershell
.\scripts\extract-archive.ps1 -Archive "C:\path\to\archive.rar" -Destination "C:\path\to\output"
```

Create a `.zip` or `.7z` file:

```powershell
.\scripts\create-archive.ps1 -Archive ".\run-logs\PsychoBackup.zip" -Path ".\HOSTING_CHECKLIST.md", ".\README.md"
```

7-Zip can extract `.rar` files, but it cannot create `.rar` archives. Use `.zip` or `.7z` for new project archives.

These scripts expect the portable JDK 12 at:

```text
C:\Users\xzero\AppData\Local\Codex\jdks\jdk-12.0.2+10
```

## Export model candidates from the ATD 921 cache

Keep `C:\Users\xzero\Downloads\Atd 921 Cache Release` outside Git. Export only small decoded candidates into the ignored local `jcache_exports` folder:

```powershell
cd "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes"
python ".\scripts\export-js5-dat2-models.py" --repo-root "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes" --source "C:\Users\xzero\Downloads\Atd 921 Cache Release\data" --output "necrotic_client-item_attributes\jcache_exports\atd-921\models" --indexes 7 --limit 900 --clear-output --model-layout exploratory
```

Then ask Unity to import that exported candidate folder:

```powershell
cd "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes"
$env:PSYCHO_JCACHE_MODEL_EXPORT_PATH = (Resolve-Path ".\necrotic_client-item_attributes\jcache_exports\atd-921\models").Path
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath ".\psycho_unity_client" -executeMethod Psycho.Editor.PsychoJCacheModelImporter.ImportJCacheModelPackBatch -logFile ".\run-logs\unity-atd921-model-import.log"
```

Render the imported model preview:

```powershell
cd "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes"
$env:PSYCHO_JCACHE_MODEL_EXPORT_PATH = (Resolve-Path ".\necrotic_client-item_attributes\jcache_exports\atd-921\models").Path
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath ".\psycho_unity_client" -executeMethod Psycho.Editor.PsychoJCacheModelImporter.RenderJCacheModelPreviewBatch -logFile ".\run-logs\unity-atd921-model-preview.log"
```
