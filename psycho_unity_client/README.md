# Psycho Unity Client

This is the next-generation Unity client track for Psycho. It is intentionally separate from the current Java client and Java server.

## Direction

- Keep the Java server authoritative.
- Preserve the current login flow, packet protocol, player saves, cache data, economy, and gameplay behavior.
- Use Unity for presentation: modern camera, lighting, water, grass, trees, particles, UI, and post-processing.
- Port protocol and cache systems in small milestones rather than rewriting the whole game at once.

## Current Milestone

This scaffold includes:

- A Unity 6000.4.7f1 project shell.
- C# protocol helpers matching the existing Java client login handshake.
- A procedural prototype scene builder for sky, wind-driven grass, trees, and water.
- Runtime scripts for animated water and foliage.
- A Java-to-Unity mirror data pipeline for items, NPCs, NPC spawns, shops, and object definitions.
- A Unity visual factory that renders mirrored definitions with generated 2048x2048 material textures.

## Open In Unity

Use Unity Hub or run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -projectPath "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client"
```

## Build/Compile Check

```powershell
cd "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes"
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath ".\psycho_unity_client" -logFile ".\run-logs\unity-compile.log"
```

## Sync Java Definitions Into Unity

Run this from the server project whenever Java definitions change:

```powershell
cd "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\necrotic_server-item_attributes"
.\gradlew.bat --no-daemon --console=plain exportUnityMirror
```

This writes Unity-friendly data to:

```text
C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client\Assets\StreamingAssets\PsychoMirror
```

The Unity menu item `Psycho/Generate 2K Visual Materials` creates the 2048x2048 procedural material library used by mirrored items and objects. The menu item `Psycho/Build Prototype Scene` adds the mirror loader, visual factory, and local-world preview spawner to the prototype scene.

## Next Milestones

1. Verify C# login handshake against the local Java server.
2. Decode enough cache metadata to render the live home region in Unity.
3. Translate server update packets into Unity entities.
4. Replace procedural placeholder forms with cache-derived or original high-resolution models, animations, and materials.
5. Add production graphics systems: terrain grass instancing, shader graph water, wind zones, day/night lighting, camera effects, and modern UI.
