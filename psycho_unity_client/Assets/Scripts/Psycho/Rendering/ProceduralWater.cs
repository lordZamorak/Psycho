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
        [SerializeField] private Color shallowColor = new Color(0.20f, 0.62f, 0.82f, 0.78f);
        [SerializeField] private Color deepColor = new Color(0.03f, 0.18f, 0.28f, 0.88f);
        [SerializeField] private Color foamColor = new Color(0.72f, 0.92f, 0.96f, 0.72f);

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] workingVertices;
        private Material material;
        private Vector3 boundsExtents;

        private void Awake()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            mesh = Instantiate(filter.sharedMesh);
            mesh.MarkDynamic();
            filter.sharedMesh = mesh;
            baseVertices = mesh.vertices;
            workingVertices = new Vector3[baseVertices.Length];
            boundsExtents = mesh.bounds.extents;
            material = GetComponent<MeshRenderer>().material;
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
                float rippleFalloff = Mathf.Clamp01(1f - centerDistance / Mathf.Max(boundsExtents.x, boundsExtents.z));
                vertex.y += Mathf.Sin(t * 2.8f + centerDistance * 8.5f) * rippleStrength * rippleFalloff;

                float edgeDistance = Mathf.Min(boundsExtents.x - Mathf.Abs(vertex.x), boundsExtents.z - Mathf.Abs(vertex.z));
                float shore = Mathf.Clamp01(1f - edgeDistance / 0.55f);
                vertex.y += Mathf.Sin(t * 3.5f + vertex.x * 4.3f + vertex.z * 2.7f) * waveAmplitude * 0.32f * shore;
                workingVertices[i] = vertex;
            }

            mesh.vertices = workingVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Color pulse = Color.Lerp(deepColor, shallowColor, 0.55f + Mathf.Sin(t) * 0.08f);
            pulse = Color.Lerp(pulse, foamColor, 0.06f + Mathf.Sin(t * 1.9f) * 0.025f);
            material.color = pulse;
            material.mainTextureOffset = new Vector2(Time.time * 0.025f, Time.time * 0.018f);

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.78f + Mathf.Sin(t * 0.7f) * 0.04f);
            }
        }
    }
}
