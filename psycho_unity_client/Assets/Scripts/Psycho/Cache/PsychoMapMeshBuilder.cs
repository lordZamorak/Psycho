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
            int worldX = landscape.RegionX * 64 + x;
            int worldY = landscape.RegionY * 64 + y;
            float noise = TileNoise(worldX, worldY);
            float macroNoise = TileNoise(worldX / 4 + 97, worldY / 4 - 31);
            float fineNoise = TileNoise(worldX * 3 + 17, worldY * 3 - 43);
            float moisture = TileNoise(worldX - worldY * 2, worldY + worldX * 2);
            float slope = TileSlope(landscape, plane, x, y);
            float elevation = Mathf.InverseLerp(80f, 480f, -landscape.Heights[plane, x, y]);
            Color rock = Color.Lerp(new Color(0.30f, 0.32f, 0.29f), new Color(0.52f, 0.50f, 0.43f), Mathf.Lerp(noise, macroNoise, 0.35f));

            if (overlay != 0)
            {
                float pathTone = Mathf.Clamp01(overlay / 34f * 0.68f + macroNoise * 0.32f);
                Color path = Color.Lerp(new Color(0.30f, 0.28f, 0.23f), new Color(0.64f, 0.58f, 0.46f), pathTone);
                Color wornDust = Color.Lerp(new Color(0.42f, 0.36f, 0.27f), new Color(0.68f, 0.62f, 0.50f), fineNoise);
                Color grassBlend = Color.Lerp(new Color(0.21f, 0.39f, 0.19f), new Color(0.34f, 0.53f, 0.27f), moisture);
                Color color = Color.Lerp(path, wornDust, 0.18f);
                color = Color.Lerp(color, grassBlend, overlay > 22 ? 0.24f : 0.075f);
                color *= Mathf.Lerp(0.86f, 1.08f, fineNoise);
                color = Color.Lerp(color, rock, Mathf.Clamp01(slope * 0.22f + elevation * 0.06f));
                return ToColor32(color);
            }

            if ((flag & 1) == 1)
            {
                Color flagged = Color.Lerp(new Color(0.18f, 0.25f, 0.26f), new Color(0.33f, 0.40f, 0.37f), Mathf.Lerp(noise, moisture, 0.45f));
                flagged = Color.Lerp(flagged, new Color(0.12f, 0.22f, 0.29f), 0.24f + elevation * 0.10f);
                return ToColor32(flagged);
            }

            if (underlay == 0)
            {
                Color coolGrass = new Color(0.14f, 0.31f, 0.15f);
                Color lushGrass = new Color(0.33f, 0.52f, 0.25f);
                Color dryGrass = new Color(0.37f, 0.41f, 0.21f);
                Color baseGrass = Color.Lerp(coolGrass, lushGrass, Mathf.Lerp(noise, moisture, 0.42f));
                baseGrass = Color.Lerp(baseGrass, dryGrass, Mathf.Clamp01(elevation * 0.28f + (1f - moisture) * 0.18f));
                baseGrass = Color.Lerp(baseGrass, new Color(0.20f, 0.29f, 0.13f), macroNoise * 0.14f);
                baseGrass *= Mathf.Lerp(0.87f, 1.13f, fineNoise);
                baseGrass = Color.Lerp(baseGrass, rock, Mathf.Clamp01(slope * 0.46f + elevation * 0.12f));
                return ToColor32(baseGrass);
            }

            float underlayBlend = Mathf.Clamp01(underlay / 32f);
            Color low = Color.Lerp(new Color(0.16f, 0.34f, 0.16f), new Color(0.24f, 0.42f, 0.20f), moisture);
            Color high = Color.Lerp(new Color(0.36f, 0.47f, 0.24f), new Color(0.48f, 0.54f, 0.29f), macroNoise);
            Color underlayColor = Color.Lerp(low, high, underlayBlend);
            underlayColor *= Mathf.Lerp(0.86f, 1.12f, fineNoise);
            underlayColor = Color.Lerp(underlayColor, rock, Mathf.Clamp01(slope * 0.44f + elevation * 0.11f));
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
