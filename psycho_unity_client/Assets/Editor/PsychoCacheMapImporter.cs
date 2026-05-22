using System;
using System.IO;
using Psycho.Cache;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoCacheMapImporter
    {
        private const int MapCacheIndex = 4;
        private const int EdgevilleRegionX = 48;
        private const int EdgevilleRegionY = 54;
        private const int EdgevilleLandscapeFile = 624;
        private const string GeneratedRoot = "Assets/Generated/CacheMaps";
        private const string ReportPath = GeneratedRoot + "/cache_map_import_report.json";
        private const string PreviewScenePath = GeneratedRoot + "/PsychoEdgevilleLandscapePreview.unity";

        [MenuItem("Psycho/Cache/Import Edgeville Landscape")]
        public static void ImportEdgevilleLandscape()
        {
            EnsureGeneratedFolders();
            using (PsychoCacheStore store = new PsychoCacheStore(PsychoCacheStore.DefaultClientCachePath))
            {
                byte[] bytes = store.ReadGzipFile(MapCacheIndex, EdgevilleLandscapeFile);
                if (bytes == null)
                {
                    throw new InvalidOperationException($"Landscape file {EdgevilleLandscapeFile} was not found in cache index {MapCacheIndex}.");
                }

                PsychoMapLandscape landscape = PsychoMapDecoder.DecodeLandscape(bytes, EdgevilleRegionX, EdgevilleRegionY);
                Mesh mesh = PsychoMapMeshBuilder.BuildTerrainMesh(landscape, 0, 0.72f, 1f / 96f);
                string meshPath = $"{GeneratedRoot}/region_{(EdgevilleRegionX << 8) + EdgevilleRegionY}_landscape_plane0.asset";
                CreateOrUpdateMeshAsset(mesh, meshPath);
                WriteReport(landscape, bytes.Length, meshPath);
                BuildPreviewScene(mesh, meshPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Psycho landscape import complete: region {(EdgevilleRegionX << 8) + EdgevilleRegionY}, mesh {meshPath}, report {ReportPath}");
            }
        }

        public static void ImportEdgevilleLandscapeBatch()
        {
            ImportEdgevilleLandscape();
        }

        [MenuItem("Psycho/Cache/Render Edgeville Landscape Preview")]
        public static void RenderEdgevilleLandscapePreview()
        {
            if (!File.Exists(ToFullPath(PreviewScenePath)))
            {
                ImportEdgevilleLandscape();
            }

            EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
            Camera camera = UnityEngine.Object.FindObjectOfType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("Edgeville landscape preview scene does not contain a camera.");
            }

            RenderTexture target = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            Texture2D capture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture previousCameraTarget = camera.targetTexture;

            camera.targetTexture = target;
            RenderTexture.active = target;
            camera.Render();
            capture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            capture.Apply();

            camera.targetTexture = previousCameraTarget;
            RenderTexture.active = previous;

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-edgeville-landscape-preview.png"));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(capture);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            Debug.Log($"Rendered Edgeville landscape preview to {outputPath}");
        }

        public static void RenderEdgevilleLandscapePreviewBatch()
        {
            RenderEdgevilleLandscapePreview();
        }

        private static Mesh CreateOrUpdateMeshAsset(Mesh mesh, string assetPath)
        {
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, assetPath);
                return mesh;
            }

            EditorUtility.CopySerialized(mesh, existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static void BuildPreviewScene(Mesh mesh, string meshPath)
        {
            Mesh assetMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) ?? mesh;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Generated/CacheModels/Psycho_RS_VertexColor.mat");
            if (material == null)
            {
                Shader shader = Shader.Find("Psycho/Vertex Color Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
            }

            GameObject terrain = new GameObject("Edgeville Landscape Mesh");
            terrain.AddComponent<MeshFilter>().sharedMesh = assetMesh;
            terrain.AddComponent<MeshRenderer>().sharedMaterial = material;
            terrain.transform.position = new Vector3(-23f, 0f, -23f);

            GameObject lightObject = new GameObject("Sun");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -28f, 0f);

            GameObject cameraObject = new GameObject("Preview Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 46f;
            camera.transform.position = new Vector3(20f, 26f, -34f);
            camera.transform.rotation = Quaternion.Euler(38f, -27f, 0f);
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/PsychoSkyboxPrototype.mat");
            RenderSettings.ambientIntensity = 0.85f;
            EditorSceneManager.SaveScene(scene, PreviewScenePath);
        }

        private static void EnsureGeneratedFolders()
        {
            Directory.CreateDirectory(ToFullPath("Assets/Generated"));
            Directory.CreateDirectory(ToFullPath(GeneratedRoot));
        }

        private static void WriteReport(PsychoMapLandscape landscape, int decodedBytes, string meshPath)
        {
            CacheMapImportReport report = new CacheMapImportReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                cacheRoot = PsychoCacheStore.DefaultClientCachePath,
                cacheIndex = MapCacheIndex,
                landscapeFile = EdgevilleLandscapeFile,
                regionId = (EdgevilleRegionX << 8) + EdgevilleRegionY,
                regionX = landscape.RegionX,
                regionY = landscape.RegionY,
                osrs = landscape.Osrs,
                decodedBytes = decodedBytes,
                meshPath = meshPath
            };

            File.WriteAllText(ToFullPath(ReportPath), JsonUtility.ToJson(report, true));
            AssetDatabase.ImportAsset(ReportPath);
        }

        private static string ToFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        [Serializable]
        private sealed class CacheMapImportReport
        {
            public string generatedAtUtc;
            public string cacheRoot;
            public int cacheIndex;
            public int landscapeFile;
            public int regionId;
            public int regionX;
            public int regionY;
            public bool osrs;
            public int decodedBytes;
            public string meshPath;
        }
    }
}
