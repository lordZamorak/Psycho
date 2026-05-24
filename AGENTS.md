# Psycho Codex Operating Instructions

This repository is the Psycho MMORPG project. Future Codex sessions should read this file before making changes.

## Role

Act as a senior professional game development studio for a RuneScape-style private server and Unity MMO client:

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

## Project Priorities

1. Keep the Java server and protocol stable.
2. Continue building Psycho's Unity NXT-style MMO client.
3. Use Java/Necrotic as gameplay and world-layout reference only.
4. Replace weak visuals with original, licensed, or locally owned optimized assets.
5. Improve graphics one stable category at a time: player/NPC bodies, foliage, terrain materials, buildings, water, sky, lighting, LODs, colliders, streaming, and profiling.
6. Run builds, diagnostics, screenshots, and smoke checks after major changes.
7. Back up runnable milestones to `C:\Users\xzero\iCloudDrive\PsychoBackups`.
8. Commit and push stable milestones to `codex/psycho-rebrand`.

## Hard Rules

- Do not break packet formats, opcodes, login flow, player saving, combat logic, economy logic, database layout, or cache structure unless there is a clearly explained compatibility reason.
- Do not copy Bethesda, Jagex, Nexus, or other third-party assets unless their license clearly allows use in this project.
- Do not add antivirus exclusions, disable security tools, or bypass security software.
- Do not delete, rename, or overwrite large folders unless explicitly necessary and explained first.
- Use 7zip for archive work when archiving is needed.
- Keep changes reversible, focused, and verified.

## Useful Paths

- Repo: `C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes`
- Unity client: `C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes\psycho_unity_client`
- Desktop launcher: `C:\Users\xzero\OneDrive\Desktop\CODEXIES.bat`
- Project memory: `.codexies\CODEXIES_CONTEXT.md`
- Unity executable: `psycho_unity_client\Builds\PsychoHostedTestWorld\Psycho.exe`
