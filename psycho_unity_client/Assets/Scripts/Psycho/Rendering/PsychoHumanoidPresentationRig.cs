using UnityEngine;

namespace Psycho.Rendering
{
    public sealed class PsychoHumanoidPresentationRig : MonoBehaviour
    {
        [SerializeField] private Transform motionSource;
        [SerializeField] private float idleBreathAmplitude = 0.006f;
        [SerializeField] private float idleBreathFrequency = 0.92f;
        [SerializeField] private float gaitBobAmplitude = 0.018f;
        [SerializeField] private float gaitRollDegrees = 1.6f;
        [SerializeField] private float gaitPitchDegrees = 0.85f;
        [SerializeField] private float turnLeanDegrees = 1.15f;
        [SerializeField] private float strideCyclesPerMeter = 1.68f;
        [SerializeField] private float motionSmoothing = 8.5f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 previousSourcePosition;
        private Quaternion previousSourceRotation;
        private float smoothedSpeed;
        private float smoothedTurnSpeed;
        private float stride;
        private float phase;
        private bool initialized;

        public void Configure(
            Transform source,
            float idleAmplitude,
            float bobAmplitude,
            float rollDegrees,
            float pitchDegrees,
            float leanDegrees)
        {
            motionSource = source;
            idleBreathAmplitude = idleAmplitude;
            gaitBobAmplitude = bobAmplitude;
            gaitRollDegrees = rollDegrees;
            gaitPitchDegrees = pitchDegrees;
            turnLeanDegrees = leanDegrees;
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                Initialize();
            }

            Transform source = ResolveMotionSource();
            if (source == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            Vector3 sourcePosition = source.position;
            Quaternion sourceRotation = source.rotation;
            Vector3 flatDelta = sourcePosition - previousSourcePosition;
            flatDelta.y = 0f;

            float planarDistance = flatDelta.magnitude;
            float targetSpeed = planarDistance / deltaTime;
            float yawDelta = Mathf.DeltaAngle(previousSourceRotation.eulerAngles.y, sourceRotation.eulerAngles.y);
            float targetTurnSpeed = yawDelta / deltaTime;
            float smoothing = 1f - Mathf.Exp(-motionSmoothing * deltaTime);
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, targetSpeed, smoothing);
            smoothedTurnSpeed = Mathf.Lerp(smoothedTurnSpeed, targetTurnSpeed, smoothing);
            stride += planarDistance * strideCyclesPerMeter;

            float motion = Mathf.Clamp01(smoothedSpeed / 3.8f);
            float strideRadians = stride * Mathf.PI * 2f + phase;
            float idleBreath = Mathf.Sin(Time.time * idleBreathFrequency + phase) * idleBreathAmplitude;
            float walkBob = Mathf.Abs(Mathf.Sin(strideRadians)) * gaitBobAmplitude * motion;
            float roll = Mathf.Sin(strideRadians) * gaitRollDegrees * motion;
            float pitch = -gaitPitchDegrees * motion + Mathf.Sin(strideRadians + Mathf.PI * 0.5f) * gaitPitchDegrees * 0.28f * motion;
            float turnLean = Mathf.Clamp(smoothedTurnSpeed * 0.015f, -turnLeanDegrees, turnLeanDegrees);

            transform.localPosition = baseLocalPosition + Vector3.up * (idleBreath + walkBob);
            transform.localRotation = baseLocalRotation * Quaternion.Euler(pitch, 0f, -roll - turnLean);

            previousSourcePosition = sourcePosition;
            previousSourceRotation = sourceRotation;
        }

        private void Initialize()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            Transform source = ResolveMotionSource();
            if (source != null)
            {
                previousSourcePosition = source.position;
                previousSourceRotation = source.rotation;
            }

            phase = Mathf.Abs(transform.position.x * 0.73f + transform.position.z * 0.41f) % (Mathf.PI * 2f);
            initialized = true;
        }

        private Transform ResolveMotionSource()
        {
            if (motionSource != null)
            {
                return motionSource;
            }

            Transform current = transform.parent;
            while (current != null)
            {
                if (current.GetComponent<CharacterController>() != null || current.GetComponent<PrototypeNpcWander>() != null)
                {
                    return current;
                }

                current = current.parent;
            }

            if (transform.parent != null && transform.parent.parent != null)
            {
                return transform.parent.parent;
            }

            return transform.parent != null ? transform.parent : transform;
        }
    }
}
