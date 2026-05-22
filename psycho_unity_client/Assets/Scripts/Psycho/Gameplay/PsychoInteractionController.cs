using UnityEngine;

namespace Psycho.Gameplay
{
    public sealed class PsychoInteractionController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float interactionDistance = 5.5f;
        [SerializeField] private LayerMask interactionMask = ~0;

        private string lastMessage;
        private float messageUntil;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E))
            {
                return;
            }

            TryInteract();
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(lastMessage) || Time.time > messageUntil)
            {
                return;
            }

            GUI.Label(new Rect(18f, 18f, 720f, 28f), lastMessage);
        }

        private void TryInteract()
        {
            if (targetCamera == null)
            {
                return;
            }

            Ray ray = new Ray(targetCamera.transform.position, targetCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask, QueryTriggerInteraction.Collide))
            {
                return;
            }

            PsychoInteractable interactable = hit.collider.GetComponentInParent<PsychoInteractable>();
            if (interactable == null)
            {
                return;
            }

            lastMessage = interactable.Interact(0);
            messageUntil = Time.time + 2.8f;
        }
    }
}
