using System;
using Psycho.Mirror;
using UnityEngine;

namespace Psycho.Rendering
{
    public enum PsychoArtAssetKind
    {
        Player,
        Npc,
        Object,
        Item,
        Foliage,
        Terrain
    }

    [CreateAssetMenu(menuName = "Psycho/Art Asset Manifest", fileName = "PsychoArtAssetManifest")]
    public sealed class PsychoArtAssetManifest : ScriptableObject
    {
        [SerializeField] private PsychoArtAssetEntry[] entries = Array.Empty<PsychoArtAssetEntry>();

        public PsychoArtAssetEntry[] Entries => entries ?? Array.Empty<PsychoArtAssetEntry>();
    }

    [Serializable]
    public sealed class PsychoArtAssetEntry
    {
        public PsychoArtAssetKind kind;
        public string displayName;
        public GameObject prefab;
        public int[] ids = Array.Empty<int>();
        public string visualClass;
        public string[] nameFragments = Array.Empty<string>();
        public float targetHeight = 0f;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEuler = Vector3.zero;
        public Vector3 localScale = Vector3.one;
        public bool markStatic;
        public bool ensureCullLod = true;
        public float cullScreenRelativeHeight = 0.018f;
        public bool receiveShadows = true;
        public bool castShadows = true;

        public bool MatchesPlayer(string playerName)
        {
            return kind == PsychoArtAssetKind.Player && MatchesName(playerName);
        }

        public bool MatchesNpc(PsychoMirrorNpc npc)
        {
            if (kind != PsychoArtAssetKind.Npc || npc == null)
            {
                return false;
            }

            return MatchesId(npc.id) || MatchesVisualClass(npc.visualClass) || MatchesName(npc.name);
        }

        public bool MatchesObject(PsychoMirrorObject worldObject)
        {
            if (kind != PsychoArtAssetKind.Object || worldObject == null)
            {
                return false;
            }

            return MatchesId(worldObject.id) || MatchesVisualClass(worldObject.visualClass) || MatchesName(worldObject.name);
        }

        public bool MatchesItem(PsychoMirrorItem item)
        {
            if (kind != PsychoArtAssetKind.Item || item == null)
            {
                return false;
            }

            return MatchesId(item.id) || MatchesVisualClass(item.visualClass) || MatchesName(item.name);
        }

        private bool MatchesId(int id)
        {
            if (ids == null)
            {
                return false;
            }

            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchesVisualClass(string candidate)
        {
            return !string.IsNullOrWhiteSpace(visualClass)
                && string.Equals(visualClass.Trim(), candidate?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesName(string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate) || nameFragments == null)
            {
                return false;
            }

            string normalized = candidate.Trim().ToLowerInvariant();
            for (int i = 0; i < nameFragments.Length; i++)
            {
                string fragment = nameFragments[i];
                if (!string.IsNullOrWhiteSpace(fragment) && normalized.Contains(fragment.Trim().ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
