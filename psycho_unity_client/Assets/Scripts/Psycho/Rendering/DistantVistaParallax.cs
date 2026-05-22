using UnityEngine;

namespace Psycho.Rendering
{
    public sealed class DistantVistaParallax : MonoBehaviour
    {
        [SerializeField] private Transform viewer;
        [SerializeField] private float parallaxStrength = 0.035f;

        private Vector3 origin;
        private Vector3 viewerOrigin;

        private void Awake()
        {
            origin = transform.position;
            if (viewer == null && Camera.main != null)
            {
                viewer = Camera.main.transform;
            }

            viewerOrigin = viewer != null ? viewer.position : Vector3.zero;
        }

        private void LateUpdate()
        {
            if (viewer == null)
            {
                return;
            }

            Vector3 delta = viewer.position - viewerOrigin;
            delta.y = 0f;
            transform.position = origin + delta * parallaxStrength;
        }
    }
}
