using System;
using Psycho.Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Editor
{
    public static class PsychoHostedVisualOverrides
    {
        private static bool attemptedLooseImport;
        private static bool attemptedCache1Probe;

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
            if (definition == null || string.IsNullOrWhiteSpace(definition.name))
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
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh != null)
            {
                return mesh;
            }

            if (looseModel && !attemptedLooseImport)
            {
                attemptedLooseImport = true;
                PsychoLooseModelImporter.EnsureLooseModelAssets();
                return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            }

            if (!looseModel && !attemptedCache1Probe)
            {
                attemptedCache1Probe = true;
                PsychoCache1ModelSampler.EnsureCache1ModelSampleAssets();
                return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            }

            return null;
        }

        private static void NormalizeMeshRoot(Transform root, float targetHeight, float maxFootprint)
        {
            if (!TryCalculateLocalBounds(root, out Bounds bounds) || bounds.size.y <= 0.001f)
            {
                return;
            }

            float scaleByHeight = targetHeight / bounds.size.y;
            float footprint = Mathf.Max(bounds.size.x, bounds.size.z);
            float scaleByFootprint = footprint <= 0.001f ? scaleByHeight : maxFootprint / footprint;
            float scale = Mathf.Min(scaleByHeight, scaleByFootprint);
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
