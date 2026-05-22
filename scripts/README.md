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

These scripts expect the portable JDK 12 at:

```text
C:\Users\xzero\AppData\Local\Codex\jdks\jdk-12.0.2+10
```
