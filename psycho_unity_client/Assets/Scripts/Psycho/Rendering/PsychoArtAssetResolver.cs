using Psycho.Mirror;
using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Rendering
{
    public static class PsychoArtAssetResolver
    {
        private const string ManifestResourcePath = "PsychoArt/PsychoArtAssetManifest";

        private static PsychoArtAssetManifest cachedManifest;
        private static bool attemptedLoad;

        public static bool TryInstantiatePlayer(string playerName, Transform parent, out GameObject instance)
        {
            instance = null;
            PsychoArtAssetEntry entry = FindEntry(entryCandidate => entryCandidate.MatchesPlayer(playerName), StableHash(playerName));
            return TryInstantiate(entry, parent, out instance);
        }

        public static bool TryInstantiateNpc(PsychoMirrorNpc npc, Transform parent, out GameObject instance)
        {
            instance = null;
            PsychoArtAssetEntry entry = FindEntry(entryCandidate => entryCandidate.MatchesNpc(npc), StableHash(npc?.id ?? 0, npc?.name, npc?.visualClass));
            return TryInstantiate(entry, parent, out instance);
        }

        public static bool TryInstantiateObject(PsychoMirrorObject worldObject, Transform parent, out GameObject instance)
        {
            instance = null;
            PsychoArtAssetEntry entry = FindEntry(entryCandidate => entryCandidate.MatchesObject(worldObject), StableHash(worldObject?.id ?? 0, worldObject?.name, worldObject?.visualClass));
            return TryInstantiate(entry, parent, out instance);
        }

        public static bool TryInstantiateItem(PsychoMirrorItem item, Transform parent, out GameObject instance)
        {
            instance = null;
            PsychoArtAssetEntry entry = FindEntry(entryCandidate => entryCandidate.MatchesItem(item), StableHash(item?.id ?? 0, item?.name, item?.visualClass));
            return TryInstantiate(entry, parent, out instance);
        }

        private static PsychoArtAssetEntry FindEntry(System.Predicate<PsychoArtAssetEntry> predicate, int seed)
        {
            PsychoArtAssetManifest manifest = LoadManifest();
            if (manifest == null || predicate == null)
            {
                return null;
            }

            PsychoArtAssetEntry[] entries = manifest.Entries;
            int matchCount = 0;
            PsychoArtAssetEntry selected = null;
            for (int i = 0; i < entries.Length; i++)
            {
                PsychoArtAssetEntry entry = entries[i];
                if (entry != null && entry.prefab != null && predicate(entry))
                {
                    matchCount++;
                    if (PositiveModulo(seed, matchCount) == 0)
                    {
                        selected = entry;
                    }
                }
            }

            return selected;
        }

        private static bool TryInstantiate(PsychoArtAssetEntry entry, Transform parent, out GameObject instance)
        {
            instance = null;
            if (entry == null || entry.prefab == null)
            {
                return false;
            }

            instance = Object.Instantiate(entry.prefab);
            instance.name = string.IsNullOrWhiteSpace(entry.displayName) ? entry.prefab.name : entry.displayName;
            if (parent != null)
            {
                instance.transform.SetParent(parent, false);
            }

            instance.transform.localPosition = entry.localPosition;
            instance.transform.localRotation = Quaternion.Euler(entry.localEuler);
            instance.transform.localScale = Vector3.Scale(instance.transform.localScale, entry.localScale);
            NormalizeHeight(instance.transform, entry.targetHeight);
            ConfigureRenderers(instance, entry);
            if (entry.markStatic)
            {
                MarkStatic(instance);
            }

            if (entry.ensureCullLod)
            {
                EnsureCullLod(instance, entry.cullScreenRelativeHeight);
            }

            return true;
        }

        private static PsychoArtAssetManifest LoadManifest()
        {
            if (!attemptedLoad)
            {
                cachedManifest = Resources.Load<PsychoArtAssetManifest>(ManifestResourcePath);
                attemptedLoad = true;
            }

            return cachedManifest;
        }

        private static void NormalizeHeight(Transform root, float targetHeight)
        {
            if (root == null || targetHeight <= 0f || !TryGetRendererBounds(root, out Bounds bounds) || bounds.size.y <= 0.001f)
            {
                return;
            }

            float scale = targetHeight / bounds.size.y;
            root.localScale *= scale;
        }

        private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            bounds = new Bounds(root.position, Vector3.zero);
            bool found = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return found;
        }

        private static void ConfigureRenderers(GameObject root, PsychoArtAssetEntry entry)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            ShadowCastingMode shadowMode = entry.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.shadowCastingMode = shadowMode;
                renderer.receiveShadows = entry.receiveShadows;
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        materials[materialIndex].enableInstancing = true;
                    }
                }
            }
        }

        private static void EnsureCullLod(GameObject root, float cullScreenRelativeHeight)
        {
            if (root.GetComponentInChildren<LODGroup>() != null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            LODGroup lodGroup = root.AddComponent<LODGroup>();
            float threshold = Mathf.Clamp(cullScreenRelativeHeight, 0.005f, 0.20f);
            lodGroup.SetLODs(new[] { new LOD(threshold, renderers) });
            lodGroup.RecalculateBounds();
        }

        private static void MarkStatic(GameObject root)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                children[i].gameObject.isStatic = true;
            }
        }

        private static int StableHash(int id, string name, string visualClass)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + id;
                hash = hash * 31 + StableHash(name);
                hash = hash * 31 + StableHash(visualClass);
                return hash;
            }
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                if (string.IsNullOrEmpty(value))
                {
                    return hash;
                }

                for (int i = 0; i < value.Length; i++)
                {
                    hash = hash * 31 + char.ToLowerInvariant(value[i]);
                }

                return hash;
            }
        }

        private static int PositiveModulo(int value, int divisor)
        {
            if (divisor <= 0)
            {
                return 0;
            }

            int remainder = value % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
        }
    }
}
