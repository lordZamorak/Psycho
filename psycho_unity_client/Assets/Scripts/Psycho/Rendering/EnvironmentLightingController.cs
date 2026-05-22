using UnityEngine;

namespace Psycho.Rendering
{
    public sealed class EnvironmentLightingController : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private float cycleDuration = 240f;
        [SerializeField] private float cycleOffset = 0.18f;
        [SerializeField] private float minFogDensity = 0.0014f;
        [SerializeField] private float maxFogDensity = 0.0045f;

        private readonly Color lowSunColor = new Color(1.00f, 0.63f, 0.38f);
        private readonly Color highSunColor = new Color(1.00f, 0.92f, 0.78f);
        private readonly Color lowAmbient = new Color(0.20f, 0.25f, 0.30f);
        private readonly Color highAmbient = new Color(0.45f, 0.49f, 0.53f);
        private readonly Color lowFog = new Color(0.25f, 0.31f, 0.38f);
        private readonly Color highFog = new Color(0.38f, 0.50f, 0.57f);

        private void Reset()
        {
            sun = FindObjectOfType<Light>();
        }

        private void Awake()
        {
            ApplyLighting(0f);
        }

        private void Update()
        {
            ApplyLighting(Time.time);
        }

        private void ApplyLighting(float time)
        {
            float duration = Mathf.Max(1f, cycleDuration);
            float phase = Mathf.Repeat(time / duration + cycleOffset, 1f);
            float arc = Mathf.Sin(phase * Mathf.PI);
            float daylight = Mathf.Clamp01(arc);
            float warmth = Mathf.Clamp01(1f - Mathf.Abs(daylight - 0.42f) * 2.6f);

            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(17f, 64f, daylight), Mathf.Lerp(-52f, -18f, phase), 0f);
                sun.intensity = Mathf.Lerp(0.55f, 1.36f, daylight);
                sun.color = Color.Lerp(highSunColor, lowSunColor, warmth * 0.35f);
            }

            RenderSettings.ambientLight = Color.Lerp(lowAmbient, highAmbient, daylight);
            RenderSettings.fogColor = Color.Lerp(lowFog, highFog, daylight);
            RenderSettings.fogDensity = Mathf.Lerp(maxFogDensity, minFogDensity, daylight);

            Material skybox = RenderSettings.skybox;
            if (skybox != null)
            {
                skybox.SetColor("_SkyTint", Color.Lerp(new Color(0.30f, 0.40f, 0.50f), new Color(0.42f, 0.56f, 0.68f), daylight));
                skybox.SetFloat("_Exposure", Mathf.Lerp(0.82f, 1.12f, daylight));
                skybox.SetFloat("_AtmosphereThickness", Mathf.Lerp(0.95f, 0.72f, daylight));
            }
        }
    }
}
