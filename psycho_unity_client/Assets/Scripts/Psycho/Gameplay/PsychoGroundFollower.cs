using System;
using System.Collections.Generic;
using UnityEngine;

namespace Psycho.Gameplay
{
    public sealed class PsychoGroundFollower : MonoBehaviour
    {
        [SerializeField] private float probeHeight = 3f;
        [SerializeField] private float probeDistance = 8f;
        [SerializeField] private float verticalLerp = 18f;
        [SerializeField] private LayerMask groundMask = ~0;

        private readonly RaycastHit[] hits = new RaycastHit[12];

        private void LateUpdate()
        {
            Vector3 origin = transform.position + Vector3.up * probeHeight;
            int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hits, probeDistance, groundMask, QueryTriggerInteraction.Ignore);
            if (hitCount <= 0)
            {
                return;
            }

            Array.Sort(hits, 0, hitCount, HitDistanceComparer.Instance);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                Vector3 position = transform.position;
                position.y = Mathf.Lerp(position.y, hit.point.y, verticalLerp * Time.deltaTime);
                transform.position = position;
                return;
            }
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
