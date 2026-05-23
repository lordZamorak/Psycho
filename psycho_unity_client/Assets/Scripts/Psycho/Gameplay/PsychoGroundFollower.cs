using System;
using System.Collections.Generic;
using UnityEngine;

namespace Psycho.Gameplay
{
    public sealed class PsychoGroundFollower : MonoBehaviour
    {
        [SerializeField] private float probeHeight = 3f;
        [SerializeField] private float probeDistance = 8f;
        [SerializeField] private float verticalSmoothTime = 0.055f;
        [SerializeField] private float maxVerticalSpeed = 18f;
        [SerializeField] private LayerMask groundMask = ~0;

        private readonly RaycastHit[] hits = new RaycastHit[12];
        private float verticalVelocity;

        private void LateUpdate()
        {
            Vector3 origin = transform.position + Vector3.up * probeHeight;
            int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hits, probeDistance, groundMask, QueryTriggerInteraction.Ignore);
            if (hitCount <= 0)
            {
                return;
            }

            Array.Sort(hits, 0, hitCount, HitDistanceComparer.Instance);
            bool foundTerrain = false;
            bool foundFallback = false;
            RaycastHit terrainHit = default;
            RaycastHit fallbackHit = default;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null || hit.collider.isTrigger || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (IsTerrainCollider(hit.collider))
                {
                    terrainHit = hit;
                    foundTerrain = true;
                    break;
                }

                if (!foundFallback)
                {
                    fallbackHit = hit;
                    foundFallback = true;
                }
            }

            if (!foundTerrain && !foundFallback)
            {
                return;
            }

            RaycastHit selectedHit = foundTerrain ? terrainHit : fallbackHit;
            Vector3 position = transform.position;
            position.y = Mathf.SmoothDamp(position.y, selectedHit.point.y, ref verticalVelocity, verticalSmoothTime, maxVerticalSpeed);
            transform.position = position;
        }

        private static bool IsTerrainCollider(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return false;
            }

            if (hitCollider.gameObject.name.Contains("Terrain"))
            {
                return true;
            }

            MeshCollider meshCollider = hitCollider as MeshCollider;
            return meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.name.StartsWith("psycho_map_region_");
        }

        private sealed class HitDistanceComparer : IComparer<RaycastHit>
        {
            public static readonly HitDistanceComparer Instance = new HitDistanceComparer();

            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}
