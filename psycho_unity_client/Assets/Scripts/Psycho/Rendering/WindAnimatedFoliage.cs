using UnityEngine;

namespace Psycho.Rendering
{
    [RequireComponent(typeof(MeshFilter))]
    public sealed class WindAnimatedFoliage : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.16f;
        [SerializeField] private float speed = 1.8f;
        [SerializeField] private float spatialFrequency = 1.4f;
        [SerializeField] private float gustStrength = 0.42f;
        [SerializeField] private float gustScale = 0.18f;
        [SerializeField] private float turbulence = 0.028f;
        [SerializeField] private Vector2 windDirection = new Vector2(1f, 0.35f);

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] workingVertices;
        private float minHeight;
        private float heightRange = 1f;
        private float phase;
        private Vector3 primaryWind;
        private Vector3 crossWind;
        private float smoothedGust = 1f;
        private float gustVelocity;

        private void Awake()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            mesh = Instantiate(filter.sharedMesh);
            mesh.MarkDynamic();
            filter.sharedMesh = mesh;
            baseVertices = mesh.vertices;
            workingVertices = new Vector3[baseVertices.Length];
            phase = transform.position.x * 0.73f + transform.position.z * 0.41f;
            minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            foreach (Vector3 vertex in baseVertices)
            {
                minHeight = Mathf.Min(minHeight, vertex.y);
                maxHeight = Mathf.Max(maxHeight, vertex.y);
            }

            heightRange = Mathf.Max(0.001f, maxHeight - minHeight);

            Vector2 direction = windDirection.sqrMagnitude <= 0.001f ? Vector2.right : windDirection.normalized;
            primaryWind = new Vector3(direction.x, 0f, direction.y);
            crossWind = new Vector3(-direction.y, 0f, direction.x);
        }

        private void Update()
        {
            float clock = Time.timeSinceLevelLoad;
            float time = clock * speed + phase;
            float targetGust = 1f + (Mathf.PerlinNoise(clock * gustScale, phase * 0.17f) - 0.5f) * gustStrength;
            smoothedGust = Mathf.SmoothDamp(smoothedGust, targetGust, ref gustVelocity, 0.42f);

            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 vertex = baseVertices[i];
                float heightWeight = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((vertex.y - minHeight) / heightRange));
                float alongWind = vertex.x * primaryWind.x + vertex.z * primaryWind.z;
                float wave = Mathf.Sin(time + alongWind * spatialFrequency + vertex.y * 0.82f) * amplitude * heightWeight * smoothedGust;
                float flutter = Mathf.Sin(time * 0.73f + (vertex.x + vertex.z) * spatialFrequency * 0.62f) * amplitude * turbulence * heightWeight * smoothedGust;
                vertex += primaryWind * wave + crossWind * flutter;
                workingVertices[i] = vertex;
            }

            mesh.vertices = workingVertices;
            mesh.RecalculateBounds();
        }
    }
}
