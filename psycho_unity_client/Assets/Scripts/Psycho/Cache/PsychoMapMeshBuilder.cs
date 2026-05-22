using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Cache
{
    public static class PsychoMapMeshBuilder
    {
        private const float DefaultTileScale = 1f;
        private const float DefaultHeightScale = 1f / 128f;

        public static Mesh BuildTerrainMesh(PsychoMapLandscape landscape, int plane = 0, float tileScale = DefaultTileScale, float heightScale = DefaultHeightScale)
        {
            List<Vector3> vertices = new List<Vector3>(64 * 64);
            List<Color32> colors = new List<Color32>(64 * 64);
            List<int> triangles = new List<int>(63 * 63 * 6);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    vertices.Add(new Vector3(x * tileScale, -landscape.Heights[plane, x, y] * heightScale, y * tileScale));
                    colors.Add(TileColor(landscape, plane, x, y));
                }
            }

            for (int x = 0; x < 63; x++)
            {
                for (int y = 0; y < 63; y++)
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
            return x * 64 + y;
        }

        private static Color32 TileColor(PsychoMapLandscape landscape, int plane, int x, int y)
        {
            int underlay = landscape.UnderlayIds[plane, x, y] & 0xff;
            int overlay = landscape.OverlayIds[plane, x, y] & 0xff;
            byte flag = landscape.RenderFlags[plane, x, y];

            if (overlay != 0)
            {
                byte shade = (byte)Mathf.Clamp(88 + overlay * 7, 72, 174);
                return new Color32(shade, (byte)Mathf.Clamp(110 + overlay * 3, 96, 190), (byte)Mathf.Clamp(82 + overlay * 2, 68, 170), 255);
            }

            if ((flag & 1) == 1)
            {
                return new Color32(95, 111, 106, 255);
            }

            if (underlay == 0)
            {
                return new Color32(72, 132, 55, 255);
            }

            return new Color32(
                (byte)Mathf.Clamp(54 + underlay * 5, 48, 122),
                (byte)Mathf.Clamp(104 + underlay * 4, 90, 170),
                (byte)Mathf.Clamp(48 + underlay * 3, 42, 112),
                255);
        }
    }
}
