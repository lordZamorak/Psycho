using System;
using System.Collections.Generic;
using Psycho.Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Editor
{
    public static class PsychoHostedVisualOverrides
    {
        private const string LooseModelRoot = "Assets/Generated/LooseModels";
        private const string Cache1ModelRoot = "Assets/Generated/Cache1Models";
        private const string JCacheModelRoot = "Assets/Generated/JCacheModels";

        private static bool attemptedLooseImport;
        private static bool attemptedCache1Probe;
        private static bool attemptedJCacheImport;
        private static readonly bool EnableRewardLikeObjectAccents = false;
        private static string[] cachedLooseShowcaseAssets;
        private static string[] cachedCache1ShowcaseAssets;
        private static string[] cachedJCacheShowcaseAssets;

        private static readonly NpcReplacement[] NpcReplacements =
        {
            new NpcReplacement(
                "raid spider",
                "Assets/Generated/LooseModels/loose_0083_raid_spider.asset",
                1.05f,
                new[] { "giant spider", "raid spider" })
        };

        private static readonly ObjectReplacement[] MissingObjectReplacements =
        {
            new ObjectReplacement(
                954,
                "cache1 fallback prop",
                "Assets/Generated/Cache1Models/cache1_idx07_file00003.asset",
                1.05f,
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 180f, 0f))
        };

        private static readonly ObjectAccent[] ObjectAccents =
        {
            new ObjectAccent("bank counter bond", "Assets/Generated/LooseModels/loose_0040_green_bond.asset", new[] { "bank booth", "closed bank booth" }, new Vector3(-0.22f, 0.82f, 0.05f), new Vector3(0f, 32f, 0f), 0.18f),
            new ObjectAccent("bank counter note", "Assets/Generated/LooseModels/loose_0113_white_bond.asset", new[] { "bank booth", "closed bank booth" }, new Vector3(0.20f, 0.84f, -0.02f), new Vector3(0f, -24f, 0f), 0.16f),
            new ObjectAccent("market red casket", "Assets/Generated/LooseModels/loose_0084_red_casket.asset", new[] { "market stall" }, new Vector3(-0.28f, 0.95f, 0.05f), new Vector3(0f, 28f, 0f), 0.24f),
            new ObjectAccent("market green casket", "Assets/Generated/LooseModels/loose_0041_green_casket.asset", new[] { "market stall" }, new Vector3(0.26f, 0.95f, -0.08f), new Vector3(0f, -19f, 0f), 0.22f),
            new ObjectAccent("tea stall tome", "Assets/Generated/LooseModels/loose_0102_tome_of_fire.asset", new[] { "tea stall" }, new Vector3(-0.22f, 0.94f, 0.04f), new Vector3(0f, 18f, 0f), 0.20f),
            new ObjectAccent("tea stall casket", "Assets/Generated/LooseModels/loose_0115_yellow_casket.asset", new[] { "tea stall" }, new Vector3(0.24f, 0.94f, -0.08f), new Vector3(0f, -22f, 0f), 0.21f),
            new ObjectAccent("fur stall pouch", "Assets/Generated/LooseModels/loose_0049_looting_pouch.asset", new[] { "fur stall" }, new Vector3(-0.22f, 0.92f, 0.03f), new Vector3(0f, 16f, 0f), 0.24f),
            new ObjectAccent("fur stall relic", "Assets/Generated/LooseModels/loose_0022_dark_relic.asset", new[] { "fur stall" }, new Vector3(0.24f, 0.94f, -0.07f), new Vector3(0f, -26f, 0f), 0.18f),
            new ObjectAccent("counter ledger", "Assets/Generated/LooseModels/loose_0102_tome_of_fire.asset", new[] { "counter", "bank table" }, new Vector3(-0.18f, 0.86f, 0.02f), new Vector3(0f, 12f, 0f), 0.15f),
            new ObjectAccent("counter key", "Assets/Generated/LooseModels/loose_0047_larren_key.asset", new[] { "counter", "bank table" }, new Vector3(0.18f, 0.88f, -0.06f), new Vector3(0f, -16f, 0f), 0.13f),
            new ObjectAccent("shelf relic", "Assets/Generated/LooseModels/loose_0022_dark_relic.asset", new[] { "shelf", "bookcase" }, new Vector3(-0.16f, 0.72f, 0.02f), new Vector3(0f, 20f, 0f), 0.13f),
            new ObjectAccent("shelf tome", "Assets/Generated/LooseModels/loose_0103_tome_of_firedrop.asset", new[] { "shelf", "bookcase" }, new Vector3(0.17f, 0.92f, -0.04f), new Vector3(0f, -18f, 0f), 0.14f),
            new ObjectAccent("closed chest casket", "Assets/Generated/LooseModels/loose_0114_white_casket.asset", new[] { "closed chest" }, new Vector3(0f, 0.42f, 0f), new Vector3(0f, 32f, 0f), 0.28f),
            new ObjectAccent("crate supply pouch", "Assets/Generated/LooseModels/loose_0048_looting_pouch_drop.asset", new[] { "crate", "cart" }, new Vector3(-0.18f, 0.55f, 0.02f), new Vector3(0f, 24f, 0f), 0.22f),
            new ObjectAccent("crate green casket", "Assets/Generated/LooseModels/loose_0041_green_casket.asset", new[] { "crate", "cart" }, new Vector3(0.18f, 0.58f, -0.03f), new Vector3(0f, -22f, 0f), 0.21f),
            new ObjectAccent("range ember orb", "Assets/Generated/LooseModels/loose_0064_orange_orb.asset", new[] { "range", "furnace" }, new Vector3(0f, 0.74f, 0f), new Vector3(0f, 0f, 0f), 0.16f),
            new ObjectAccent("anvil hammer", "Assets/Generated/LooseModels/loose_0029_dragon_war_hammer_drop.asset", new[] { "anvil" }, new Vector3(0.16f, 0.48f, 0.02f), new Vector3(0f, -34f, 88f), 0.34f)
        };

        public static bool TryCreateNpcReplacement(PsychoMirrorNpc npc, Material material, out GameObject root, out string replacementName)
        {
            root = null;
            replacementName = null;
            if (npc == null || string.IsNullOrWhiteSpace(npc.name))
            {
                return false;
            }

            string normalizedName = npc.name.ToLowerInvariant();
            foreach (NpcReplacement replacement in NpcReplacements)
            {
                if (!replacement.Matches(normalizedName))
                {
                    continue;
                }

                Mesh mesh = LoadMesh(replacement.AssetPath, true);
                if (mesh == null)
                {
                    return false;
                }

                root = new GameObject($"NPC Visual Override - {replacement.DisplayName}");
                GameObject meshObject = CreateMeshChild(root.transform, replacement.DisplayName, mesh, material);
                meshObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                NormalizeMeshRoot(meshObject.transform, replacement.TargetHeight, 2.2f);
                replacementName = replacement.DisplayName;
                return true;
            }

            return false;
        }

        public static bool TryCreateMissingObjectReplacement(PsychoMapObjectPlacementAdapter placement, Material material, out GameObject root, out string replacementName)
        {
            root = null;
            replacementName = null;
            foreach (ObjectReplacement replacement in MissingObjectReplacements)
            {
                if (replacement.ObjectId != placement.ObjectId)
                {
                    continue;
                }

                Mesh mesh = LoadMesh(replacement.AssetPath, false);
                if (mesh == null)
                {
                    return false;
                }

                root = new GameObject($"Decoded Object Replacement - {replacement.DisplayName}");
                root.transform.localPosition = replacement.LocalPosition;
                root.transform.localRotation = Quaternion.Euler(replacement.LocalEuler);
                GameObject meshObject = CreateMeshChild(root.transform, replacement.DisplayName, mesh, material);
                NormalizeMeshRoot(meshObject.transform, replacement.TargetHeight, 1.6f);
                replacementName = replacement.DisplayName;
                return true;
            }

            return false;
        }

        public static int AddObjectAccents(PsychoMirrorObject definition, Transform parent, Material material)
        {
            if (!EnableRewardLikeObjectAccents || definition == null || string.IsNullOrWhiteSpace(definition.name))
            {
                return 0;
            }

            int added = 0;
            string normalizedName = definition.name.ToLowerInvariant();
            foreach (ObjectAccent accent in ObjectAccents)
            {
                if (!accent.Matches(normalizedName))
                {
                    continue;
                }

                Mesh mesh = LoadMesh(accent.AssetPath, true);
                if (mesh == null)
                {
                    continue;
                }

                GameObject meshObject = CreateMeshChild(parent, accent.DisplayName, mesh, material);
                meshObject.transform.localPosition = accent.LocalPosition;
                meshObject.transform.localRotation = Quaternion.Euler(accent.LocalEuler);
                NormalizeMeshRoot(meshObject.transform, accent.TargetHeight, 0.72f);
                added++;
            }

            return added;
        }

        public static int AddLooseModelShowcase(
            Transform parent,
            Material modelMaterial,
            string displayPrefix,
            int startIndex,
            int maxItems,
            Vector3 localOrigin,
            int columns,
            float spacingX,
            float spacingZ,
            float targetHeight,
            float maxFootprint)
        {
            return AddGeneratedModelShowcase(
                parent,
                modelMaterial,
                displayPrefix,
                GeneratedModelSource.Loose,
                LooseModelRoot,
                ref cachedLooseShowcaseAssets,
                startIndex,
                maxItems,
                localOrigin,
                columns,
                spacingX,
                spacingZ,
                targetHeight,
                maxFootprint);
        }

        public static int AddCache1ModelShowcase(
            Transform parent,
            Material modelMaterial,
            string displayPrefix,
            int startIndex,
            int maxItems,
            Vector3 localOrigin,
            int columns,
            float spacingX,
            float spacingZ,
            float targetHeight,
            float maxFootprint)
        {
            return AddGeneratedModelShowcase(
                parent,
                modelMaterial,
                displayPrefix,
                GeneratedModelSource.Cache1,
                Cache1ModelRoot,
                ref cachedCache1ShowcaseAssets,
                startIndex,
                maxItems,
                localOrigin,
                columns,
                spacingX,
                spacingZ,
                targetHeight,
                maxFootprint);
        }

        public static int AddJCacheModelShowcase(
            Transform parent,
            Material modelMaterial,
            string displayPrefix,
            int startIndex,
            int maxItems,
            Vector3 localOrigin,
            int columns,
            float spacingX,
            float spacingZ,
            float targetHeight,
            float maxFootprint)
        {
            return AddGeneratedModelShowcase(
                parent,
                modelMaterial,
                displayPrefix,
                GeneratedModelSource.JCache,
                JCacheModelRoot,
                ref cachedJCacheShowcaseAssets,
                startIndex,
                maxItems,
                localOrigin,
                columns,
                spacingX,
                spacingZ,
                targetHeight,
                maxFootprint);
        }

        private static int AddGeneratedModelShowcase(
            Transform parent,
            Material modelMaterial,
            string displayPrefix,
            GeneratedModelSource source,
            string assetRoot,
            ref string[] cachedAssetPaths,
            int startIndex,
            int maxItems,
            Vector3 localOrigin,
            int columns,
            float spacingX,
            float spacingZ,
            float targetHeight,
            float maxFootprint)
        {
            if (parent == null || maxItems <= 0)
            {
                return 0;
            }

            string[] assetPaths = CollectGeneratedMeshAssets(assetRoot, source, ref cachedAssetPaths);
            if (assetPaths.Length == 0)
            {
                return 0;
            }

            int safeColumns = Mathf.Max(1, columns);
            int added = 0;
            for (int scanned = 0; scanned < assetPaths.Length && added < maxItems; scanned++)
            {
                int assetIndex = PositiveModulo(startIndex + scanned, assetPaths.Length);
                string assetPath = assetPaths[assetIndex];
                Mesh mesh = LoadMesh(assetPath, source);
                if (mesh == null)
                {
                    continue;
                }

                if (source == GeneratedModelSource.JCache && !LooksLikeReadableJCacheShowcaseMesh(mesh))
                {
                    continue;
                }

                int column = added % safeColumns;
                int row = added / safeColumns;
                Vector3 cellPosition = localOrigin + new Vector3(
                    (column - (safeColumns - 1) * 0.5f) * spacingX,
                    0f,
                    row * spacingZ);

                GameObject displayRoot = new GameObject($"{displayPrefix} {added + 1:D3} - {DisplayNameFromAsset(assetPath)}");
                displayRoot.transform.SetParent(parent, false);
                displayRoot.transform.localPosition = cellPosition;
                displayRoot.transform.localRotation = Quaternion.Euler(0f, (assetIndex * 37) % 360, 0f);

                GameObject meshObject = CreateMeshChild(displayRoot.transform, "Decoded Mesh Display", mesh, modelMaterial);
                meshObject.transform.localRotation = Quaternion.Euler(0f, 180f + (assetIndex * 11) % 45, 0f);
                float sizeJitter = 0.86f + Deterministic01(assetIndex * 131 + displayPrefix.Length * 17) * 0.28f;
                NormalizeMeshRoot(meshObject.transform, targetHeight * sizeJitter, maxFootprint);
                added++;
            }

            return added;
        }

        private static string[] CollectGeneratedMeshAssets(string assetRoot, GeneratedModelSource source, ref string[] cachedAssetPaths)
        {
            if (cachedAssetPaths != null)
            {
                return cachedAssetPaths;
            }

            string[] guids = AssetDatabase.FindAssets("t:Mesh", new[] { assetRoot });
            if (guids.Length == 0)
            {
                EnsureGeneratedModelAssets(source);

                guids = AssetDatabase.FindAssets("t:Mesh", new[] { assetRoot });
            }

            List<string> paths = new List<string>(guids.Length);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                paths.Add(path);
            }

            paths.Sort(StringComparer.OrdinalIgnoreCase);
            cachedAssetPaths = paths.ToArray();
            return cachedAssetPaths;
        }

        private static bool LooksLikeReadableJCacheShowcaseMesh(Mesh mesh)
        {
            if (mesh == null || mesh.vertexCount < 24)
            {
                return false;
            }

            Bounds bounds = mesh.bounds;
            if (!IsFinite(bounds.center) || !IsFinite(bounds.size))
            {
                return false;
            }

            float largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            float smallest = Mathf.Min(bounds.size.x, Mathf.Min(bounds.size.y, bounds.size.z));
            if (largest < 0.25f || largest > 96f || smallest < 0.025f)
            {
                return false;
            }

            float aspectRatio = largest / Mathf.Max(smallest, 0.001f);
            return aspectRatio <= 28f;
        }

        private static void EnsureGeneratedModelAssets(GeneratedModelSource source)
        {
            switch (source)
            {
                case GeneratedModelSource.Loose:
                    if (!attemptedLooseImport)
                    {
                        attemptedLooseImport = true;
                        PsychoLooseModelImporter.EnsureLooseModelAssets();
                    }

                    break;
                case GeneratedModelSource.Cache1:
                    if (!attemptedCache1Probe)
                    {
                        attemptedCache1Probe = true;
                        PsychoCache1ModelSampler.EnsureCache1ModelSampleAssets();
                    }

                    break;
                case GeneratedModelSource.JCache:
                    if (!attemptedJCacheImport)
                    {
                        attemptedJCacheImport = true;
                        PsychoJCacheModelImporter.EnsureJCacheModelAssets();
                    }

                    break;
            }
        }

        private static GameObject CreateMeshChild(Transform parent, string name, Mesh mesh, Material material)
        {
            GameObject meshObject = new GameObject(name);
            meshObject.transform.SetParent(parent, false);
            MeshFilter filter = meshObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return meshObject;
        }

        private static Mesh LoadMesh(string assetPath, bool looseModel)
        {
            return LoadMesh(assetPath, looseModel ? GeneratedModelSource.Loose : GeneratedModelSource.Cache1);
        }

        private static Mesh LoadMesh(string assetPath, GeneratedModelSource source)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh != null)
            {
                return mesh;
            }

            EnsureGeneratedModelAssets(source);

            return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        }

        private static void NormalizeMeshRoot(Transform root, float targetHeight, float maxFootprint)
        {
            if (!TryCalculateLocalBounds(root, out Bounds bounds))
            {
                return;
            }

            float originalHeight = bounds.size.y;
            float footprint = Mathf.Max(bounds.size.x, bounds.size.z);
            if (originalHeight <= 0.001f && footprint <= 0.001f)
            {
                return;
            }

            float height = Mathf.Max(originalHeight, 0.01f);
            float scaleByHeight = targetHeight / height;
            float scaleByFootprint = footprint <= 0.001f ? scaleByHeight : maxFootprint / footprint;
            float scale = Mathf.Min(scaleByHeight, scaleByFootprint);
            if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0.001f)
            {
                return;
            }

            root.localScale = Vector3.one * scale;
            root.localPosition += new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, -bounds.center.z * scale);
        }

        private static bool TryCalculateLocalBounds(Transform root, out Bounds bounds)
        {
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            bool hasBounds = false;
            foreach (MeshFilter filter in filters)
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                Bounds meshBounds = filter.sharedMesh.bounds;
                Vector3 min = filter.transform.TransformPoint(meshBounds.min);
                Vector3 max = filter.transform.TransformPoint(meshBounds.max);
                Bounds transformed = new Bounds((min + max) * 0.5f, Abs(max - min));
                if (!hasBounds)
                {
                    bounds = transformed;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(transformed);
                }
            }

            return hasBounds;
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x)
                && !float.IsNaN(value.y) && !float.IsInfinity(value.y)
                && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static int PositiveModulo(int value, int divisor)
        {
            if (divisor <= 0)
            {
                return 0;
            }

            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        private static float Deterministic01(int value)
        {
            unchecked
            {
                uint x = (uint)value;
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                return (x & 0x00ffffff) / 16777215f;
            }
        }

        private static string DisplayNameFromAsset(string assetPath)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            return string.IsNullOrWhiteSpace(fileName) ? "decoded_model" : fileName;
        }

        private static void RemoveCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private enum GeneratedModelSource
        {
            Loose,
            Cache1,
            JCache
        }

        public readonly struct PsychoMapObjectPlacementAdapter
        {
            public readonly int ObjectId;

            public PsychoMapObjectPlacementAdapter(int objectId)
            {
                ObjectId = objectId;
            }
        }

        private readonly struct NpcReplacement
        {
            public readonly string DisplayName;
            public readonly string AssetPath;
            public readonly float TargetHeight;
            private readonly string[] fragments;

            public NpcReplacement(string displayName, string assetPath, float targetHeight, string[] fragments)
            {
                DisplayName = displayName;
                AssetPath = assetPath;
                TargetHeight = targetHeight;
                this.fragments = fragments;
            }

            public bool Matches(string normalizedName)
            {
                foreach (string fragment in fragments)
                {
                    if (normalizedName.Contains(fragment))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private readonly struct ObjectReplacement
        {
            public readonly int ObjectId;
            public readonly string DisplayName;
            public readonly string AssetPath;
            public readonly float TargetHeight;
            public readonly Vector3 LocalPosition;
            public readonly Vector3 LocalEuler;

            public ObjectReplacement(int objectId, string displayName, string assetPath, float targetHeight, Vector3 localPosition, Vector3 localEuler)
            {
                ObjectId = objectId;
                DisplayName = displayName;
                AssetPath = assetPath;
                TargetHeight = targetHeight;
                LocalPosition = localPosition;
                LocalEuler = localEuler;
            }
        }

        private readonly struct ObjectAccent
        {
            public readonly string DisplayName;
            public readonly string AssetPath;
            public readonly Vector3 LocalPosition;
            public readonly Vector3 LocalEuler;
            public readonly float TargetHeight;
            private readonly string[] fragments;

            public ObjectAccent(string displayName, string assetPath, string[] fragments, Vector3 localPosition, Vector3 localEuler, float targetHeight)
            {
                DisplayName = displayName;
                AssetPath = assetPath;
                this.fragments = fragments;
                LocalPosition = localPosition;
                LocalEuler = localEuler;
                TargetHeight = targetHeight;
            }

            public bool Matches(string normalizedName)
            {
                foreach (string fragment in fragments)
                {
                    if (normalizedName.Contains(fragment))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
