using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string TerrainMaterialPath = GeneratedRoot + "/Psycho_RS_Terrain_VertexBlend.mat";
        private const string NpcMaterialPath = GeneratedRoot + "/Psycho_RS_Npc_VertexColor.mat";
        private const string ReportPath = GeneratedRoot + "/cache_model_import_report.json";
        private const string PreviewScenePath = GeneratedRoot + "/PsychoCacheMeshPreview.unity";
        private const string AmbientCgRoot = "Assets/PsychoArtSource/Materials/AmbientCG";
        private const string GrassAlbedoPath = AmbientCgRoot + "/Grass001/Grass001_2K-JPG_Color.jpg";
        private const string GrassNormalPath = AmbientCgRoot + "/Grass001/Grass001_2K-JPG_NormalGL.jpg";
        private const string PathAlbedoPath = AmbientCgRoot + "/PavingStones107/PavingStones107_2K-JPG_Color.jpg";
        private const string PathNormalPath = AmbientCgRoot + "/PavingStones107/PavingStones107_2K-JPG_NormalGL.jpg";
        private const string RockAlbedoPath = AmbientCgRoot + "/Rock015/Rock015_2K-JPG_Color.jpg";
        private const string RockNormalPath = AmbientCgRoot + "/Rock015/Rock015_2K-JPG_NormalGL.jpg";

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

        public static Material LoadOrCreateVertexColorMaterial()
        {
            EnsureGeneratedFolders();
            return CreateOrUpdateVertexColorMaterial(MaterialPath, 0.22f, 0.22f, 0.045f, 0.30f);
        }

        public static Material LoadOrCreateTerrainVertexColorMaterial()
        {
            EnsureGeneratedFolders();
            ConfigureTerrainTextureImports();
            Material material = CreateOrUpdateVertexColorMaterial(TerrainMaterialPath, 0.20f, 0.58f, 0.16f, 0.54f);
            SetMaterialFloat(material, "_GroundBlendStrength", 0.94f);
            SetMaterialColor(material, "_GrassTint", new Color(0.23f, 0.42f, 0.18f, 1f));
            SetMaterialColor(material, "_PathTint", new Color(0.49f, 0.42f, 0.31f, 1f));
            SetMaterialColor(material, "_RockTint", new Color(0.39f, 0.39f, 0.35f, 1f));
            SetMaterialFloat(material, "_BlendNoiseScale", 0.52f);
            SetMaterialFloat(material, "_BlendNoiseStrength", 0.76f);
            SetMaterialFloat(material, "_HighlandTextureStrength", 0.92f);
            SetMaterialFloat(material, "_StoneStrataStrength", 0.62f);
            SetMaterialFloat(material, "_SnowDustStrength", 0.018f);
            SetMaterialColor(material, "_DistanceTint", new Color(0.50f, 0.59f, 0.62f, 1f));
            SetMaterialFloat(material, "_DistanceStart", 92f);
            SetMaterialFloat(material, "_DistanceEnd", 520f);
            SetMaterialFloat(material, "_DistanceBlend", 0.16f);
            SetMaterialColor(material, "_TopWarmth", new Color(0.96f, 0.98f, 0.91f, 1f));
            SetMaterialFloat(material, "_HemisphereContrast", 0.21f);
            SetMaterialFloat(material, "_SpecularLift", 0.040f);
            SetMaterialTextureWithFallback(material, "_GrassAlbedo", GrassAlbedoPath, "Assets/Resources/PsychoMaterials/Psycho_Grass_Albedo_2K.png");
            SetMaterialTextureWithFallback(material, "_PathAlbedo", PathAlbedoPath, "Assets/Resources/PsychoMaterials/Psycho_Organic_Albedo_2K.png");
            SetMaterialTextureWithFallback(material, "_RockAlbedo", RockAlbedoPath, "Assets/Resources/PsychoMaterials/Psycho_Mountain_Albedo_2K.png");
            SetMaterialTextureWithFallback(material, "_GrassNormalMap", GrassNormalPath, "Assets/Resources/PsychoMaterials/Psycho_Grass_Normal_2K.png");
            SetMaterialTextureWithFallback(material, "_PathNormalMap", PathNormalPath, "Assets/Resources/PsychoMaterials/Psycho_Organic_Normal_2K.png");
            SetMaterialTextureWithFallback(material, "_RockNormalMap", RockNormalPath, "Assets/Resources/PsychoMaterials/Psycho_Mountain_Normal_2K.png");
            SetMaterialFloat(material, "_TerrainTexScale", 0.18f);
            SetMaterialFloat(material, "_TerrainAlbedoStrength", 0.96f);
            SetMaterialFloat(material, "_TerrainNormalStrength", 0.68f);
            SetMaterialFloat(material, "_SplatContrast", 1.28f);
            SetMaterialFloat(material, "_MacroVariationScale", 0.033f);
            SetMaterialFloat(material, "_MacroVariationStrength", 0.18f);
            SetMaterialFloat(material, "_SlopeProjectionStrength", 0.86f);
            SetMaterialFloat(material, "_PathPebbleStrength", 0.52f);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material LoadOrCreateNpcVertexColorMaterial()
        {
            EnsureGeneratedFolders();
            Material material = CreateOrUpdateVertexColorMaterial(NpcMaterialPath, 0.24f, 0.035f, 0.006f, 0.035f);
            SetMaterialFloat(material, "_HighlandTextureStrength", 0.06f);
            SetMaterialFloat(material, "_StoneStrataStrength", 0.0f);
            SetMaterialFloat(material, "_SnowDustStrength", 0.0f);
            SetMaterialFloat(material, "_RimStrength", 0.035f);
            SetMaterialFloat(material, "_SpecularLift", 0.030f);
            EditorUtility.SetDirty(material);
            return material;
        }

        [MenuItem("Psycho/Cache/Rebuild Generated Model Assets")]
        public static void RebuildGeneratedModelAssets()
        {
            EnsureGeneratedFolders();
            string[] modelAssets = Directory.GetFiles(ToFullPath(GeneratedRoot), "model_*.asset", SearchOption.TopDirectoryOnly);
            int rebuilt = 0;
            int failed = 0;

            using (PsychoCacheStore store = new PsychoCacheStore(PsychoCacheStore.DefaultClientCachePath))
            {
                foreach (string fullPath in modelAssets)
                {
                    string fileName = Path.GetFileNameWithoutExtension(fullPath);
                    if (string.IsNullOrWhiteSpace(fileName) || !fileName.StartsWith("model_", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string idText = fileName.Substring("model_".Length);
                    if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int modelId))
                    {
                        continue;
                    }

                    if (TryImportModelAsset(store, modelId, out _, out _, out _, out string error, true))
                    {
                        rebuilt++;
                    }
                    else
                    {
                        failed++;
                        Debug.LogWarning($"Failed to rebuild generated cache model {modelId}: {error}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Rebuilt {rebuilt} generated cache model assets with current decode/material rules. Failed: {failed}.");
        }

        public static void RebuildGeneratedModelAssetsBatch()
        {
            RebuildGeneratedModelAssets();
        }

        public static bool TryImportModelAsset(PsychoCacheStore store, int modelId, out Mesh mesh, out string assetPath, out string source, out string error, bool forceRebuild = false)
        {
            EnsureGeneratedFolders();
            mesh = null;
            assetPath = $"{GeneratedRoot}/model_{modelId}.asset";
            source = "generated";
            error = null;

            mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh != null && !forceRebuild)
            {
                return true;
            }

            source = "standard";
            byte[] bytes = store.ReadGzipFile(StandardModelCacheIndex, modelId);
            if (bytes == null)
            {
                source = "osrs";
                bytes = store.ReadGzipFile(OsrsModelCacheIndex, modelId);
            }

            if (bytes == null)
            {
                error = "model payload missing from standard and OSRS caches";
                return false;
            }

            try
            {
                PsychoCacheModel decoded = PsychoModelDecoder.Decode(bytes, modelId);
                Mesh decodedMesh = PsychoModelMeshBuilder.BuildMesh(decoded);
                mesh = CreateOrUpdateMeshAsset(decodedMesh, assetPath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static ImportResult ImportObjectModels(int maxModels, bool buildPreviewScene)
        {
            EnsureGeneratedFolders();
            PsychoMirrorDatabase database = PsychoMirrorDatabase.LoadFromStreamingAssets();
            List<int> modelIds = CollectObjectModelIds(database, maxModels);
            Material material = CreateOrUpdateVertexColorMaterial(MaterialPath, 0.22f, 0.22f, 0.045f, 0.30f);
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
            mesh.name = Path.GetFileNameWithoutExtension(assetPath);
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

        private static Material CreateOrUpdateVertexColorMaterial(string materialPath, float smoothness, float noiseScale, float noiseStrength, float slopeDarkening)
        {
            Shader shader = Shader.Find("Psycho/Vertex Color Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }

            material.SetColor("_Tint", Color.white);
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_NoiseScale"))
            {
                material.SetFloat("_NoiseScale", noiseScale);
            }

            if (material.HasProperty("_NoiseStrength"))
            {
                material.SetFloat("_NoiseStrength", noiseStrength);
            }

            if (material.HasProperty("_SlopeDarkening"))
            {
                material.SetFloat("_SlopeDarkening", slopeDarkening);
            }

            SetMaterialColor(material, "_DistanceTint", new Color(0.56f, 0.66f, 0.72f, 1f));
            SetMaterialFloat(material, "_DistanceStart", 95f);
            SetMaterialFloat(material, "_DistanceEnd", 360f);
            SetMaterialFloat(material, "_DistanceBlend", 0.16f);
            SetMaterialColor(material, "_TopWarmth", new Color(0.99f, 1.00f, 0.94f, 1f));
            SetMaterialFloat(material, "_HemisphereContrast", 0.14f);
            SetMaterialColor(material, "_RimColor", new Color(0.70f, 0.84f, 1.00f, 1f));
            SetMaterialFloat(material, "_RimStrength", 0.075f);
            SetMaterialFloat(material, "_SpecularLift", 0.08f);
            SetMaterialFloat(material, "_HighlandTextureStrength", 0.12f);
            SetMaterialFloat(material, "_StoneStrataStrength", 0.18f);
            SetMaterialFloat(material, "_SnowDustStrength", 0.0f);

            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetMaterialColor(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetMaterialTexture(Material material, string propertyName, string assetPath)
        {
            if (!material.HasProperty(propertyName))
            {
                return;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture != null)
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetMaterialTextureWithFallback(Material material, string propertyName, string preferredAssetPath, string fallbackAssetPath)
        {
            string selectedPath = AssetDatabase.LoadAssetAtPath<Texture2D>(preferredAssetPath) != null
                ? preferredAssetPath
                : fallbackAssetPath;
            SetMaterialTexture(material, propertyName, selectedPath);
        }

        private static void ConfigureTerrainTextureImports()
        {
            ConfigureTextureImport(GrassAlbedoPath, false);
            ConfigureTextureImport(PathAlbedoPath, false);
            ConfigureTextureImport(RockAlbedoPath, false);
            ConfigureTextureImport(GrassNormalPath, true);
            ConfigureTextureImport(PathNormalPath, true);
            ConfigureTextureImport(RockNormalPath, true);
        }

        private static void ConfigureTextureImport(string assetPath, bool normalMap)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normalMap;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 12;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            AssetDatabase.WriteImportSettingsIfDirty(assetPath);
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
