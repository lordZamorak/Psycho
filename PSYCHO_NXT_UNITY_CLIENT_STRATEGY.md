# Psycho NXT-Style Unity Client Strategy

## Direction

Psycho should not try to become a Jagex NXT clone. The goal is to build Psycho's own NXT-style Unity MMO client that mirrors the existing Java game while replacing weak old visuals with modern, original assets and rendering systems.

The Java server remains authoritative. Unity becomes the presentation, input, camera, UI, audio, and high-detail rendering layer.

## Non-Goals

- Do not rewrite combat, economy, skills, trading, banking, drops, staff permissions, or player saves in Unity.
- Do not change packet formats or server behavior for graphics-only work.
- Do not depend on private Jagex client code or proprietary NXT behavior.
- Do not force unknown JS5/NXT blobs through legacy model decoders when the output is visually unsafe.
- Do not replace the Java client until the Unity client can prove parity through repeatable tests.

## Compatibility Guardrails

- Java server is the source of truth for login, movement, region state, inventory, equipment, NPCs, objects, combat, chat, shops, GE, and persistence.
- Unity client must validate against the same local server used by the Java client.
- Cache-derived data is read-only unless a tool explicitly writes Unity-friendly generated output.
- Visual replacement assets must preserve gameplay footprint, click targets, collision intent, object identity, and NPC identity.
- Every graphics pass must keep a reversible fallback to the Java/cache-derived representation.

## Client Architecture

- Networking: C# protocol layer mirrors the Java client handshake and packet decoding.
- Simulation view: Unity maintains a client-side presentation model of server-owned entities.
- World streaming: regions stream as Unity chunks with terrain, collision, objects, NPCs, water, foliage, and lights.
- Asset pipeline: cache and mirror data are converted into Unity-friendly meshes, prefabs, materials, textures, animation clips, and metadata.
- Rendering: Unity owns terrain blending, sky, clouds, water, lighting, shadows, post-processing, grass wind, tree sway, particles, and camera behavior.
- UI: Unity owns modern login, loading, HUD, inventory, chat, bank, shops, GE, settings, and staff/debug overlays after protocol parity is stable.

## Visual Upgrade Policy

Use a three-tier replacement rule:

1. Cache-faithful: render the decoded Java/cache asset when it is recognizable and stable.
2. Enhanced: improve material, lighting, smoothing, LOD, shader response, and animation without changing identity.
3. Original high-detail replacement: use a new Unity asset when the decoded asset is weak, broken, too low-poly, or not suitable for modern close camera work.

Every replacement should keep an asset note tying it back to the source object, NPC, item, or region.

## NXT-Style Feature Targets

- Large draw distance with fog, atmospheric perspective, chunk LODs, and distant terrain silhouettes.
- Wind-reactive grass, reeds, tree canopies, banners, smoke, embers, and water edges.
- Terrain material blending so grass, paths, rock, mud, and banks do not look painted on.
- Water with depth tint, edge foam, ripples, reflections where affordable, and clear river/ocean semantics.
- Smooth NPC and player animation using interpolation, cleaned meshes, and identity-preserving replacements.
- Lighting that supports time-of-day mood while preserving gameplay readability.
- Modern login/loading presentation using original Psycho art, not copied client imagery.

## Milestones

1. Preserve Java parity: server starts, Java client works, Unity client builds, and local login handshake stays testable.
2. Stable hosted Unity scene: terrain, collision, movement preview, NPC placement, objects, banks, shops, GE landmarks, and Edgeville home area.
3. Presentation parity: Unity screenshots should match the Java scene layout and identity before replacing visuals.
4. High-detail replacements: upgrade weak terrain, trees, water, NPCs, buildings, banks, shops, and GE assets with Unity-native art.
5. Protocol expansion: decode enough live server packets for real login, movement, entity updates, chat, inventory, and object interactions.
6. Streaming world: expand beyond Edgeville using region chunks and collision-safe terrain generation.
7. Hosted test stage: repeatable build, config, logging, crash recovery, and test checklist.

## Coding Style

- Keep server logic in Java/Kotlin and presentation logic in Unity C#.
- Prefer small C# components with clear ownership over large scene scripts.
- Keep generated assets under ignored generated folders unless they are curated runtime assets.
- Use data-driven definitions for NPCs, objects, items, regions, materials, and replacements.
- Avoid per-frame allocations in runtime systems.
- Use object pooling, GPU instancing, LOD groups, texture atlases, and async loading for scale.
- Make visual systems deterministic enough to render comparable screenshots for QA.
- Keep tools read-only by default when touching cache and JS5 sources.

## QA Standard

For each major pass, verify:

- Java server build still passes.
- Java client build still passes when touched.
- Unity batch build or compile check passes when Unity files are touched.
- Hosted scene collision audit passes when terrain changes.
- Screenshot output is produced for visual work.
- No server protocol, packet, save, combat, or economy behavior changed unless explicitly intended.

