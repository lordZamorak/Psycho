using UnityEngine;

namespace Psycho.Rendering
{
    public sealed class PrototypeNpcWander : MonoBehaviour
    {
        [SerializeField] private float wanderRadius = 4f;
        [SerializeField] private float speed = 2.4f;
        [SerializeField] private float turnSpeed = 5.5f;
        [SerializeField] private float acceleration = 3.35f;
        [SerializeField] private float arrivalDistance = 0.32f;
        [SerializeField] private float pauseDuration = 0.8f;
        [SerializeField] private float visualStrideBob = 0.0035f;
        [SerializeField] private float visualStrideSway = 0.42f;

        private Vector3 origin;
        private Vector3 destination;
        private Vector3 velocity;
        private float pauseTimer;
        private float phase;
        private float stride;
        private Transform visualRoot;
        private Vector3 visualBaseLocalPosition;
        private Quaternion visualBaseLocalRotation;

        private void Awake()
        {
            origin = transform.position;
            phase = origin.x * 0.117f + origin.z * 0.091f;
            CacheVisualRoot();
            PickDestination();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            if (pauseTimer > 0f)
            {
                pauseTimer -= deltaTime;
                velocity = Vector3.MoveTowards(velocity, Vector3.zero, acceleration * deltaTime);
                MoveAndFace(deltaTime);
                AnimateVisual(deltaTime);
                return;
            }

            Vector3 delta = destination - transform.position;
            delta.y = 0f;
            float distance = delta.magnitude;
            if (distance < arrivalDistance && velocity.sqrMagnitude < 0.012f)
            {
                pauseTimer = pauseDuration + Mathf.PerlinNoise(Time.time * 0.25f, phase) * 0.9f;
                PickDestination();
                return;
            }

            Vector3 desiredVelocity = Vector3.zero;
            if (distance > 0.001f)
            {
                Vector3 direction = delta / distance;
                float arrival = Mathf.Clamp01(distance / Mathf.Max(arrivalDistance * 4f, wanderRadius * 0.35f));
                float easedSpeed = speed * Mathf.SmoothStep(0.22f, 1f, arrival);
                desiredVelocity = direction * easedSpeed;
            }

            velocity = Vector3.MoveTowards(velocity, desiredVelocity, acceleration * deltaTime);
            MoveAndFace(deltaTime);
            AnimateVisual(deltaTime);
        }

        private void PickDestination()
        {
            float angle = Mathf.PerlinNoise(Time.time * 0.37f + phase, phase * 0.73f) * Mathf.PI * 2f;
            float radius = Mathf.Lerp(wanderRadius * 0.25f, wanderRadius, Mathf.PerlinNoise(phase, Time.time * 0.23f));
            destination = origin + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        private void MoveAndFace(float deltaTime)
        {
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (flatVelocity.sqrMagnitude > 0.0001f && !WouldHitBlockingCollider(flatVelocity.normalized, flatVelocity.magnitude * deltaTime))
            {
                transform.position += flatVelocity * deltaTime;
            }
            else if (flatVelocity.sqrMagnitude > 0.0001f)
            {
                velocity *= 0.2f;
                PickDestination();
            }

            if (flatVelocity.sqrMagnitude > 0.0012f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatVelocity.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * deltaTime);
            }
        }

        private bool WouldHitBlockingCollider(Vector3 direction, float distance)
        {
            if (distance <= 0f)
            {
                return false;
            }

            Vector3 originPoint = transform.position + Vector3.up * 0.46f;
            if (!Physics.SphereCast(originPoint, 0.22f, direction, out RaycastHit hit, distance + 0.18f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
            {
                return false;
            }

            return !IsTerrainCollider(hit.collider);
        }

        private void CacheVisualRoot()
        {
            if (transform.childCount == 0)
            {
                return;
            }

            visualRoot = transform.GetChild(0);
            visualBaseLocalPosition = visualRoot.localPosition;
            visualBaseLocalRotation = visualRoot.localRotation;
        }

        private void AnimateVisual(float deltaTime)
        {
            if (visualRoot == null)
            {
                return;
            }

            float planarSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            float motion = Mathf.Clamp01(planarSpeed / Mathf.Max(0.01f, speed));
            stride += deltaTime * Mathf.Lerp(1.05f, 3.85f, motion);
            float idleBreath = Mathf.Sin(Time.time * 1.15f + phase) * 0.0022f;
            float walkBob = Mathf.Sin(stride * Mathf.PI * 2f) * visualStrideBob * motion;
            float walkSway = Mathf.Sin(stride * Mathf.PI) * visualStrideSway * motion;
            visualRoot.localPosition = visualBaseLocalPosition + Vector3.up * (idleBreath + walkBob);
            visualRoot.localRotation = visualBaseLocalRotation * Quaternion.Euler(0f, 0f, walkSway);
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
    }
}
