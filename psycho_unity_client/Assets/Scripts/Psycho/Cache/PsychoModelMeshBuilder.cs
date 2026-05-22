using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Cache
{
    public static class PsychoModelMeshBuilder
    {
        private const float DefaultScale = 1f / 128f;

        public static Mesh BuildMesh(PsychoCacheModel model, float scale = DefaultScale)
        {
            List<Vector3> vertices = new List<Vector3>(model.FaceCount * 3);
            List<int> triangles = new List<int>(model.FaceCount * 3);
            List<Color32> colors = new List<Color32>(model.FaceCount * 3);

            for (int face = 0; face < model.FaceCount; face++)
            {
                int a = model.TriangleA[face];
                int b = model.TriangleB[face];
                int c = model.TriangleC[face];
                if (!IsValidIndex(model, a) || !IsValidIndex(model, b) || !IsValidIndex(model, c))
                {
                    continue;
                }

                Color32 color = DecodeRsHsl(model.FaceColors == null || face >= model.FaceColors.Length ? 0 : model.FaceColors[face]);
                int baseIndex = vertices.Count;
                vertices.Add(ToUnityPosition(model, a, scale));
                vertices.Add(ToUnityPosition(model, b, scale));
                vertices.Add(ToUnityPosition(model, c, scale));
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);
            }

            Mesh mesh = new Mesh();
            mesh.name = $"psycho_cache_model_{model.ModelId}_{model.Format}";
            if (vertices.Count > 65535)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetColors(colors);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static bool IsValidIndex(PsychoCacheModel model, int index)
        {
            return index >= 0 && index < model.VertexCount;
        }

        private static Vector3 ToUnityPosition(PsychoCacheModel model, int vertex, float scale)
        {
            return new Vector3(model.VertexX[vertex] * scale, -model.VertexY[vertex] * scale, model.VertexZ[vertex] * scale);
        }

        private static Color32 DecodeRsHsl(int hsl)
        {
            if (hsl == 65535)
            {
                return new Color32(128, 128, 128, 255);
            }

            float hue = ((hsl >> 10) & 0x3f) / 64f;
            float saturation = ((hsl >> 7) & 0x07) / 8f;
            float lightness = (hsl & 0x7f) / 127f;
            Color color = HslToRgb(hue, saturation, Mathf.Clamp01(lightness * 1.08f));
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255),
                255);
        }

        private static Color HslToRgb(float h, float s, float l)
        {
            if (s <= 0.001f)
            {
                return new Color(l, l, l, 1f);
            }

            float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            float p = 2f * l - q;
            return new Color(HueToRgb(p, q, h + 1f / 3f), HueToRgb(p, q, h), HueToRgb(p, q, h - 1f / 3f), 1f);
        }

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0f)
            {
                t += 1f;
            }

            if (t > 1f)
            {
                t -= 1f;
            }

            if (t < 1f / 6f)
            {
                return p + (q - p) * 6f * t;
            }

            if (t < 1f / 2f)
            {
                return q;
            }

            if (t < 2f / 3f)
            {
                return p + (q - p) * (2f / 3f - t) * 6f;
            }

            return p;
        }
    }
}
