# Psycho Mod Asset Triage

This file tracks third-party game-mod archives before they are allowed into the Unity client. Nothing from this list should be imported into `Assets/` until it passes both permission and technical checks.

## Intake Rules

- Use mod assets only when the license or Nexus permissions clearly allow use outside the original game, conversion, modification, redistribution, and any commercial hosting path Psycho may need.
- Do not import Bethesda/Jagex/Skyrim/RS assets, FaceGen outputs, ESP records, vanilla meshes, vanilla textures, or assets that depend on another game's proprietary files.
- Keep unclear archives as visual reference only until the original author grants permission in writing.
- Shipping format for approved 3D art is Unity-ready prefab source plus optimized FBX/GLB, authored colliders, LODs, and documented material budgets.

## Reviewed Local Archives

### Denizens of Skyrim - Markarth V.1.0

- Local path: `C:\Users\xzero\Downloads\Denizens of Skyrim - Markarth V.1.0-121571-1-0-1778038171`
- Detected files: 23 `.NIF`, 23 `.dds`, 23 `.tga`, 1 `.esp`
- Nexus page inspected: `https://www.nexusmods.com/skyrimspecialedition/mods/179359`
- Permission result: reference-only for Psycho.
- Reason: The archive is Skyrim FaceGen and plugin content, and the Nexus credits include Bethesda. Even though the author grants broad permission for their mod, the delivered files are tied to Skyrim character data and are not clean Unity character source art for Psycho.
- Allowed Psycho use: NPC visual direction, town population inspiration, and scheduling/layout reference.
- Blocked Psycho use: importing the meshes, textures, ESP data, names, faces, or derived Skyrim character assets into the shipped Unity project.

### Dirtcliff Mesh Edits - High Poly Dirtcliffs

- Local path: `C:\Users\xzero\Downloads\Dirtcliff Mesh Edits - High Poly Dirtcliffs-121435-1-1775675815`
- Detected files: 6 `.nif`
- Nexus page inspected: `https://www.nexusmods.com/skyrimspecialedition/mods/176789`
- Permission result: promising, but not imported yet.
- Reason: Nexus permissions allow conversion, modification, redistribution, and use in other games with credit. The file is still NIF-only and the page warns about performance and collision issues, so it needs a controlled conversion/optimization pass before any Unity use.
- Required before import: verify the meshes do not embed proprietary Skyrim geometry dependencies, convert through Blender/NIF tooling, replace any Skyrim material references with Psycho-owned/CC0 materials, generate simplified colliders, create LODs, and add attribution.
- Current Psycho use: visual reference for original layered cliff dressing generated inside the Unity scene builder.

