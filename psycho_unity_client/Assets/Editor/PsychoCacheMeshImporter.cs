using System;
using System.Collections.Generic;
using System.IO;
using Psycho.Cache;
using Psycho.Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoCacheMeshImporter
    {
        private const int StandardModelCacheIndex = 1;
        private const int OsrsModelCacheIndex = 6;
        private const int MaxSampleModels = 96;
        private const string GeneratedRoot = "Assets/Generated/CacheModels";
        private const string MaterialPath = GeneratedRoot + "/Psycho_RS_VertexColor.mat";
        private const string ReportPath = GeneratedRoot + "/cache_model_import_report.json";
        private const string PreviewScenePath = GeneratedRoot + "/PsychoCacheMeshPreview.unity";

        [MenuItem("Psycho/Cache/Import Object Model Sample")]
        public static void ImportObjectModelSample()
        {
            ImportResult result = ImportObjectModels(MaxSampleModels, true);
            Debug.Log($"Psycho cache mesh import complete: {result.Imported} imported, {result.Missing} missing, {result.Failed} failed. Report: {ReportPath}");
        }

        public static void ImportObjectModelSampleBatch()
        {
            ImportObjectModelSample();
        }

        private static ImportResult ImportObjectModels(int maxModels, bool buildPreviewScene)
        {
            EnsureGeneratedFolders();
            PsychoMirrorDatabase database = PsychoMirrorDatabase.LoadFromStreamingAssets();
            List<int> modelIds = CollectObjectModelIds(database, maxModels);
            Material material = CreateOrUpdateVertexColorMaterial();
            List<ImportedModel> imported = new List<ImportedModel>();
            List<CacheModelImportReportEntry> reportEntries = new List<CacheModelImportReportEntry>();
            ImportResult result = new ImportResult { Scanned = modelIds.Count };

            using (PsychoCacheStore store = new PsychoCacheStore(PsychoCacheStore.DefaultClientCachePath))
            {
                foreach (int modelId in modelIds)
                {
                    string source = "standard";
                    byte[] bytes = store.ReadGzipFile(StandardModelCacheIndex, modelId);
                    if (bytes == null)
                    {
                        source = "osrs";
                        bytes = store.ReadGzipFile(OsrsModelCacheIndex, modelId);
                    }

                    if (bytes == null)
                    {
                        result.Missing++;
                        reportEntries.Add(CacheModelImportReportEntry.Missing(modelId));
                        continue;
                    }

                    try
                    {
                        PsychoCacheModel decoded = PsychoModelDecoder.Decode(bytes, modelId);
                        Mesh mesh = PsychoModelMeshBuilder.BuildMesh(decoded);
                        string assetPath = $"{GeneratedRoot}/model_{modelId}.asset";
                        Mesh assetMesh = CreateOrUpdateMeshAsset(mesh, assetPath);
                        imported.Add(new ImportedModel { ModelId = modelId, Mesh = assetMesh });
                        result.Imported++;
                        reportEntries.Add(CacheModelImportReportEntry.Imported(modelId, source, decoded.Format.ToString(), decoded.VertexCount, decoded.FaceCount, assetPath));
                    }
                    catch (Exception ex)
                    {
                        result.Failed++;
                        reportEntries.Add(CacheModelImportReportEntry.Failed(modelId, source, ex.Message));
                        Debug.LogWarning($"Failed to decode cache model {modelId} from {source}: {ex.Message}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            WriteReport(result, reportEntries);

            if (buildPreviewScene)
            {
                BuildPreviewScene(imported, material);
            }

            return result;
        }

        private static List<int> CollectObjectModelIds(PsychoMirrorDatabase database, int maxModels)
        {
            HashSet<int> seen = new HashSet<int>();
            List<int> ids = new List<int>();
            foreach (PsychoMirrorObject worldObject in database.Objects)
            {
                if (worldObject.modelIds == null)
                {
                    continue;
                }

                foreach (int modelId in worldObject.modelIds)
                {
                    if (modelId <= 0 || !seen.Add(modelId))
                    {
                        continue;
                    }

                    ids.Add(modelId);
                    if (ids.Count >= maxModels)
                    {
                        return ids;
                    }
                }
            }

            return ids;
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

        private static Material CreateOrUpdateVertexColorMaterial()
        {
            Shader shader = Shader.Find("Psycho/Vertex Color Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }

            material.SetColor("_Tint", Color.white);
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.28f);
            }

            return material;
        }

        private static void BuildPreviewScene(List<ImportedModel> imported, Material material)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Psycho Cache Mesh Preview");

            GameObject lightObject = new GameObject("Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.18f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            GameObject cameraObject = new GameObject("Preview Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 42f;
            camera.transform.position = new Vector3(7.5f, 5.8f, -10.5f);
            camera.transform.rotation = Quaternion.Euler(32f, -36f, 0f);

            int columns = 12;
            for (int i = 0; i < imported.Count; i++)
            {
                ImportedModel importedModel = imported[i];
                GameObject objectRoot = new GameObject($"model_{importedModel.ModelId}");
                objectRoot.transform.SetParent(root.transform, false);
                objectRoot.transform.localPosition = new Vector3((i % columns) * 2.15f, 0f, (i / columns) * 2.15f);
                MeshFilter filter = objectRoot.AddComponent<MeshFilter>();
                filter.sharedMesh = importedModel.Mesh;
                MeshRenderer renderer = objectRoot.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
            }

            root.transform.position = new Vector3(-(columns - 1) * 1.075f, 0f, -3.5f);
            RenderSettings.ambientIntensity = 0.85f;
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/PsychoSkyboxPrototype.mat");
            EditorSceneManager.SaveScene(scene, PreviewScenePath);
        }

        private static void EnsureGeneratedFolders()
        {
            Directory.CreateDirectory(ToFullPath("Assets/Generated"));
            Directory.CreateDirectory(ToFullPath(GeneratedRoot));
        }

        private static void WriteReport(ImportResult result, List<CacheModelImportReportEntry> entries)
        {
            CacheModelImportReport report = new CacheModelImportReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                cacheRoot = PsychoCacheStore.DefaultClientCachePath,
                scanned = result.Scanned,
                imported = result.Imported,
                missing = result.Missing,
                failed = result.Failed,
                entries = entries.ToArray()
            };

            File.WriteAllText(ToFullPath(ReportPath), JsonUtility.ToJson(report, true));
            AssetDatabase.ImportAsset(ReportPath);
        }

        private static string ToFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private sealed class ImportResult
        {
            public int Scanned;
            public int Imported;
            public int Missing;
            public int Failed;
        }

        private sealed class ImportedModel
        {
            public int ModelId;
            public Mesh Mesh;
        }

        [Serializable]
        private sealed class CacheModelImportReport
        {
            public string generatedAtUtc;
            public string cacheRoot;
            public int scanned;
            public int imported;
            public int missing;
            public int failed;
            public CacheModelImportReportEntry[] entries;
        }

        [Serializable]
        private sealed class CacheModelImportReportEntry
        {
            public int modelId;
            public string status;
            public string source;
            public string format;
            public int vertices;
            public int faces;
            public string assetPath;
            public string error;

            public static CacheModelImportReportEntry Imported(int modelId, string source, string format, int vertices, int faces, string assetPath)
            {
                return new CacheModelImportReportEntry
                {
                    modelId = modelId,
                    status = "imported",
                    source = source,
                    format = format,
                    vertices = vertices,
                    faces = faces,
                    assetPath = assetPath
                };
            }

            public static CacheModelImportReportEntry Missing(int modelId)
            {
                return new CacheModelImportReportEntry
                {
                    modelId = modelId,
                    status = "missing"
                };
            }

            public static CacheModelImportReportEntry Failed(int modelId, string source, string error)
            {
                return new CacheModelImportReportEntry
                {
                    modelId = modelId,
                    status = "failed",
                    source = source,
                    error = error
                };
            }
        }
    }
}
