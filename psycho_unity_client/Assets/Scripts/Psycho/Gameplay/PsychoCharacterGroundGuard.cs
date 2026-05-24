using UnityEngine;

namespace Psycho.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PsychoCharacterGroundGuard : MonoBehaviour
    {
        [SerializeField] private float probeHeight = 6f;
        [SerializeField] private float probeDistance = 18f;
        [SerializeField] private float groundOffset = 0.04f;
        [SerializeField] private float sunkTolerance = 0.08f;
        [SerializeField] private float fallResetDepth = 4.5f;

        private readonly RaycastHit[] hits = new RaycastHit[32];
        private CharacterController controller;
        private Vector3 lastSafePosition;
        private bool hasLastSafePosition;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            lastSafePosition = transform.position;
            hasLastSafePosition = true;
        }

        private void Start()
        {
            if (TryFindGround(out RaycastHit groundHit))
            {
                Vector3 groundedPosition = new Vector3(transform.position.x, GroundedCenterY(groundHit.point.y), transform.position.z);
                Teleport(groundedPosition);
                lastSafePosition = groundedPosition;
                hasLastSafePosition = true;
            }
        }

        private void LateUpdate()
        {
            if (!TryFindGround(out RaycastHit groundHit))
            {
                if (hasLastSafePosition && transform.position.y < lastSafePosition.y - fallResetDepth)
                {
                    Teleport(lastSafePosition);
                }

                return;
            }

            float targetY = GroundedCenterY(groundHit.point.y);
            float feetY = FeetY();
            if (feetY < groundHit.point.y - sunkTolerance || transform.position.y < targetY - sunkTolerance)
            {
                Teleport(new Vector3(transform.position.x, targetY, transform.position.z));
            }

            if (Mathf.Abs(FeetY() - groundHit.point.y) <= 0.22f)
            {
                lastSafePosition = new Vector3(transform.position.x, GroundedCenterY(groundHit.point.y), transform.position.z);
                hasLastSafePosition = true;
            }
        }

        private bool TryFindGround(out RaycastHit bestHit)
        {
            bestHit = default;
            bool foundTerrain = false;
            bool foundFallback = false;
            RaycastHit fallbackHit = default;
            float bestTerrainDistance = float.MaxValue;
            float bestFallbackDistance = float.MaxValue;
            float radius = Mathf.Max(0.05f, controller.radius * 0.82f);

            for (int offsetIndex = 0; offsetIndex < 5; offsetIndex++)
            {
                Vector3 offset = ProbeOffset(offsetIndex, radius);
                Vector3 origin = transform.position + offset + Vector3.up * probeHeight;
                int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hits, probeDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit hit = hits[i];
                    if (!IsUsableGroundHit(hit))
                    {
                        continue;
                    }

                    if (IsTerrainCollider(hit.collider))
                    {
                        if (hit.distance < bestTerrainDistance)
                        {
                            bestTerrainDistance = hit.distance;
                            bestHit = hit;
                            foundTerrain = true;
                        }
                    }
                    else if (!foundTerrain && hit.distance < bestFallbackDistance)
                    {
                        bestFallbackDistance = hit.distance;
                        fallbackHit = hit;
                        foundFallback = true;
                    }
                }
            }

            if (foundTerrain)
            {
                return true;
            }

            if (foundFallback)
            {
                bestHit = fallbackHit;
                return true;
            }

            return false;
        }

        private static Vector3 ProbeOffset(int index, float radius)
        {
            switch (index)
            {
                case 1:
                    return new Vector3(radius, 0f, 0f);
                case 2:
                    return new Vector3(-radius, 0f, 0f);
                case 3:
                    return new Vector3(0f, 0f, radius);
                case 4:
                    return new Vector3(0f, 0f, -radius);
                default:
                    return Vector3.zero;
            }
        }

        private bool IsUsableGroundHit(RaycastHit hit)
        {
            Collider hitCollider = hit.collider;
            if (hitCollider == null || hitCollider.isTrigger)
            {
                return false;
            }

            return !hitCollider.transform.IsChildOf(transform);
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

        private float FeetY()
        {
            return transform.position.y + controller.center.y - controller.height * 0.5f;
        }

        private float GroundedCenterY(float groundY)
        {
            return groundY - controller.center.y + controller.height * 0.5f + groundOffset;
        }

        private void Teleport(Vector3 position)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = position;
            controller.enabled = wasEnabled;
        }
    }
}
