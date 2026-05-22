# Psycho Unity Client

This is the next-generation Unity client track for Psycho. It is intentionally separate from the current Java client and Java server.

## Direction

- Keep the Java server authoritative.
- Preserve the current login flow, packet protocol, player saves, cache data, economy, and gameplay behavior.
- Use Unity for presentation: modern camera, lighting, water, grass, trees, particles, UI, and post-processing.
- Port protocol and cache systems in small milestones rather than rewriting the whole game at once.

## Current Milestone

This scaffold includes:

- A Unity 2022.3 project shell.
- C# protocol helpers matching the existing Java client login handshake.
- A procedural prototype scene builder for sky, wind-driven grass, trees, and water.
- Runtime scripts for animated water and foliage.

## Open In Unity

Use Unity Hub or run:

```powershell
& "C:\Program Files\Unity 2022.3.0f1\Editor\Unity.exe" -projectPath "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client"
```

## Build/Compile Check

```powershell
cd "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes"
& "C:\Program Files\Unity 2022.3.0f1\Editor\Unity.exe" -batchmode -quit -projectPath ".\psycho_unity_client" -logFile ".\run-logs\unity-compile.log"
```

## Next Milestones

1. Verify C# login handshake against the local Java server.
2. Decode enough cache metadata to render the live home region in Unity.
3. Translate server update packets into Unity entities.
4. Replace prototype terrain with cache-derived terrain, models, animations, and materials.
5. Add production graphics systems: terrain grass instancing, shader graph water, wind zones, day/night lighting, camera effects, and modern UI.
