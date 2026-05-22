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

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] workingVertices;
        private float maxHeight = 1f;
        private float phase;

        private void Awake()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            mesh = Instantiate(filter.sharedMesh);
            mesh.MarkDynamic();
            filter.sharedMesh = mesh;
            baseVertices = mesh.vertices;
            workingVertices = new Vector3[baseVertices.Length];
            phase = transform.position.x * 0.73f + transform.position.z * 0.41f;

            foreach (Vector3 vertex in baseVertices)
            {
                maxHeight = Mathf.Max(maxHeight, vertex.y);
            }
        }

        private void Update()
        {
            float time = Time.time * speed + phase;
            float gust = 1f + (Mathf.PerlinNoise(Time.time * gustScale, phase * 0.17f) - 0.5f) * gustStrength;
            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 vertex = baseVertices[i];
                float heightWeight = Mathf.Clamp01(vertex.y / maxHeight);
                float wave = Mathf.Sin(time + vertex.y * spatialFrequency + vertex.x * 0.33f) * amplitude * heightWeight * gust;
                vertex.x += wave;
                vertex.z += wave * 0.45f;
                workingVertices[i] = vertex;
            }

            mesh.vertices = workingVertices;
            mesh.RecalculateBounds();
        }
    }
}
