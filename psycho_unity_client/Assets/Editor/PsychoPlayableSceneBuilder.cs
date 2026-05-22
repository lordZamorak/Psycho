using System;
using System.Collections.Generic;
using System.IO;
using Psycho.Cache;
using Psycho.Gameplay;
using Psycho.Mirror;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoPlayableSceneBuilder
    {
        private const int MapCacheIndex = 4;
        private const int EdgevilleRegionX = 48;
        private const int EdgevilleRegionY = 54;
        private const int EdgevilleLandscapeFile = 624;
        private const int EdgevilleObjectFile = 625;
        private const int MaxObjectsInScene = 1400;
        private const int MaxModelsPerObject = 4;
        private const float TileScale = 0.72f;
        private const float HeightScale = 1f / 96f;
        private const string ScenePath = "Assets/Scenes/PsychoEdgevillePlayable.unity";
        private const string GeneratedRoot = "Assets/Generated/Playable";
        private const string ReportPath = GeneratedRoot + "/edgeville_playable_report.json";
        private const string WindowsBuildPath = "Builds/PsychoEdgevillePlayable/Psycho.exe";
        private const string FallbackGroundMaterialPath = GeneratedRoot + "/Psycho_Fallback_Ground.mat";
        private const string FallbackWallMaterialPath = GeneratedRoot + "/Psycho_Fallback_Wall.mat";
        private const string FallbackStructureMaterialPath = GeneratedRoot + "/Psycho_Fallback_Structure.mat";

        [MenuItem("Psycho/Build Playable Edgeville Scene")]
        public static void BuildPlayableEdgevilleScene()
        {
            EnsureFolders();
            PsychoMirrorDatabase database = PsychoMirrorDatabase.LoadFromStreamingAssets();
            Material vertexColorMaterial = PsychoCacheMeshImporter.LoadOrCreateVertexColorMaterial();
            FallbackMaterials fallbackMaterials = LoadOrCreateFallbackMaterials();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayableBuildReport report = new PlayableBuildReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                regionId = (EdgevilleRegionX << 8) + EdgevilleRegionY,
                regionX = EdgevilleRegionX,
                regionY = EdgevilleRegionY,
                landscapeFile = EdgevilleLandscapeFile,
                objectFile = EdgevilleObjectFile
            };

            using (PsychoCacheStore store = new PsychoCacheStore(PsychoCacheStore.DefaultClientCachePath))
            {
                byte[] landscapeBytes = store.ReadGzipFile(MapCacheIndex, EdgevilleLandscapeFile);
                byte[] objectBytes = store.ReadGzipFile(MapCacheIndex, EdgevilleObjectFile);
                if (landscapeBytes == null || objectBytes == null)
                {
                    throw new InvalidOperationException("Edgeville map data was not found in the cache.");
                }

                PsychoMapLandscape landscape = PsychoMapDecoder.DecodeLandscape(landscapeBytes, EdgevilleRegionX, EdgevilleRegionY);
                PsychoMapObjects objects = PsychoMapDecoder.DecodeObjects(objectBytes, EdgevilleRegionX, EdgevilleRegionY);
                report.decodedObjectPlacements = objects.Placements.Count;

                BuildTerrain(landscape, vertexColorMaterial);
                BuildObjects(store, database, landscape, objects, vertexColorMaterial, fallbackMaterials, report);
                BuildLighting();
                BuildPlayer(landscape);
            }

            WriteReport(report);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Playable Edgeville scene built: {ScenePath}. Placed {report.placedObjects} objects, imported {report.importedModels} new meshes, skipped {report.skippedObjects} objects.");
        }

        public static void BuildPlayableEdgevilleSceneBatch()
        {
            BuildPlayableEdgevilleScene();
        }

        [MenuItem("Psycho/Render Playable Edgeville Preview")]
        public static void RenderPlayableEdgevillePreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildPlayableEdgevilleScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("Playable Edgeville scene does not contain a camera.");
            }

            RenderTexture target = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            Texture2D capture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture previousCameraTarget = camera.targetTexture;

            camera.transform.position = new Vector3(15f, 13f, 11f);
            camera.transform.rotation = Quaternion.Euler(34f, 43f, 0f);
            camera.targetTexture = target;
            RenderTexture.active = target;
            camera.Render();
            capture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            capture.Apply();

            camera.targetTexture = previousCameraTarget;
            RenderTexture.active = previous;

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-edgeville-playable-preview.png"));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(capture);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            Debug.Log($"Rendered playable Edgeville preview to {outputPath}");
        }

        public static void RenderPlayableEdgevillePreviewBatch()
        {
            RenderPlayableEdgevillePreview();
        }

        [MenuItem("Psycho/Build Windows Playable")]
        public static void BuildWindowsPlayable()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildPlayableEdgevilleScene();
            }

            string outputPath = ToProjectFullPath(WindowsBuildPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport buildReport = BuildPipeline.BuildPlayer(options);
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Windows playable build failed: {buildReport.summary.result}");
            }

            Debug.Log($"Built Windows playable to {outputPath} ({buildReport.summary.totalSize} bytes).");
        }

        public static void BuildWindowsPlayableBatch()
        {
            BuildWindowsPlayable();
        }

        private static void BuildTerrain(PsychoMapLandscape landscape, Material material)
        {
            Mesh terrainMesh = PsychoMapMeshBuilder.BuildTerrainMesh(landscape, 0, TileScale, HeightScale);
            GameObject terrain = new GameObject("Edgeville Terrain - Cache Region 12342");
            terrain.isStatic = true;
            terrain.AddComponent<MeshFilter>().sharedMesh = terrainMesh;
            terrain.AddComponent<MeshRenderer>().sharedMaterial = material;
            terrain.AddComponent<MeshCollider>().sharedMesh = terrainMesh;
        }

        private static void BuildObjects(
            PsychoCacheStore store,
            PsychoMirrorDatabase database,
            PsychoMapLandscape landscape,
            PsychoMapObjects objects,
            Material vertexColorMaterial,
            FallbackMaterials fallbackMaterials,
            PlayableBuildReport report)
        {
            GameObject root = new GameObject("Edgeville Cache Objects");
            HashSet<int> importedThisPass = new HashSet<int>();
            foreach (PsychoMapObjectPlacement placement in objects.Placements)
            {
                if (placement.Plane != 0)
                {
                    report.skippedObjects++;
                    continue;
                }

                if (report.placedObjects >= MaxObjectsInScene)
                {
                    report.skippedObjects++;
                    continue;
                }

                bool hasDefinition = database.TryGetObject(placement.ObjectId, out PsychoMirrorObject definition);
                if (!hasDefinition)
                {
                    report.missingDefinitions++;
                }

                string objectName = hasDefinition ? definition.name : "Unknown cache object";
                GameObject placed = new GameObject($"Object {placement.ObjectId} - {objectName}");
                placed.isStatic = true;
                placed.transform.SetParent(root.transform, false);
                placed.transform.position = PlacementPosition(landscape, placement);
                placed.transform.rotation = Quaternion.Euler(0f, placement.Orientation * 90f, 0f);
                placed.transform.localScale = Vector3.one * TileScale;

                int addedMeshes = 0;
                if (hasDefinition && definition.modelIds != null)
                {
                    for (int i = 0; i < definition.modelIds.Length && i < MaxModelsPerObject; i++)
                    {
                        int modelId = definition.modelIds[i];
                        if (modelId <= 0)
                        {
                            continue;
                        }

                        if (!PsychoCacheMeshImporter.TryImportModelAsset(store, modelId, out Mesh mesh, out _, out _, out _))
                        {
                            report.missingModels++;
                            continue;
                        }

                        if (importedThisPass.Add(modelId))
                        {
                            report.importedModels++;
                        }

                        GameObject modelObject = new GameObject($"model_{modelId}");
                        modelObject.isStatic = true;
                        modelObject.transform.SetParent(placed.transform, false);
                        modelObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                        modelObject.AddComponent<MeshRenderer>().sharedMaterial = vertexColorMaterial;
                        addedMeshes++;
                    }
                }

                if (addedMeshes == 0)
                {
                    CreateFallbackObject(placed.transform, definition, placement, fallbackMaterials);
                    report.fallbackObjects++;
                }

                report.placedObjects++;
            }
        }

        private static void BuildLighting()
        {
            GameObject sunObject = new GameObject("Sun");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/PsychoSkyboxPrototype.mat");
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.52f, 0.58f, 0.64f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.38f, 0.34f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.17f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.42f, 0.50f, 0.54f);
            RenderSettings.fogDensity = 0.004f;
        }

        private static void BuildPlayer(PsychoMapLandscape landscape)
        {
            GameObject player = new GameObject("Playable Adventurer");
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.9f;
            controller.radius = 0.32f;
            controller.stepOffset = 0.45f;
            controller.slopeLimit = 48f;

            Vector3 spawn = TilePosition(landscape, 21, 37);
            player.transform.position = spawn + Vector3.up * 1.15f;
            player.AddComponent<PsychoPlayableCharacter>();
            player.AddComponent<PsychoCharacterGroundGuard>();

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body Preview";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
            Collider bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(bodyCollider);
            }

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 550f;
            cameraObject.AddComponent<AudioListener>();
        }

        private static Vector3 PlacementPosition(PsychoMapLandscape landscape, PsychoMapObjectPlacement placement)
        {
            return TilePosition(landscape, placement.LocalX, placement.LocalY);
        }

        private static Vector3 TilePosition(PsychoMapLandscape landscape, int localX, int localY)
        {
            int x = Mathf.Clamp(localX, 0, 63);
            int y = Mathf.Clamp(localY, 0, 63);
            float height = -landscape.Heights[0, x, y] * HeightScale;
            return new Vector3((x + 0.5f) * TileScale, height, (y + 0.5f) * TileScale);
        }

        private static void CreateFallbackObject(
            Transform parent,
            PsychoMirrorObject definition,
            PsychoMapObjectPlacement placement,
            FallbackMaterials materials)
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallback.name = definition == null ? $"fallback_object_{placement.ObjectId}" : "fallback_bounds";
            fallback.isStatic = true;
            fallback.transform.SetParent(parent, false);

            Vector3 size = FallbackSize(definition, placement);
            fallback.transform.localPosition = new Vector3(0f, size.y * 0.5f, 0f);
            fallback.transform.localScale = size;
            fallback.GetComponent<MeshRenderer>().sharedMaterial = FallbackMaterialFor(placement, materials);
            Collider collider = fallback.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static Vector3 FallbackSize(PsychoMirrorObject definition, PsychoMapObjectPlacement placement)
        {
            float width = definition == null ? 0.72f : Mathf.Max(1f, definition.sizeX);
            float depth = definition == null ? 0.72f : Mathf.Max(1f, definition.sizeY);

            if (placement.Type >= 0 && placement.Type <= 3)
            {
                return new Vector3(0.18f, 1.35f, 1.05f);
            }

            if (placement.Type >= 4 && placement.Type <= 8)
            {
                return new Vector3(0.12f, 0.82f, 0.72f);
            }

            if (placement.Type == 22)
            {
                return new Vector3(0.68f, 0.035f, 0.68f);
            }

            if (placement.Type == 9)
            {
                return new Vector3(0.72f, 0.7f, 0.72f);
            }

            return new Vector3(width, 0.95f, depth);
        }

        private static Material FallbackMaterialFor(PsychoMapObjectPlacement placement, FallbackMaterials materials)
        {
            if (placement.Type == 22)
            {
                return materials.Ground;
            }

            if (placement.Type >= 0 && placement.Type <= 8)
            {
                return materials.Wall;
            }

            return materials.Structure;
        }

        private static FallbackMaterials LoadOrCreateFallbackMaterials()
        {
            return new FallbackMaterials
            {
                Ground = LoadOrCreateSolidMaterial(FallbackGroundMaterialPath, new Color(0.34f, 0.39f, 0.25f, 1f)),
                Wall = LoadOrCreateSolidMaterial(FallbackWallMaterialPath, new Color(0.43f, 0.41f, 0.36f, 1f)),
                Structure = LoadOrCreateSolidMaterial(FallbackStructureMaterialPath, new Color(0.46f, 0.39f, 0.29f, 1f))
            };
        }

        private static Material LoadOrCreateSolidMaterial(string assetPath, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = color;
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.12f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void WriteReport(PlayableBuildReport report)
        {
            File.WriteAllText(ToFullPath(ReportPath), JsonUtility.ToJson(report, true));
            AssetDatabase.ImportAsset(ReportPath);
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(ToFullPath("Assets/Scenes"));
            Directory.CreateDirectory(ToFullPath("Assets/Generated"));
            Directory.CreateDirectory(ToFullPath(GeneratedRoot));
        }

        private static string ToFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private static string ToProjectFullPath(string projectRelativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
        }

        [Serializable]
        private sealed class PlayableBuildReport
        {
            public string generatedAtUtc;
            public int regionId;
            public int regionX;
            public int regionY;
            public int landscapeFile;
            public int objectFile;
            public int decodedObjectPlacements;
            public int placedObjects;
            public int skippedObjects;
            public int importedModels;
            public int missingDefinitions;
            public int missingModels;
            public int fallbackObjects;
        }

        private sealed class FallbackMaterials
        {
            public Material Ground;
            public Material Wall;
            public Material Structure;
        }
    }
}
