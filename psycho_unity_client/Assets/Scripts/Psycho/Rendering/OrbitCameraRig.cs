using UnityEngine;

namespace Psycho.Rendering
{
    public sealed class OrbitCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 13.5f;
        [SerializeField] private float height = 3.2f;
        [SerializeField] private float pitch = 24f;
        [SerializeField] private float focusHeight = 1.25f;
        [SerializeField] private float rotationSpeed = 18f;

        private float yaw = 42f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target.position + rotation * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            transform.LookAt(target.position + Vector3.up * focusHeight);
        }
    }
}
