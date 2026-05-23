using Psycho.UI;
using UnityEngine;

namespace Psycho.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PsychoPlayableCharacter : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float walkSpeed = 4.25f;
        [SerializeField] private float sprintSpeed = 7.25f;
        [SerializeField] private float jumpSpeed = 5.6f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float thirdPersonDistance = 3.8f;
        [SerializeField] private float thirdPersonHeight = 1.45f;
        [SerializeField] private float cameraSideOffset = 0.28f;
        [SerializeField] private float minPitch = -32f;
        [SerializeField] private float maxPitch = 58f;
        [SerializeField] private LayerMask cameraCollisionMask = ~0;
        [SerializeField] private float cameraCollisionRadius = 0.22f;
        [SerializeField] private float cameraSmoothTime = 0.055f;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;
        private Vector3 cameraVelocity;
        private readonly RaycastHit[] cameraHits = new RaycastHit[8];

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }

            EnsureCameraRig();
            pitch = Mathf.Clamp(NormalizeAngle(cameraPivot.localEulerAngles.x), minPitch, maxPitch);
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                Cursor.lockState = PsychoHudController.HasActiveHud ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = PsychoHudController.HasActiveHud;
            }
        }

        private void Update()
        {
            bool hudActive = PsychoHudController.HasActiveHud;
            if (PsychoHudController.BlocksPlayerInput)
            {
                return;
            }

            if (!hudActive || Input.GetMouseButton(1))
            {
                Look();
            }

            Move();

            if (!hudActive && Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Look()
        {
            float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            transform.Rotate(0f, yaw, 0f);

            if (cameraPivot == null)
            {
                return;
            }

            pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);
            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            Vector3 planar = transform.right * input.x + transform.forward * input.y;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (controller.isGrounded && Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpSpeed;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = planar * speed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void LateUpdate()
        {
            UpdateCameraRig();
        }

        private void EnsureCameraRig()
        {
            if (cameraPivot == null)
            {
                Transform existingPivot = transform.Find("Player Camera Pivot");
                if (existingPivot == null)
                {
                    GameObject pivotObject = new GameObject("Player Camera Pivot");
                    pivotObject.transform.SetParent(transform, false);
                    existingPivot = pivotObject.transform;
                }

                cameraPivot = existingPivot;
            }

            cameraPivot.localPosition = new Vector3(0f, thirdPersonHeight, 0f);

            if (playerCamera != null)
            {
                playerCamera.transform.SetParent(cameraPivot, true);
                playerCamera.transform.localRotation = Quaternion.identity;
            }
        }

        private void UpdateCameraRig()
        {
            if (playerCamera == null)
            {
                return;
            }

            EnsureCameraRig();

            Vector3 focus = cameraPivot.position;
            Vector3 desiredPosition = focus
                - cameraPivot.forward * thirdPersonDistance
                + cameraPivot.right * cameraSideOffset;
            desiredPosition = ResolveCameraCollision(focus, desiredPosition);

            Transform cameraTransform = playerCamera.transform;
            cameraTransform.position = Vector3.SmoothDamp(
                cameraTransform.position,
                desiredPosition,
                ref cameraVelocity,
                cameraSmoothTime,
                100f,
                Time.deltaTime);
            cameraTransform.rotation = Quaternion.LookRotation(focus - cameraTransform.position, Vector3.up);
        }

        private Vector3 ResolveCameraCollision(Vector3 focus, Vector3 desiredPosition)
        {
            Vector3 toDesired = desiredPosition - focus;
            float distance = toDesired.magnitude;
            if (distance <= 0.01f)
            {
                return desiredPosition;
            }

            Vector3 direction = toDesired / distance;
            int hitCount = Physics.SphereCastNonAlloc(
                focus,
                cameraCollisionRadius,
                direction,
                cameraHits,
                distance,
                cameraCollisionMask,
                QueryTriggerInteraction.Ignore);

            float closestDistance = distance;
            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = cameraHits[i].collider;
                if (hitCollider == null || hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
                {
                    continue;
                }

                closestDistance = Mathf.Min(closestDistance, Mathf.Max(cameraHits[i].distance - 0.18f, 0.45f));
            }

            return focus + direction * closestDistance;
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
