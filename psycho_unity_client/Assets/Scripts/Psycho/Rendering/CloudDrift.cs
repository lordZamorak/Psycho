using UnityEngine;

namespace Psycho.Rendering
{
    public sealed class CloudDrift : MonoBehaviour
    {
        [SerializeField] private Vector3 driftDirection = new Vector3(1f, 0f, 0.35f);
        [SerializeField] private float driftSpeed = 0.55f;
        [SerializeField] private float wrapDistance = 115f;
        [SerializeField] private float pulseScale = 0.035f;

        private Vector3 origin;
        private Vector3 baseScale;
        private float phase;

        private void Awake()
        {
            origin = transform.position;
            baseScale = transform.localScale;
            driftDirection = driftDirection.sqrMagnitude <= 0.001f ? Vector3.right : driftDirection.normalized;
            phase = origin.x * 0.071f + origin.z * 0.053f;
        }

        private void Update()
        {
            float offset = Mathf.Repeat(Time.time * driftSpeed + phase, wrapDistance * 2f) - wrapDistance;
            transform.position = origin + driftDirection * offset;
            float pulse = 1f + Mathf.Sin(Time.time * 0.18f + phase) * pulseScale;
            transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z * (1f + (pulse - 1f) * 0.4f));
        }
    }
}
