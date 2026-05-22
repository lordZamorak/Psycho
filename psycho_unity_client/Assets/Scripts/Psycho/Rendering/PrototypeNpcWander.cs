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
                AnimateIdle();
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
            AnimateRun();
        }

        private void PickDestination()
        {
            float angle = Mathf.PerlinNoise(Time.time * 0.37f + phase, phase * 0.73f) * Mathf.PI * 2f;
            float radius = Mathf.Lerp(wanderRadius * 0.25f, wanderRadius, Mathf.PerlinNoise(phase, Time.time * 0.23f));
            destination = origin + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        private void AnimateRun()
        {
            float stride = Mathf.Sin(Time.time * 9.5f + phase) * 0.055f;
            transform.localScale = new Vector3(1f + Mathf.Abs(stride) * 0.25f, 1f + stride, 1f - Mathf.Abs(stride) * 0.18f);
        }

        private void AnimateIdle()
        {
            float breathe = Mathf.Sin(Time.time * 2.3f + phase) * 0.018f;
            transform.localScale = new Vector3(1f + breathe, 1f - breathe, 1f + breathe);
        }
    }
}
