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
                    colors.Add(BlendedTileColor(landscape, plane, sampleX, sampleY));
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

        private static Color32 BlendedTileColor(PsychoMapLandscape landscape, int plane, int x, int y)
        {
            float boundary = MaterialBoundaryStrength(landscape, plane, x, y);
            float centerWeight = Mathf.Lerp(0.82f, 0.48f, boundary);
            float cardinalWeight = Mathf.Lerp(0.035f, 0.105f, boundary);
            float diagonalWeight = Mathf.Lerp(0.01f, 0.025f, boundary);

            Color color = RawTileColor(landscape, plane, x, y) * centerWeight;
            float totalWeight = centerWeight;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    int sampleX = Mathf.Clamp(x + dx, 0, RegionTileCount - 1);
                    int sampleY = Mathf.Clamp(y + dy, 0, RegionTileCount - 1);
                    float weight = dx == 0 || dy == 0 ? cardinalWeight : diagonalWeight;
                    color += RawTileColor(landscape, plane, sampleX, sampleY) * weight;
                    totalWeight += weight;
                }
            }

            return ToColor32(color / Mathf.Max(0.001f, totalWeight));
        }

        private static float MaterialBoundaryStrength(PsychoMapLandscape landscape, int plane, int x, int y)
        {
            int materialClass = MaterialClass(landscape, plane, x, y);
            int differences = 0;
            int samples = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    int sampleX = Mathf.Clamp(x + dx, 0, RegionTileCount - 1);
                    int sampleY = Mathf.Clamp(y + dy, 0, RegionTileCount - 1);
                    samples++;
                    if (MaterialClass(landscape, plane, sampleX, sampleY) != materialClass)
                    {
                        differences++;
                    }
                }
            }

            return Mathf.Clamp01(differences / Mathf.Max(1f, samples) * 1.65f);
        }

        private static int MaterialClass(PsychoMapLandscape landscape, int plane, int x, int y)
        {
            int overlay = landscape.OverlayIds[plane, x, y] & 0xff;
            if (overlay != 0)
            {
                return overlay > 22 ? 2 : 1;
            }

            if ((landscape.RenderFlags[plane, x, y] & 1) == 1)
            {
                return 3;
            }

            int underlay = landscape.UnderlayIds[plane, x, y] & 0xff;
            return underlay == 0 ? 0 : 4 + Mathf.Clamp(underlay / 12, 0, 4);
        }

        private static Color RawTileColor(PsychoMapLandscape landscape, int plane, int x, int y)
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
            Color rock = Color.Lerp(new Color(0.25f, 0.28f, 0.28f), new Color(0.50f, 0.50f, 0.47f), Mathf.Lerp(noise, macroNoise, 0.35f));

            if (overlay != 0)
            {
                float pathTone = Mathf.Clamp01(overlay / 34f * 0.68f + macroNoise * 0.32f);
                Color path = Color.Lerp(new Color(0.25f, 0.24f, 0.21f), new Color(0.54f, 0.50f, 0.42f), pathTone);
                Color wornDust = Color.Lerp(new Color(0.34f, 0.31f, 0.26f), new Color(0.60f, 0.56f, 0.48f), fineNoise);
                Color grassBlend = Color.Lerp(new Color(0.16f, 0.29f, 0.17f), new Color(0.28f, 0.42f, 0.24f), moisture);
                Color color = Color.Lerp(path, wornDust, 0.24f);
                color = Color.Lerp(color, grassBlend, overlay > 22 ? 0.18f : 0.045f);
                color *= Mathf.Lerp(0.82f, 1.06f, fineNoise);
                color = Color.Lerp(color, rock, Mathf.Clamp01(slope * 0.34f + elevation * 0.11f));
                return ApplyHighlandFinish(color, rock, slope, elevation, moisture, noise, fineNoise);
            }

            if ((flag & 1) == 1)
            {
                Color flagged = Color.Lerp(new Color(0.14f, 0.20f, 0.22f), new Color(0.29f, 0.35f, 0.34f), Mathf.Lerp(noise, moisture, 0.45f));
                flagged = Color.Lerp(flagged, new Color(0.10f, 0.17f, 0.23f), 0.30f + elevation * 0.12f);
                return ApplyHighlandFinish(flagged, rock, slope, elevation, moisture, noise, fineNoise);
            }

            if (underlay == 0)
            {
                Color coolGrass = new Color(0.11f, 0.24f, 0.14f);
                Color lushGrass = new Color(0.25f, 0.40f, 0.22f);
                Color dryGrass = new Color(0.31f, 0.34f, 0.21f);
                Color baseGrass = Color.Lerp(coolGrass, lushGrass, Mathf.Lerp(noise, moisture, 0.42f));
                baseGrass = Color.Lerp(baseGrass, dryGrass, Mathf.Clamp01(elevation * 0.36f + (1f - moisture) * 0.22f));
                baseGrass = Color.Lerp(baseGrass, new Color(0.15f, 0.22f, 0.13f), macroNoise * 0.18f);
                baseGrass *= Mathf.Lerp(0.82f, 1.10f, fineNoise);
                baseGrass = Color.Lerp(baseGrass, rock, Mathf.Clamp01(slope * 0.58f + elevation * 0.18f + (1f - moisture) * 0.04f));
                return ApplyHighlandFinish(baseGrass, rock, slope, elevation, moisture, noise, fineNoise);
            }

            float underlayBlend = Mathf.Clamp01(underlay / 32f);
            Color low = Color.Lerp(new Color(0.12f, 0.26f, 0.15f), new Color(0.20f, 0.34f, 0.19f), moisture);
            Color high = Color.Lerp(new Color(0.30f, 0.39f, 0.24f), new Color(0.42f, 0.46f, 0.29f), macroNoise);
            Color underlayColor = Color.Lerp(low, high, underlayBlend);
            underlayColor *= Mathf.Lerp(0.82f, 1.08f, fineNoise);
            underlayColor = Color.Lerp(underlayColor, rock, Mathf.Clamp01(slope * 0.54f + elevation * 0.16f));
            return ApplyHighlandFinish(underlayColor, rock, slope, elevation, moisture, noise, fineNoise);
        }

        private static Color ApplyHighlandFinish(Color color, Color rock, float slope, float elevation, float moisture, float noise, float fineNoise)
        {
            Color coldLichen = Color.Lerp(new Color(0.18f, 0.25f, 0.19f), new Color(0.33f, 0.36f, 0.27f), moisture);
            float lichenMask = Mathf.Clamp01((noise - 0.58f) * 1.25f + (1f - moisture) * 0.10f);
            color = Color.Lerp(color, coldLichen, lichenMask * 0.10f);

            float exposedRock = Mathf.Clamp01(slope * 0.42f + elevation * 0.20f + (1f - moisture) * 0.06f);
            color = Color.Lerp(color, rock, exposedRock * 0.22f);

            float snowMask = Mathf.Clamp01((elevation - 0.68f) * 2.65f + slope * 0.16f + (fineNoise - 0.62f) * 0.18f);
            Color snowDust = Color.Lerp(new Color(0.56f, 0.60f, 0.58f), new Color(0.76f, 0.78f, 0.73f), fineNoise);
            color = Color.Lerp(color, snowDust, snowMask * 0.34f);

            color.r *= 0.96f;
            color.g *= 0.99f;
            color.b *= 1.03f;
            return color;
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
