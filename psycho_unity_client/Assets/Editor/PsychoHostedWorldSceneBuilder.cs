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
        private const int MaxNpcSpawns = 180;
        private const float TileScale = 0.72f;
        private const float HeightScale = 1f / 96f;
        private const string ScenePath = "Assets/Scenes/PsychoHostedTestWorld.unity";
        private const string GeneratedRoot = "Assets/Generated/Hosted";
        private const string ReportPath = GeneratedRoot + "/hosted_test_world_report.json";
        private const string WindowsBuildPath = "Builds/PsychoHostedTestWorld/Psycho.exe";
        private const string GrassMaterialPath = GeneratedRoot + "/Psycho_Hosted_Grass.mat";
        private const string FlowerMaterialPath = GeneratedRoot + "/Psycho_Hosted_Flowers.mat";
        private const string ReedMaterialPath = GeneratedRoot + "/Psycho_Hosted_Reeds.mat";
        private const string WaterMaterialPath = GeneratedRoot + "/Psycho_Hosted_Water.mat";
        private const string HillMaterialPath = GeneratedRoot + "/Psycho_Hosted_Hills.mat";
        private const string MountainMaterialPath = GeneratedRoot + "/Psycho_Hosted_Mountains.mat";
        private const string CloudMaterialPath = GeneratedRoot + "/Psycho_Hosted_Clouds.mat";

        [MenuItem("Psycho/Build Hosted Test World Scene")]
        public static void BuildHostedTestWorldScene()
        {
            EnsureFolders();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PsychoMirrorDatabase database = PsychoMirrorDatabase.LoadFromStreamingAssets();
            Material vertexColorMaterial = PsychoCacheMeshImporter.LoadOrCreateVertexColorMaterial();
            HostedBuildContext context = new HostedBuildContext(database);

            using (PsychoCacheStore store = new PsychoCacheStore(PsychoCacheStore.DefaultClientCachePath))
            {
                BuildRegions(store, database, vertexColorMaterial, context);
                BuildNpcSpawns(database, context);
                BuildWorldDressing(context);
                BuildLighting();
                BuildPlayer(context);
                BuildNetworkBootstrap();
            }

            WriteReport(context.Report);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Hosted test world built: {ScenePath}. Regions {context.Report.loadedRegions}, objects {context.Report.placedObjects}, NPCs {context.Report.npcSpawns}.");
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
            Camera camera = UnityEngine.Object.FindObjectOfType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene does not contain a camera.");
            }

            RenderTexture target = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            Texture2D capture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture previousCameraTarget = camera.targetTexture;

            camera.transform.position = new Vector3(35f, 24f, 26f);
            camera.transform.rotation = Quaternion.Euler(38f, 42f, 0f);
            camera.targetTexture = target;
            RenderTexture.active = target;
            camera.Render();
            capture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            capture.Apply();

            camera.targetTexture = previousCameraTarget;
            RenderTexture.active = previous;

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-test-world-preview.png"));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(capture);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            Debug.Log($"Rendered hosted test world preview to {outputPath}");
        }

        public static void RenderHostedTestWorldPreviewBatch()
        {
            RenderHostedTestWorldPreview();
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

        private static void BuildRegions(PsychoCacheStore store, PsychoMirrorDatabase database, Material material, HostedBuildContext context)
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
                    BuildObjects(store, database, objectRoot.transform, landscape, objects, material, context);
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
            HostedBuildContext context)
        {
            int placedInRegion = 0;
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
                GameObject placed = new GameObject($"Object {placement.ObjectId} - {objectName}");
                placed.isStatic = true;
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
                        modelObject.isStatic = true;
                        modelObject.transform.SetParent(placed.transform, false);
                        modelObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                        modelObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                        addedMeshes++;
                    }
                }

                if (addedMeshes == 0)
                {
                    AddFallbackBounds(placed.transform, definition, placement);
                    context.Report.fallbackObjects++;
                }

                AddInteractionAndCollision(placed, definition, placement);
                placedInRegion++;
                context.Report.placedObjects++;
            }
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

        private static void AddFallbackBounds(Transform parent, PsychoMirrorObject definition, PsychoMapObjectPlacement placement)
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallback.name = definition == null ? $"fallback_object_{placement.ObjectId}" : "fallback_bounds";
            fallback.isStatic = true;
            fallback.transform.SetParent(parent, false);
            Vector3 size = ColliderSize(definition, placement);
            fallback.transform.localPosition = new Vector3(0f, size.y * 0.5f, 0f);
            fallback.transform.localScale = size;
            Collider collider = fallback.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void BuildNpcSpawns(PsychoMirrorDatabase database, HostedBuildContext context)
        {
            GameObject npcRoot = new GameObject("Hosted NPC Spawns");
            GameObject factoryObject = new GameObject("Psycho Visual Factory");
            PsychoVisualFactory factory = factoryObject.AddComponent<PsychoVisualFactory>();

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

                GameObject npcObject = factory.CreateNpcVisual(npc);
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

        private static void BuildWorldDressing(HostedBuildContext context)
        {
            HostedMaterials materials = LoadOrCreateHostedMaterials();
            BuildGrassField(context, materials);
            BuildWaterways(context, materials);
            BuildDistantVista(materials);
            BuildCloudLayer(materials.Cloud);
        }

        private static void BuildGrassField(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Wind Animated Ground Detail");
            int minX = (BaseRegionX - RegionRadius) * 64 + 4;
            int maxX = (BaseRegionX + RegionRadius + 1) * 64 - 4;
            int minY = (BaseRegionY - RegionRadius) * 64 + 4;
            int maxY = (BaseRegionY + RegionRadius + 1) * 64 - 4;

            for (int i = 0; i < 520; i++)
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

                float scatterX = (Deterministic01(i * 13 + 3) - 0.5f) * TileScale * 0.72f;
                float scatterZ = (Deterministic01(i * 17 + 5) - 0.5f) * TileScale * 0.72f;
                Vector3 position = WorldTilePosition(context, worldX, worldY) + new Vector3(scatterX, 0.055f, scatterZ);
                Material material = i % 23 == 0 ? materials.Flowers : materials.Grass;
                CreateGrassBlade(root.transform, position, 0.28f + Deterministic01(i * 59 + 11) * 0.28f, material);
            }
        }

        private static void BuildWaterways(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Animated Waterways");
            CreateWaterStrip(root.transform, "River Lum Ripple Strip", context, 3139, 3480, 2.35f, 78f, materials.Water);

            for (int i = 0; i < 76; i++)
            {
                int worldX = 3137 + Mathf.FloorToInt(Deterministic01(i * 29 + 5) * 4f);
                int worldY = 3441 + Mathf.FloorToInt(Deterministic01(i * 37 + 9) * 84f);
                Vector3 position = WorldTilePosition(context, worldX, worldY) + Vector3.up * 0.11f;
                CreateReed(root.transform, position, 0.48f + Deterministic01(i * 41 + 13) * 0.42f, materials.Reeds);
            }
        }

        private static void BuildDistantVista(HostedMaterials materials)
        {
            GameObject vistaRoot = new GameObject("Distant Hosted Vista");
            vistaRoot.AddComponent<DistantVistaParallax>();

            for (int i = 0; i < 30; i++)
            {
                float angle = i * Mathf.PI * 2f / 30f;
                float radius = 315f + Mathf.Sin(i * 1.71f) * 26f;
                float width = 34f + (i % 5) * 7f;
                float height = 11f + Mathf.Sin(i * 0.83f) * 2.4f + (i % 4) * 1.5f;
                float depth = 22f + (i % 3) * 6f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, -15.5f, Mathf.Sin(angle) * radius);
                GameObject mountain = CreateMountain($"Distant Mountain {i + 1}", position, width, height, depth, materials.Mountains);
                mountain.transform.SetParent(vistaRoot.transform, true);
            }

        }

        private static void BuildCloudLayer(Material material)
        {
            GameObject root = new GameObject("Moving Cloud Layer");
            for (int i = 0; i < 22; i++)
            {
                float x = Mathf.Sin(i * 2.91f) * 92f;
                float z = Mathf.Cos(i * 1.73f) * 98f;
                float y = 34f + (i % 5) * 3.1f;
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
                    puff.transform.localScale = new Vector3(8.2f + lobe * 0.75f, 0.86f + (lobe % 2) * 0.26f, 3.4f + (lobe % 3) * 0.72f);
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
                driftObject.FindProperty("driftSpeed").floatValue = 0.35f + (i % 4) * 0.055f;
                driftObject.FindProperty("wrapDistance").floatValue = 185f;
                driftObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void BuildLighting()
        {
            QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 4);
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.softParticles = true;

            GameObject sunObject = new GameObject("Sun");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.18f;
            sun.color = new Color(1f, 0.91f, 0.76f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.58f;
            sunObject.transform.rotation = Quaternion.Euler(48f, -34f, 0f);

            GameObject fillObject = new GameObject("Soft Sky Fill");
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.16f;
            fill.color = new Color(0.45f, 0.58f, 0.72f);
            fillObject.transform.rotation = Quaternion.Euler(24f, 136f, 0f);

            GameObject lightingController = new GameObject("Environment Lighting Controller");
            EnvironmentLightingController controller = lightingController.AddComponent<EnvironmentLightingController>();
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
            probe.size = new Vector3(220f, 96f, 220f);
            reflectionObject.transform.position = new Vector3(0f, 18f, 0f);

            RenderSettings.skybox = LoadOrCreateSkybox();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.50f, 0.58f, 0.66f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.40f, 0.35f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.17f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.48f, 0.58f, 0.64f);
            RenderSettings.fogDensity = 0.00135f;
            RenderSettings.reflectionIntensity = 0.42f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }

        private static void BuildPlayer(HostedBuildContext context)
        {
            GameObject player = new GameObject("Playable Adventurer");
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.9f;
            controller.radius = 0.32f;
            controller.stepOffset = 0.45f;
            controller.slopeLimit = 48f;

            player.transform.position = WorldTilePosition(context, 3093, 3493) + Vector3.up * 1.15f;
            player.AddComponent<PsychoPlayableCharacter>();
            player.AddComponent<PsychoInteractionController>();

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
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 1600f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.depthTextureMode = DepthTextureMode.Depth;
            cameraObject.AddComponent<AudioListener>();
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

        private static void CreateWaterStrip(Transform root, string name, HostedBuildContext context, int worldX, int worldY, float width, float depth, Material material)
        {
            Vector3 position = WorldTilePosition(context, worldX, worldY) + Vector3.up * 0.08f;
            GameObject water = CreateSubdividedPlane(name, position, width, depth, 36, material);
            water.transform.SetParent(root, true);
            water.AddComponent<ProceduralWater>();

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

        private static void CreateGrassBlade(Transform root, Vector3 position, float height, Material material)
        {
            Mesh mesh = new Mesh { name = "Hosted Wind Grass Mesh" };
            float width = height * 0.16f;
            Vector3[] vertices = new Vector3[12];
            int[] triangles = new int[18];
            for (int blade = 0; blade < 3; blade++)
            {
                float angle = blade * Mathf.PI * 2f / 3f;
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
            AddWind(clump, 0.12f, 1.9f, 0.38f);
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
                Grass = LoadOrCreateSolidMaterial(GrassMaterialPath, new Color(0.20f, 0.43f, 0.17f, 1f), 0.18f),
                Flowers = LoadOrCreateSolidMaterial(FlowerMaterialPath, new Color(0.84f, 0.70f, 0.34f, 1f), 0.22f),
                Reeds = LoadOrCreateSolidMaterial(ReedMaterialPath, new Color(0.34f, 0.46f, 0.18f, 1f), 0.16f),
                Water = LoadOrCreateSolidMaterial(WaterMaterialPath, new Color(0.06f, 0.28f, 0.39f, 0.48f), 0.72f),
                Hills = LoadOrCreateSolidMaterial(HillMaterialPath, new Color(0.25f, 0.37f, 0.22f, 1f), 0.26f),
                Mountains = LoadOrCreateSolidMaterial(MountainMaterialPath, new Color(0.34f, 0.37f, 0.39f, 1f), 0.42f),
                Cloud = LoadOrCreateUnlitMaterial(CloudMaterialPath, new Color(0.92f, 0.95f, 0.96f, 0.76f))
            };
            ConfigureTransparent(materials.Water);
            ConfigureTransparent(materials.Cloud);
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

        private static void AddWind(GameObject target, float amplitude, float speed, float gustStrength)
        {
            WindAnimatedFoliage wind = target.AddComponent<WindAnimatedFoliage>();
            SerializedObject windObject = new SerializedObject(wind);
            windObject.FindProperty("amplitude").floatValue = amplitude;
            windObject.FindProperty("speed").floatValue = speed;
            windObject.FindProperty("gustStrength").floatValue = gustStrength;
            windObject.ApplyModifiedPropertiesWithoutUndo();
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
        }
    }
}
