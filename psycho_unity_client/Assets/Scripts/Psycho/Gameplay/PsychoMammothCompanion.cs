using Psycho.Rendering;
using UnityEngine;

namespace Psycho.Gameplay
{
    public sealed class PsychoMammothCompanion : MonoBehaviour
    {
        [SerializeField] private Transform leader;
        [SerializeField] private Vector3 followOffset = new Vector3(2.8f, 0f, -2.4f);
        [SerializeField] private float followDistance = 2.2f;
        [SerializeField] private float leashDistance = 5.4f;
        [SerializeField] private float followSpeed = 1.35f;
        [SerializeField] private float defendSpeed = 3.35f;
        [SerializeField] private float defendDuration = 8.0f;
        [SerializeField] private float defendStopDistance = 2.1f;
        [SerializeField] private float turnSpeed = 4.8f;

        private PrototypeNpcWander wander;
        private Transform defendTarget;
        private float defendTimer;

        public void Configure(Transform leaderTransform, Vector3 offset, float followRadius, float leashRadius, float companionSpeed, float defenseSpeed, float defenseTime)
        {
            leader = leaderTransform;
            followOffset = offset;
            followDistance = followRadius;
            leashDistance = leashRadius;
            followSpeed = companionSpeed;
            defendSpeed = defenseSpeed;
            defendDuration = defenseTime;
        }

        public void DefendHonor(Transform target)
        {
            if (target == null)
            {
                return;
            }

            defendTarget = target;
            defendTimer = defendDuration;
            SetWanderEnabled(false);
            Debug.Log($"{name} is defending its giant companion.");
        }

        private void Awake()
        {
            wander = GetComponent<PrototypeNpcWander>();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            if (defendTimer > 0f && defendTarget != null)
            {
                defendTimer -= deltaTime;
                MoveToward(defendTarget.position, defendStopDistance, defendSpeed, deltaTime);
                if (defendTimer <= 0f)
                {
                    defendTarget = null;
                    SetWanderEnabled(true);
                }

                return;
            }

            if (leader == null)
            {
                SetWanderEnabled(true);
                return;
            }

            Vector3 guardPoint = leader.TransformPoint(followOffset);
            Vector3 flatDelta = guardPoint - transform.position;
            flatDelta.y = 0f;
            if (flatDelta.magnitude > leashDistance)
            {
                SetWanderEnabled(false);
                MoveToward(guardPoint, followDistance, followSpeed, deltaTime);
            }
            else
            {
                SetWanderEnabled(true);
            }
        }

        private void MoveToward(Vector3 target, float stopDistance, float speed, float deltaTime)
        {
            Vector3 flatDelta = target - transform.position;
            flatDelta.y = 0f;
            float distance = flatDelta.magnitude;
            if (distance <= 0.001f)
            {
                return;
            }

            Vector3 direction = flatDelta / distance;
            if (distance > stopDistance)
            {
                transform.position += direction * Mathf.Min(speed * deltaTime, distance - stopDistance);
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * deltaTime);
        }

        private void SetWanderEnabled(bool enabled)
        {
            if (wander == null)
            {
                wander = GetComponent<PrototypeNpcWander>();
            }

            if (wander != null && wander.enabled != enabled)
            {
                wander.enabled = enabled;
            }
        }
    }
}
