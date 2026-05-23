using UnityEngine;

namespace Psycho.Rendering
{
    [RequireComponent(typeof(Camera))]
    public sealed class PsychoCameraColorGrade : MonoBehaviour
    {
        [SerializeField] private Shader shader;
        [SerializeField] private float exposure = 1.03f;
        [SerializeField] private float contrast = 1.08f;
        [SerializeField] private float saturation = 1.09f;
        [SerializeField] private float warmth = 0.10f;
        [SerializeField] private float vignette = 0.18f;
        [SerializeField] private float sharpen = 0.12f;

        private Material material;

        private void OnEnable()
        {
            if (shader == null)
            {
                shader = Shader.Find("Hidden/Psycho/Camera Color Grade");
            }

            if (shader != null && material == null)
            {
                material = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
        }

        private void OnDisable()
        {
            if (material == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (material == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            material.SetFloat("_Exposure", exposure);
            material.SetFloat("_Contrast", contrast);
            material.SetFloat("_Saturation", saturation);
            material.SetFloat("_Warmth", warmth);
            material.SetFloat("_Vignette", vignette);
            material.SetFloat("_Sharpen", sharpen);
            material.SetVector("_TexelSize", new Vector4(1f / source.width, 1f / source.height, source.width, source.height));
            Graphics.Blit(source, destination, material);
        }
    }
}
