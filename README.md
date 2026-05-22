# Psycho RSPS Local Workspace

This workspace contains the Psycho/Psychotic server and Java client used for local hosted-test preparation.

## Structure

- `necrotic_server-item_attributes/` - Java game server.
- `necrotic_client-item_attributes/` - Java client.
- `scripts/` - repeatable PowerShell build/run helpers.
- `run-logs/` - stdout/stderr logs created by the helper scripts.
- `_codex_backups/` - backups created before repair passes.

## Toolchain

Gradle wrapper `5.2.1` is used by both modules and requires the pinned JDK 12 runtime:

```text
C:\Users\xzero\AppData\Local\Codex\jdks\jdk-12.0.2+10
```

Using the machine default JDK 17/21 will fail during Gradle startup.

## Entrypoints

- Server: `com.ruse.GameServer`
- Client: `org.necrotic.client.Client`
- Default server port: `13377`
- Default client host: `localhost`
- Client cache source: `necrotic_client-item_attributes/cache`
- Client user cache: `C:\Users\xzero\PsychoCache`

## Build

Run from this directory:

```powershell
.\scripts\build-all.ps1
```

## Run

Start or restart the server:

```powershell
.\scripts\run-server.ps1
.\scripts\run-server.ps1 -Restart
```

Start the client:

```powershell
.\scripts\run-client.ps1
```

Stop local Java game processes:

```powershell
.\scripts\stop-game.ps1
```

See `scripts/README.md` for optional flags.
