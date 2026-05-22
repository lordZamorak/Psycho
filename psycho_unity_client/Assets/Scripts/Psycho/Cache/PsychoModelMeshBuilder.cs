using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Cache
{
    public static class PsychoModelMeshBuilder
    {
        private const float DefaultScale = 1f / 128f;
        private const double RsBrightness = 0.80000000000000004d;
        private static readonly Color32[] RsColorPalette = BuildRsColorPalette();

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

            return RsColorPalette[hsl & 0xffff];
        }

        private static Color32[] BuildRsColorPalette()
        {
            Color32[] palette = new Color32[65536];
            int index = 0;

            for (int hueSaturation = 0; hueSaturation < 512; hueSaturation++)
            {
                double hue = (hueSaturation / 8) / 64d + 0.0078125d;
                double saturation = (hueSaturation & 7) / 8d + 0.0625d;

                for (int lightnessIndex = 0; lightnessIndex < 128; lightnessIndex++)
                {
                    double lightness = lightnessIndex / 128d;
                    double red = lightness;
                    double green = lightness;
                    double blue = lightness;

                    if (saturation != 0d)
                    {
                        double max = lightness < 0.5d
                            ? lightness * (1d + saturation)
                            : lightness + saturation - lightness * saturation;
                        double min = 2d * lightness - max;
                        red = HueToRgb(min, max, hue + 1d / 3d);
                        green = HueToRgb(min, max, hue);
                        blue = HueToRgb(min, max, hue - 1d / 3d);
                    }

                    int rgb = ((int)(red * 256d) << 16) + ((int)(green * 256d) << 8) + (int)(blue * 256d);
                    rgb = ApplyRsBrightness(rgb, RsBrightness);
                    if (rgb == 0)
                    {
                        rgb = 1;
                    }

                    palette[index++] = new Color32(
                        (byte)((rgb >> 16) & 0xff),
                        (byte)((rgb >> 8) & 0xff),
                        (byte)(rgb & 0xff),
                        255);
                }
            }

            return palette;
        }

        private static int ApplyRsBrightness(int rgb, double brightness)
        {
            double red = (rgb >> 16) / 256d;
            double green = ((rgb >> 8) & 0xff) / 256d;
            double blue = (rgb & 0xff) / 256d;
            red = System.Math.Pow(red, brightness);
            green = System.Math.Pow(green, brightness);
            blue = System.Math.Pow(blue, brightness);
            return ((int)(red * 256d) << 16) + ((int)(green * 256d) << 8) + (int)(blue * 256d);
        }

        private static double HueToRgb(double min, double max, double hue)
        {
            if (hue < 0d)
            {
                hue += 1d;
            }

            if (hue > 1d)
            {
                hue -= 1d;
            }

            if (6d * hue < 1d)
            {
                return min + (max - min) * 6d * hue;
            }

            if (2d * hue < 1d)
            {
                return max;
            }

            if (3d * hue < 2d)
            {
                return min + (max - min) * (2d / 3d - hue) * 6d;
            }

            return min;
        }
    }
}
