using System;
using UnityEngine;

namespace Psycho.Gameplay
{
    public sealed class PsychoGiantProvocation : MonoBehaviour
    {
        [SerializeField] private PsychoMammothCompanion[] mammothCompanions = Array.Empty<PsychoMammothCompanion>();
        [SerializeField] private float defenseChance = 0.58f;
        [SerializeField] private string campName = "Giant camp";
        [SerializeField] private float cooldownSeconds = 3.0f;

        private float cooldownTimer;

        public void Configure(PsychoMammothCompanion[] companions, float chance, string displayCampName)
        {
            mammothCompanions = companions ?? Array.Empty<PsychoMammothCompanion>();
            defenseChance = Mathf.Clamp01(chance);
            campName = string.IsNullOrWhiteSpace(displayCampName) ? "Giant camp" : displayCampName;
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        private void OnPsychoInteract(string actionName)
        {
            if (!string.Equals(actionName, "Provoke", StringComparison.OrdinalIgnoreCase) || cooldownTimer > 0f)
            {
                return;
            }

            cooldownTimer = cooldownSeconds;
            Transform player = FindPlayerTarget();
            if (player == null)
            {
                Debug.Log($"{campName}: the giant was provoked, but no playable adventurer was found.");
                return;
            }

            int defenders = 0;
            for (int i = 0; i < mammothCompanions.Length; i++)
            {
                PsychoMammothCompanion mammoth = mammothCompanions[i];
                if (mammoth == null)
                {
                    continue;
                }

                float roll = Mathf.Repeat(Mathf.Sin(Time.time * 1.731f + transform.position.x * 0.137f + transform.position.z * 0.191f + i * 2.47f) * 43758.5453f, 1f);
                if (roll <= defenseChance)
                {
                    mammoth.DefendHonor(player);
                    defenders++;
                }
            }

            Debug.Log($"{campName}: {defenders} mammoth companion(s) answered the giant's provocation.");
        }

        private static Transform FindPlayerTarget()
        {
            GameObject player = GameObject.Find("Playable Adventurer");
            if (player != null)
            {
                return player.transform;
            }

            Camera camera = Camera.main;
            return camera == null ? null : camera.transform;
        }
    }
}
