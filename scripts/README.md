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
