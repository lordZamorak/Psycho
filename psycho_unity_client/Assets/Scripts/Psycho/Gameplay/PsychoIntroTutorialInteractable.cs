using System;
using Psycho.UI;
using UnityEngine;

namespace Psycho.Gameplay
{
    public sealed class PsychoIntroTutorialInteractable : MonoBehaviour
    {
        public const string LootSignal = "underkeep_loot";
        public const string CombatSignal = "underkeep_combat";

        public static event Action<string> SignalRaised;

        [SerializeField] private string signalId;
        [SerializeField] private string toastMessage;
        [SerializeField] private bool hideAfterUse;
        [SerializeField] private bool disableCollidersAfterUse = true;

        private bool completed;

        public void Configure(string signal, string toast, bool hideAfterUse = false, bool disableCollidersAfterUse = true)
        {
            signalId = signal;
            toastMessage = toast;
            this.hideAfterUse = hideAfterUse;
            this.disableCollidersAfterUse = disableCollidersAfterUse;
            completed = false;
        }

        private void OnPsychoInteract(string actionName)
        {
            if (completed || string.IsNullOrWhiteSpace(signalId))
            {
                return;
            }

            completed = true;
            PsychoHudController hud = FindAnyObjectByType<PsychoHudController>();
            if (hud != null && !string.IsNullOrWhiteSpace(toastMessage))
            {
                hud.ShowToastMessage(toastMessage);
            }

            SignalRaised?.Invoke(signalId);

            if (disableCollidersAfterUse)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = false;
                }
            }

            if (hideAfterUse)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                }
            }
        }
    }
}
