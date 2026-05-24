# CODEXIES Psycho Project Brief

This file exists so a future Codex session can quickly recover the working context for the Psycho project.

## Project

- Main repo: `C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes`
- Unity client: `C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client`
- Java/Necrotic server/client source is a gameplay/layout reference, not the final art source.
- Active branch used during current work: `codex/psycho-rebrand`
- Remote: `https://github.com/lordZamorak/Psycho.git`
- Built Unity executable: `psycho_unity_client\Builds\PsychoHostedTestWorld\Psycho.exe`
- iCloud backups are stored under: `C:\Users\xzero\iCloudDrive\PsychoBackups`
- Latest pushed checkpoint: `77f5e4a0 Import creature art replacements`

## Studio Role To Resume

Act as Codex working like a senior 30+ year professional RSPS/MMORPG/Unity game studio:

- Lead Java/Kotlin game server engineer
- RSPS protocol/cache specialist
- Multiplayer networking engineer
- Unity/C# MMO client engineer
- Game engine systems architect
- Gameplay/content developer
- Combat/economy balance designer
- Database/persistence engineer
- Security, anti-cheat, and anti-dupe engineer
- RuneLite/Java client integration engineer
- DevOps/hosting engineer
- QA/test automation engineer
- Technical director and project manager

Work with strong autonomy, but keep changes safe, reversible, and verified.

## Hard Rules

- Keep server behavior and protocol stable unless a compatibility fix is absolutely required.
- Do not change packet formats, opcodes, login flow, combat logic, economy logic, database layout, or cache structure without explaining why first.
- Use Java/Necrotic only as gameplay/world-layout reference.
- Use Skyrim-style screenshots only as art direction. Do not copy Bethesda/Jagex/Nexus assets unless the license clearly permits it.
- Use original, licensed, or locally owned assets for the final Unity client.
- Do not add antivirus exclusions or bypass security software.
- Use 7zip for archives when archiving is needed; do not use WinZip.
- Back up runnable project states to iCloud before risky changes.
- Commit and push stable milestones to `codex/psycho-rebrand`.

## Current Visual Direction

Build Psycho's own NXT-style Unity MMO client:

- Modern high-fidelity medieval fantasy MMORPG look.
- Smooth, clean, non-blocky player/NPC bodies and faces.
- Rich foliage silhouettes, wind movement, stronger terrain materials, better water, sky, lighting, LODs, colliders, and performance budgets.
- Mirror the Java game's layout/gameplay feel, not its low-poly final visuals.
- Replace weak placeholders one category at a time with optimized real authored assets.

## Latest Stable Checkpoint

- Rebranded toward Psycho/Psychotic naming.
- Created Unity hosted test world generation from cache/world references.
- Added terrain collision audits and player grounding checks.
- Fixed generated player spawn grounding so the player starts above actual terrain.
- Improved terrain color blending toward greener meadow/highland visuals.
- Added meadow ground-cover patches.
- Brightened skybox and tuned terrain material response.
- Built and verified the Windows Unity playable.
- Backed up runnable states to iCloud.
- Imported CC0 Quaternius creature assets into the Unity art pipeline:
  - `Pug` drives dog, puppy, and terror dog replacements.
  - `Cow` drives cow and calf replacements.
  - `Sheep` drives sheep, lamb, and ram replacements.
  - `Dragon` is the current starter imp replacement.
- Added exact NPC-id-first art resolution so fuzzy words like `guard` or `champion` do not override dog/imp bodies.
- Rendered proof screenshots:
  - `run-logs\unity-starter-creature-preview.png`
  - `run-logs\unity-hosted-third-person-preview.png`
  - `run-logs\unity-hosted-npc-preview.png`
- Latest verified hosted world report:
  - Regions: `81`
  - NPC spawns: `460`
  - Visual NPC replacements: `417`
  - Cache NPC visuals remaining: `43`
  - Terrain collision samples: `331776`
  - Terrain collision misses: `0`
- Latest verified budget report:
  - Renderers: `46163`
  - Triangles: `975119`
  - LOD groups: `1251`
- Latest successful Windows build:
  - `psycho_unity_client\Builds\PsychoHostedTestWorld\Psycho.exe`
  - Size reported by Unity: `991795872` bytes
- iCloud checkpoint for creature source assets:
  - `C:\Users\xzero\iCloudDrive\PsychoBackups\SourceFileBackups\creature_source_assets_20260524-072415`
- Pushed latest stable milestone at commit `77f5e4a0` with message `Import creature art replacements`.

## Latest Verified Commands

Build hosted Unity scene:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client" -executeMethod Psycho.Editor.PsychoHostedWorldSceneBuilder.BuildHostedTestWorldSceneBatch -logFile "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\run-logs\unity-hosted-scene-build.log"
```

Run Unity playable:

```powershell
cd "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes"
.\psycho_unity_client\Builds\PsychoHostedTestWorld\Psycho.exe
```

Render starter creature proof screenshot:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client" -executeMethod Psycho.Editor.PsychoStarterArtManifestBuilder.RenderStarterCreaturePreviewBatch -logFile "C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\run-logs\unity-starter-creature-preview.log"
```

## Next Best Work

Continue graphics/stability in this order:

1. Replace low-poly player/NPC humanoid bodies with real optimized original/licensed humanoid assets.
2. Add rigs, animation controllers, smoothing, scale correction, and outward-facing third-person presentation.
3. Replace weak tree/foliage silhouettes with real foliage meshes, LOD groups, GPU instancing, and wind.
4. Replace weak building/object meshes with higher-fidelity medieval assets while preserving world layout.
5. Improve true terrain splat textures/material layers and reduce painted-on ground.
6. Improve water edge depth/ripples/foam.
7. Profile CPU/GPU/memory after each category and keep performance playable.
8. Keep rebuilding the scene, running diagnostics, showing screenshots, backing up, committing, and pushing stable milestones.
