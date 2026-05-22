using UnityEngine;

namespace Psycho.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PsychoPlayableCharacter : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float walkSpeed = 4.25f;
        [SerializeField] private float sprintSpeed = 7.25f;
        [SerializeField] private float jumpSpeed = 5.6f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float mouseSensitivity = 2.2f;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            Look();
            Move();

            if (Input.GetKeyDown(KeyCode.Escape))
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

            if (playerCamera == null)
            {
                return;
            }

            pitch = Mathf.Clamp(pitch - mouseY, -78f, 78f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
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
    }
}
