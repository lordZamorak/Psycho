using UnityEngine;

namespace Psycho.Rendering
{
    public sealed class PrototypeNpcWander : MonoBehaviour
    {
        [SerializeField] private float wanderRadius = 4f;
        [SerializeField] private float speed = 2.4f;
        [SerializeField] private float turnSpeed = 8f;
        [SerializeField] private float pauseDuration = 0.8f;

        private Vector3 origin;
        private Vector3 destination;
        private float pauseTimer;
        private float phase;

        private void Awake()
        {
            origin = transform.position;
            phase = origin.x * 0.117f + origin.z * 0.091f;
            PickDestination();
        }

        private void Update()
        {
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                return;
            }

            Vector3 delta = destination - transform.position;
            delta.y = 0f;
            if (delta.magnitude < 0.12f)
            {
                pauseTimer = pauseDuration + Mathf.PerlinNoise(Time.time * 0.25f, phase) * 0.9f;
                PickDestination();
                return;
            }

            Vector3 direction = delta.normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }

        private void PickDestination()
        {
            float angle = Mathf.PerlinNoise(Time.time * 0.37f + phase, phase * 0.73f) * Mathf.PI * 2f;
            float radius = Mathf.Lerp(wanderRadius * 0.25f, wanderRadius, Mathf.PerlinNoise(phase, Time.time * 0.23f));
            destination = origin + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

    }
}
