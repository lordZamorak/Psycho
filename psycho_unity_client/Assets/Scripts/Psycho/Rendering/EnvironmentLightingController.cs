using UnityEngine;

namespace Psycho.Rendering
{
    public sealed class EnvironmentLightingController : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private float cycleDuration = 240f;
        [SerializeField] private float cycleOffset = 0.18f;
        [SerializeField] private float minFogDensity = 0.0009f;
        [SerializeField] private float maxFogDensity = 0.0028f;

        private readonly Color lowSunColor = new Color(1.00f, 0.63f, 0.38f);
        private readonly Color highSunColor = new Color(1.00f, 0.92f, 0.78f);
        private readonly Color lowAmbientSky = new Color(0.22f, 0.28f, 0.34f);
        private readonly Color highAmbientSky = new Color(0.56f, 0.66f, 0.76f);
        private readonly Color lowAmbientEquator = new Color(0.20f, 0.25f, 0.23f);
        private readonly Color highAmbientEquator = new Color(0.37f, 0.43f, 0.37f);
        private readonly Color lowAmbientGround = new Color(0.10f, 0.10f, 0.09f);
        private readonly Color highAmbientGround = new Color(0.20f, 0.19f, 0.15f);
        private readonly Color lowFog = new Color(0.30f, 0.39f, 0.48f);
        private readonly Color highFog = new Color(0.56f, 0.66f, 0.74f);

        private void Reset()
        {
            sun = FindAnyObjectByType<Light>();
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
                sun.intensity = Mathf.Lerp(0.50f, 1.16f, daylight);
                sun.color = Color.Lerp(highSunColor, lowSunColor, warmth * 0.35f);
            }

            RenderSettings.ambientSkyColor = Color.Lerp(lowAmbientSky, highAmbientSky, daylight);
            RenderSettings.ambientEquatorColor = Color.Lerp(lowAmbientEquator, highAmbientEquator, daylight);
            RenderSettings.ambientGroundColor = Color.Lerp(lowAmbientGround, highAmbientGround, daylight);
            RenderSettings.ambientIntensity = Mathf.Lerp(0.72f, 0.98f, daylight);
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
