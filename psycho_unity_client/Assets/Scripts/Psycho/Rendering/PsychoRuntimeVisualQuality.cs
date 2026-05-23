using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Rendering
{
    public sealed class PsychoRuntimeVisualQuality : MonoBehaviour
    {
        [SerializeField] private int antiAliasing = 8;
        [SerializeField] private float shadowDistance = 260f;
        [SerializeField] private float lodBias = 1.65f;

        private void Awake()
        {
            Apply();
        }

        private void Apply()
        {
            QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, antiAliasing);
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 4);
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            QualitySettings.shadowProjection = ShadowProjection.CloseFit;
            QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, shadowDistance);
            QualitySettings.shadowCascades = Mathf.Max(QualitySettings.shadowCascades, 4);
            QualitySettings.lodBias = Mathf.Max(QualitySettings.lodBias, lodBias);
            QualitySettings.softParticles = true;
            QualitySettings.softVegetation = true;
            QualitySettings.realtimeReflectionProbes = true;
        }
    }
}
