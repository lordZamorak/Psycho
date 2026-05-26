using System;
using UnityEngine;

namespace Psycho.Gameplay
{
    public sealed class PsychoInteractable : MonoBehaviour
    {
        [SerializeField] private int objectId;
        [SerializeField] private string displayName;
        [SerializeField] private string[] actions = Array.Empty<string>();

        public int ObjectId => objectId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? $"Object {objectId}" : displayName;

        public void Configure(int id, string name, string[] availableActions)
        {
            objectId = id;
            displayName = name;
            actions = availableActions ?? Array.Empty<string>();
        }

        public string Interact(int actionIndex)
        {
            string verb = ActionName(actionIndex);
            string message = $"{verb} {DisplayName}";
            PsychoInteractionResponse response = GetComponent<PsychoInteractionResponse>();
            if (response != null)
            {
                message = response.BuildMessage(verb, DisplayName);
            }

            Debug.Log($"Psycho interaction: {message} ({objectId})");
            SendMessage("OnPsychoInteract", verb, SendMessageOptions.DontRequireReceiver);
            return message;
        }

        private string ActionName(int actionIndex)
        {
            if (actions != null && actionIndex >= 0 && actionIndex < actions.Length && !string.IsNullOrWhiteSpace(actions[actionIndex]))
            {
                return actions[actionIndex];
            }

            return "Inspect";
        }
    }
}
