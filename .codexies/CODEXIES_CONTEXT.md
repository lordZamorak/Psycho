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

## Recently Completed

- Rebranded toward Psycho/Psychotic naming.
- Created Unity hosted test world generation from cache/world references.
- Added terrain collision audits and player grounding checks.
- Fixed generated player spawn grounding so the player starts above actual terrain.
- Improved terrain color blending toward greener meadow/highland visuals.
- Added meadow ground-cover patches.
- Brightened skybox and tuned terrain material response.
- Built and verified the Windows Unity playable.
- Backed up runnable states to iCloud.
- Pushed latest stable milestone at commit `501c473a` with message `Enrich Unity terrain visuals and spawn grounding`.

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

## Next Best Work

Continue graphics/stability in this order:

1. Replace low-poly player/NPC bodies with real optimized original/licensed humanoid and creature assets.
2. Add rigs, animation controllers, smoothing, scale correction, and outward-facing third-person presentation.
3. Replace weak tree/foliage silhouettes with real foliage meshes, LOD groups, GPU instancing, and wind.
4. Replace weak building/object meshes with higher-fidelity medieval assets while preserving world layout.
5. Improve true terrain splat textures/material layers and reduce painted-on ground.
6. Improve water edge depth/ripples/foam.
7. Profile CPU/GPU/memory after each category and keep performance playable.
8. Keep rebuilding the scene, running diagnostics, showing screenshots, backing up, committing, and pushing stable milestones.
