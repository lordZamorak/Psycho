using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Rendering
{
    public sealed class PsychoRuntimeVisualQuality : MonoBehaviour
    {
        public const string PresetKey = "PsychoVisualQualityPreset";

        [SerializeField] private int antiAliasing = 4;
        [SerializeField] private int pixelLightCount = 2;
        [SerializeField] private int shadowCascades = 2;
        [SerializeField] private float shadowDistance = 150f;
        [SerializeField] private float lodBias = 1.25f;
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            if (PlayerPrefs.HasKey(PresetKey))
            {
                ApplyPreset(PlayerPrefs.GetInt(PresetKey, 2), false);
            }
            else
            {
                ApplyValues(antiAliasing, pixelLightCount, shadowCascades, shadowDistance, lodBias, targetFrameRate);
            }
        }

        public static int CurrentPreset => Mathf.Clamp(PlayerPrefs.GetInt(PresetKey, 2), 0, 2);

        public static string CurrentPresetName => PresetName(CurrentPreset);

        public static void ApplyPreset(int preset, bool save = true)
        {
            int clamped = Mathf.Clamp(preset, 0, 2);
            if (save)
            {
                PlayerPrefs.SetInt(PresetKey, clamped);
                PlayerPrefs.Save();
            }

            switch (clamped)
            {
                case 0:
                    ApplyValues(0, 1, 1, 55f, 0.80f, 60);
                    break;
                case 1:
                    ApplyValues(2, 2, 2, 105f, 1.10f, 60);
                    break;
                default:
                    ApplyValues(4, 3, 4, 180f, 1.55f, 60);
                    break;
            }
        }

        public static string PresetName(int preset)
        {
            switch (Mathf.Clamp(preset, 0, 2))
            {
                case 0:
                    return "Performance";
                case 1:
                    return "Balanced";
                default:
                    return "Ultra";
            }
        }

        private static void ApplyValues(int antiAliasing, int pixelLightCount, int shadowCascades, float shadowDistance, float lodBias, int targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.antiAliasing = Mathf.Clamp(antiAliasing, 0, 8);
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.pixelLightCount = Mathf.Clamp(pixelLightCount, 0, 4);
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;
            QualitySettings.shadowDistance = Mathf.Clamp(shadowDistance, 45f, 240f);
            QualitySettings.shadowCascades = Mathf.Clamp(shadowCascades, 1, 4);
            QualitySettings.lodBias = Mathf.Clamp(lodBias, 0.75f, 2.0f);
            QualitySettings.softParticles = true;
            QualitySettings.softVegetation = true;
            QualitySettings.realtimeReflectionProbes = false;
        }
    }
}
