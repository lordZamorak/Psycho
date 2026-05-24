using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Rendering
{
    public sealed class PsychoRuntimeVisualQuality : MonoBehaviour
    {
        [SerializeField] private int antiAliasing = 4;
        [SerializeField] private int pixelLightCount = 2;
        [SerializeField] private int shadowCascades = 2;
        [SerializeField] private float shadowDistance = 150f;
        [SerializeField] private float lodBias = 1.25f;
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Apply();
        }

        private void Apply()
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
