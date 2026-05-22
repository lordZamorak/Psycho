using UnityEngine;

namespace Psycho.Rendering
{
    [RequireComponent(typeof(MeshFilter))]
    public sealed class ProceduralWater : MonoBehaviour
    {
        [SerializeField] private float waveAmplitude = 0.08f;
        [SerializeField] private float waveSpeed = 1.25f;
        [SerializeField] private float waveScale = 2.4f;
        [SerializeField] private Color shallowColor = new Color(0.20f, 0.62f, 0.82f, 0.78f);
        [SerializeField] private Color deepColor = new Color(0.03f, 0.18f, 0.28f, 0.88f);

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] workingVertices;
        private Material material;

        private void Awake()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            mesh = Instantiate(filter.sharedMesh);
            filter.sharedMesh = mesh;
            baseVertices = mesh.vertices;
            workingVertices = new Vector3[baseVertices.Length];
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
                workingVertices[i] = vertex;
            }

            mesh.vertices = workingVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Color pulse = Color.Lerp(deepColor, shallowColor, 0.55f + Mathf.Sin(t) * 0.08f);
            material.color = pulse;
            material.mainTextureOffset = new Vector2(Time.time * 0.025f, Time.time * 0.018f);
        }
    }
}
