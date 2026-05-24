using UnityEngine;

namespace Psycho.Rendering
{
    [RequireComponent(typeof(MeshFilter))]
    public sealed class ProceduralWater : MonoBehaviour
    {
        [SerializeField] private float waveAmplitude = 0.08f;
        [SerializeField] private float waveSpeed = 1.25f;
        [SerializeField] private float waveScale = 2.4f;
        [SerializeField] private float rippleStrength = 0.04f;
        [SerializeField] private Color shallowColor = new Color(0.16f, 0.50f, 0.68f, 0.54f);
        [SerializeField] private Color deepColor = new Color(0.03f, 0.16f, 0.24f, 0.62f);
        [SerializeField] private Color foamColor = new Color(0.70f, 0.90f, 0.94f, 0.42f);

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] workingVertices;
        private float[] vertexPhaseOffsets;
        private float[] edgeFactors;
        private float[] centerFactors;
        private Material material;
        private Vector3 boundsExtents;
        private Bounds animatedBounds;

        private void Awake()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            mesh = Instantiate(filter.sharedMesh);
            mesh.MarkDynamic();
            filter.sharedMesh = mesh;
            baseVertices = mesh.vertices;
            workingVertices = new Vector3[baseVertices.Length];
            vertexPhaseOffsets = new float[baseVertices.Length];
            edgeFactors = new float[baseVertices.Length];
            centerFactors = new float[baseVertices.Length];
            boundsExtents = mesh.bounds.extents;
            animatedBounds = mesh.bounds;
            animatedBounds.Expand(new Vector3(0f, waveAmplitude * 4.5f + rippleStrength * 3f, 0f));
            material = GetComponent<MeshRenderer>().material;

            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 vertex = baseVertices[i];
                float centerDistance = new Vector2(vertex.x * 1.15f, vertex.z).magnitude;
                float maxRadius = Mathf.Max(0.001f, Mathf.Max(boundsExtents.x, boundsExtents.z));
                float edgeDistance = Mathf.Min(boundsExtents.x - Mathf.Abs(vertex.x), boundsExtents.z - Mathf.Abs(vertex.z));
                vertexPhaseOffsets[i] = Mathf.Sin(vertex.x * 12.9898f + vertex.z * 78.233f) * 6.28318f;
                edgeFactors[i] = Mathf.Clamp01(1f - edgeDistance / 0.72f);
                centerFactors[i] = Mathf.Clamp01(1f - centerDistance / maxRadius);
            }
        }

        private void Update()
        {
            float t = Time.time * waveSpeed;
            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 vertex = baseVertices[i];
                vertex.y += Mathf.Sin(t + vertex.x * waveScale) * waveAmplitude;
                vertex.y += Mathf.Cos(t * 1.31f + vertex.z * waveScale * 0.7f) * waveAmplitude * 0.65f;
                float centerDistance = new Vector2(vertex.x * 1.15f, vertex.z).magnitude;
                float rippleFalloff = centerFactors[i];
                vertex.y += Mathf.Sin(t * 2.8f + centerDistance * 8.5f) * rippleStrength * rippleFalloff;
                vertex.y += Mathf.Sin(t * 1.74f + vertexPhaseOffsets[i]) * waveAmplitude * 0.18f * (0.35f + rippleFalloff);

                float shore = edgeFactors[i];
                vertex.y += Mathf.Sin(t * 3.5f + vertex.x * 4.3f + vertex.z * 2.7f) * waveAmplitude * 0.32f * shore;
                vertex.y += Mathf.Sin(t * 5.2f + vertexPhaseOffsets[i] * 1.7f) * waveAmplitude * 0.16f * shore;
                workingVertices[i] = vertex;
            }

            mesh.vertices = workingVertices;
            mesh.RecalculateNormals();
            mesh.bounds = animatedBounds;

            Color pulse = Color.Lerp(deepColor, shallowColor, 0.55f + Mathf.Sin(t) * 0.08f);
            pulse = Color.Lerp(pulse, foamColor, 0.07f + Mathf.Sin(t * 1.9f) * 0.025f);
            material.color = pulse;
            material.mainTextureOffset = new Vector2(Time.time * 0.025f, Time.time * 0.018f);

            if (material.HasProperty("_BumpMap"))
            {
                material.SetTextureOffset("_BumpMap", new Vector2(Time.time * -0.018f, Time.time * 0.031f));
                material.SetFloat("_BumpScale", 0.92f + Mathf.Sin(t * 0.56f) * 0.035f);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.78f + Mathf.Sin(t * 0.7f) * 0.04f);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }
        }
    }
}
