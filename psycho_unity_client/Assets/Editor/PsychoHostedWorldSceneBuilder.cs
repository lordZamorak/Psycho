using System;
using System.Collections.Generic;
using System.IO;
using Psycho.Cache;
using Psycho.Gameplay;
using Psycho.Mirror;
using Psycho.Networking;
using Psycho.Rendering;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoHostedWorldSceneBuilder
    {
        private const int MapCacheIndex = 4;
        private const int BaseRegionX = 48;
        private const int BaseRegionY = 54;
        private const int RegionRadius = 1;
        private const int MaxObjectsPerRegion = 1300;
        private const int MaxObjectsTotal = 9000;
        private const int MaxModelsPerObject = 4;
        private const int MaxModelsPerNpc = 12;
        private const int MaxNpcSpawns = 180;
        private const float TileScale = 0.72f;
        private const float HeightScale = 1f / 96f;
        private const string ScenePath = "Assets/Scenes/PsychoHostedTestWorld.unity";
        private const string GeneratedRoot = "Assets/Generated/Hosted";
        private const string ReportPath = GeneratedRoot + "/hosted_test_world_report.json";
        private const string WindowsBuildPath = "Builds/PsychoHostedTestWorld/Psycho.exe";
        private const string HostedPlayerUsername = "Sirenicbeast";
        private const int EquipmentHeadSlot = 0;
        private const int EquipmentCapeSlot = 1;
        private const int EquipmentWeaponSlot = 3;
        private const int EquipmentBodySlot = 4;
        private const int EquipmentShieldSlot = 5;
        private const int EquipmentLegSlot = 7;
        private const int EquipmentHandsSlot = 9;
        private const int EquipmentFeetSlot = 10;
        private const string GrassMaterialPath = GeneratedRoot + "/Psycho_Hosted_Grass.mat";
        private const string FlowerMaterialPath = GeneratedRoot + "/Psycho_Hosted_Flowers.mat";
        private const string ReedMaterialPath = GeneratedRoot + "/Psycho_Hosted_Reeds.mat";
        private const string WaterMaterialPath = GeneratedRoot + "/Psycho_Hosted_Water.mat";
        private const string HillMaterialPath = GeneratedRoot + "/Psycho_Hosted_Hills.mat";
        private const string MountainMaterialPath = GeneratedRoot + "/Psycho_Hosted_Mountains.mat";
        private const string CloudMaterialPath = GeneratedRoot + "/Psycho_Hosted_Clouds.mat";
        private const string TreeCanopyMaterialPath = GeneratedRoot + "/Psycho_Hosted_Tree_Canopy.mat";
        private const string TreeBarkMaterialPath = GeneratedRoot + "/Psycho_Hosted_Tree_Bark.mat";
        private const string WaterFoamMaterialPath = GeneratedRoot + "/Psycho_Hosted_Water_Foam.mat";
        private const string WaterDepthMaterialPath = GeneratedRoot + "/Psycho_Hosted_Water_Depth.mat";
        private const string PlayerClothMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Cloth.mat";
        private const string PlayerLeatherMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Leather.mat";
        private const string PlayerMetalMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Metal.mat";
        private const string PlayerSkinMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Skin.mat";
        private const string PlayerHairMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Hair.mat";
        private const string PlayerPaperMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Paper.mat";
        private const string PlayerWoodMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Wood.mat";
        private const string FallbackMaterialPath = GeneratedRoot + "/Psycho_Hosted_Fallback.mat";
        private static readonly string[] WindResponsiveObjectNameFragments =
        {
            "tree",
            "oak",
            "willow",
            "maple",
            "yew",
            "evergreen",
            "palm",
            "bush",
            "fern",
            "plant",
            "reed",
            "root",
            "ivy",
            "branch",
            "leaves",
            "hedge",
            "sapling"
        };

        [MenuItem("Psycho/Build Hosted Test World Scene")]
        public static void BuildHostedTestWorldScene()
        {
            EnsureFolders();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PsychoMirrorDatabase database = PsychoMirrorDatabase.LoadFromStreamingAssets();
            Material vertexColorMaterial = PsychoCacheMeshImporter.LoadOrCreateVertexColorMaterial();
            Material npcVertexColorMaterial = PsychoCacheMeshImporter.LoadOrCreateNpcVertexColorMaterial();
            HostedMaterials hostedMaterials = LoadOrCreateHostedMaterials();
            HostedBuildContext context = new HostedBuildContext(database);

            using (PsychoCacheStore store = new PsychoCacheStore(PsychoCacheStore.DefaultClientCachePath))
            {
                BuildRegions(store, database, vertexColorMaterial, hostedMaterials, context);
                BuildNpcSpawns(store, database, npcVertexColorMaterial, context);
                BuildWorldDressing(context, hostedMaterials);
                BuildLighting();
                BuildPlayer(context, hostedMaterials, database);
                BuildNetworkBootstrap();
                ValidateTerrainCollisionCoverage(context);
            }

            WriteReport(context.Report);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Hosted test world built: {ScenePath}. Regions {context.Report.loadedRegions}, objects {context.Report.placedObjects}, NPCs {context.Report.npcSpawns}, cache NPC visuals {context.Report.cacheNpcVisuals}, foliage silhouettes {context.Report.enhancedFoliageObjects}, foam edges {context.Report.waterFoamEdges}.");
        }

        public static void BuildHostedTestWorldSceneBatch()
        {
            BuildHostedTestWorldScene();
        }

        [MenuItem("Psycho/Render Hosted Test World Preview")]
        public static void RenderHostedTestWorldPreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene does not contain a camera.");
            }

            camera.transform.position = new Vector3(35f, 24f, 26f);
            camera.transform.rotation = Quaternion.Euler(38f, 42f, 0f);
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-test-world-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedTestWorldPreviewBatch()
        {
            RenderHostedTestWorldPreview();
        }

        [MenuItem("Psycho/Render Hosted Third Person Preview")]
        public static void RenderHostedThirdPersonPreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Playable Adventurer");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (player == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both the playable adventurer and camera.");
            }

            Vector3 focus = player.transform.position + new Vector3(0f, 1.10f, 0f);
            Vector3 viewOffset = new Vector3(-3.2f, 1.65f, -3.6f);
            camera.transform.position = focus + viewOffset;
            camera.transform.rotation = Quaternion.LookRotation(focus + new Vector3(0.55f, 0.2f, 0.35f) - camera.transform.position, Vector3.up);
            camera.fieldOfView = 46f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-third-person-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedThirdPersonPreviewBatch()
        {
            RenderHostedThirdPersonPreview();
        }

        [MenuItem("Psycho/Render Hosted NPC Preview")]
        public static void RenderHostedNpcPreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject npcRoot = GameObject.Find("Hosted NPC Spawns");
            if (npcRoot == null || npcRoot.transform.childCount == 0)
            {
                throw new InvalidOperationException("Hosted test world scene does not contain NPC spawns.");
            }

            Bounds bounds = BuildNpcPreviewBounds(npcRoot.transform);

            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene does not contain a camera.");
            }

            Vector3 focus = bounds.center + Vector3.up * 0.35f;
            float viewSize = Mathf.Max(bounds.size.x, bounds.size.z, bounds.size.y * 2.2f);
            float cameraDistance = Mathf.Clamp(viewSize * 1.15f, 4.4f, 9.0f);
            camera.transform.position = focus + new Vector3(-0.62f, 0.36f, -0.70f).normalized * cameraDistance;
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 34f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-npc-preview.png"));
            RenderCameraToPng(camera, outputPath, 1400, 900);
        }

        public static void RenderHostedNpcPreviewBatch()
        {
            RenderHostedNpcPreview();
        }

        private static Bounds BuildNpcPreviewBounds(Transform npcRoot)
        {
            Vector3 average = Vector3.zero;
            for (int i = 0; i < npcRoot.childCount; i++)
            {
                average += npcRoot.GetChild(i).position;
            }

            average /= Mathf.Max(1, npcRoot.childCount);
            List<Transform> sorted = new List<Transform>(npcRoot.childCount);
            for (int i = 0; i < npcRoot.childCount; i++)
            {
                sorted.Add(npcRoot.GetChild(i));
            }

            sorted.Sort((left, right) =>
                (left.position - average).sqrMagnitude.CompareTo((right.position - average).sqrMagnitude));

            int selectedCount = Mathf.Min(18, sorted.Count);
            Bounds bounds = new Bounds(sorted[0].position, Vector3.one);
            for (int i = 0; i < selectedCount; i++)
            {
                Renderer[] renderers = sorted[i].GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                {
                    bounds.Encapsulate(sorted[i].position);
                    continue;
                }

                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            bounds.Expand(0.35f);
            return bounds;
        }

        private static void RenderCameraToPng(Camera camera, string outputPath, int width, int height)
        {
            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
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

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(capture);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            Debug.Log($"Rendered hosted preview to {outputPath}");
        }

        [MenuItem("Psycho/Build Windows Hosted Playable")]
        public static void BuildWindowsHostedPlayable()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
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
                throw new InvalidOperationException($"Windows hosted playable build failed: {buildReport.summary.result}");
            }

            Debug.Log($"Built Windows hosted playable to {outputPath} ({buildReport.summary.totalSize} bytes).");
        }

        public static void BuildWindowsHostedPlayableBatch()
        {
            BuildWindowsHostedPlayable();
        }

        private static void BuildRegions(PsychoCacheStore store, PsychoMirrorDatabase database, Material material, HostedMaterials hostedMaterials, HostedBuildContext context)
        {
            GameObject terrainRoot = new GameObject("Hosted Terrain Regions");
            GameObject objectRoot = new GameObject("Hosted Cache Objects");

            for (int regionX = BaseRegionX - RegionRadius; regionX <= BaseRegionX + RegionRadius; regionX++)
            {
                for (int regionY = BaseRegionY - RegionRadius; regionY <= BaseRegionY + RegionRadius; regionY++)
                {
                    int regionId = (regionX << 8) + regionY;
                    if (!database.TryGetMapRegion(regionId, out PsychoMirrorMapRegion mapRegion))
                    {
                        context.Report.missingRegions++;
                        continue;
                    }

                    byte[] landscapeBytes = store.ReadGzipFile(MapCacheIndex, mapRegion.landscapeFile);
                    byte[] objectBytes = store.ReadGzipFile(MapCacheIndex, mapRegion.objectFile);
                    if (landscapeBytes == null || objectBytes == null)
                    {
                        context.Report.missingRegions++;
                        continue;
                    }

                    PsychoMapLandscape landscape = PsychoMapDecoder.DecodeLandscape(landscapeBytes, regionX, regionY);
                    PsychoMapObjects objects = PsychoMapDecoder.DecodeObjects(objectBytes, regionX, regionY);
                    context.Landscapes[regionId] = landscape;
                    context.Report.loadedRegions++;
                    context.Report.decodedObjectPlacements += objects.Placements.Count;

                    BuildTerrain(terrainRoot.transform, landscape, material);
                    BuildObjects(store, database, objectRoot.transform, landscape, objects, material, hostedMaterials, context);
                }
            }
        }

        private static void BuildTerrain(Transform root, PsychoMapLandscape landscape, Material material)
        {
            Mesh terrainMesh = PsychoMapMeshBuilder.BuildTerrainMesh(landscape, 0, TileScale, HeightScale);
            GameObject terrain = new GameObject($"Terrain Region {(landscape.RegionX << 8) + landscape.RegionY}");
            terrain.isStatic = true;
            terrain.transform.SetParent(root, false);
            terrain.transform.position = RegionOrigin(landscape.RegionX, landscape.RegionY);
            terrain.AddComponent<MeshFilter>().sharedMesh = terrainMesh;
            terrain.AddComponent<MeshRenderer>().sharedMaterial = material;
            terrain.AddComponent<MeshCollider>().sharedMesh = terrainMesh;
        }

        private static void BuildObjects(
            PsychoCacheStore store,
            PsychoMirrorDatabase database,
            Transform root,
            PsychoMapLandscape landscape,
            PsychoMapObjects objects,
            Material material,
            HostedMaterials hostedMaterials,
            HostedBuildContext context)
        {
            int placedInRegion = 0;
            Material fallbackMaterial = LoadOrCreateSolidMaterial(FallbackMaterialPath, new Color(0.34f, 0.31f, 0.25f, 1f), 0.22f);
            foreach (PsychoMapObjectPlacement placement in objects.Placements)
            {
                if (placement.Plane != 0)
                {
                    context.Report.skippedObjects++;
                    continue;
                }

                if (placedInRegion >= MaxObjectsPerRegion || context.Report.placedObjects >= MaxObjectsTotal)
                {
                    context.Report.skippedObjects++;
                    continue;
                }

                bool hasDefinition = database.TryGetObject(placement.ObjectId, out PsychoMirrorObject definition);
                if (!hasDefinition)
                {
                    context.Report.missingDefinitions++;
                }

                string objectName = hasDefinition ? definition.name : "Unknown cache object";
                bool windResponsive = IsWindResponsiveObject(definition);
                bool addedWind = false;
                GameObject placed = new GameObject($"Object {placement.ObjectId} - {objectName}");
                placed.isStatic = !windResponsive;
                placed.transform.SetParent(root, false);
                placed.transform.position = TilePosition(landscape, placement.LocalX, placement.LocalY);
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
                            context.Report.missingModels++;
                            continue;
                        }

                        if (context.ImportedModels.Add(modelId))
                        {
                            context.Report.importedModels++;
                        }

                        GameObject modelObject = new GameObject($"model_{modelId}");
                        modelObject.isStatic = !windResponsive;
                        modelObject.transform.SetParent(placed.transform, false);
                        modelObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                        modelObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                        if (windResponsive)
                        {
                            AddWindToWorldObject(modelObject, mesh, placement.ObjectId);
                            addedWind = true;
                        }

                        addedMeshes++;
                    }
                }

                if (addedMeshes == 0)
                {
                    AddFallbackBounds(placed.transform, definition, placement, fallbackMaterial);
                    context.Report.fallbackObjects++;
                    if (windResponsive && IsTreeLikeObject(definition))
                    {
                        AddFoliageSilhouette(placed.transform, definition, placement, hostedMaterials);
                        context.Report.enhancedFoliageObjects++;
                        addedWind = true;
                    }
                }
                else if (windResponsive && ShouldAddFoliageSilhouette(placed.transform, definition))
                {
                    AddFoliageSilhouette(placed.transform, definition, placement, hostedMaterials);
                    context.Report.enhancedFoliageObjects++;
                    addedWind = true;
                }

                AddInteractionAndCollision(placed, definition, placement);
                if (addedWind)
                {
                    context.Report.windAnimatedObjects++;
                }

                placedInRegion++;
                context.Report.placedObjects++;
            }
        }

        private static bool IsWindResponsiveObject(PsychoMirrorObject definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.name))
            {
                return false;
            }

            string name = definition.name.ToLowerInvariant();
            foreach (string fragment in WindResponsiveObjectNameFragments)
            {
                if (name.Contains(fragment))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTreeLikeObject(PsychoMirrorObject definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.name))
            {
                return false;
            }

            string name = definition.name.ToLowerInvariant();
            return name.Contains("tree")
                || name.Contains("oak")
                || name.Contains("willow")
                || name.Contains("maple")
                || name.Contains("yew")
                || name.Contains("evergreen")
                || name.Contains("palm")
                || name.Contains("sapling");
        }

        private static bool ShouldAddFoliageSilhouette(Transform placed, PsychoMirrorObject definition)
        {
            if (!IsTreeLikeObject(definition))
            {
                return false;
            }

            if (!TryCalculateLocalBounds(placed, out Bounds bounds))
            {
                return true;
            }

            float horizontalWidth = Mathf.Max(bounds.size.x, bounds.size.z);
            return horizontalWidth < 1.15f || bounds.size.y > horizontalWidth * 2.15f;
        }

        private static void AddFoliageSilhouette(Transform parent, PsychoMirrorObject definition, PsychoMapObjectPlacement placement, HostedMaterials materials)
        {
            float footprint = Mathf.Max(1f, Mathf.Max(definition.sizeX, definition.sizeY));
            float trunkHeight = 1.35f + footprint * 0.22f;
            float canopyWidth = 1.45f + footprint * 0.36f;
            float canopyHeight = 1.10f + footprint * 0.18f;
            float seed = placement.ObjectId * 0.071f + placement.LocalX * 0.19f + placement.LocalY * 0.13f;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Silhouette Bark Volume";
            trunk.transform.SetParent(parent, false);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight * 0.36f, 0f);
            trunk.transform.localScale = new Vector3(0.18f + footprint * 0.025f, trunkHeight * 0.36f, 0.18f + footprint * 0.025f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = materials.TreeBark;
            RemoveCollider(trunk);

            for (int i = 0; i < 4; i++)
            {
                float angle = (i * Mathf.PI * 0.5f) + seed;
                float radius = i == 0 ? 0f : 0.18f + Deterministic01(placement.ObjectId * 97 + i * 23) * 0.18f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, trunkHeight + i * 0.12f, Mathf.Sin(angle) * radius);
                Vector3 scale = new Vector3(
                    canopyWidth * (0.92f + Deterministic01(placement.ObjectId * 43 + i * 7) * 0.28f),
                    canopyHeight * (0.72f + i * 0.06f),
                    canopyWidth * (0.78f + Deterministic01(placement.ObjectId * 61 + i * 11) * 0.20f));
                GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.name = "Wind Canopy Silhouette";
                canopy.transform.SetParent(parent, false);
                canopy.transform.localPosition = offset;
                canopy.transform.localRotation = Quaternion.Euler(0f, i * 47f + seed * 29f, 0f);
                canopy.transform.localScale = scale;
                MeshRenderer renderer = canopy.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = materials.TreeCanopy;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                RemoveCollider(canopy);
                AddWind(canopy, 0.055f + i * 0.008f, 0.92f + i * 0.12f, 0.30f, 0.82f, 0.055f);
            }
        }

        private static void AddWindToWorldObject(GameObject target, Mesh mesh, int objectId)
        {
            float meshHeight = mesh == null ? 2f : Mathf.Max(0.1f, mesh.bounds.size.y);
            float amplitude = Mathf.Clamp(meshHeight * 0.018f, 0.035f, 0.20f);
            float speed = 0.82f + Deterministic01(objectId * 67 + 41) * 0.58f;
            float gust = 0.32f + Deterministic01(objectId * 73 + 29) * 0.24f;
            AddWind(target, amplitude, speed, gust, 0.95f, 0.07f);
        }

        private static void AddInteractionAndCollision(GameObject placed, PsychoMirrorObject definition, PsychoMapObjectPlacement placement)
        {
            bool hasActions = definition?.actions != null && Array.Exists(definition.actions, action => !string.IsNullOrWhiteSpace(action));
            bool interactable = definition != null && (definition.interactive || hasActions);
            bool blocksMovement = definition != null && definition.unwalkable && placement.Type != 22;
            bool wall = placement.Type >= 0 && placement.Type <= 8;

            if (interactable)
            {
                placed.AddComponent<PsychoInteractable>().Configure(definition.id, definition.name, definition.actions);
            }

            if (!interactable && !blocksMovement && !wall)
            {
                return;
            }

            BoxCollider collider = placed.AddComponent<BoxCollider>();
            collider.center = ColliderCenter(definition, placement);
            collider.size = ColliderSize(definition, placement);
            collider.isTrigger = !blocksMovement && !wall;
        }

        private static Vector3 ColliderCenter(PsychoMirrorObject definition, PsychoMapObjectPlacement placement)
        {
            Vector3 size = ColliderSize(definition, placement);
            return new Vector3(0f, size.y * 0.5f, 0f);
        }

        private static Vector3 ColliderSize(PsychoMirrorObject definition, PsychoMapObjectPlacement placement)
        {
            if (placement.Type >= 0 && placement.Type <= 3)
            {
                return new Vector3(0.26f, 2.1f, 1.25f);
            }

            if (placement.Type >= 4 && placement.Type <= 8)
            {
                return new Vector3(0.18f, 1.25f, 0.9f);
            }

            float width = definition == null ? 1f : Mathf.Max(1f, definition.sizeX);
            float depth = definition == null ? 1f : Mathf.Max(1f, definition.sizeY);
            float height = placement.Type == 22 ? 0.18f : 1.35f;
            return new Vector3(width, height, depth);
        }

        private static void AddFallbackBounds(Transform parent, PsychoMirrorObject definition, PsychoMapObjectPlacement placement, Material material)
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallback.name = definition == null ? $"fallback_object_{placement.ObjectId}" : "fallback_bounds";
            fallback.isStatic = true;
            fallback.transform.SetParent(parent, false);
            Vector3 size = ColliderSize(definition, placement);
            fallback.transform.localPosition = new Vector3(0f, size.y * 0.5f, 0f);
            fallback.transform.localScale = size;
            MeshRenderer renderer = fallback.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = fallback.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void BuildNpcSpawns(PsychoCacheStore store, PsychoMirrorDatabase database, Material npcMaterial, HostedBuildContext context)
        {
            GameObject npcRoot = new GameObject("Hosted NPC Spawns");
            GameObject factoryObject = new GameObject("Psycho Visual Factory");
            PsychoVisualFactory factory = factoryObject.AddComponent<PsychoVisualFactory>();
            factory.SetNpcPreviewAnimation(false);

            foreach (PsychoMirrorNpcSpawn spawn in database.NpcSpawns)
            {
                if (context.Report.npcSpawns >= MaxNpcSpawns || !IsWorldTileInsideHostedBounds(spawn.x, spawn.y))
                {
                    continue;
                }

                if (!database.TryGetNpc(spawn.npcId, out PsychoMirrorNpc npc))
                {
                    continue;
                }

                bool cacheVisual = TryCreateCacheNpcVisual(store, database, npc, npcMaterial, out GameObject npcObject, context);
                if (!cacheVisual)
                {
                    npcObject = factory.CreateNpcVisual(npc);
                    context.Report.fallbackNpcVisuals++;
                }

                npcObject.transform.SetParent(npcRoot.transform, false);
                npcObject.transform.position = WorldTilePosition(context, spawn.x, spawn.y) + Vector3.up * 0.03f;
                npcObject.name = $"NPC {spawn.npcId} - {npc.name} ({spawn.x}, {spawn.y})";
                npcObject.AddComponent<PsychoGroundFollower>();
                PrototypeNpcWander wander = npcObject.AddComponent<PrototypeNpcWander>();
                SerializedObject wanderObject = new SerializedObject(wander);
                wanderObject.FindProperty("wanderRadius").floatValue = Mathf.Clamp(spawn.walkRadius > 0 ? spawn.walkRadius * TileScale : 2.4f, 1.4f, 7.5f);
                wanderObject.FindProperty("speed").floatValue = Mathf.Clamp(0.85f + npc.scale * 0.42f, 0.85f, 2.35f);
                wanderObject.FindProperty("pauseDuration").floatValue = 0.65f + (spawn.npcId % 5) * 0.14f;
                wanderObject.ApplyModifiedPropertiesWithoutUndo();
                context.Report.npcSpawns++;
            }
        }

        private static bool TryCreateCacheNpcVisual(
            PsychoCacheStore store,
            PsychoMirrorDatabase database,
            PsychoMirrorNpc npc,
            Material material,
            out GameObject npcObject,
            HostedBuildContext context)
        {
            npcObject = null;
            if (!database.TryGetNpcModel(npc.id, out PsychoMirrorNpcModel model) || model.modelIds == null || model.modelIds.Length == 0)
            {
                return false;
            }

            GameObject root = new GameObject($"NPC {npc.id} - {npc.name}");
            GameObject modelRoot = new GameObject("Cache Model");
            modelRoot.transform.SetParent(root.transform, false);

            int importedParts = 0;
            for (int i = 0; i < model.modelIds.Length && i < MaxModelsPerNpc; i++)
            {
                int modelId = model.modelIds[i];
                if (modelId <= 0)
                {
                    continue;
                }

                if (!PsychoCacheMeshImporter.TryImportModelAsset(store, modelId, out Mesh mesh, out _, out _, out _))
                {
                    context.Report.missingNpcModels++;
                    continue;
                }

                if (context.ImportedModels.Add(modelId))
                {
                    context.Report.importedModels++;
                }

                GameObject part = new GameObject($"npc_model_{modelId}");
                part.transform.SetParent(modelRoot.transform, false);
                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                part.AddComponent<MeshRenderer>().sharedMaterial = material;
                importedParts++;
            }

            if (importedParts <= 0)
            {
                UnityEngine.Object.DestroyImmediate(root);
                return false;
            }

            NormalizeNpcModel(modelRoot.transform, npc, model);
            context.Report.cacheNpcVisuals++;
            npcObject = root;
            return true;
        }

        private static void NormalizeNpcModel(Transform modelRoot, PsychoMirrorNpc npc, PsychoMirrorNpcModel model)
        {
            if (!TryCalculateLocalBounds(modelRoot, out Bounds bounds) || bounds.size.y <= 0.001f)
            {
                return;
            }

            float declaredSize = Mathf.Max(1f, Mathf.Max(npc.size, model.size));
            float targetHeight = Mathf.Clamp(1.72f + (declaredSize - 1f) * 0.58f, 1.35f, 5.8f);
            float scale = targetHeight / bounds.size.y;
            modelRoot.localScale = Vector3.one * scale;
            modelRoot.localPosition = new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, -bounds.center.z * scale);
        }

        private static bool TryCalculateLocalBounds(Transform root, out Bounds bounds)
        {
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            bool hasBounds = false;
            foreach (MeshFilter filter in filters)
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                Bounds meshBounds = filter.sharedMesh.bounds;
                Vector3 min = filter.transform.TransformPoint(meshBounds.min);
                Vector3 max = filter.transform.TransformPoint(meshBounds.max);
                Bounds transformed = new Bounds((min + max) * 0.5f, Abs(max - min));
                if (!hasBounds)
                {
                    bounds = transformed;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(transformed);
                }
            }

            return hasBounds;
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

        private static void BuildWorldDressing(HostedBuildContext context, HostedMaterials materials)
        {
            BuildGrassField(context, materials);
            BuildWaterways(context, materials);
            BuildDistantVista(context, materials);
            BuildCloudLayer(materials.Cloud);
        }

        private static void BuildGrassField(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Wind Animated Ground Detail");
            int minX = (BaseRegionX - RegionRadius) * 64 + 4;
            int maxX = (BaseRegionX + RegionRadius + 1) * 64 - 4;
            int minY = (BaseRegionY - RegionRadius) * 64 + 4;
            int maxY = (BaseRegionY + RegionRadius + 1) * 64 - 4;

            for (int i = 0; i < 980; i++)
            {
                int worldX = minX + Mathf.FloorToInt(Deterministic01(i * 31 + 7) * (maxX - minX));
                int worldY = minY + Mathf.FloorToInt(Deterministic01(i * 47 + 19) * (maxY - minY));
                if (!TryGetLandscapeTile(context, worldX, worldY, out PsychoMapLandscape landscape, out int localX, out int localY))
                {
                    continue;
                }

                byte overlay = landscape.OverlayIds[0, localX, localY];
                byte flags = landscape.RenderFlags[0, localX, localY];
                if (overlay != 0 || (flags & 1) == 1)
                {
                    continue;
                }

                float scatterX = (Deterministic01(i * 13 + 3) - 0.5f) * TileScale * 0.78f;
                float scatterZ = (Deterministic01(i * 17 + 5) - 0.5f) * TileScale * 0.78f;
                Vector3 position = WorldTilePosition(context, worldX, worldY) + new Vector3(scatterX, 0.052f, scatterZ);
                Material material = i % 17 == 0 ? materials.Flowers : materials.Grass;
                CreateGrassBlade(root.transform, position, 0.24f + Deterministic01(i * 59 + 11) * 0.34f, material);
            }
        }

        private static void BuildWaterways(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Animated Waterways");
            CreateWaterStrip(root.transform, "River Lum Ripple Strip", context, 3139, 3480, 2.35f, 78f, materials.Water, materials.WaterFoam, materials.WaterDepth, context);

            for (int i = 0; i < 126; i++)
            {
                int worldX = 3137 + Mathf.FloorToInt(Deterministic01(i * 29 + 5) * 4f);
                int worldY = 3441 + Mathf.FloorToInt(Deterministic01(i * 37 + 9) * 84f);
                Vector3 position = WorldTilePosition(context, worldX, worldY) + Vector3.up * 0.11f;
                CreateReed(root.transform, position, 0.48f + Deterministic01(i * 41 + 13) * 0.42f, materials.Reeds);
            }
        }

        private static void BuildDistantVista(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject vistaRoot = new GameObject("Distant Hosted Vista");
            vistaRoot.AddComponent<DistantVistaParallax>();

            for (int i = 0; i < 42; i++)
            {
                float angle = i * Mathf.PI * 2f / 42f;
                float radius = 315f + Mathf.Sin(i * 1.71f) * 26f;
                float width = 34f + (i % 5) * 7f;
                float height = 11f + Mathf.Sin(i * 0.83f) * 2.4f + (i % 4) * 1.5f;
                float depth = 22f + (i % 3) * 6f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, -15.5f, Mathf.Sin(angle) * radius);
                GameObject mountain = CreateMountain($"Distant Mountain {i + 1}", position, width, height, depth, materials.Mountains);
                mountain.transform.SetParent(vistaRoot.transform, true);
            }

            for (int i = 0; i < 34; i++)
            {
                float angle = (i + 0.5f) * Mathf.PI * 2f / 34f;
                float radius = 245f + Mathf.Cos(i * 1.21f) * 18f;
                float width = 46f + (i % 4) * 9f;
                float height = 5.5f + Mathf.Sin(i * 0.94f) * 1.2f;
                float depth = 24f + (i % 3) * 6f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, -13.8f, Mathf.Sin(angle) * radius);
                GameObject hill = CreateMountain($"Distant Hill {i + 1}", position, width, height, depth, materials.Hills);
                hill.transform.SetParent(vistaRoot.transform, true);
            }

        }

        private static void BuildCloudLayer(Material material)
        {
            GameObject root = new GameObject("Moving Cloud Layer");
            for (int i = 0; i < 32; i++)
            {
                float x = Mathf.Sin(i * 2.91f) * 112f;
                float z = Mathf.Cos(i * 1.73f) * 118f;
                float y = 36f + (i % 5) * 3.3f;
                GameObject cloud = new GameObject($"Moving Cloud {i + 1}");
                cloud.transform.SetParent(root.transform, false);
                cloud.transform.position = new Vector3(x, y, z);
                cloud.transform.rotation = Quaternion.Euler(0f, i * 23f, 0f);

                int lobes = 3 + i % 4;
                for (int lobe = 0; lobe < lobes; lobe++)
                {
                    GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    puff.name = "Cloud Puff";
                    puff.transform.SetParent(cloud.transform, false);
                    float localX = (lobe - (lobes - 1) * 0.5f) * 4.8f;
                    puff.transform.localPosition = new Vector3(localX, Mathf.Sin(lobe * 1.7f) * 0.42f, Mathf.Cos(lobe * 1.1f) * 1.35f);
                    puff.transform.localScale = new Vector3(9.4f + lobe * 0.82f, 0.78f + (lobe % 2) * 0.24f, 3.8f + (lobe % 3) * 0.78f);
                    MeshRenderer renderer = puff.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = material;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    Collider collider = puff.GetComponent<Collider>();
                    if (collider != null)
                    {
                        UnityEngine.Object.DestroyImmediate(collider);
                    }
                }

                CloudDrift drift = cloud.AddComponent<CloudDrift>();
                SerializedObject driftObject = new SerializedObject(drift);
                driftObject.FindProperty("driftSpeed").floatValue = 0.28f + (i % 4) * 0.045f;
                driftObject.FindProperty("wrapDistance").floatValue = 230f;
                driftObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void BuildLighting()
        {
            QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 8);
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 4);
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            QualitySettings.shadowProjection = ShadowProjection.CloseFit;
            QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, 260f);
            QualitySettings.shadowCascades = Mathf.Max(QualitySettings.shadowCascades, 4);
            QualitySettings.lodBias = Mathf.Max(QualitySettings.lodBias, 1.65f);
            QualitySettings.softParticles = true;
            QualitySettings.softVegetation = true;
            QualitySettings.realtimeReflectionProbes = true;

            GameObject sunObject = new GameObject("Sun");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.12f;
            sun.color = new Color(1f, 0.93f, 0.82f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.66f;
            sunObject.transform.rotation = Quaternion.Euler(48f, -34f, 0f);

            GameObject fillObject = new GameObject("Soft Sky Fill");
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.25f;
            fill.color = new Color(0.50f, 0.62f, 0.78f);
            fillObject.transform.rotation = Quaternion.Euler(24f, 136f, 0f);

            GameObject bounceObject = new GameObject("Warm Ground Bounce");
            Light bounce = bounceObject.AddComponent<Light>();
            bounce.type = LightType.Directional;
            bounce.intensity = 0.08f;
            bounce.color = new Color(0.76f, 0.62f, 0.42f);
            bounce.shadows = LightShadows.None;
            bounceObject.transform.rotation = Quaternion.Euler(-32f, -18f, 0f);

            GameObject lightingController = new GameObject("Environment Lighting Controller");
            EnvironmentLightingController controller = lightingController.AddComponent<EnvironmentLightingController>();
            lightingController.AddComponent<PsychoRuntimeVisualQuality>();
            typeof(EnvironmentLightingController).GetField("sun", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(controller, sun);

            GameObject windObject = new GameObject("World Wind");
            WindZone wind = windObject.AddComponent<WindZone>();
            wind.mode = WindZoneMode.Directional;
            wind.windMain = 0.45f;
            wind.windPulseMagnitude = 0.28f;
            wind.windPulseFrequency = 0.55f;
            windObject.transform.rotation = Quaternion.Euler(18f, 38f, 0f);

            GameObject reflectionObject = new GameObject("World Reflection Probe");
            ReflectionProbe probe = reflectionObject.AddComponent<ReflectionProbe>();
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
            probe.size = new Vector3(340f, 116f, 340f);
            reflectionObject.transform.position = new Vector3(0f, 18f, 0f);

            RenderSettings.skybox = LoadOrCreateSkybox();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.56f, 0.66f, 0.76f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.43f, 0.39f);
            RenderSettings.ambientGroundColor = new Color(0.17f, 0.17f, 0.14f);
            RenderSettings.ambientIntensity = 0.96f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.56f, 0.66f, 0.74f);
            RenderSettings.fogDensity = 0.00085f;
            RenderSettings.reflectionIntensity = 0.50f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }

        private static void BuildPlayer(HostedBuildContext context, HostedMaterials materials, PsychoMirrorDatabase database)
        {
            HostedPlayerSave playerSave = LoadHostedPlayerSave(HostedPlayerUsername);
            int spawnX = playerSave?.position != null ? playerSave.position.x : 3093;
            int spawnY = playerSave?.position != null ? playerSave.position.y : 3493;
            if (!IsWorldTileInsideHostedBounds(spawnX, spawnY))
            {
                Debug.LogWarning($"Hosted player save position {spawnX}, {spawnY} is outside the generated test world. Falling back to Edgeville.");
                spawnX = 3093;
                spawnY = 3493;
            }

            GameObject player = new GameObject("Playable Adventurer");
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.9f;
            controller.radius = 0.32f;
            controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
            controller.stepOffset = 0.45f;
            controller.slopeLimit = 48f;

            player.transform.position = WorldTilePosition(context, spawnX, spawnY) + Vector3.up * 0.04f;
            player.AddComponent<PsychoPlayableCharacter>();
            player.AddComponent<PsychoCharacterGroundGuard>();
            player.AddComponent<PsychoInteractionController>();
            BuildPlayerVisual(player.transform, materials, database, playerSave);

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 2200f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.depthTextureMode = DepthTextureMode.Depth;
            PsychoCameraColorGrade colorGrade = cameraObject.AddComponent<PsychoCameraColorGrade>();
            SerializedObject colorGradeObject = new SerializedObject(colorGrade);
            SerializedProperty shaderProperty = colorGradeObject.FindProperty("shader");
            if (shaderProperty != null)
            {
                shaderProperty.objectReferenceValue = Shader.Find("Hidden/Psycho/Camera Color Grade");
            }

            SetSerializedFloat(colorGradeObject, "exposure", 1.00f);
            SetSerializedFloat(colorGradeObject, "contrast", 1.07f);
            SetSerializedFloat(colorGradeObject, "saturation", 1.06f);
            SetSerializedFloat(colorGradeObject, "warmth", 0.03f);
            SetSerializedFloat(colorGradeObject, "vignette", 0.16f);
            SetSerializedFloat(colorGradeObject, "sharpen", 0.10f);
            colorGradeObject.ApplyModifiedPropertiesWithoutUndo();
            cameraObject.AddComponent<AudioListener>();
        }

        private static void BuildPlayerVisual(Transform parent, HostedMaterials materials, PsychoMirrorDatabase database, HostedPlayerSave playerSave)
        {
            GameObject visualRoot = new GameObject("Adventurer Visual");
            visualRoot.transform.SetParent(parent, false);

            PsychoMirrorItem head = GetEquippedItem(database, playerSave, EquipmentHeadSlot);
            PsychoMirrorItem cape = GetEquippedItem(database, playerSave, EquipmentCapeSlot);
            PsychoMirrorItem weapon = GetEquippedItem(database, playerSave, EquipmentWeaponSlot);
            PsychoMirrorItem body = GetEquippedItem(database, playerSave, EquipmentBodySlot);
            PsychoMirrorItem shield = GetEquippedItem(database, playerSave, EquipmentShieldSlot);
            PsychoMirrorItem legs = GetEquippedItem(database, playerSave, EquipmentLegSlot);
            PsychoMirrorItem hands = GetEquippedItem(database, playerSave, EquipmentHandsSlot);
            PsychoMirrorItem feet = GetEquippedItem(database, playerSave, EquipmentFeetSlot);

            Material bodyMaterial = MaterialForEquippedItem(body, materials, materials.PlayerCloth);
            Material legMaterial = MaterialForEquippedItem(legs, materials, materials.PlayerLeather);
            Material bootMaterial = MaterialForEquippedItem(feet, materials, materials.PlayerLeather);
            Material handMaterial = MaterialForEquippedItem(hands, materials, materials.PlayerLeather);

            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Torso", new Vector3(0f, 1.02f, 0f), new Vector3(0.52f, 0.62f, 0.30f), bodyMaterial, body);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Chest Plate", new Vector3(0f, 1.12f, -0.02f), new Vector3(0.58f, 0.40f, 0.08f), bodyMaterial, body);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Left Shoulder", new Vector3(-0.36f, 1.24f, 0f), new Vector3(0.22f, 0.18f, 0.26f), bodyMaterial, body);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Right Shoulder", new Vector3(0.36f, 1.24f, 0f), new Vector3(0.22f, 0.18f, 0.26f), bodyMaterial, body);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Left Upper Arm", new Vector3(-0.42f, 0.93f, 0.02f), new Vector3(0.16f, 0.44f, 0.17f), bodyMaterial, body);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Right Upper Arm", new Vector3(0.42f, 0.93f, 0.02f), new Vector3(0.16f, 0.44f, 0.17f), bodyMaterial, body);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Left Hand", new Vector3(-0.43f, 0.63f, 0.04f), new Vector3(0.15f, 0.16f, 0.15f), handMaterial, hands);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Right Hand", new Vector3(0.43f, 0.63f, 0.04f), new Vector3(0.15f, 0.16f, 0.15f), handMaterial, hands);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Left Leg", new Vector3(-0.15f, 0.55f, 0.02f), new Vector3(0.18f, 0.58f, 0.20f), legMaterial, legs);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Right Leg", new Vector3(0.15f, 0.55f, 0.02f), new Vector3(0.18f, 0.58f, 0.20f), legMaterial, legs);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Left Boot", new Vector3(-0.15f, 0.15f, 0.04f), new Vector3(0.18f, 0.28f, 0.24f), bootMaterial, feet);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Right Boot", new Vector3(0.15f, 0.15f, 0.04f), new Vector3(0.18f, 0.28f, 0.24f), bootMaterial, feet);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 1.63f, 0f), new Vector3(0.27f, 0.29f, 0.25f), materials.PlayerSkin);

            if (head != null)
            {
                Material helmetMaterial = MaterialForEquippedItem(head, materials, materials.PlayerMetal);
                CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Sphere, "Helmet", new Vector3(0f, 1.72f, 0f), new Vector3(0.30f, 0.25f, 0.28f), helmetMaterial, head);
                CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Helmet Visor", new Vector3(0f, 1.62f, -0.20f), new Vector3(0.34f, 0.09f, 0.08f), helmetMaterial, head);
            }
            else
            {
                CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Sphere, "Hair", new Vector3(0f, 1.78f, 0f), new Vector3(0.28f, 0.12f, 0.24f), materials.PlayerHair);
            }

            if (cape != null)
            {
                GameObject capePanel = CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Cape", new Vector3(0f, 0.92f, 0.22f), new Vector3(0.56f, 0.82f, 0.05f), MaterialForEquippedItem(cape, materials, materials.PlayerCloth), cape);
                capePanel.transform.localRotation = Quaternion.Euler(-5f, 0f, 0f);
            }

            if (weapon != null)
            {
                CreateEquippedWeapon(visualRoot.transform, weapon, MaterialForEquippedItem(weapon, materials, materials.PlayerMetal), materials);
            }

            if (shield != null)
            {
                CreateEquippedOffhand(visualRoot.transform, shield, MaterialForEquippedItem(shield, materials, materials.PlayerMetal), materials);
            }

            CreatePlayerNameplate(visualRoot.transform, playerSave);
            Debug.Log($"Hosted player visual loaded from Java save for {PlayerDisplayName(playerSave)} with {CountEquippedItems(playerSave)} equipped item slots.");
        }

        private static GameObject CreatePlayerPrimitive(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Material material, PsychoMirrorItem item = null)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = item == null || string.IsNullOrWhiteSpace(item.name) ? name : $"{name} - {item.name}";
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<MeshRenderer>().sharedMaterial = material;
            RemoveCollider(primitive);
            return primitive;
        }

        private static void CreateEquippedWeapon(Transform visualRoot, PsychoMirrorItem weapon, Material material, HostedMaterials materials)
        {
            string weaponName = weapon.name == null ? string.Empty : weapon.name.ToLowerInvariant();
            if (weaponName.Contains("scimitar"))
            {
                GameObject blade = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Scimitar Blade", new Vector3(0.48f, 0.88f, -0.02f), new Vector3(0.05f, 0.74f, 0.08f), material, weapon);
                blade.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
                GameObject tip = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Scimitar Tip", new Vector3(0.61f, 1.14f, -0.02f), new Vector3(0.04f, 0.28f, 0.075f), material, weapon);
                tip.transform.localRotation = Quaternion.Euler(0f, 0f, -55f);
                GameObject guard = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Scimitar Guard", new Vector3(0.40f, 0.62f, -0.02f), new Vector3(0.24f, 0.04f, 0.08f), materials.PlayerMetal, weapon);
                guard.transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
                return;
            }

            if (weaponName.Contains("bow"))
            {
                GameObject bow = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Bow", new Vector3(0.48f, 0.90f, 0.00f), new Vector3(0.06f, 0.86f, 0.06f), materials.PlayerWood, weapon);
                bow.transform.localRotation = Quaternion.Euler(0f, 0f, -10f);
                CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Bow String", new Vector3(0.56f, 0.90f, 0.00f), new Vector3(0.015f, 0.82f, 0.015f), materials.PlayerPaper, weapon);
                return;
            }

            if (weaponName.Contains("staff"))
            {
                GameObject staff = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Staff", new Vector3(0.46f, 0.93f, 0.00f), new Vector3(0.055f, 1.18f, 0.055f), materials.PlayerWood, weapon);
                staff.transform.localRotation = Quaternion.Euler(0f, 0f, -8f);
                CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, "Staff Focus", new Vector3(0.54f, 1.50f, 0.00f), new Vector3(0.13f, 0.13f, 0.13f), material, weapon);
                return;
            }

            GameObject bladeFallback = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Weapon", new Vector3(0.46f, 0.88f, -0.02f), new Vector3(0.055f, 0.82f, 0.065f), material, weapon);
            bladeFallback.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
        }

        private static void CreateEquippedOffhand(Transform visualRoot, PsychoMirrorItem shield, Material material, HostedMaterials materials)
        {
            string shieldName = shield.name == null ? string.Empty : shield.name.ToLowerInvariant();
            if (shieldName.Contains("book"))
            {
                CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Offhand Book Cover", new Vector3(-0.46f, 0.86f, -0.02f), new Vector3(0.08f, 0.42f, 0.30f), material, shield);
                CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Offhand Book Pages", new Vector3(-0.515f, 0.86f, -0.02f), new Vector3(0.025f, 0.36f, 0.25f), materials.PlayerPaper, shield);
                return;
            }

            GameObject shieldObject = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Shield", new Vector3(-0.47f, 0.88f, -0.02f), new Vector3(0.08f, 0.52f, 0.36f), material, shield);
            shieldObject.transform.localRotation = Quaternion.Euler(0f, 0f, 4f);
        }

        private static void CreatePlayerNameplate(Transform visualRoot, HostedPlayerSave playerSave)
        {
            GameObject plate = new GameObject("Player Nameplate");
            plate.transform.SetParent(visualRoot, false);
            plate.transform.localPosition = new Vector3(0f, 2.06f, 0f);
            TextMesh text = plate.AddComponent<TextMesh>();
            text.text = PlayerDisplayName(playerSave);
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.028f;
            text.fontSize = 52;
            text.color = new Color(1f, 0.93f, 0.45f, 1f);
        }

        private static PsychoMirrorItem GetEquippedItem(PsychoMirrorDatabase database, HostedPlayerSave playerSave, int slot)
        {
            if (database == null || playerSave?.equipment == null || slot < 0 || slot >= playerSave.equipment.Length)
            {
                return null;
            }

            HostedSaveItem saveItem = playerSave.equipment[slot];
            if (saveItem == null || saveItem.id < 0)
            {
                return null;
            }

            return database.TryGetItem(saveItem.id, out PsychoMirrorItem item) ? item : null;
        }

        private static Material MaterialForEquippedItem(PsychoMirrorItem item, HostedMaterials materials, Material fallback)
        {
            string materialClass = item?.materialClass == null ? string.Empty : item.materialClass.ToLowerInvariant();
            switch (materialClass)
            {
                case "metal":
                case "rune":
                case "crystal":
                    return materials.PlayerMetal;
                case "leather":
                    return materials.PlayerLeather;
                case "paper":
                case "note":
                    return materials.PlayerPaper;
                case "wood":
                    return materials.PlayerWood;
                case "cloth":
                    return materials.PlayerCloth;
                default:
                    return fallback;
            }
        }

        private static HostedPlayerSave LoadHostedPlayerSave(string username)
        {
            string savePath = FindHostedPlayerSavePath(username);
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogWarning($"Could not find Java player save for {username}. Using hosted visual defaults.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(savePath);
                HostedPlayerSave save = JsonUtility.FromJson<HostedPlayerSave>(json);
                if (save == null)
                {
                    Debug.LogWarning($"Could not parse Java player save at {savePath}. Using hosted visual defaults.");
                }

                return save;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not read Java player save for {username}: {exception.Message}");
                return null;
            }
        }

        private static string FindHostedPlayerSavePath(string username)
        {
            string repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string charactersPath = Path.Combine(repoRoot, "necrotic_server-item_attributes", "data", "saves", "characters");
            if (!Directory.Exists(charactersPath))
            {
                return null;
            }

            string directPath = Path.Combine(charactersPath, username + ".json");
            if (File.Exists(directPath))
            {
                return directPath;
            }

            foreach (string file in Directory.GetFiles(charactersPath, "*.json", SearchOption.TopDirectoryOnly))
            {
                if (string.Equals(Path.GetFileNameWithoutExtension(file), username, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            return null;
        }

        private static string PlayerDisplayName(HostedPlayerSave playerSave)
        {
            return string.IsNullOrWhiteSpace(playerSave?.username) ? HostedPlayerUsername : playerSave.username;
        }

        private static int CountEquippedItems(HostedPlayerSave playerSave)
        {
            if (playerSave?.equipment == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < playerSave.equipment.Length; i++)
            {
                if (playerSave.equipment[i] != null && playerSave.equipment[i].id >= 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static void BuildNetworkBootstrap()
        {
            GameObject network = new GameObject("Server Protocol Bootstrap");
            network.AddComponent<PsychoProtocolClient>();
            network.AddComponent<PsychoNetworkBootstrap>();
        }

        private static Vector3 WorldTilePosition(HostedBuildContext context, int worldX, int worldY)
        {
            int regionX = Mathf.FloorToInt(worldX / 64f);
            int regionY = Mathf.FloorToInt(worldY / 64f);
            int regionId = (regionX << 8) + regionY;
            if (context.Landscapes.TryGetValue(regionId, out PsychoMapLandscape landscape))
            {
                return TilePosition(landscape, worldX - regionX * 64, worldY - regionY * 64);
            }

            float x = (worldX - BaseRegionX * 64 + 0.5f) * TileScale;
            float z = (worldY - BaseRegionY * 64 + 0.5f) * TileScale;
            return new Vector3(x, 0f, z);
        }

        private static Vector3 TilePosition(PsychoMapLandscape landscape, int localX, int localY)
        {
            int x = Mathf.Clamp(localX, 0, 63);
            int y = Mathf.Clamp(localY, 0, 63);
            float height = -landscape.Heights[0, x, y] * HeightScale;
            return RegionOrigin(landscape.RegionX, landscape.RegionY) + new Vector3((x + 0.5f) * TileScale, height, (y + 0.5f) * TileScale);
        }

        private static Vector3 RegionOrigin(int regionX, int regionY)
        {
            return new Vector3((regionX - BaseRegionX) * 64f * TileScale, 0f, (regionY - BaseRegionY) * 64f * TileScale);
        }

        private static bool IsWorldTileInsideHostedBounds(int worldX, int worldY)
        {
            int minX = (BaseRegionX - RegionRadius) * 64;
            int maxX = (BaseRegionX + RegionRadius + 1) * 64;
            int minY = (BaseRegionY - RegionRadius) * 64;
            int maxY = (BaseRegionY + RegionRadius + 1) * 64;
            return worldX >= minX && worldX < maxX && worldY >= minY && worldY < maxY;
        }

        private static bool TryGetLandscapeTile(HostedBuildContext context, int worldX, int worldY, out PsychoMapLandscape landscape, out int localX, out int localY)
        {
            int regionX = Mathf.FloorToInt(worldX / 64f);
            int regionY = Mathf.FloorToInt(worldY / 64f);
            int regionId = (regionX << 8) + regionY;
            localX = worldX - regionX * 64;
            localY = worldY - regionY * 64;
            return context.Landscapes.TryGetValue(regionId, out landscape) && localX >= 0 && localX < 64 && localY >= 0 && localY < 64;
        }

        private static void ValidateTerrainCollisionCoverage(HostedBuildContext context)
        {
            RaycastHit[] hits = new RaycastHit[32];
            Physics.SyncTransforms();

            foreach (PsychoMapLandscape landscape in context.Landscapes.Values)
            {
                for (int x = 0; x < 64; x++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        context.Report.terrainCollisionSamples++;
                        Vector3 tile = TilePosition(landscape, x, y);
                        Vector3 origin = new Vector3(tile.x, tile.y + 128f, tile.z);
                        int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hits, 256f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                        if (!HasTerrainHit(hits, hitCount))
                        {
                            context.Report.terrainCollisionMisses++;
                        }
                    }
                }
            }

            if (context.Report.terrainCollisionMisses > 0)
            {
                Debug.LogWarning($"Hosted terrain collision audit found {context.Report.terrainCollisionMisses} missing tile-center hits across {context.Report.terrainCollisionSamples} samples.");
            }
            else
            {
                Debug.Log($"Hosted terrain collision audit passed {context.Report.terrainCollisionSamples} tile-center samples.");
            }
        }

        private static bool HasTerrainHit(RaycastHit[] hits, int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = hits[i].collider;
                if (hitCollider == null)
                {
                    continue;
                }

                if (hitCollider.gameObject.name.Contains("Terrain"))
                {
                    return true;
                }

                MeshCollider meshCollider = hitCollider as MeshCollider;
                if (meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.name.StartsWith("psycho_map_region_"))
                {
                    return true;
                }
            }

            return false;
        }

        private static void CreateWaterStrip(Transform root, string name, HostedBuildContext context, int worldX, int worldY, float width, float depth, Material material, Material foamMaterial, Material depthMaterial, HostedBuildContext buildContext)
        {
            Vector3 position = WorldTilePosition(context, worldX, worldY) + Vector3.up * 0.08f;
            GameObject water = CreateSubdividedPlane(name, position, width, depth, 72, material);
            water.transform.SetParent(root, true);
            water.AddComponent<ProceduralWater>();
            CreateWaterDepthRibbon(water.transform, "Deep Center Current", width * 0.52f, depth, depthMaterial);
            CreateWaterFoamRibbon(water.transform, "West Shoreline Foam", -width * 0.47f, depth, foamMaterial, 0.17f);
            CreateWaterFoamRibbon(water.transform, "East Shoreline Foam", width * 0.47f, depth, foamMaterial, 0.61f);
            buildContext.Report.waterDepthChannels++;
            buildContext.Report.waterFoamEdges += 2;

            BoxCollider trigger = water.AddComponent<BoxCollider>();
            trigger.center = Vector3.zero;
            trigger.size = new Vector3(width, 0.3f, depth);
            trigger.isTrigger = true;
        }

        private static GameObject CreateSubdividedPlane(string name, Vector3 position, float width, float depth, int segments, Material material)
        {
            Mesh mesh = new Mesh { name = name + " Mesh" };
            Vector3[] vertices = new Vector3[(segments + 1) * (segments + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[segments * segments * 6];
            int vertex = 0;
            for (int z = 0; z <= segments; z++)
            {
                for (int x = 0; x <= segments; x++)
                {
                    float px = ((float)x / segments - 0.5f) * width;
                    float pz = ((float)z / segments - 0.5f) * depth;
                    vertices[vertex] = new Vector3(px, 0f, pz);
                    uv[vertex] = new Vector2((float)x / segments, (float)z / segments);
                    vertex++;
                }
            }

            int tri = 0;
            for (int z = 0; z < segments; z++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int i = z * (segments + 1) + x;
                    triangles[tri++] = i;
                    triangles[tri++] = i + segments + 1;
                    triangles[tri++] = i + 1;
                    triangles[tri++] = i + 1;
                    triangles[tri++] = i + segments + 1;
                    triangles[tri++] = i + segments + 2;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject plane = new GameObject(name);
            plane.transform.position = position;
            plane.AddComponent<MeshFilter>().sharedMesh = mesh;
            plane.AddComponent<MeshRenderer>().sharedMaterial = material;
            return plane;
        }

        private static void CreateWaterDepthRibbon(Transform parent, string name, float width, float depth, Material material)
        {
            const int segments = 44;
            Mesh mesh = new Mesh { name = name + " Mesh" };
            Vector3[] vertices = new Vector3[(segments + 1) * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[segments * 6];

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float z = (t - 0.5f) * depth;
                float edgeWobble = Mathf.Sin(t * Mathf.PI * 7.0f) * 0.055f + Mathf.Sin(t * Mathf.PI * 17.0f) * 0.024f;
                int v = i * 2;
                vertices[v] = new Vector3(-width * 0.5f + edgeWobble, 0.018f, z);
                vertices[v + 1] = new Vector3(width * 0.5f + edgeWobble * 0.55f, 0.018f, z);
                uv[v] = new Vector2(0f, t * 7f);
                uv[v + 1] = new Vector2(1f, t * 7f);
            }

            int tri = 0;
            for (int i = 0; i < segments; i++)
            {
                int v = i * 2;
                triangles[tri++] = v;
                triangles[tri++] = v + 2;
                triangles[tri++] = v + 1;
                triangles[tri++] = v + 1;
                triangles[tri++] = v + 2;
                triangles[tri++] = v + 3;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject ribbon = new GameObject(name);
            ribbon.transform.SetParent(parent, false);
            ribbon.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = ribbon.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void CreateWaterFoamRibbon(Transform parent, string name, float xOffset, float depth, Material material, float seed)
        {
            const int segments = 40;
            const float ribbonWidth = 0.34f;
            Mesh mesh = new Mesh { name = name + " Mesh" };
            Vector3[] vertices = new Vector3[(segments + 1) * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[segments * 6];

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float z = (t - 0.5f) * depth;
                float wobble = Mathf.Sin(t * Mathf.PI * 8.0f + seed * 11f) * 0.035f + Mathf.Sin(t * Mathf.PI * 19.0f + seed * 7f) * 0.018f;
                float inner = Mathf.Sign(xOffset) * wobble;
                int v = i * 2;
                vertices[v] = new Vector3(xOffset + inner, 0.026f, z);
                vertices[v + 1] = new Vector3(xOffset - Mathf.Sign(xOffset) * ribbonWidth + inner * 0.45f, 0.032f, z);
                uv[v] = new Vector2(0f, t * 8f);
                uv[v + 1] = new Vector2(1f, t * 8f);
            }

            int tri = 0;
            for (int i = 0; i < segments; i++)
            {
                int v = i * 2;
                triangles[tri++] = v;
                triangles[tri++] = v + 2;
                triangles[tri++] = v + 1;
                triangles[tri++] = v + 1;
                triangles[tri++] = v + 2;
                triangles[tri++] = v + 3;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject ribbon = new GameObject(name);
            ribbon.transform.SetParent(parent, false);
            ribbon.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = ribbon.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void CreateGrassBlade(Transform root, Vector3 position, float height, Material material)
        {
            Mesh mesh = new Mesh { name = "Hosted Wind Grass Mesh" };
            float width = height * 0.16f;
            const int bladeCount = 4;
            Vector3[] vertices = new Vector3[bladeCount * 4];
            int[] triangles = new int[bladeCount * 6];
            for (int blade = 0; blade < bladeCount; blade++)
            {
                float angle = blade * Mathf.PI * 2f / bladeCount;
                Vector3 side = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 lean = new Vector3(Mathf.Sin(angle * 1.7f), 0f, Mathf.Cos(angle * 1.3f)) * height * 0.08f;
                float localHeight = height * (0.88f + blade * 0.08f);
                float localWidth = width * (1f - blade * 0.12f);
                int v = blade * 4;
                vertices[v] = -side * localWidth;
                vertices[v + 1] = side * localWidth;
                vertices[v + 2] = -side * localWidth * 0.24f + Vector3.up * localHeight + lean;
                vertices[v + 3] = side * localWidth * 0.24f + Vector3.up * localHeight + lean;

                int t = blade * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + 2;
                triangles[t + 5] = v + 3;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject clump = new GameObject("Wind Grass");
            clump.transform.SetParent(root, false);
            clump.transform.position = position;
            clump.transform.rotation = Quaternion.Euler(0f, position.x * 37f + position.z * 17f, 0f);
            clump.AddComponent<MeshFilter>().sharedMesh = mesh;
            clump.AddComponent<MeshRenderer>().sharedMaterial = material;
            AddWind(clump, 0.13f, 1.95f, 0.44f);
        }

        private static void CreateReed(Transform root, Vector3 position, float height, Material material)
        {
            Mesh mesh = new Mesh { name = "Hosted Reed Mesh" };
            float width = height * 0.06f;
            mesh.vertices = new[]
            {
                new Vector3(-width, 0f, 0f),
                new Vector3(width, 0f, 0f),
                new Vector3(-width * 0.25f, height, 0f),
                new Vector3(width * 0.25f, height, 0f)
            };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject reed = new GameObject("Water Reed");
            reed.transform.SetParent(root, false);
            reed.transform.position = position;
            reed.transform.rotation = Quaternion.Euler(0f, position.x * 51f + position.z * 29f, 0f);
            reed.AddComponent<MeshFilter>().sharedMesh = mesh;
            reed.AddComponent<MeshRenderer>().sharedMaterial = material;
            AddWind(reed, 0.18f, 1.55f, 0.52f);
        }

        private static GameObject CreateMountain(string name, Vector3 position, float width, float height, float depth, Material material)
        {
            Mesh mesh = new Mesh { name = name + " Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-width * 0.5f, 0f, -depth * 0.5f),
                new Vector3(width * 0.5f, 0f, -depth * 0.5f),
                new Vector3(width * 0.58f, 0f, depth * 0.45f),
                new Vector3(-width * 0.58f, 0f, depth * 0.45f),
                new Vector3(-width * 0.16f, height * 0.72f, -depth * 0.04f),
                new Vector3(width * 0.18f, height, depth * 0.06f)
            };
            mesh.triangles = new[]
            {
                0, 4, 1,
                1, 4, 5,
                1, 5, 2,
                2, 5, 3,
                3, 5, 4,
                3, 4, 0,
                0, 1, 2,
                0, 2, 3
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject mountain = new GameObject(name);
            mountain.transform.position = position;
            Vector3 inward = new Vector3(-position.x, 0f, -position.z);
            if (inward.sqrMagnitude > 0.01f)
            {
                mountain.transform.rotation = Quaternion.LookRotation(inward.normalized);
            }

            mountain.AddComponent<MeshFilter>().sharedMesh = mesh;
            mountain.AddComponent<MeshRenderer>().sharedMaterial = material;
            return mountain;
        }

        private static HostedMaterials LoadOrCreateHostedMaterials()
        {
            HostedMaterials materials = new HostedMaterials
            {
                Grass = LoadOrCreateTexturedMaterial(GrassMaterialPath, "Grass", new Color(0.29f, 0.47f, 0.23f, 1f), 0.24f, new Vector2(8.5f, 8.5f), 0.68f),
                Flowers = LoadOrCreateTexturedMaterial(FlowerMaterialPath, "Organic", new Color(0.92f, 0.77f, 0.38f, 1f), 0.28f, new Vector2(5.2f, 5.2f), 0.56f),
                Reeds = LoadOrCreateTexturedMaterial(ReedMaterialPath, "Leaf", new Color(0.42f, 0.55f, 0.24f, 1f), 0.20f, new Vector2(3.5f, 5.8f), 0.72f),
                Water = LoadOrCreateTexturedMaterial(WaterMaterialPath, "Water", new Color(0.06f, 0.32f, 0.48f, 0.58f), 0.88f, new Vector2(2.6f, 6.2f), 0.92f),
                Hills = LoadOrCreateTexturedMaterial(HillMaterialPath, "Grass", new Color(0.27f, 0.40f, 0.24f, 1f), 0.30f, new Vector2(5.8f, 5.8f), 0.58f),
                Mountains = LoadOrCreateTexturedMaterial(MountainMaterialPath, "Mountain", new Color(0.46f, 0.46f, 0.42f, 1f), 0.48f, new Vector2(3.2f, 3.2f), 0.72f),
                Cloud = LoadOrCreateUnlitMaterial(CloudMaterialPath, new Color(0.93f, 0.96f, 0.98f, 0.70f)),
                TreeCanopy = LoadOrCreateTexturedMaterial(TreeCanopyMaterialPath, "Leaf", new Color(0.25f, 0.47f, 0.18f, 1f), 0.23f, new Vector2(3.6f, 3.6f), 0.66f),
                TreeBark = LoadOrCreateTexturedMaterial(TreeBarkMaterialPath, "Wood", new Color(0.37f, 0.22f, 0.11f, 1f), 0.24f, new Vector2(2.8f, 4.4f), 0.74f),
                WaterFoam = LoadOrCreateSolidMaterial(WaterFoamMaterialPath, new Color(0.73f, 0.92f, 0.95f, 0.34f), 0.36f),
                WaterDepth = LoadOrCreateSolidMaterial(WaterDepthMaterialPath, new Color(0.02f, 0.12f, 0.18f, 0.30f), 0.54f),
                PlayerCloth = LoadOrCreateTexturedMaterial(PlayerClothMaterialPath, "Cloth", new Color(0.28f, 0.42f, 0.56f, 1f), 0.34f, new Vector2(2.5f, 2.5f), 0.52f),
                PlayerLeather = LoadOrCreateTexturedMaterial(PlayerLeatherMaterialPath, "Leather", new Color(0.52f, 0.34f, 0.22f, 1f), 0.30f, new Vector2(2.2f, 2.2f), 0.48f),
                PlayerMetal = LoadOrCreateTexturedMaterial(PlayerMetalMaterialPath, "Metal", new Color(0.70f, 0.70f, 0.66f, 1f), 0.62f, new Vector2(2.2f, 2.2f), 0.42f),
                PlayerSkin = LoadOrCreateSolidMaterial(PlayerSkinMaterialPath, new Color(0.77f, 0.57f, 0.42f, 1f), 0.28f),
                PlayerHair = LoadOrCreateSolidMaterial(PlayerHairMaterialPath, new Color(0.23f, 0.16f, 0.09f, 1f), 0.32f),
                PlayerPaper = LoadOrCreateTexturedMaterial(PlayerPaperMaterialPath, "Paper", new Color(0.74f, 0.68f, 0.54f, 1f), 0.24f, new Vector2(1.8f, 1.8f), 0.36f),
                PlayerWood = LoadOrCreateTexturedMaterial(PlayerWoodMaterialPath, "Wood", new Color(0.38f, 0.24f, 0.12f, 1f), 0.28f, new Vector2(2.2f, 2.2f), 0.50f)
            };
            ConfigureTransparent(materials.Water);
            ConfigureTransparent(materials.Cloud);
            ConfigureTransparent(materials.WaterFoam);
            ConfigureTransparent(materials.WaterDepth);
            return materials;
        }

        private static Material LoadOrCreateSkybox()
        {
            const string skyboxPath = "Assets/PsychoSkyboxPrototype.mat";
            Material skybox = AssetDatabase.LoadAssetAtPath<Material>(skyboxPath);
            if (skybox == null)
            {
                skybox = new Material(Shader.Find("Skybox/Procedural"));
                AssetDatabase.CreateAsset(skybox, skyboxPath);
            }

            skybox.SetColor("_SkyTint", new Color(0.37f, 0.54f, 0.70f));
            skybox.SetColor("_GroundColor", new Color(0.31f, 0.35f, 0.34f));
            skybox.SetFloat("_AtmosphereThickness", 0.72f);
            skybox.SetFloat("_Exposure", 1.12f);
            skybox.SetFloat("_SunSize", 0.035f);
            skybox.SetFloat("_SunSizeConvergence", 5.5f);
            EditorUtility.SetDirty(skybox);
            return skybox;
        }

        private static Material LoadOrCreateSolidMaterial(string assetPath, Color color, float smoothness)
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
            material.enableInstancing = true;
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateTexturedMaterial(string assetPath, string textureKey, Color tint, float smoothness, Vector2 tiling, float normalScale)
        {
            Material material = LoadOrCreateSolidMaterial(assetPath, tint, smoothness);
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/PsychoMaterials/Psycho_{textureKey}_Albedo_2K.png");
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/PsychoMaterials/Psycho_{textureKey}_Normal_2K.png");

            if (albedo != null)
            {
                material.mainTexture = albedo;
                material.mainTextureScale = tiling;
            }

            if (normal != null && material.HasProperty("_BumpMap"))
            {
                material.SetTexture("_BumpMap", normal);
                material.SetTextureScale("_BumpMap", tiling);
                material.SetFloat("_BumpScale", normalScale);
                material.EnableKeyword("_NORMALMAP");
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateUnlitMaterial(string assetPath, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Shader shader = Shader.Find("Unlit/Transparent") ?? Shader.Find("Unlit/Color");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }
            else if (shader != null)
            {
                material.shader = shader;
            }

            material.color = color;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTransparent(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static void AddWind(GameObject target, float amplitude, float speed, float gustStrength, float spatialFrequency = 1.4f, float turbulence = 0.045f)
        {
            WindAnimatedFoliage wind = target.AddComponent<WindAnimatedFoliage>();
            SerializedObject windObject = new SerializedObject(wind);
            SetSerializedFloat(windObject, "amplitude", amplitude);
            SetSerializedFloat(windObject, "speed", speed);
            SetSerializedFloat(windObject, "gustStrength", gustStrength);
            SetSerializedFloat(windObject, "spatialFrequency", spatialFrequency);
            SetSerializedFloat(windObject, "turbulence", turbulence);
            windObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RemoveCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void SetSerializedFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static float Deterministic01(int seed)
        {
            unchecked
            {
                int hash = seed * 1103515245 + 12345;
                hash ^= hash >> 13;
                hash *= 1274126177;
                hash ^= hash >> 16;
                return (hash & 0xffff) / 65535f;
            }
        }

        private static void WriteReport(HostedBuildReport report)
        {
            report.generatedAtUtc = DateTime.UtcNow.ToString("O");
            report.baseRegionId = (BaseRegionX << 8) + BaseRegionY;
            report.regionRadius = RegionRadius;
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

        private sealed class HostedBuildContext
        {
            public readonly PsychoMirrorDatabase Database;
            public readonly HostedBuildReport Report = new HostedBuildReport();
            public readonly Dictionary<int, PsychoMapLandscape> Landscapes = new Dictionary<int, PsychoMapLandscape>();
            public readonly HashSet<int> ImportedModels = new HashSet<int>();

            public HostedBuildContext(PsychoMirrorDatabase database)
            {
                Database = database;
            }
        }

        private sealed class HostedMaterials
        {
            public Material Grass;
            public Material Flowers;
            public Material Reeds;
            public Material Water;
            public Material Hills;
            public Material Mountains;
            public Material Cloud;
            public Material TreeCanopy;
            public Material TreeBark;
            public Material WaterFoam;
            public Material WaterDepth;
            public Material PlayerCloth;
            public Material PlayerLeather;
            public Material PlayerMetal;
            public Material PlayerSkin;
            public Material PlayerHair;
            public Material PlayerPaper;
            public Material PlayerWood;
        }

        [Serializable]
        private sealed class HostedPlayerSave
        {
            public string username;
            public string gender;
            public HostedSavePosition position;
            public int[] appearance;
            public HostedSaveItem[] equipment;
        }

        [Serializable]
        private sealed class HostedSavePosition
        {
            public int x;
            public int y;
            public int z;
        }

        [Serializable]
        private sealed class HostedSaveItem
        {
            public int id;
            public int amount;
        }

        [Serializable]
        private sealed class HostedBuildReport
        {
            public string generatedAtUtc;
            public int baseRegionId;
            public int regionRadius;
            public int loadedRegions;
            public int missingRegions;
            public int decodedObjectPlacements;
            public int placedObjects;
            public int skippedObjects;
            public int importedModels;
            public int missingDefinitions;
            public int missingModels;
            public int fallbackObjects;
            public int npcSpawns;
            public int windAnimatedObjects;
            public int cacheNpcVisuals;
            public int fallbackNpcVisuals;
            public int missingNpcModels;
            public int terrainCollisionSamples;
            public int terrainCollisionMisses;
            public int enhancedFoliageObjects;
            public int waterFoamEdges;
            public int waterDepthChannels;
            public int horizonMistPanels;
        }
    }
}
