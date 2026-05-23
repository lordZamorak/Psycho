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

## Unity Hosted Client

Unity project:

```text
C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client
```

Installed editor used for the hosted test build:

```text
C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe
```

Regenerate the 3x3 hosted test world around Edgeville:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client' -executeMethod Psycho.Editor.PsychoHostedWorldSceneBuilder.BuildHostedTestWorldSceneBatch -logFile 'C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\run-logs\unity-hosted-world-build.log'
```

Build the Windows hosted playable:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client' -executeMethod Psycho.Editor.PsychoHostedWorldSceneBuilder.BuildWindowsHostedPlayableBatch -logFile 'C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\run-logs\unity-windows-hosted-playable-build.log'
```

Run the hosted playable:

```powershell
& 'C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client\Builds\PsychoHostedTestWorld\Psycho.exe'
```

### Repository assets

This repository uses Git LFS for required binary assets such as vendored JARs, packed data files, archives, and PNG textures. After cloning on a new machine, run:

```powershell
git lfs install
git lfs pull
```

Unity outputs under `psycho_unity_client\Assets\Generated\` and generated playable scenes are intentionally ignored. Recreate them with the Unity batch commands above instead of committing the generated output.

Server connection settings live in:

```text
psycho_unity_client\Assets\StreamingAssets\PsychoClient\server.json
```

## Psycho NXT-Style Direction

The Unity track is not a Jagex NXT clone. It is Psycho's own NXT-style MMO client: the Java server stays authoritative, Unity mirrors the live game, and weak old visuals are replaced with modern assets and rendering systems without changing gameplay or packet behavior.

See `PSYCHO_NXT_UNITY_CLIENT_STRATEGY.md` for the guardrails, milestones, asset replacement policy, and coding style.
