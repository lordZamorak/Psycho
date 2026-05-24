# Psycho Art Pipeline

This folder is for original or properly licensed source art before it is wired into the hosted Unity MMO client.

Use the Unity menu item `Psycho > Art Pipeline > Create Default Manifest` to create:

- `Assets/Resources/PsychoArt/PsychoArtAssetManifest.asset`
- Character, NPC, environment, foliage, prop, and material source folders.

Assign real prefabs in the manifest instead of adding more procedural cube/capsule visuals. The runtime resolver will use matching prefabs by id, visual class, or name fragment, then fall back to the current generated visuals if no art is assigned.

Initial budgets:

- Player hero: 20k-45k triangles at LOD0, 2-4 material slots, rigged humanoid, 2K PBR textures.
- Human NPC: 8k-25k triangles at LOD0, shared skeleton where possible, 1K-2K PBR textures.
- Common props: 500-8k triangles with authored colliders and LODs.
- Trees/foliage: LODGroup required, GPU-instanced materials, billboard or low-poly far LOD.
- Buildings: modular pieces with combined static meshes, baked/occlusion-friendly colliders.
- Terrain: tile/region streaming, splat materials, no one-object-per-blade grass.

Every imported prefab should include sane scale, origin at ground contact when possible, materials with instancing enabled, LODs for anything repeated, and simple colliders that match gameplay rather than render mesh detail.
