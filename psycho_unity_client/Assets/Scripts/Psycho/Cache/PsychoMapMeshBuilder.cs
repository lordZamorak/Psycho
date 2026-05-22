using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Cache
{
    public static class PsychoMapMeshBuilder
    {
        private const int RegionTileCount = 64;
        private const int TerrainVertexCount = RegionTileCount + 1;
        private const float DefaultTileScale = 1f;
        private const float DefaultHeightScale = 1f / 128f;

        public static Mesh BuildTerrainMesh(PsychoMapLandscape landscape, int plane = 0, float tileScale = DefaultTileScale, float heightScale = DefaultHeightScale)
        {
            List<Vector3> vertices = new List<Vector3>(TerrainVertexCount * TerrainVertexCount);
            List<Color32> colors = new List<Color32>(TerrainVertexCount * TerrainVertexCount);
            List<int> triangles = new List<int>(RegionTileCount * RegionTileCount * 6);

            for (int x = 0; x <= RegionTileCount; x++)
            {
                for (int y = 0; y <= RegionTileCount; y++)
                {
                    int sampleX = Mathf.Min(x, RegionTileCount - 1);
                    int sampleY = Mathf.Min(y, RegionTileCount - 1);
                    vertices.Add(new Vector3(x * tileScale, -landscape.Heights[plane, sampleX, sampleY] * heightScale, y * tileScale));
                    colors.Add(TileColor(landscape, plane, sampleX, sampleY));
                }
            }

            for (int x = 0; x < RegionTileCount; x++)
            {
                for (int y = 0; y < RegionTileCount; y++)
                {
                    int southwest = VertexIndex(x, y);
                    int southeast = VertexIndex(x + 1, y);
                    int northwest = VertexIndex(x, y + 1);
                    int northeast = VertexIndex(x + 1, y + 1);
                    triangles.Add(southwest);
                    triangles.Add(northwest);
                    triangles.Add(southeast);
                    triangles.Add(southeast);
                    triangles.Add(northwest);
                    triangles.Add(northeast);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = $"psycho_map_region_{landscape.RegionX}_{landscape.RegionY}_plane_{plane}";
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static int VertexIndex(int x, int y)
        {
            return x * TerrainVertexCount + y;
        }

        private static Color32 TileColor(PsychoMapLandscape landscape, int plane, int x, int y)
        {
            int underlay = landscape.UnderlayIds[plane, x, y] & 0xff;
            int overlay = landscape.OverlayIds[plane, x, y] & 0xff;
            byte flag = landscape.RenderFlags[plane, x, y];
            float noise = TileNoise(landscape.RegionX * 64 + x, landscape.RegionY * 64 + y);
            float slope = TileSlope(landscape, plane, x, y);
            float elevation = Mathf.InverseLerp(80f, 480f, -landscape.Heights[plane, x, y]);
            Color rock = Color.Lerp(new Color(0.31f, 0.33f, 0.30f), new Color(0.47f, 0.46f, 0.39f), noise);

            if (overlay != 0)
            {
                Color path = Color.Lerp(new Color(0.34f, 0.31f, 0.25f), new Color(0.58f, 0.53f, 0.43f), Mathf.Clamp01(overlay / 28f));
                Color grassBlend = new Color(0.24f, 0.46f, 0.24f);
                Color color = Color.Lerp(path, grassBlend, overlay > 22 ? 0.22f : 0.06f);
                color *= Mathf.Lerp(0.82f, 1.08f, noise);
                color = Color.Lerp(color, rock, Mathf.Clamp01(slope * 0.20f + elevation * 0.05f));
                return ToColor32(color);
            }

            if ((flag & 1) == 1)
            {
                Color flagged = Color.Lerp(new Color(0.22f, 0.29f, 0.29f), new Color(0.30f, 0.37f, 0.35f), noise);
                flagged = Color.Lerp(flagged, new Color(0.15f, 0.25f, 0.30f), 0.24f);
                return ToColor32(flagged);
            }

            if (underlay == 0)
            {
                Color baseGrass = Color.Lerp(new Color(0.19f, 0.39f, 0.17f), new Color(0.38f, 0.58f, 0.30f), noise);
                baseGrass = Color.Lerp(baseGrass, rock, Mathf.Clamp01(slope * 0.55f + elevation * 0.14f));
                return ToColor32(baseGrass);
            }

            float underlayBlend = Mathf.Clamp01(underlay / 32f);
            Color low = new Color(0.22f, 0.42f, 0.20f);
            Color high = new Color(0.47f, 0.55f, 0.31f);
            Color underlayColor = Color.Lerp(low, high, underlayBlend);
            underlayColor *= Mathf.Lerp(0.84f, 1.10f, noise);
            underlayColor = Color.Lerp(underlayColor, rock, Mathf.Clamp01(slope * 0.50f + elevation * 0.12f));
            return ToColor32(underlayColor);
        }

        private static float TileSlope(PsychoMapLandscape landscape, int plane, int x, int y)
        {
            int west = landscape.Heights[plane, Mathf.Max(0, x - 1), y];
            int east = landscape.Heights[plane, Mathf.Min(63, x + 1), y];
            int south = landscape.Heights[plane, x, Mathf.Max(0, y - 1)];
            int north = landscape.Heights[plane, x, Mathf.Min(63, y + 1)];
            int rise = Mathf.Max(Mathf.Abs(east - west), Mathf.Abs(north - south));
            return Mathf.Clamp01(rise / 256f);
        }

        private static float TileNoise(int x, int y)
        {
            int hash = x * 73428767 ^ y * 912931 ^ 0x2c1b3c6d;
            hash ^= hash >> 13;
            hash *= 1274126177;
            hash ^= hash >> 16;
            return (hash & 0xffff) / 65535f;
        }

        private static Color32 ToColor32(Color color)
        {
            color.r = Mathf.Clamp01(color.r);
            color.g = Mathf.Clamp01(color.g);
            color.b = Mathf.Clamp01(color.b);
            color.a = 1f;
            return color;
        }
    }
}
