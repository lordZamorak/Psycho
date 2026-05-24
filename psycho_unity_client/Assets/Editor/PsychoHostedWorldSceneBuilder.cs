using System;
using System.Collections.Generic;
using System.IO;
using Psycho.Cache;
using Psycho.Gameplay;
using Psycho.Mirror;
using Psycho.Networking;
using Psycho.Rendering;
using Psycho.UI;
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
        private const int RegionRadius = 4;
        private const int MaxObjectsPerRegion = 540;
        private const int MaxObjectsTotal = 28000;
        private const int MaxModelsPerObject = 4;
        private const int MaxModelsPerNpc = 12;
        private const int MaxNpcSpawns = 460;
        private const int GroundDetailCount = 1850;
        private const int GroundCoverPatchesPerAnchor = 24;
        private const int MaxWindAnimatedComponents = 1200;
        private const float DogSizedImpHeight = 0.62f;
        private const float RegularRatHeight = 0.18f;
        private const float GiantRatHeight = 0.36f;
        private const float ChickenHeight = 0.38f;
        private const float DuckHeight = 0.28f;
        private const float DogHeight = 0.58f;
        private const float PigHeight = 0.46f;
        private const float PigletHeight = 0.28f;
        private const float SheepHeight = 0.58f;
        private const float CowHeight = 0.95f;
        private const float CamelHeight = 1.25f;
        private const float YakHeight = 1.05f;
        private const float CharacterNormalSmoothingTolerance = 0.00075f;
        private const float TileScale = 0.72f;
        private const float HeightScale = 1f / 96f;
        private const string ScenePath = "Assets/Scenes/PsychoHostedTestWorld.unity";
        private const string GeneratedRoot = "Assets/Generated/Hosted";
        private const string ReportPath = GeneratedRoot + "/hosted_test_world_report.json";
        private const string WindowsBuildPath = "Builds/PsychoHostedTestWorld/Psycho.exe";
        private const string HostedPlayerUsername = "Sirenicbeast";
        private const int EquipmentHeadSlot = 0;
        private const int EquipmentCapeSlot = 1;
        private const int EquipmentAmuletSlot = 2;
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
        private const string HorizonMistMaterialPath = GeneratedRoot + "/Psycho_Hosted_Horizon_Mist.mat";
        private const string TreeCanopyMaterialPath = GeneratedRoot + "/Psycho_Hosted_Tree_Canopy.mat";
        private const string TreeBarkMaterialPath = GeneratedRoot + "/Psycho_Hosted_Tree_Bark.mat";
        private const string FrostStoneMaterialPath = GeneratedRoot + "/Psycho_Hosted_Frost_Stone.mat";
        private const string MossMaterialPath = GeneratedRoot + "/Psycho_Hosted_Moss.mat";
        private const string CliffFaceMaterialPath = GeneratedRoot + "/Psycho_Hosted_Cliff_Face.mat";
        private const string WaterFoamMaterialPath = GeneratedRoot + "/Psycho_Hosted_Water_Foam.mat";
        private const string WaterDepthMaterialPath = GeneratedRoot + "/Psycho_Hosted_Water_Depth.mat";
        private const string PlayerClothMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Cloth.mat";
        private const string PlayerLeatherMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Leather.mat";
        private const string PlayerMetalMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Metal.mat";
        private const string PlayerSkinMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Skin.mat";
        private const string PlayerHairMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Hair.mat";
        private const string PlayerPaperMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Paper.mat";
        private const string PlayerWoodMaterialPath = GeneratedRoot + "/Psycho_Hosted_Player_Wood.mat";
        private const string LandmarkStoneMaterialPath = GeneratedRoot + "/Psycho_Hosted_Landmark_Stone.mat";
        private const string LandmarkRoofMaterialPath = GeneratedRoot + "/Psycho_Hosted_Landmark_Roof.mat";
        private const string LandmarkRoadMaterialPath = GeneratedRoot + "/Psycho_Hosted_Landmark_Road.mat";
        private const string LandmarkBannerMaterialPath = GeneratedRoot + "/Psycho_Hosted_Landmark_Banner.mat";
        private const string LandmarkGlassMaterialPath = GeneratedRoot + "/Psycho_Hosted_Landmark_Glass.mat";
        private const string FallbackMaterialPath = GeneratedRoot + "/Psycho_Hosted_Fallback.mat";
        private const bool EnableHostedJCacheScenePlacement = false;
        private static readonly bool EnableHostedRewardModelShowcases = false;
        private static int windAnimatedComponentsAdded;
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
            windAnimatedComponentsAdded = 0;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PsychoMirrorDatabase database = PsychoMirrorDatabase.LoadFromStreamingAssets();
            Material vertexColorMaterial = PsychoCacheMeshImporter.LoadOrCreateVertexColorMaterial();
            Material terrainVertexColorMaterial = PsychoCacheMeshImporter.LoadOrCreateTerrainVertexColorMaterial();
            Material npcVertexColorMaterial = PsychoCacheMeshImporter.LoadOrCreateNpcVertexColorMaterial();
            HostedMaterials hostedMaterials = LoadOrCreateHostedMaterials();
            HostedBuildContext context = new HostedBuildContext(database);

            using (PsychoCacheStore store = new PsychoCacheStore(PsychoCacheStore.DefaultClientCachePath))
            {
                BuildRegions(store, database, terrainVertexColorMaterial, vertexColorMaterial, hostedMaterials, context);
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
            Debug.Log($"Hosted test world built: {ScenePath}. Regions {context.Report.loadedRegions}, objects {context.Report.placedObjects}, NPCs {context.Report.npcSpawns}, cache NPC visuals {context.Report.cacheNpcVisuals}, visual NPC replacements {context.Report.visualReplacementNpcs}, visual object replacements {context.Report.visualReplacementObjects}, decoded object accents {context.Report.decodedObjectAccents}, smoothed character meshes {context.Report.smoothedCharacterMeshes}, landmarks {context.Report.landmarkDressingObjects}, cliffs {context.Report.cliffDressingObjects}, ground cover {context.Report.groundCoverPatches}, foliage silhouettes {context.Report.enhancedFoliageObjects}, foam edges {context.Report.waterFoamEdges}.");
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

            Vector3 focus = new Vector3(12f, 0f, 8f);
            camera.transform.position = focus + new Vector3(112f, 76f, -126f);
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 44f;
            camera.farClipPlane = 2400f;
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

        [MenuItem("Psycho/Render Hosted Player Character Preview")]
        public static void RenderHostedPlayerCharacterPreview()
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

            Vector3 focus = player.transform.position + new Vector3(0f, 1.06f, 0f);
            Vector3 viewOffset = new Vector3(0.78f, 0.40f, -2.25f);
            camera.transform.position = focus + viewOffset;
            camera.transform.rotation = Quaternion.LookRotation(focus + new Vector3(0f, 0.18f, 0f) - camera.transform.position, Vector3.up);
            camera.fieldOfView = 32f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-player-character-preview.png"));
            RenderCameraToPng(camera, outputPath, 1200, 900);
        }

        public static void RenderHostedPlayerCharacterPreviewBatch()
        {
            RenderHostedPlayerCharacterPreview();
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

        [MenuItem("Psycho/Render Hosted Imp Scale Preview")]
        public static void RenderHostedImpScalePreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject imp = FindHostedNpcByNameFragment(" - Imp (");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (imp == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both an Imp NPC and camera.");
            }

            Bounds bounds = BuildObjectPreviewBounds(imp.transform);
            Vector3 focus = bounds.center + Vector3.up * 0.10f;
            camera.transform.position = focus + new Vector3(-2.25f, 1.05f, -2.55f);
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 31f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-imp-scale-preview.png"));
            RenderCameraToPng(camera, outputPath, 1400, 900);
        }

        public static void RenderHostedImpScalePreviewBatch()
        {
            RenderHostedImpScalePreview();
        }

        [MenuItem("Psycho/Render Hosted Rat Scale Preview")]
        public static void RenderHostedRatScalePreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject rat = FindHostedNpcByNameFragment(" - Rat (");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (rat == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both a Rat NPC and camera.");
            }

            Bounds bounds = BuildObjectPreviewBounds(rat.transform);
            Vector3 focus = bounds.center + Vector3.up * 0.04f;
            camera.transform.position = focus + new Vector3(-0.95f, 0.44f, -1.08f);
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 27f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-rat-scale-preview.png"));
            RenderCameraToPng(camera, outputPath, 1200, 900);
        }

        public static void RenderHostedRatScalePreviewBatch()
        {
            RenderHostedRatScalePreview();
        }

        [MenuItem("Psycho/Render Hosted Grand Exchange Preview")]
        public static void RenderHostedGrandExchangePreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject grandExchange = GameObject.Find("Grand Exchange Landmark");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (grandExchange == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both the Grand Exchange landmark and camera.");
            }

            Vector3 focus = grandExchange.transform.position + new Vector3(0f, 0.78f, -5.25f);
            Vector3 viewOffset = new Vector3(6.0f, 3.05f, -6.8f);
            camera.transform.position = focus + viewOffset;
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 34f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-ge-showcase-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedGrandExchangePreviewBatch()
        {
            RenderHostedGrandExchangePreview();
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

        private static GameObject FindHostedNpcByNameFragment(string fragment)
        {
            GameObject npcRoot = GameObject.Find("Hosted NPC Spawns");
            if (npcRoot == null)
            {
                return null;
            }

            for (int i = 0; i < npcRoot.transform.childCount; i++)
            {
                GameObject child = npcRoot.transform.GetChild(i).gameObject;
                if (child.name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return child;
                }
            }

            return null;
        }

        private static Bounds BuildObjectPreviewBounds(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds(root.position, Vector3.one * 0.8f);
            bool hasRenderer = false;
            foreach (Renderer renderer in renderers)
            {
                if (!hasRenderer)
                {
                    bounds = renderer.bounds;
                    hasRenderer = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            bounds.Expand(0.22f);
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

        private static void BuildRegions(PsychoCacheStore store, PsychoMirrorDatabase database, Material terrainMaterial, Material objectMaterial, HostedMaterials hostedMaterials, HostedBuildContext context)
        {
            GameObject terrainRoot = new GameObject("Hosted Terrain Regions");
            GameObject objectRoot = new GameObject("Hosted Cache Objects");

            foreach (Vector2Int region in BuildHostedRegionTraversal())
            {
                int regionX = region.x;
                int regionY = region.y;
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

                BuildTerrain(terrainRoot.transform, landscape, terrainMaterial);
                BuildObjects(store, database, objectRoot.transform, landscape, objects, objectMaterial, hostedMaterials, context);
            }
        }

        private static List<Vector2Int> BuildHostedRegionTraversal()
        {
            List<Vector2Int> regions = new List<Vector2Int>((RegionRadius * 2 + 1) * (RegionRadius * 2 + 1));
            for (int regionX = BaseRegionX - RegionRadius; regionX <= BaseRegionX + RegionRadius; regionX++)
            {
                for (int regionY = BaseRegionY - RegionRadius; regionY <= BaseRegionY + RegionRadius; regionY++)
                {
                    regions.Add(new Vector2Int(regionX, regionY));
                }
            }

            regions.Sort((left, right) =>
            {
                int leftDistance = (left.x - BaseRegionX) * (left.x - BaseRegionX) + (left.y - BaseRegionY) * (left.y - BaseRegionY);
                int rightDistance = (right.x - BaseRegionX) * (right.x - BaseRegionX) + (right.y - BaseRegionY) * (right.y - BaseRegionY);
                int distanceCompare = leftDistance.CompareTo(rightDistance);
                if (distanceCompare != 0)
                {
                    return distanceCompare;
                }

                int xCompare = left.x.CompareTo(right.x);
                return xCompare != 0 ? xCompare : left.y.CompareTo(right.y);
            });

            return regions;
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
                bool usedArtReplacement = false;
                if (hasDefinition && PsychoArtAssetResolver.TryInstantiateObject(definition, placed.transform, out GameObject artObject))
                {
                    artObject.name = $"Art Replacement - {definition.name}";
                    context.Report.visualReplacementObjects++;
                    usedArtReplacement = true;
                    addedMeshes++;
                }

                if (addedMeshes == 0 && hasDefinition && definition.modelIds != null)
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

                if (addedMeshes == 0 && PsychoHostedVisualOverrides.TryCreateMissingObjectReplacement(
                    new PsychoHostedVisualOverrides.PsychoMapObjectPlacementAdapter(placement.ObjectId),
                    material,
                    out GameObject replacementObject,
                    out _))
                {
                    replacementObject.transform.SetParent(placed.transform, false);
                    context.Report.visualReplacementObjects++;
                    addedMeshes++;
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
                else if (!usedArtReplacement && windResponsive && ShouldAddFoliageSilhouette(placed.transform, definition))
                {
                    AddFoliageSilhouette(placed.transform, definition, placement, hostedMaterials);
                    context.Report.enhancedFoliageObjects++;
                    addedWind = true;
                }

                if (!usedArtReplacement)
                {
                    context.Report.decodedObjectAccents += PsychoHostedVisualOverrides.AddObjectAccents(definition, placed.transform, material);
                    context.Report.decodedObjectAccents += AddArchitecturalDressing(definition, placed.transform, hostedMaterials);
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

        private static int AddArchitecturalDressing(PsychoMirrorObject definition, Transform parent, HostedMaterials materials)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.name))
            {
                return 0;
            }

            string name = definition.name.ToLowerInvariant();
            bool market = name.Contains("stall") || name.Contains("booth") || name.Contains("counter");
            bool building = name.Contains("bank") || name.Contains("shop") || name.Contains("house") || name.Contains("building") || name.Contains("hall");
            if (!market && !building)
            {
                return 0;
            }

            float width = Mathf.Clamp(definition.sizeX <= 0 ? (market ? 1.4f : 2.6f) : definition.sizeX, market ? 1.2f : 1.8f, market ? 2.8f : 5.0f);
            float depth = Mathf.Clamp(definition.sizeY <= 0 ? (market ? 1.1f : 2.2f) : definition.sizeY, market ? 0.9f : 1.6f, market ? 2.4f : 4.5f);

            GameObject dressing = new GameObject(building ? "Hosted Architectural Dressing" : "Hosted Market Dressing");
            dressing.transform.SetParent(parent, false);

            if (market)
            {
                CreateLandmarkDetailBox(dressing.transform, "Counter Worn Front Rail", new Vector3(0f, 0.64f, -depth * 0.43f), new Vector3(width * 0.96f, 0.075f, 0.045f), materials.TreeBark);
                CreateLandmarkDetailBox(dressing.transform, "Counter Stone Kick Plate", new Vector3(0f, 0.22f, -depth * 0.44f), new Vector3(width * 0.90f, 0.070f, 0.035f), materials.FrostStone);
                CreateLandmarkDetailBox(dressing.transform, "Counter Back Ledge", new Vector3(0f, 0.80f, depth * 0.34f), new Vector3(width * 0.84f, 0.060f, 0.050f), materials.TreeBark);
                CreateLandmarkDetailBox(dressing.transform, "Counter Cloth Valance", new Vector3(0f, 0.53f, -depth * 0.47f), new Vector3(width * 0.66f, 0.18f, 0.026f), materials.LandmarkBanner);
                return 4;
            }

            CreateLandmarkDetailBox(dressing.transform, "Facade Foundation Course", new Vector3(0f, 0.18f, -depth * 0.49f), new Vector3(width * 0.96f, 0.100f, 0.050f), materials.FrostStone);
            CreateLandmarkDetailBox(dressing.transform, "Facade Upper Timber Beam", new Vector3(0f, 1.44f, -depth * 0.50f), new Vector3(width * 0.94f, 0.080f, 0.045f), materials.TreeBark);
            CreateLandmarkDetailBox(dressing.transform, "Facade Left Upright", new Vector3(-width * 0.42f, 0.92f, -depth * 0.505f), new Vector3(0.060f, 0.96f, 0.045f), materials.TreeBark);
            CreateLandmarkDetailBox(dressing.transform, "Facade Right Upright", new Vector3(width * 0.42f, 0.92f, -depth * 0.505f), new Vector3(0.060f, 0.96f, 0.045f), materials.TreeBark);
            CreateLandmarkDetailBox(dressing.transform, "Facade Door Shadow", new Vector3(0f, 0.68f, -depth * 0.515f), new Vector3(width * 0.22f, 0.88f, 0.030f), materials.TreeBark);

            for (int i = -1; i <= 1; i += 2)
            {
                float x = i * width * 0.27f;
                CreateLandmarkDetailBox(dressing.transform, $"Facade Window Glass {i}", new Vector3(x, 1.02f, -depth * 0.520f), new Vector3(width * 0.16f, 0.34f, 0.024f), materials.LandmarkGlass);
                CreateLandmarkDetailBox(dressing.transform, $"Facade Window Sill {i}", new Vector3(x, 0.80f, -depth * 0.525f), new Vector3(width * 0.19f, 0.045f, 0.038f), materials.FrostStone);
                GameObject brace = CreateLandmarkDetailBox(dressing.transform, $"Facade Diagonal Brace {i}", new Vector3(x * 0.56f, 1.03f, -depth * 0.530f), new Vector3(0.050f, 0.78f, 0.032f), materials.TreeBark);
                brace.transform.localRotation = Quaternion.Euler(0f, 0f, i * 24f);
            }

            return 11;
        }

        private static void AddFoliageSilhouette(Transform parent, PsychoMirrorObject definition, PsychoMapObjectPlacement placement, HostedMaterials materials)
        {
            float footprint = Mathf.Max(1f, Mathf.Max(definition.sizeX, definition.sizeY));
            float trunkHeight = 1.55f + footprint * 0.30f;
            float canopyWidth = 1.58f + footprint * 0.42f;
            float canopyHeight = 1.18f + footprint * 0.20f;
            float seed = placement.ObjectId * 0.071f + placement.LocalX * 0.19f + placement.LocalY * 0.13f;
            string name = definition.name == null ? string.Empty : definition.name.ToLowerInvariant();
            bool broadleaf = name.Contains("oak") || name.Contains("willow") || name.Contains("maple") || name.Contains("dead");

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Detailed Bark Trunk Silhouette";
            trunk.transform.SetParent(parent, false);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight * 0.46f, 0f);
            trunk.transform.localRotation = Quaternion.Euler(0f, seed * 73f, Mathf.Sin(seed) * 2.5f);
            trunk.transform.localScale = new Vector3(0.16f + footprint * 0.030f, trunkHeight * 0.46f, 0.16f + footprint * 0.030f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = materials.TreeBark;
            RemoveCollider(trunk);

            for (int i = 0; i < 4; i++)
            {
                float angle = seed + i * Mathf.PI * 0.5f;
                Vector3 start = new Vector3(0f, trunkHeight * (0.46f + i * 0.10f), 0f);
                Vector3 end = start + new Vector3(Mathf.Cos(angle), 0.18f + i * 0.03f, Mathf.Sin(angle)) * (0.42f + footprint * 0.07f);
                CreateFoliageBranch(parent, $"Wind Branch Silhouette {i + 1}", start, end, 0.040f + footprint * 0.006f, materials.TreeBark);
            }

            if (broadleaf)
            {
                for (int i = 0; i < 7; i++)
                {
                    float angle = (i * Mathf.PI * 2f / 7f) + seed;
                    float radius = i == 0 ? 0f : 0.22f + Deterministic01(placement.ObjectId * 97 + i * 23) * 0.38f;
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, trunkHeight + (i % 3) * 0.13f, Mathf.Sin(angle) * radius);
                    Vector3 scale = new Vector3(
                        canopyWidth * (0.52f + Deterministic01(placement.ObjectId * 43 + i * 7) * 0.28f),
                        canopyHeight * (0.42f + i * 0.020f),
                        canopyWidth * (0.46f + Deterministic01(placement.ObjectId * 61 + i * 11) * 0.22f));
                    GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    canopy.name = "Wind Broadleaf Canopy Silhouette";
                    canopy.transform.SetParent(parent, false);
                    canopy.transform.localPosition = offset;
                    canopy.transform.localRotation = Quaternion.Euler(0f, i * 47f + seed * 29f, 0f);
                    canopy.transform.localScale = scale;
                    MeshRenderer renderer = canopy.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = materials.TreeCanopy;
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                    RemoveCollider(canopy);
                    AddWind(canopy, 0.052f + i * 0.005f, 0.86f + i * 0.08f, 0.30f, 0.82f, 0.055f);
                }

                return;
            }

            int layerCount = 6;
            for (int i = 0; i < layerCount; i++)
            {
                float t = i / Mathf.Max(1f, layerCount - 1f);
                float layerRadius = canopyWidth * Mathf.Lerp(0.54f, 0.14f, t);
                float layerHeight = canopyHeight * Mathf.Lerp(0.32f, 0.20f, t);
                GameObject canopy = new GameObject($"Wind Conifer Bough Silhouette {i + 1}");
                canopy.transform.SetParent(parent, false);
                canopy.transform.localPosition = new Vector3(
                    (Deterministic01(placement.ObjectId * 41 + i * 7) - 0.5f) * 0.12f,
                    trunkHeight * 0.66f + t * canopyHeight * 1.05f,
                    (Deterministic01(placement.ObjectId * 53 + i * 11) - 0.5f) * 0.12f);
                canopy.transform.localRotation = Quaternion.Euler(0f, i * 31f + seed * 19f, 0f);
                canopy.AddComponent<MeshFilter>().sharedMesh = CreateTaperedConeMesh($"Tree Object Bough Mesh {placement.ObjectId}_{i}", 14, layerRadius, layerRadius * 0.10f, layerHeight, placement.ObjectId + i * 61);
                MeshRenderer renderer = canopy.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = materials.TreeCanopy;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                AddWind(canopy, 0.040f + i * 0.007f, 0.82f + i * 0.10f, 0.30f, 0.82f, 0.052f);

                for (int arm = 0; arm < 3; arm++)
                {
                    float branchAngle = seed + i * 0.67f + arm * Mathf.PI * 2f / 3f;
                    Vector3 sprayOffset = new Vector3(Mathf.Cos(branchAngle) * layerRadius * 0.34f, -layerHeight * 0.10f, Mathf.Sin(branchAngle) * layerRadius * 0.34f);
                    CreateFoliageSpray(
                        parent,
                        $"Wind Conifer Needle Spray {i + 1}.{arm + 1}",
                        canopy.transform.localPosition + sprayOffset,
                        new Vector3(layerRadius * 0.54f, layerHeight * 0.115f, layerRadius * 0.20f),
                        branchAngle * Mathf.Rad2Deg,
                        materials.TreeCanopy,
                        0.030f + i * 0.006f,
                        0.78f + i * 0.09f);
                }
            }
        }

        private static void CreateFoliageSpray(Transform parent, string name, Vector3 localPosition, Vector3 localScale, float yaw, Material material, float windAmplitude, float windSpeed)
        {
            GameObject spray = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spray.name = name;
            spray.transform.SetParent(parent, false);
            spray.transform.localPosition = localPosition;
            spray.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            spray.transform.localScale = localScale;
            MeshRenderer renderer = spray.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            RemoveCollider(spray);
            AddWind(spray, windAmplitude, windSpeed, 0.26f, 0.82f, 0.038f);
        }

        private static void CreateFoliageBranch(Transform parent, string name, Vector3 localStart, Vector3 localEnd, float radius, Material material)
        {
            Vector3 direction = localEnd - localStart;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch.name = name;
            branch.transform.SetParent(parent, false);
            branch.transform.localPosition = (localStart + localEnd) * 0.5f;
            branch.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            branch.transform.localScale = new Vector3(radius, direction.magnitude * 0.5f, radius);
            branch.GetComponent<MeshRenderer>().sharedMaterial = material;
            RemoveCollider(branch);
            AddWind(branch, 0.018f, 0.74f, 0.18f, 0.85f, 0.030f);
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

                bool cacheVisual = false;
                bool artVisual = false;
                bool proceduralVisual = false;
                GameObject npcObject;
                if (PsychoArtAssetResolver.TryInstantiateNpc(npc, null, out npcObject))
                {
                    context.Report.visualReplacementNpcs++;
                    artVisual = true;
                }
                else if (ShouldUseProceduralHumanoidNpc(npc))
                {
                    npcObject = factory.CreateNpcVisual(npc);
                    context.Report.visualReplacementNpcs++;
                    proceduralVisual = true;
                }
                else if (PsychoHostedVisualOverrides.TryCreateNpcReplacement(npc, npcMaterial, out npcObject, out _))
                {
                    context.Report.visualReplacementNpcs++;
                    artVisual = true;
                }
                else
                {
                    cacheVisual = TryCreateCacheNpcVisual(store, database, npc, npcMaterial, out npcObject, context);
                }

                if (!cacheVisual && npcObject == null)
                {
                    npcObject = factory.CreateNpcVisual(npc);
                    context.Report.fallbackNpcVisuals++;
                    proceduralVisual = true;
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
                SetSerializedFloat(wanderObject, "turnSpeed", artVisual ? 5.25f : 4.75f);
                SetSerializedFloat(wanderObject, "acceleration", proceduralVisual ? 3.0f : 3.35f);
                SetSerializedFloat(wanderObject, "visualStrideBob", artVisual ? 0.0028f : cacheVisual ? 0.0036f : 0.0022f);
                SetSerializedFloat(wanderObject, "visualStrideSway", artVisual ? 0.34f : cacheVisual ? 0.46f : 0.28f);
                wanderObject.ApplyModifiedPropertiesWithoutUndo();
                context.Report.npcSpawns++;
            }
        }

        private static bool ShouldUseProceduralHumanoidNpc(PsychoMirrorNpc npc)
        {
            if (npc == null)
            {
                return false;
            }

            if (ShouldUseDogSizedImpScale(npc))
            {
                return false;
            }

            string visualClass = npc.visualClass == null ? string.Empty : npc.visualClass.ToLowerInvariant();
            if (visualClass == "citizen" || visualClass == "merchant" || visualClass == "banker" || visualClass == "guard")
            {
                return true;
            }

            string name = npc.name == null ? string.Empty : npc.name.ToLowerInvariant();
            return name.Contains("man")
                || name.Contains("woman")
                || name.Contains("guard")
                || name.Contains("warrior")
                || name.Contains("knight")
                || name.Contains("archer")
                || name.Contains("mage")
                || name.Contains("wizard")
                || name.Contains("monk")
                || name.Contains("banker")
                || name.Contains("shop")
                || name.Contains("merchant");
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

            context.Report.smoothedCharacterMeshes += ApplySoftCharacterNormals(modelRoot.transform);
            NormalizeNpcModel(modelRoot.transform, npc, model);
            context.Report.cacheNpcVisuals++;
            npcObject = root;
            return true;
        }

        private static int ApplySoftCharacterNormals(Transform root)
        {
            int smoothedMeshes = 0;
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter filter in filters)
            {
                if (TryCreateSoftNormalMesh(filter.sharedMesh, out Mesh smoothedMesh))
                {
                    filter.sharedMesh = smoothedMesh;
                    smoothedMeshes++;
                }
            }

            return smoothedMeshes;
        }

        private static bool TryCreateSoftNormalMesh(Mesh source, out Mesh smoothedMesh)
        {
            smoothedMesh = null;
            if (source == null || source.vertexCount < 4)
            {
                return false;
            }

            Vector3[] vertices = source.vertices;
            int[] triangles = source.triangles;
            if (vertices == null || triangles == null || vertices.Length == 0 || triangles.Length < 3)
            {
                return false;
            }

            Dictionary<VertexNormalKey, Vector3> normalSums = new Dictionary<VertexNormalKey, Vector3>(vertices.Length);
            for (int index = 0; index + 2 < triangles.Length; index += 3)
            {
                int a = triangles[index];
                int b = triangles[index + 1];
                int c = triangles[index + 2];
                if (a < 0 || b < 0 || c < 0 || a >= vertices.Length || b >= vertices.Length || c >= vertices.Length)
                {
                    continue;
                }

                Vector3 normal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                if (normal.sqrMagnitude < 0.0000001f)
                {
                    continue;
                }

                normal.Normalize();
                AddNormal(normalSums, vertices[a], normal);
                AddNormal(normalSums, vertices[b], normal);
                AddNormal(normalSums, vertices[c], normal);
            }

            Vector3[] smoothedNormals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                VertexNormalKey key = new VertexNormalKey(vertices[i], CharacterNormalSmoothingTolerance);
                smoothedNormals[i] = normalSums.TryGetValue(key, out Vector3 normal) && normal.sqrMagnitude > 0.0000001f
                    ? normal.normalized
                    : Vector3.up;
            }

            smoothedMesh = UnityEngine.Object.Instantiate(source);
            smoothedMesh.name = source.name + "_soft_character_normals";
            smoothedMesh.normals = smoothedNormals;
            Vector2[] uv = smoothedMesh.uv;
            if (uv != null && uv.Length == vertices.Length)
            {
                smoothedMesh.RecalculateTangents();
            }

            smoothedMesh.RecalculateBounds();
            return true;
        }

        private static void AddNormal(Dictionary<VertexNormalKey, Vector3> normalSums, Vector3 vertex, Vector3 normal)
        {
            VertexNormalKey key = new VertexNormalKey(vertex, CharacterNormalSmoothingTolerance);
            normalSums.TryGetValue(key, out Vector3 current);
            normalSums[key] = current + normal;
        }

        private static void NormalizeNpcModel(Transform modelRoot, PsychoMirrorNpc npc, PsychoMirrorNpcModel model)
        {
            if (!TryCalculateLocalBounds(modelRoot, out Bounds bounds) || bounds.size.y <= 0.001f)
            {
                return;
            }

            float declaredSize = Mathf.Max(1f, Mathf.Max(npc.size, model.size));
            float targetHeight;
            if (TryGetSmallAnimalNpcHeight(npc, out float smallAnimalHeight))
            {
                targetHeight = smallAnimalHeight;
            }
            else if (ShouldUseDogSizedImpScale(npc))
            {
                targetHeight = DogSizedImpHeight;
            }
            else
            {
                targetHeight = Mathf.Clamp(1.72f + (declaredSize - 1f) * 0.58f, 1.35f, 5.8f);
            }

            float scale = targetHeight / bounds.size.y;
            modelRoot.localScale = Vector3.one * scale;
            modelRoot.localPosition = new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, -bounds.center.z * scale);
        }

        private static bool TryGetSmallAnimalNpcHeight(PsychoMirrorNpc npc, out float targetHeight)
        {
            targetHeight = 0f;
            if (npc == null || string.IsNullOrWhiteSpace(npc.name))
            {
                return false;
            }

            string name = npc.name.Trim().ToLowerInvariant();
            if (name.Contains("rat burgiss"))
            {
                return false;
            }

            if (HasNpcNameToken(name, "rat") || HasNpcNameToken(name, "rats"))
            {
                targetHeight = IsLargerRatVariant(name) ? GiantRatHeight : RegularRatHeight;
                return true;
            }

            if (HasNpcNameToken(name, "duckling"))
            {
                targetHeight = 0.20f;
                return true;
            }

            if (HasNpcNameToken(name, "duck"))
            {
                targetHeight = DuckHeight;
                return true;
            }

            if (HasNpcNameToken(name, "chicken") || HasNpcNameToken(name, "rooster") || HasNpcNameToken(name, "hen"))
            {
                targetHeight = ChickenHeight;
                return true;
            }

            if (HasNpcNameToken(name, "piglet"))
            {
                targetHeight = PigletHeight;
                return true;
            }

            if (HasNpcNameToken(name, "pig"))
            {
                targetHeight = PigHeight;
                return true;
            }

            if (HasNpcNameToken(name, "puppy"))
            {
                targetHeight = 0.34f;
                return true;
            }

            if (HasNpcNameToken(name, "dog") || HasNpcNameToken(name, "sheepdog"))
            {
                targetHeight = name.Contains("terror") ? 0.85f : DogHeight;
                return true;
            }

            if (HasNpcNameToken(name, "calf"))
            {
                targetHeight = 0.70f;
                return true;
            }

            if (HasNpcNameToken(name, "cow") || HasNpcNameToken(name, "bull"))
            {
                targetHeight = CowHeight;
                return true;
            }

            if (HasNpcNameToken(name, "sheep") || HasNpcNameToken(name, "lamb"))
            {
                targetHeight = SheepHeight;
                return true;
            }

            if (HasNpcNameToken(name, "camel"))
            {
                targetHeight = CamelHeight;
                return true;
            }

            if (HasNpcNameToken(name, "yak"))
            {
                targetHeight = YakHeight;
                return true;
            }

            return false;
        }

        private static bool IsLargerRatVariant(string name)
        {
            return HasNpcNameToken(name, "giant")
                || HasNpcNameToken(name, "dungeon")
                || HasNpcNameToken(name, "crypt")
                || HasNpcNameToken(name, "brine")
                || HasNpcNameToken(name, "zombie")
                || HasNpcNameToken(name, "albino")
                || HasNpcNameToken(name, "king")
                || HasNpcNameToken(name, "hell")
                || HasNpcNameToken(name, "warped")
                || HasNpcNameToken(name, "corrupted")
                || HasNpcNameToken(name, "crystalline")
                || HasNpcNameToken(name, "blessed")
                || HasNpcNameToken(name, "angry");
        }

        private static bool HasNpcNameToken(string name, string token)
        {
            int index = name.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                bool leftBoundary = index == 0 || !char.IsLetterOrDigit(name[index - 1]);
                int rightIndex = index + token.Length;
                bool rightBoundary = rightIndex >= name.Length || !char.IsLetterOrDigit(name[rightIndex]);
                if (leftBoundary && rightBoundary)
                {
                    return true;
                }

                index = name.IndexOf(token, index + 1, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool ShouldUseDogSizedImpScale(PsychoMirrorNpc npc)
        {
            if (npc == null || string.IsNullOrWhiteSpace(npc.name))
            {
                return false;
            }

            string name = npc.name.Trim().ToLowerInvariant();
            if (name.Contains("impling") || name.Contains("chimp") || name.Contains("snow imp"))
            {
                return false;
            }

            return name == "imp"
                || name == "imp champion"
                || name == "imp defender"
                || name == "booth imp"
                || name == "reanimated imp"
                || name == "revenant imp";
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
            BuildGroundCoverPatches(context, materials);
            BuildWaterways(context, materials);
            BuildWorldLandmarks(context, materials);
            BuildHighlandForestDressing(context, materials);
            BuildRockOutcropDressing(context, materials);
            BuildHighlandCliffDressing(context, materials);
            BuildDistantVista(context, materials);
            BuildHorizonMist(context, materials);
            BuildCloudLayer(materials.Cloud);
        }

        private static void BuildGrassField(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Wind Animated Ground Detail");
            int minX = (BaseRegionX - RegionRadius) * 64 + 4;
            int maxX = (BaseRegionX + RegionRadius + 1) * 64 - 4;
            int minY = (BaseRegionY - RegionRadius) * 64 + 4;
            int maxY = (BaseRegionY + RegionRadius + 1) * 64 - 4;

            for (int i = 0; i < GroundDetailCount; i++)
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
                CreateGrassBlade(root.transform, position, 0.18f + Deterministic01(i * 59 + 11) * 0.24f, material);
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

        private static void BuildGroundCoverPatches(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Blended Meadow Ground Cover Patches");
            Vector2Int[] anchors =
            {
                new Vector2Int(3088, 3492),
                new Vector2Int(3100, 3508),
                new Vector2Int(3164, 3486),
                new Vector2Int(3218, 3430),
                new Vector2Int(3094, 3248),
                new Vector2Int(2964, 3378),
                new Vector2Int(3031, 3218),
                new Vector2Int(3120, 3515),
                new Vector2Int(3188, 3456),
                new Vector2Int(3068, 3514)
            };

            int created = 0;
            for (int cluster = 0; cluster < anchors.Length; cluster++)
            {
                Vector2Int anchor = anchors[cluster];
                for (int i = 0; i < GroundCoverPatchesPerAnchor; i++)
                {
                    int seed = 4300 + cluster * 811 + i * 97;
                    float angle = Deterministic01(seed + 3) * Mathf.PI * 2f;
                    float radius = 2.0f + Mathf.Pow(Deterministic01(seed + 5), 0.72f) * 20.5f;
                    int worldX = anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                    int worldY = anchor.y + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                    if (!TryGetNaturalDressingPosition(context, worldX, worldY, seed, out Vector3 position))
                    {
                        continue;
                    }

                    if (!TryGetLandscapeTile(context, worldX, worldY, out PsychoMapLandscape landscape, out int localX, out int localY))
                    {
                        continue;
                    }

                    byte overlay = landscape.OverlayIds[0, localX, localY];
                    if (overlay != 0 && Deterministic01(seed + 7) < 0.78f)
                    {
                        continue;
                    }

                    float roll = Deterministic01(seed + 11);
                    Material material = roll > 0.76f ? materials.Flowers : roll > 0.42f ? materials.Moss : materials.Grass;
                    float width = 1.15f + Deterministic01(seed + 17) * 2.55f;
                    float depth = 0.72f + Deterministic01(seed + 23) * 1.90f;
                    GameObject patch = CreateGroundCoverPatch($"Meadow Ground Cover Patch {created + 1}", position + Vector3.up * 0.018f, width, depth, material, seed);
                    patch.transform.SetParent(root.transform, true);
                    created++;
                }
            }

            context.Report.groundCoverPatches += created;
        }

        private static void BuildWorldLandmarks(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("High Definition Psycho Landmark Dressing");
            CreateGrandExchangeLandmark(root.transform, context, materials);
            CreateBankLandmark(root.transform, context, materials, "Edgeville Bank Landmark", 3094, 3498, 6.8f, 4.2f, 0f);
            CreateBankLandmark(root.transform, context, materials, "Varrock West Bank Landmark", 3185, 3436, 8.4f, 5.6f, 90f);
            CreateBankLandmark(root.transform, context, materials, "Draynor Bank Landmark", 3092, 3245, 6.6f, 4.8f, 0f);
            CreateBankLandmark(root.transform, context, materials, "Falador Bank Landmark", 2946, 3368, 8.2f, 5.4f, 90f);
            CreateHarborLandmark(root.transform, context, materials, "Port Sarim Harbor Landmark", 3028, 3216);
            CreateTownClusterLandmark(root.transform, context, materials, "Varrock House Cluster", 3218, 3430, 5, 0f);
            CreateTownClusterLandmark(root.transform, context, materials, "Draynor House Cluster", 3099, 3250, 4, 90f);
            CreateTownClusterLandmark(root.transform, context, materials, "Falador House Cluster", 2965, 3377, 5, 0f);
        }

        private static void CreateGrandExchangeLandmark(Transform parent, HostedBuildContext context, HostedMaterials materials)
        {
            if (!TryCreateLandmarkRoot(parent, "Grand Exchange Landmark", context, 3165, 3487, out Transform root))
            {
                return;
            }

            CreateGroundPlate(root, "Grand Exchange Paved Plaza", Vector3.zero, 19.5f, 17.5f, materials.LandmarkRoad);
            CreateLandmarkPaverBands(root, "Grand Exchange Plaza", 19.5f, 17.5f, materials.FrostStone);
            CreateLandmarkCylinder(root, "Grand Exchange Center Dais", new Vector3(0f, 0.10f, 0f), new Vector3(4.6f, 0.10f, 4.6f), materials.LandmarkStone);
            CreateLandmarkCylinder(root, "Grand Exchange Inner Ring", new Vector3(0f, 0.24f, 0f), new Vector3(3.4f, 0.10f, 3.4f), materials.LandmarkRoad);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Vector3 columnPosition = new Vector3(Mathf.Cos(angle) * 4.9f, 1.05f, Mathf.Sin(angle) * 4.1f);
                CreateLandmarkCylinder(root, $"Grand Exchange Stone Column {i + 1}", columnPosition, new Vector3(0.26f, 1.05f, 0.26f), materials.LandmarkStone);
                CreateLandmarkBox(root, $"Grand Exchange Roof Beam {i + 1}", new Vector3(Mathf.Cos(angle) * 4.3f, 2.17f, Mathf.Sin(angle) * 3.6f), new Vector3(1.65f, 0.16f, 0.24f), materials.LandmarkRoof).transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
            }

            CreateLandmarkBox(root, "Grand Exchange Blue Canopy", new Vector3(0f, 2.38f, 0f), new Vector3(7.5f, 0.22f, 6.2f), materials.PlayerCloth);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                Vector3 boothPosition = new Vector3(Mathf.Cos(angle) * 6.1f, 0.62f, Mathf.Sin(angle) * 5.2f);
                GameObject booth = CreateLandmarkBox(root, $"Grand Exchange Trading Booth {i + 1}", boothPosition, new Vector3(1.8f, 1.05f, 0.72f), materials.TreeBark);
                booth.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                CreateLandmarkBox(root, $"Grand Exchange Booth Banner {i + 1}", boothPosition + Vector3.up * 0.78f, new Vector3(1.95f, 0.30f, 0.08f), materials.LandmarkBanner).transform.localRotation = booth.transform.localRotation;
            }

            int looseDisplays = 0;
            int cache1Displays = 0;
            int jcacheDisplays = 0;
            if (EnableHostedRewardModelShowcases)
            {
                Material decodedModelMaterial = PsychoCacheMeshImporter.LoadOrCreateVertexColorMaterial();
                looseDisplays = PsychoHostedVisualOverrides.AddLooseModelShowcase(
                    root,
                    decodedModelMaterial,
                    "Grand Exchange Loose Cache Display",
                    0,
                    116,
                    new Vector3(0f, 0.29f, -7.25f),
                    16,
                    0.72f,
                    0.68f,
                    0.54f,
                    0.50f);
                cache1Displays = PsychoHostedVisualOverrides.AddCache1ModelShowcase(
                    root,
                    decodedModelMaterial,
                    "Grand Exchange Cache1 Display",
                    0,
                    160,
                    new Vector3(0f, 0.28f, 2.25f),
                    16,
                    0.72f,
                    0.58f,
                    0.46f,
                    0.48f);
                jcacheDisplays = EnableHostedJCacheScenePlacement
                    ? PsychoHostedVisualOverrides.AddJCacheModelShowcase(
                        root,
                        decodedModelMaterial,
                        "Grand Exchange JS5 NXT Display",
                        0,
                        24,
                        new Vector3(-3.3f, 0.31f, -4.85f),
                        4,
                        1.45f,
                        1.05f,
                        0.96f,
                        1.05f)
                    : 0;
            }

            context.Report.landmarkDressingObjects += 24 + looseDisplays + cache1Displays + jcacheDisplays;
        }

        private static void CreateBankLandmark(Transform parent, HostedBuildContext context, HostedMaterials materials, string name, int worldX, int worldY, float width, float depth, float yaw)
        {
            if (!TryCreateLandmarkRoot(parent, name, context, worldX, worldY, out Transform root))
            {
                return;
            }

            root.localRotation = Quaternion.Euler(0f, yaw, 0f);
            CreateGroundPlate(root, "Bank Stone Approach", new Vector3(0f, 0.01f, -depth * 0.70f), width * 0.96f, depth * 0.46f, materials.LandmarkRoad);
            CreateLandmarkBox(root, "Bank Hall Walls", new Vector3(0f, 1.05f, 0f), new Vector3(width, 2.10f, depth), materials.LandmarkStone);
            CreateLandmarkBox(root, "Bank Foundation Plinth", new Vector3(0f, 0.20f, 0f), new Vector3(width * 1.04f, 0.22f, depth * 1.06f), materials.LandmarkRoad);
            CreateLandmarkStoneCourses(root, "Bank Stone Course", width, depth, 0.48f, 1.72f, materials.FrostStone);
            CreateLandmarkTimberBracing(root, "Bank Timber Bracing", width, depth, 1.10f, 1.42f, materials.TreeBark);
            CreateLandmarkBox(root, "Bank Upper Timber Fascia Front", new Vector3(0f, 1.86f, -depth * 0.53f), new Vector3(width * 1.06f, 0.18f, 0.12f), materials.TreeBark);
            CreateLandmarkBox(root, "Bank Upper Timber Fascia Back", new Vector3(0f, 1.86f, depth * 0.53f), new Vector3(width * 1.06f, 0.18f, 0.12f), materials.TreeBark);
            CreateLandmarkBox(root, "Bank Upper Timber Fascia Left", new Vector3(-width * 0.53f, 1.86f, 0f), new Vector3(0.12f, 0.18f, depth * 1.06f), materials.TreeBark);
            CreateLandmarkBox(root, "Bank Upper Timber Fascia Right", new Vector3(width * 0.53f, 1.86f, 0f), new Vector3(0.12f, 0.18f, depth * 1.06f), materials.TreeBark);
            CreateLandmarkGabledRoof(root, "Bank Dark Timber Gabled Roof", new Vector3(0f, 2.60f, 0f), width * 1.18f, depth * 1.22f, 0.92f, materials.LandmarkRoof);
            CreateLandmarkRoofShingles(root, "Bank Roof Shingles", width * 1.18f, depth * 1.22f, 2.34f, 0.92f, materials.TreeBark);
            CreateLandmarkChimney(root, "Bank Chimney", new Vector3(width * 0.28f, 2.98f, depth * 0.08f), materials.FrostStone, materials.TreeBark);
            CreateLandmarkBox(root, "Bank Roof Ridge Beam", new Vector3(0f, 3.10f, 0f), new Vector3(width * 1.04f, 0.13f, 0.18f), materials.TreeBark);
            CreateLandmarkBox(root, "Bank Front Shadow Eave", new Vector3(0f, 2.12f, -depth * 0.66f), new Vector3(width * 1.22f, 0.12f, 0.16f), materials.TreeBark);
            CreateLandmarkBox(root, "Bank Rear Shadow Eave", new Vector3(0f, 2.12f, depth * 0.66f), new Vector3(width * 1.22f, 0.12f, 0.16f), materials.TreeBark);
            CreateLandmarkBox(root, "Bank Counter", new Vector3(0f, 0.72f, -depth * 0.18f), new Vector3(width * 0.70f, 0.62f, 0.42f), materials.TreeBark);
            CreateLandmarkBox(root, "Bank Doorway", new Vector3(0f, 0.78f, -depth * 0.51f), new Vector3(1.25f, 1.45f, 0.16f), materials.LandmarkGlass);

            for (int i = 0; i < 4; i++)
            {
                float cornerX = i < 2 ? -width * 0.50f : width * 0.50f;
                float cornerZ = i % 2 == 0 ? -depth * 0.50f : depth * 0.50f;
                CreateLandmarkBox(root, $"Bank Stone Corner Quoin {i + 1}", new Vector3(cornerX, 1.04f, cornerZ), new Vector3(0.24f, 1.78f, 0.24f), materials.FrostStone);
            }

            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * width * 0.25f;
                CreateLandmarkBox(root, $"Bank Window {i + 1}", new Vector3(x, 1.38f, -depth * 0.515f), new Vector3(0.78f, 0.58f, 0.08f), materials.LandmarkGlass);
                CreateLandmarkBox(root, $"Bank Window {i + 1} Timber Lintel", new Vector3(x, 1.72f, -depth * 0.525f), new Vector3(0.94f, 0.09f, 0.10f), materials.TreeBark);
                CreateLandmarkBox(root, $"Bank Window {i + 1} Stone Sill", new Vector3(x, 1.02f, -depth * 0.525f), new Vector3(0.96f, 0.08f, 0.12f), materials.FrostStone);
            }

            context.Report.landmarkDressingObjects += 55;
        }

        private static void CreateHarborLandmark(Transform parent, HostedBuildContext context, HostedMaterials materials, string name, int worldX, int worldY)
        {
            if (!TryCreateLandmarkRoot(parent, name, context, worldX, worldY, out Transform root))
            {
                return;
            }

            CreateGroundPlate(root, "Harbor Packed Sand", new Vector3(0f, 0.01f, 0f), 14f, 10f, materials.LandmarkRoad);
            GameObject harborWater = CreateSubdividedPlane("Harbor Inlet Water", root.position + new Vector3(0f, 0.035f, -8.3f), 15.5f, 5.8f, 24, materials.Water);
            harborWater.transform.SetParent(root, true);
            harborWater.transform.localPosition = new Vector3(0f, 0.035f, -8.3f);
            harborWater.AddComponent<ProceduralWater>();
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * 2.2f;
                CreateLandmarkBox(root, $"Harbor Pier Span {i + 1}", new Vector3(x, 0.24f, -4.8f), new Vector3(1.75f, 0.22f, 7.4f), materials.TreeBark);
                CreateLandmarkCylinder(root, $"Harbor Mooring Post {i + 1}A", new Vector3(x - 0.76f, 0.72f, -7.6f), new Vector3(0.14f, 0.72f, 0.14f), materials.TreeBark);
                CreateLandmarkCylinder(root, $"Harbor Mooring Post {i + 1}B", new Vector3(x + 0.76f, 0.72f, -2.1f), new Vector3(0.14f, 0.72f, 0.14f), materials.TreeBark);
            }

            CreateLandmarkBox(root, "Harbor Ship Hull", new Vector3(5.2f, 0.62f, -7.2f), new Vector3(4.4f, 0.78f, 1.35f), materials.TreeBark);
            CreateLandmarkBox(root, "Harbor Ship Deck", new Vector3(5.2f, 1.14f, -7.2f), new Vector3(3.7f, 0.18f, 1.04f), materials.LandmarkRoad);
            CreateLandmarkBox(root, "Harbor Ship Sail", new Vector3(5.2f, 2.18f, -7.2f), new Vector3(0.12f, 1.9f, 1.45f), materials.PlayerPaper);
            CreateLandmarkCylinder(root, "Harbor Ship Mast", new Vector3(5.2f, 1.92f, -7.2f), new Vector3(0.08f, 1.65f, 0.08f), materials.TreeBark);
            context.Report.landmarkDressingObjects += 16;
        }

        private static void CreateTownClusterLandmark(Transform parent, HostedBuildContext context, HostedMaterials materials, string name, int worldX, int worldY, int houseCount, float yaw)
        {
            if (!TryCreateLandmarkRoot(parent, name, context, worldX, worldY, out Transform root))
            {
                return;
            }

            root.localRotation = Quaternion.Euler(0f, yaw, 0f);
            CreateGroundPlate(root, "Town Lane", new Vector3(0f, 0.01f, 0f), houseCount * 3.7f, 2.4f, materials.LandmarkRoad);
            for (int i = 0; i < houseCount; i++)
            {
                float x = (i - (houseCount - 1) * 0.5f) * 3.45f;
                float side = i % 2 == 0 ? 2.35f : -2.35f;
                CreateLandmarkBox(root, $"House {i + 1} Stone Walls", new Vector3(x, 0.86f, side), new Vector3(2.6f, 1.72f, 2.35f), materials.LandmarkStone);
                CreateLandmarkGabledRoof(root, $"House {i + 1} Gabled Roof", new Vector3(x, 2.02f, side), 3.10f, 2.78f, 0.74f, materials.LandmarkRoof);
                Transform houseRoot = root;
                CreateHouseDetailSet(houseRoot, $"House {i + 1}", new Vector3(x, 0f, side), 2.6f, 2.35f, materials);
                CreateLandmarkBox(root, $"House {i + 1} Ridge Beam", new Vector3(x, 2.43f, side), new Vector3(2.75f, 0.10f, 0.13f), materials.TreeBark);
                CreateLandmarkBox(root, $"House {i + 1} Foundation", new Vector3(x, 0.20f, side), new Vector3(2.76f, 0.22f, 2.50f), materials.LandmarkRoad);
                CreateLandmarkBox(root, $"House {i + 1} Door", new Vector3(x, 0.58f, side - Mathf.Sign(side) * 1.21f), new Vector3(0.58f, 1.05f, 0.12f), materials.TreeBark);
                CreateLandmarkBox(root, $"House {i + 1} Window", new Vector3(x - 0.72f, 1.12f, side - Mathf.Sign(side) * 1.205f), new Vector3(0.46f, 0.40f, 0.08f), materials.LandmarkGlass);
            }

            context.Report.landmarkDressingObjects += houseCount * 20 + 1;
        }

        private static void BuildHighlandForestDressing(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Rugged Highland Conifer Dressing");
            Vector2Int[] anchors =
            {
                new Vector2Int(3058, 3516),
                new Vector2Int(3084, 3528),
                new Vector2Int(3122, 3511),
                new Vector2Int(3152, 3502),
                new Vector2Int(3188, 3456),
                new Vector2Int(3214, 3428),
                new Vector2Int(3032, 3264),
                new Vector2Int(3090, 3248),
                new Vector2Int(2962, 3374)
            };

            int created = 0;
            for (int cluster = 0; cluster < anchors.Length; cluster++)
            {
                Vector2Int anchor = anchors[cluster];
                for (int i = 0; i < 26; i++)
                {
                    int seed = cluster * 997 + i * 131;
                    float angle = Deterministic01(seed + 11) * Mathf.PI * 2f;
                    float radius = 3.5f + Mathf.Pow(Deterministic01(seed + 23), 0.62f) * 17.5f;
                    int worldX = anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                    int worldY = anchor.y + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                    if (!TryGetNaturalDressingPosition(context, worldX, worldY, seed, out Vector3 position))
                    {
                        continue;
                    }

                    float height = 2.45f + Deterministic01(seed + 37) * 2.35f;
                    CreateHighlandConifer(root.transform, position, height, materials.TreeBark, materials.TreeCanopy, seed);
                    created++;
                }
            }

            context.Report.enhancedFoliageObjects += created;
            context.Report.windAnimatedObjects += created;
        }

        private static void BuildRockOutcropDressing(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Rugged Highland Rock Outcrop Dressing");
            Vector2Int[] anchors =
            {
                new Vector2Int(3076, 3510),
                new Vector2Int(3138, 3496),
                new Vector2Int(3176, 3470),
                new Vector2Int(3218, 3422),
                new Vector2Int(3014, 3242),
                new Vector2Int(2968, 3386),
                new Vector2Int(3095, 3256)
            };

            int created = 0;
            for (int cluster = 0; cluster < anchors.Length; cluster++)
            {
                Vector2Int anchor = anchors[cluster];
                for (int i = 0; i < 16; i++)
                {
                    int seed = 2900 + cluster * 733 + i * 113;
                    float angle = Deterministic01(seed + 3) * Mathf.PI * 2f;
                    float radius = 2.5f + Deterministic01(seed + 7) * 13.5f;
                    int worldX = anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                    int worldY = anchor.y + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                    if (!TryGetNaturalDressingPosition(context, worldX, worldY, seed, out Vector3 position))
                    {
                        continue;
                    }

                    CreateHighlandRockOutcrop(root.transform, position, 0.78f + Deterministic01(seed + 41) * 1.05f, materials.FrostStone, materials.Moss, seed);
                    created++;
                }
            }

            context.Report.landmarkDressingObjects += created;
        }

        private static void BuildHighlandCliffDressing(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("High Definition Highland Cliff Dressing");
            Vector2Int[] anchors =
            {
                new Vector2Int(3068, 3514),
                new Vector2Int(3117, 3509),
                new Vector2Int(3168, 3478),
                new Vector2Int(3212, 3434),
                new Vector2Int(3020, 3254),
                new Vector2Int(2960, 3378),
                new Vector2Int(3096, 3262)
            };

            int createdParts = 0;
            for (int cluster = 0; cluster < anchors.Length; cluster++)
            {
                Vector2Int anchor = anchors[cluster];
                for (int i = 0; i < 9; i++)
                {
                    int seed = 8100 + cluster * 947 + i * 151;
                    float angle = Deterministic01(seed + 3) * Mathf.PI * 2f;
                    float radius = 3.0f + Deterministic01(seed + 5) * 15.5f;
                    int worldX = anchor.x + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                    int worldY = anchor.y + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                    if (!TryGetNaturalDressingPosition(context, worldX, worldY, seed, out Vector3 position))
                    {
                        continue;
                    }

                    float width = 1.75f + Deterministic01(seed + 11) * 2.35f;
                    float height = 0.95f + Deterministic01(seed + 17) * 1.15f;
                    float depth = 0.72f + Deterministic01(seed + 23) * 0.54f;
                    float yaw = angle * Mathf.Rad2Deg + 90f + (Deterministic01(seed + 29) - 0.5f) * 38f;
                    createdParts += CreateLayeredDirtCliff(root.transform, position, yaw, width, height, depth, materials.CliffFace, materials.Moss, materials.FrostStone, seed);
                }
            }

            context.Report.cliffDressingObjects += createdParts;
        }

        private static int CreateLayeredDirtCliff(Transform parent, Vector3 position, float yaw, float width, float height, float depth, Material cliffMaterial, Material mossMaterial, Material stoneMaterial, int seed)
        {
            GameObject cliff = new GameObject("Layered Highland Dirt Cliff");
            cliff.isStatic = true;
            cliff.transform.SetParent(parent, false);
            cliff.transform.position = position;
            cliff.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            int created = 0;
            int ledges = 2 + Mathf.FloorToInt(Deterministic01(seed + 31) * 2f);
            float yCursor = 0f;
            for (int ledge = 0; ledge < ledges; ledge++)
            {
                float t = ledge / Mathf.Max(1f, ledges - 1f);
                float ledgeWidth = width * Mathf.Lerp(1.0f, 0.66f, t) * (0.92f + Deterministic01(seed + ledge * 41) * 0.14f);
                float ledgeHeight = height * Mathf.Lerp(0.48f, 0.30f, t);
                float ledgeDepth = depth * Mathf.Lerp(1.0f, 0.70f, t);
                Vector3 localPosition = new Vector3((Deterministic01(seed + ledge * 43) - 0.5f) * width * 0.12f, yCursor, -ledge * depth * 0.13f);

                Mesh mesh = CreateCliffFaceMesh($"Layered Dirt Cliff Mesh {seed}_{ledge}", ledgeWidth, ledgeHeight, ledgeDepth, seed + ledge * 59);
                GameObject face = new GameObject($"Stratified Cliff Face {ledge + 1}");
                face.isStatic = true;
                face.transform.SetParent(cliff.transform, false);
                face.transform.localPosition = localPosition;
                face.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer renderer = face.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = cliffMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;

                MeshCollider collider = face.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                created++;

                GameObject mossCap = CreateLandmarkDetailBox(
                    cliff.transform,
                    $"Mossy Cliff Shelf {ledge + 1}",
                    localPosition + new Vector3(0f, ledgeHeight + 0.025f, -ledgeDepth * 0.22f),
                    new Vector3(ledgeWidth * 0.82f, 0.035f, ledgeDepth * 0.38f),
                    mossMaterial);
                mossCap.transform.localRotation = Quaternion.Euler(0f, (Deterministic01(seed + ledge * 67) - 0.5f) * 7f, 0f);
                created++;

                yCursor += ledgeHeight * 0.74f;
            }

            int footRocks = 2 + Mathf.FloorToInt(Deterministic01(seed + 101) * 2f);
            for (int i = 0; i < footRocks; i++)
            {
                float x = (i - (footRocks - 1) * 0.5f) * width * 0.28f + (Deterministic01(seed + i * 19) - 0.5f) * width * 0.16f;
                float z = -depth * (0.46f + Deterministic01(seed + i * 23) * 0.18f);
                Vector3 rockPosition = cliff.transform.TransformPoint(new Vector3(x, 0.015f, z));
                CreateHighlandRockOutcrop(parent, rockPosition, width * (0.16f + Deterministic01(seed + i * 29) * 0.09f), stoneMaterial, mossMaterial, seed + i * 127);
                created++;
            }

            return created;
        }

        private static Mesh CreateCliffFaceMesh(string name, float width, float height, float depth, int seed)
        {
            const int horizontalSegments = 9;
            const int verticalSegments = 4;
            int stride = horizontalSegments + 1;
            Mesh mesh = new Mesh { name = name };
            Vector3[] vertices = new Vector3[(horizontalSegments + 1) * (verticalSegments + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            List<int> triangles = new List<int>(horizontalSegments * verticalSegments * 6);

            for (int y = 0; y <= verticalSegments; y++)
            {
                float v = (float)y / verticalSegments;
                float shelfInset = Mathf.Lerp(0f, depth * 0.28f, v);
                float rowWidth = width * Mathf.Lerp(1.0f, 0.78f, v);
                for (int x = 0; x <= horizontalSegments; x++)
                {
                    float u = (float)x / horizontalSegments;
                    float centeredX = (u - 0.5f) * rowWidth;
                    float fracture = Mathf.Sin((u * 6.7f + v * 2.1f + seed * 0.003f) * Mathf.PI) * depth * 0.045f;
                    float chip = (Deterministic01(seed + x * 37 + y * 83) - 0.5f) * depth * 0.095f;
                    float ledge = Mathf.Sin(v * Mathf.PI * 5f) * depth * 0.055f;
                    vertices[y * stride + x] = new Vector3(centeredX, v * height, -shelfInset + fracture + chip + ledge);
                    uv[y * stride + x] = new Vector2(u * Mathf.Max(1.0f, width * 0.72f), v * Mathf.Max(1.0f, height * 1.8f));
                }
            }

            for (int y = 0; y < verticalSegments; y++)
            {
                for (int x = 0; x < horizontalSegments; x++)
                {
                    int i = y * stride + x;
                    triangles.Add(i);
                    triangles.Add(i + stride);
                    triangles.Add(i + 1);
                    triangles.Add(i + 1);
                    triangles.Add(i + stride);
                    triangles.Add(i + stride + 1);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void BuildHorizonMist(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Cold Highland Horizon Mist");
            int panels = 0;
            for (int i = 0; i < 34; i++)
            {
                float angle = (i + 0.35f) * Mathf.PI * 2f / 34f;
                float radius = 205f + Mathf.Sin(i * 1.37f) * 26f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, -4.6f + (i % 4) * 0.25f, Mathf.Sin(angle) * radius);
                CreateVerticalMistPanel(root.transform, $"Horizon Mist Veil {i + 1}", position, 22f + (i % 5) * 5f, 4.4f + (i % 4) * 1.2f, materials.HorizonMist);
                panels++;
            }

            for (int i = 0; i < 14; i++)
            {
                int worldX = 3138 + Mathf.FloorToInt(Deterministic01(7000 + i * 19) * 4f);
                int worldY = 3444 + Mathf.FloorToInt(Deterministic01(7100 + i * 23) * 78f);
                Vector3 position = WorldTilePosition(context, worldX, worldY) + Vector3.up * (0.21f + Deterministic01(7200 + i * 29) * 0.12f);
                GameObject fog = CreateSubdividedPlane($"River Surface Mist {i + 1}", position, 4.2f + (i % 3) * 1.0f, 1.5f + (i % 4) * 0.45f, 3, materials.HorizonMist);
                fog.transform.SetParent(root.transform, true);
                fog.transform.rotation = Quaternion.Euler(0f, Deterministic01(7300 + i * 31) * 180f, 0f);
                MeshRenderer renderer = fog.GetComponent<MeshRenderer>();
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                panels++;
            }

            context.Report.horizonMistPanels += panels;
        }

        private static bool TryGetNaturalDressingPosition(HostedBuildContext context, int worldX, int worldY, int seed, out Vector3 position)
        {
            position = default;
            if (!TryGetLandscapeTile(context, worldX, worldY, out PsychoMapLandscape landscape, out int localX, out int localY))
            {
                return false;
            }

            byte flags = landscape.RenderFlags[0, localX, localY];
            if ((flags & 1) == 1)
            {
                return false;
            }

            float scatterX = (Deterministic01(seed + 101) - 0.5f) * TileScale * 0.86f;
            float scatterZ = (Deterministic01(seed + 103) - 0.5f) * TileScale * 0.86f;
            position = WorldTilePosition(context, worldX, worldY) + new Vector3(scatterX, 0.045f, scatterZ);
            return true;
        }

        private static void CreateHighlandConifer(Transform parent, Vector3 position, float height, Material barkMaterial, Material canopyMaterial, int seed)
        {
            GameObject tree = new GameObject("Wind Swept Highland Conifer");
            tree.transform.SetParent(parent, false);
            tree.transform.position = position;
            tree.transform.rotation = Quaternion.Euler(0f, Deterministic01(seed + 5) * 360f, 0f);

            float trunkHeight = height * (0.52f + Deterministic01(seed + 9) * 0.10f);
            float trunkRadius = height * (0.026f + Deterministic01(seed + 13) * 0.010f);
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Dark Bark Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight * 0.5f, 0f);
            trunk.transform.localScale = new Vector3(trunkRadius, trunkHeight * 0.5f, trunkRadius);
            MeshRenderer trunkRenderer = trunk.GetComponent<MeshRenderer>();
            trunkRenderer.sharedMaterial = barkMaterial;
            trunkRenderer.shadowCastingMode = ShadowCastingMode.On;
            trunkRenderer.receiveShadows = true;
            RemoveCollider(trunk);

            int layerCount = 5 + Mathf.FloorToInt(Deterministic01(seed + 17) * 2f);
            for (int layer = 0; layer < layerCount; layer++)
            {
                float t = layer / Mathf.Max(1f, layerCount - 1f);
                float baseY = height * Mathf.Lerp(0.20f, 0.76f, t);
                float layerHeight = height * Mathf.Lerp(0.24f, 0.16f, t);
                float radius = height * Mathf.Lerp(0.32f, 0.12f, t) * (0.92f + Deterministic01(seed + layer * 29) * 0.18f);
                GameObject canopy = new GameObject($"Layered Blue-Green Boughs {layer + 1}");
                canopy.transform.SetParent(tree.transform, false);
                canopy.transform.localPosition = new Vector3(
                    (Deterministic01(seed + layer * 41) - 0.5f) * 0.08f,
                    baseY,
                    (Deterministic01(seed + layer * 47) - 0.5f) * 0.08f);
                canopy.transform.localRotation = Quaternion.Euler(0f, layer * 31f + Deterministic01(seed + layer * 53) * 18f, 0f);
                canopy.AddComponent<MeshFilter>().sharedMesh = CreateTaperedConeMesh($"Conifer Bough Mesh {seed}_{layer}", 12, radius, radius * 0.10f, layerHeight, seed + layer * 71);
                MeshRenderer renderer = canopy.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = canopyMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                AddWind(canopy, 0.038f + t * 0.024f, 0.70f + t * 0.34f, 0.30f, 0.76f, 0.042f);

                for (int arm = 0; arm < 4; arm++)
                {
                    float branchAngle = layer * 0.74f + arm * Mathf.PI * 0.5f + Deterministic01(seed + layer * 101 + arm * 17) * 0.28f;
                    Vector3 sprayOffset = new Vector3(Mathf.Cos(branchAngle) * radius * 0.38f, -layerHeight * 0.16f, Mathf.Sin(branchAngle) * radius * 0.38f);
                    CreateFoliageSpray(
                        tree.transform,
                        $"Highland Needle Spray {layer + 1}.{arm + 1}",
                        canopy.transform.localPosition + sprayOffset,
                        new Vector3(radius * 0.52f, layerHeight * 0.11f, radius * 0.18f),
                        branchAngle * Mathf.Rad2Deg,
                        canopyMaterial,
                        0.030f + t * 0.026f,
                        0.68f + t * 0.32f);
                }
            }
        }

        private static Mesh CreateTaperedConeMesh(string name, int segments, float bottomRadius, float topRadius, float height, int seed)
        {
            Mesh mesh = new Mesh { name = name };
            Vector3[] vertices = new Vector3[segments * 2];
            int[] triangles = new int[segments * 6];

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float bottomNoise = 0.90f + Deterministic01(seed + i * 17) * 0.20f;
                float topNoise = 0.86f + Deterministic01(seed + i * 19) * 0.24f;
                vertices[i] = new Vector3(Mathf.Cos(angle) * bottomRadius * bottomNoise, 0f, Mathf.Sin(angle) * bottomRadius * bottomNoise);
                vertices[i + segments] = new Vector3(Mathf.Cos(angle) * topRadius * topNoise, height, Mathf.Sin(angle) * topRadius * topNoise);
            }

            int tri = 0;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles[tri++] = i;
                triangles[tri++] = i + segments;
                triangles[tri++] = next;
                triangles[tri++] = next;
                triangles[tri++] = i + segments;
                triangles[tri++] = next + segments;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void CreateHighlandRockOutcrop(Transform parent, Vector3 position, float size, Material rockMaterial, Material mossMaterial, int seed)
        {
            GameObject outcrop = new GameObject("Layered Highland Rock Outcrop");
            outcrop.transform.SetParent(parent, false);
            outcrop.transform.position = position;
            outcrop.transform.rotation = Quaternion.Euler(0f, Deterministic01(seed + 5) * 360f, 0f);

            int rockCount = 3 + Mathf.FloorToInt(Deterministic01(seed + 11) * 3f);
            for (int i = 0; i < rockCount; i++)
            {
                float angle = i * Mathf.PI * 2f / rockCount + Deterministic01(seed + i * 19) * 0.62f;
                float offset = size * (0.10f + Deterministic01(seed + i * 23) * 0.36f);
                GameObject rock = new GameObject($"Cold Slate Boulder {i + 1}");
                rock.transform.SetParent(outcrop.transform, false);
                rock.transform.localPosition = new Vector3(Mathf.Cos(angle) * offset, 0.04f, Mathf.Sin(angle) * offset);
                rock.transform.localRotation = Quaternion.Euler(
                    Deterministic01(seed + i * 31) * 6f,
                    Deterministic01(seed + i * 37) * 360f,
                    Deterministic01(seed + i * 41) * 6f);
                float width = size * (0.42f + Deterministic01(seed + i * 43) * 0.54f);
                float height = size * (0.34f + Deterministic01(seed + i * 47) * 0.64f);
                float depth = size * (0.40f + Deterministic01(seed + i * 53) * 0.52f);
                rock.AddComponent<MeshFilter>().sharedMesh = CreateIrregularRockMesh($"Cold Slate Boulder Mesh {seed}_{i}", width, height, depth, seed + i * 79);
                MeshRenderer renderer = rock.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = rockMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            GameObject moss = CreateSubdividedPlane("Moss and Heather Skirt", outcrop.transform.position + Vector3.up * 0.025f, size * 1.95f, size * 1.50f, 4, mossMaterial);
            moss.transform.SetParent(outcrop.transform, true);
            moss.transform.localRotation = Quaternion.Euler(0f, Deterministic01(seed + 97) * 360f, 0f);
        }

        private static Mesh CreateIrregularRockMesh(string name, float width, float height, float depth, int seed)
        {
            const int segments = 8;
            Mesh mesh = new Mesh { name = name };
            Vector3[] vertices = new Vector3[segments * 3];
            List<int> triangles = new List<int>(segments * 12);

            for (int ring = 0; ring < 3; ring++)
            {
                float y = ring == 0 ? 0f : ring == 1 ? height * 0.56f : height;
                float ringScale = ring == 0 ? 0.86f : ring == 1 ? 1.0f : 0.25f;
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float noise = 0.72f + Deterministic01(seed + ring * 101 + i * 13) * 0.48f;
                    vertices[ring * segments + i] = new Vector3(
                        Mathf.Cos(angle) * width * ringScale * noise,
                        y,
                        Mathf.Sin(angle) * depth * ringScale * (0.82f + Deterministic01(seed + ring * 89 + i * 17) * 0.36f));
                }
            }

            for (int ring = 0; ring < 2; ring++)
            {
                int current = ring * segments;
                int nextRing = (ring + 1) * segments;
                for (int i = 0; i < segments; i++)
                {
                    int next = (i + 1) % segments;
                    triangles.Add(current + i);
                    triangles.Add(nextRing + i);
                    triangles.Add(current + next);
                    triangles.Add(current + next);
                    triangles.Add(nextRing + i);
                    triangles.Add(nextRing + next);
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void CreateVerticalMistPanel(Transform parent, string name, Vector3 position, float width, float height, Material material)
        {
            Mesh mesh = new Mesh { name = name + " Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-width * 0.5f, 0f, 0f),
                new Vector3(width * 0.5f, 0f, 0f),
                new Vector3(-width * 0.5f, height, 0f),
                new Vector3(width * 0.5f, height, 0f)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };
            mesh.triangles = new[] { 0, 1, 2, 1, 3, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.transform.position = position;
            Vector3 inward = new Vector3(-position.x, 0f, -position.z);
            if (inward.sqrMagnitude > 0.01f)
            {
                panel.transform.rotation = Quaternion.LookRotation(inward.normalized);
            }

            panel.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = panel.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static bool TryCreateLandmarkRoot(Transform parent, string name, HostedBuildContext context, int worldX, int worldY, out Transform root)
        {
            root = null;
            if (!IsWorldTileInsideHostedBounds(worldX, worldY))
            {
                return false;
            }

            GameObject landmark = new GameObject(name);
            landmark.transform.SetParent(parent, false);
            landmark.transform.position = WorldTilePosition(context, worldX, worldY) + Vector3.up * 0.04f;
            root = landmark.transform;
            return true;
        }

        private static void CreateGroundPlate(Transform parent, string name, Vector3 localPosition, float width, float depth, Material material)
        {
            GameObject plate = CreateSubdividedPlane(name, parent.position + localPosition, width, depth, 8, material);
            plate.transform.SetParent(parent, true);
            plate.transform.localRotation = Quaternion.identity;
            plate.transform.localPosition = localPosition;
        }

        private static void CreateLandmarkPaverBands(Transform parent, string prefix, float width, float depth, Material material)
        {
            for (int i = -3; i <= 3; i++)
            {
                float x = i * width / 8f;
                CreateLandmarkDetailBox(parent, $"{prefix} North-South Paver Joint {i + 4}", new Vector3(x, 0.035f, 0f), new Vector3(0.030f, 0.020f, depth * 0.92f), material);
            }

            for (int i = -2; i <= 2; i++)
            {
                float z = i * depth / 7f;
                CreateLandmarkDetailBox(parent, $"{prefix} East-West Paver Joint {i + 3}", new Vector3(0f, 0.040f, z), new Vector3(width * 0.92f, 0.020f, 0.030f), material);
            }
        }

        private static void CreateLandmarkStoneCourses(Transform parent, string prefix, float width, float depth, float bottomY, float topY, Material material)
        {
            int rows = 5;
            for (int row = 0; row < rows; row++)
            {
                float t = row / Mathf.Max(1f, rows - 1f);
                float y = Mathf.Lerp(bottomY, topY, t);
                float rowHeight = row % 2 == 0 ? 0.045f : 0.035f;
                CreateLandmarkDetailBox(parent, $"{prefix} Front Row {row + 1}", new Vector3(0f, y, -depth * 0.535f), new Vector3(width * 0.96f, rowHeight, 0.030f), material);
                CreateLandmarkDetailBox(parent, $"{prefix} Back Row {row + 1}", new Vector3(0f, y, depth * 0.535f), new Vector3(width * 0.96f, rowHeight, 0.030f), material);
                CreateLandmarkDetailBox(parent, $"{prefix} Left Row {row + 1}", new Vector3(-width * 0.535f, y, 0f), new Vector3(0.030f, rowHeight, depth * 0.94f), material);
                CreateLandmarkDetailBox(parent, $"{prefix} Right Row {row + 1}", new Vector3(width * 0.535f, y, 0f), new Vector3(0.030f, rowHeight, depth * 0.94f), material);
            }
        }

        private static void CreateLandmarkTimberBracing(Transform parent, string prefix, float width, float depth, float centerY, float braceHeight, Material material)
        {
            float frontZ = -depth * 0.545f;
            float backZ = depth * 0.545f;
            float sideX = width * 0.545f;
            for (int i = -1; i <= 1; i += 2)
            {
                float x = i * width * 0.23f;
                GameObject frontBrace = CreateLandmarkDetailBox(parent, $"{prefix} Front Diagonal {i}", new Vector3(x, centerY, frontZ), new Vector3(0.065f, braceHeight, 0.035f), material);
                frontBrace.transform.localRotation = Quaternion.Euler(0f, 0f, i * 24f);
                GameObject backBrace = CreateLandmarkDetailBox(parent, $"{prefix} Back Diagonal {i}", new Vector3(x, centerY, backZ), new Vector3(0.065f, braceHeight, 0.035f), material);
                backBrace.transform.localRotation = Quaternion.Euler(0f, 0f, -i * 24f);
                CreateLandmarkDetailBox(parent, $"{prefix} Front Upright {i}", new Vector3(x * 1.22f, centerY, frontZ), new Vector3(0.075f, braceHeight * 0.82f, 0.040f), material);
                CreateLandmarkDetailBox(parent, $"{prefix} Back Upright {i}", new Vector3(x * 1.22f, centerY, backZ), new Vector3(0.075f, braceHeight * 0.82f, 0.040f), material);
            }

            CreateLandmarkDetailBox(parent, $"{prefix} Left Wall Plate", new Vector3(-sideX, centerY, 0f), new Vector3(0.040f, braceHeight * 0.82f, depth * 0.68f), material);
            CreateLandmarkDetailBox(parent, $"{prefix} Right Wall Plate", new Vector3(sideX, centerY, 0f), new Vector3(0.040f, braceHeight * 0.82f, depth * 0.68f), material);
        }

        private static void CreateLandmarkRoofShingles(Transform parent, string prefix, float width, float depth, float centerY, float roofHeight, Material material)
        {
            int rows = 6;
            for (int row = 0; row < rows; row++)
            {
                float t = row / Mathf.Max(1f, rows - 1f);
                float y = centerY + Mathf.Lerp(-roofHeight * 0.30f, roofHeight * 0.34f, t);
                float z = Mathf.Lerp(depth * 0.49f, depth * 0.10f, t);
                float stripWidth = width * Mathf.Lerp(1.02f, 0.70f, t);
                CreateLandmarkDetailBox(parent, $"{prefix} Front Strip {row + 1}", new Vector3(0f, y, -z), new Vector3(stripWidth, 0.028f, 0.070f), material);
                CreateLandmarkDetailBox(parent, $"{prefix} Back Strip {row + 1}", new Vector3(0f, y, z), new Vector3(stripWidth, 0.028f, 0.070f), material);
            }
        }

        private static void CreateLandmarkChimney(Transform parent, string prefix, Vector3 localPosition, Material stone, Material cap)
        {
            CreateLandmarkDetailBox(parent, $"{prefix} Stack", localPosition, new Vector3(0.34f, 0.82f, 0.34f), stone);
            CreateLandmarkDetailBox(parent, $"{prefix} Cap", localPosition + Vector3.up * 0.44f, new Vector3(0.44f, 0.10f, 0.44f), cap);
        }

        private static void CreateHouseDetailSet(Transform parent, string prefix, Vector3 origin, float width, float depth, HostedMaterials materials)
        {
            GameObject detailRoot = new GameObject($"{prefix} Stone Timber Roof Details");
            detailRoot.transform.SetParent(parent, false);
            detailRoot.transform.localPosition = origin;
            CreateLandmarkStoneCourses(detailRoot.transform, $"{prefix} Stone Course", width, depth, 0.42f, 1.44f, materials.FrostStone);
            CreateLandmarkTimberBracing(detailRoot.transform, $"{prefix} Timber Frame", width, depth, 1.02f, 1.04f, materials.TreeBark);
            CreateLandmarkRoofShingles(detailRoot.transform, $"{prefix} Roof Shingles", width * 1.20f, depth * 1.18f, 1.88f, 0.74f, materials.TreeBark);
            CreateLandmarkChimney(detailRoot.transform, $"{prefix} Chimney", new Vector3(width * 0.22f, 2.28f, depth * 0.04f), materials.FrostStone, materials.TreeBark);
        }

        private static GameObject CreateLandmarkDetailBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject box = CreateLandmarkBox(parent, name, localPosition, localScale, material);
            RemoveCollider(box);
            return box;
        }

        private static GameObject CreateLandmarkBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localScale = localScale;
            MeshRenderer renderer = box.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return box;
        }

        private static GameObject CreateLandmarkCylinder(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localScale = localScale;
            MeshRenderer renderer = cylinder.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return cylinder;
        }

        private static GameObject CreateLandmarkGabledRoof(Transform parent, string name, Vector3 localPosition, float width, float depth, float height, Material material)
        {
            Mesh mesh = new Mesh { name = name + " Mesh" };
            float halfWidth = width * 0.5f;
            float halfDepth = depth * 0.5f;
            float low = -height * 0.5f;
            float high = height * 0.5f;
            mesh.vertices = new[]
            {
                new Vector3(-halfWidth, low, -halfDepth),
                new Vector3(halfWidth, low, -halfDepth),
                new Vector3(-halfWidth, low, halfDepth),
                new Vector3(halfWidth, low, halfDepth),
                new Vector3(-halfWidth, high, 0f),
                new Vector3(halfWidth, high, 0f)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 1f)
            };
            mesh.triangles = new[]
            {
                0, 4, 2,
                1, 3, 5,
                0, 1, 5,
                0, 5, 4,
                2, 4, 5,
                2, 5, 3,
                0, 2, 1,
                1, 2, 3
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject roof = new GameObject(name);
            roof.transform.SetParent(parent, false);
            roof.transform.localPosition = localPosition;
            roof.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = roof.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return roof;
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
            for (int i = 0; i < 38; i++)
            {
                float x = Mathf.Sin(i * 2.91f) * 236f;
                float z = Mathf.Cos(i * 1.73f) * 244f;
                float y = 88f + (i % 6) * 5.6f;
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
                    float localX = (lobe - (lobes - 1) * 0.5f) * 6.2f;
                    puff.transform.localPosition = new Vector3(localX, Mathf.Sin(lobe * 1.7f) * 0.34f, Mathf.Cos(lobe * 1.1f) * 1.65f);
                    puff.transform.localScale = new Vector3(5.8f + lobe * 0.52f, 0.30f + (lobe % 2) * 0.08f, 1.85f + (lobe % 3) * 0.34f);
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
                driftObject.FindProperty("driftSpeed").floatValue = 0.20f + (i % 4) * 0.035f;
                driftObject.FindProperty("wrapDistance").floatValue = 480f;
                driftObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void BuildLighting()
        {
            QualitySettings.antiAliasing = 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.pixelLightCount = 2;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;
            QualitySettings.shadowDistance = 150f;
            QualitySettings.shadowCascades = 2;
            QualitySettings.lodBias = 1.25f;
            QualitySettings.softParticles = true;
            QualitySettings.softVegetation = true;
            QualitySettings.realtimeReflectionProbes = false;

            GameObject sunObject = new GameObject("Sun");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.02f;
            sun.color = new Color(1f, 0.91f, 0.78f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.78f;
            sunObject.transform.rotation = Quaternion.Euler(42f, -39f, 0f);

            GameObject fillObject = new GameObject("Soft Sky Fill");
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.18f;
            fill.color = new Color(0.42f, 0.53f, 0.68f);
            fillObject.transform.rotation = Quaternion.Euler(24f, 136f, 0f);

            GameObject bounceObject = new GameObject("Warm Ground Bounce");
            Light bounce = bounceObject.AddComponent<Light>();
            bounce.type = LightType.Directional;
            bounce.intensity = 0.045f;
            bounce.color = new Color(0.58f, 0.48f, 0.34f);
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
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
            probe.size = new Vector3(920f, 180f, 920f);
            reflectionObject.transform.position = new Vector3(0f, 18f, 0f);

            RenderSettings.skybox = LoadOrCreateSkybox();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.46f, 0.56f, 0.66f);
            RenderSettings.ambientEquatorColor = new Color(0.26f, 0.33f, 0.32f);
            RenderSettings.ambientGroundColor = new Color(0.11f, 0.12f, 0.11f);
            RenderSettings.ambientIntensity = 0.78f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.50f, 0.58f, 0.64f);
            RenderSettings.fogDensity = 0.00115f;
            RenderSettings.reflectionIntensity = 0.42f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }

        private static void BuildPlayer(HostedBuildContext context, HostedMaterials materials, PsychoMirrorDatabase database)
        {
            HostedPlayerSave playerSave = LoadHostedPlayerSave(HostedPlayerUsername);
            int spawnX = 3087;
            int spawnY = 3489;
            if (!IsWorldTileInsideHostedBounds(spawnX, spawnY))
            {
                Debug.LogWarning($"Hosted visual spawn {spawnX}, {spawnY} is outside the generated test world. Falling back to Edgeville.");
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

            Vector3 requestedSpawn = WorldTilePosition(context, spawnX, spawnY);
            player.transform.position = TerrainSurfacePosition(requestedSpawn, 0.04f);
            PsychoPlayableCharacter playableCharacter = player.AddComponent<PsychoPlayableCharacter>();
            player.AddComponent<PsychoCharacterGroundGuard>();
            player.AddComponent<PsychoInteractionController>();
            BuildPlayerVisual(player.transform, materials, database, playerSave);

            GameObject cameraPivot = new GameObject("Player Camera Pivot");
            cameraPivot.transform.SetParent(player.transform, false);
            cameraPivot.transform.localPosition = new Vector3(0f, 1.45f, 0f);
            cameraPivot.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(cameraPivot.transform, false);
            cameraObject.transform.localPosition = new Vector3(0.22f, 0.12f, -3.8f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraObject.transform.rotation = Quaternion.LookRotation(cameraPivot.transform.position - cameraObject.transform.position, Vector3.up);
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 2400f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.depthTextureMode = DepthTextureMode.Depth;

            SerializedObject characterObject = new SerializedObject(playableCharacter);
            SerializedProperty characterCameraProperty = characterObject.FindProperty("playerCamera");
            if (characterCameraProperty != null)
            {
                characterCameraProperty.objectReferenceValue = camera;
            }

            SerializedProperty cameraPivotProperty = characterObject.FindProperty("cameraPivot");
            if (cameraPivotProperty != null)
            {
                cameraPivotProperty.objectReferenceValue = cameraPivot.transform;
            }

            SetSerializedFloat(characterObject, "thirdPersonDistance", 3.8f);
            SetSerializedFloat(characterObject, "minThirdPersonDistance", 1.55f);
            SetSerializedFloat(characterObject, "maxThirdPersonDistance", 8.5f);
            SetSerializedFloat(characterObject, "scrollZoomSensitivity", 1.15f);
            SetSerializedFloat(characterObject, "middleMouseTurnSensitivity", 1.05f);
            SetSerializedFloat(characterObject, "thirdPersonHeight", 1.45f);
            SetSerializedFloat(characterObject, "cameraSideOffset", 0.22f);
            SetSerializedFloat(characterObject, "minPitch", -32f);
            SetSerializedFloat(characterObject, "maxPitch", 58f);
            SetSerializedFloat(characterObject, "cameraCollisionRadius", 0.22f);
            SetSerializedFloat(characterObject, "cameraSmoothTime", 0.055f);
            characterObject.ApplyModifiedPropertiesWithoutUndo();

            PsychoCameraColorGrade colorGrade = cameraObject.AddComponent<PsychoCameraColorGrade>();
            SerializedObject colorGradeObject = new SerializedObject(colorGrade);
            SerializedProperty shaderProperty = colorGradeObject.FindProperty("shader");
            if (shaderProperty != null)
            {
                shaderProperty.objectReferenceValue = Shader.Find("Hidden/Psycho/Camera Color Grade");
            }

            SetSerializedFloat(colorGradeObject, "exposure", 0.98f);
            SetSerializedFloat(colorGradeObject, "contrast", 1.16f);
            SetSerializedFloat(colorGradeObject, "saturation", 0.96f);
            SetSerializedFloat(colorGradeObject, "warmth", -0.04f);
            SetSerializedFloat(colorGradeObject, "vignette", 0.18f);
            SetSerializedFloat(colorGradeObject, "sharpen", 0.20f);
            colorGradeObject.ApplyModifiedPropertiesWithoutUndo();
            cameraObject.AddComponent<AudioListener>();
            BuildGameplayHud(player.transform, playerSave);
        }

        private static void BuildGameplayHud(Transform player, HostedPlayerSave playerSave)
        {
            GameObject hudObject = new GameObject("Psycho Gameplay HUD");
            PsychoHudController hud = hudObject.AddComponent<PsychoHudController>();
            SerializedObject hudSerializedObject = new SerializedObject(hud);

            SetSerializedObject(hudSerializedObject, "player", player);
            SetSerializedInt(hudSerializedObject, "currentHealth", Mathf.Max(1, playerSave?.currentHealth ?? 100));
            SetSerializedInt(hudSerializedObject, "maxHealth", Mathf.Max(1, playerSave?.maxHealth ?? 100));
            SetSerializedInt(hudSerializedObject, "currentPrayer", Mathf.Max(0, playerSave?.currentPrayer ?? 1));
            SetSerializedInt(hudSerializedObject, "maxPrayer", Mathf.Max(1, playerSave?.maxPrayer ?? 1));
            SetSerializedInt(hudSerializedObject, "runEnergy", Mathf.Clamp(playerSave?.runEnergy ?? 100, 0, 100));
            SetSerializedLong(hudSerializedObject, "moneyPouch", Math.Max(0L, playerSave?.moneyPouch ?? 0L));
            SetSerializedString(hudSerializedObject, "recipeForDisasterStatus", playerSave?.recipeForDisasterStatus ?? "Recipe for Disaster: Not started");
            SetSerializedString(hudSerializedObject, "nomadStatus", playerSave?.nomadStatus ?? "Nomad's Requiem: Not started");
            SetSerializedString(hudSerializedObject, "questSummary", playerSave?.questSummary ?? "Quest Progress: 0/2");
            hudSerializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildPlayerVisual(Transform parent, HostedMaterials materials, PsychoMirrorDatabase database, HostedPlayerSave playerSave)
        {
            if (PsychoArtAssetResolver.TryInstantiatePlayer(PlayerDisplayName(playerSave), parent, out GameObject artPlayer))
            {
                artPlayer.name = "Psycho Hero Art Prefab";
                CreatePlayerNameplate(artPlayer.transform, playerSave);
                Debug.Log($"Hosted player visual uses art-pipeline prefab for {PlayerDisplayName(playerSave)}.");
                return;
            }

            GameObject visualRoot = new GameObject("Psycho Hero Visual");
            visualRoot.transform.SetParent(parent, false);

            PsychoMirrorItem heroHelm = CreateHostedHeroItem("Psycho Frost-Steel War Helm", "Helmet", "Metal");
            PsychoMirrorItem heroCape = CreateHostedHeroItem("Psycho Highland Cloak", "Cape", "Cloth");
            PsychoMirrorItem heroAmulet = CreateHostedHeroItem("Psycho Dawn Crystal Amulet", "Amulet", "Crystal");
            PsychoMirrorItem heroBlade = CreateHostedHeroItem("Psycho Frost Runeblade", "TwoHandedWeapon", "Metal");
            PsychoMirrorItem heroShield = CreateHostedHeroItem("Psycho Rune-Kite Ward", "Shield", "Metal");
            PsychoMirrorItem heroBody = CreateHostedHeroItem("Psycho Layered Plate Harness", "Body", "Metal");
            PsychoMirrorItem heroHands = CreateHostedHeroItem("Psycho Leather Gauntlets", "Gloves", "Leather");
            PsychoMirrorItem heroLegs = CreateHostedHeroItem("Psycho Scale Greaves", "Legs", "Metal");
            PsychoMirrorItem heroFeet = CreateHostedHeroItem("Psycho Highland Boots", "Boots", "Leather");

            Material armorMaterial = materials.PlayerMetal;
            Material tunicMaterial = materials.PlayerLeather;
            Material trouserMaterial = materials.PlayerLeather;
            Material handMaterial = materials.PlayerLeather;
            Material palmMaterial = materials.PlayerSkin;

            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Capsule, "Rugged Highland Torso", new Vector3(0f, 1.03f, 0.015f), new Vector3(0.27f, 0.39f, 0.18f), tunicMaterial);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Sphere, "Rounded Chest Volume", new Vector3(0f, 1.15f, -0.045f), new Vector3(0.245f, 0.190f, 0.150f), armorMaterial, heroBody);
            CreatePlayerTaperedPrism(visualRoot.transform, "Layered Cuirass Front", new Vector3(0f, 1.13f, -0.205f), 0.40f, 0.30f, 0.37f, 0.050f, armorMaterial, heroBody);
            CreatePlayerTaperedPrism(visualRoot.transform, "Left Cuirass Rib Plate", new Vector3(-0.170f, 1.08f, -0.220f), 0.075f, 0.060f, 0.30f, 0.032f, armorMaterial, heroBody);
            CreatePlayerTaperedPrism(visualRoot.transform, "Right Cuirass Rib Plate", new Vector3(0.170f, 1.08f, -0.220f), 0.075f, 0.060f, 0.30f, 0.032f, armorMaterial, heroBody);
            CreatePlayerTaperedPrism(visualRoot.transform, "Lower Mail Fauld", new Vector3(0f, 0.74f, -0.105f), 0.33f, 0.42f, 0.17f, 0.050f, armorMaterial, heroBody);
            CreatePlayerTaperedPrism(visualRoot.transform, "Left Hanging Scale Fauld", new Vector3(-0.145f, 0.61f, -0.085f), 0.11f, 0.15f, 0.22f, 0.038f, materials.PlayerMetal, heroBody).transform.localRotation = Quaternion.Euler(0f, 0f, 5f);
            CreatePlayerTaperedPrism(visualRoot.transform, "Right Hanging Scale Fauld", new Vector3(0.145f, 0.61f, -0.085f), 0.11f, 0.15f, 0.22f, 0.038f, materials.PlayerMetal, heroBody).transform.localRotation = Quaternion.Euler(0f, 0f, -5f);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Wide Leather War Belt", new Vector3(0f, 0.83f, -0.055f), new Vector3(0.45f, 0.062f, 0.24f), materials.PlayerLeather);
            CreatePlayerPrimitive(visualRoot.transform, PrimitiveType.Cube, "Belt Steel Buckle", new Vector3(0f, 0.83f, -0.205f), new Vector3(0.088f, 0.070f, 0.025f), materials.PlayerMetal);
            CreatePlayerArmorStrap(visualRoot.transform, "Left Chest Harness", new Vector3(-0.12f, 1.15f, -0.245f), -24f, materials.PlayerLeather);
            CreatePlayerArmorStrap(visualRoot.transform, "Right Chest Harness", new Vector3(0.12f, 1.15f, -0.245f), 24f, materials.PlayerLeather);
            CreateHighlandFurMantle(visualRoot.transform, materials);
            CreatePlayerAmulet(visualRoot.transform, heroAmulet, materials);

            CreateHighlandArm(visualRoot.transform, true, tunicMaterial, armorMaterial, handMaterial, palmMaterial, materials, heroBody, heroHands);
            CreateHighlandArm(visualRoot.transform, false, tunicMaterial, armorMaterial, handMaterial, palmMaterial, materials, heroBody, heroHands);
            CreateHighlandLeg(visualRoot.transform, true, trouserMaterial, materials.PlayerMetal, materials.PlayerLeather, materials, heroLegs, heroFeet, true);
            CreateHighlandLeg(visualRoot.transform, false, trouserMaterial, materials.PlayerMetal, materials.PlayerLeather, materials, heroLegs, heroFeet, true);
            CreateHighlandHead(visualRoot.transform, materials, heroHelm);
            CreateCapePanel(visualRoot.transform, materials.PlayerCloth, materials, heroCape);
            CreatePsychoRuneblade(visualRoot.transform, materials, heroBlade);
            CreatePsychoRuneKiteShield(visualRoot.transform, materials, heroShield);

            CreatePlayerNameplate(visualRoot.transform, playerSave);
            Debug.Log($"Hosted player visual uses original Psycho hero art for {PlayerDisplayName(playerSave)}; Java equipment slots are ignored in the Unity visual preview.");
        }

        private static PsychoMirrorItem CreateHostedHeroItem(string name, string visualClass, string materialClass)
        {
            return new PsychoMirrorItem
            {
                id = -1,
                name = name,
                visualClass = visualClass,
                materialClass = materialClass,
                scale = 1f
            };
        }

        private static void CreatePsychoRuneblade(Transform visualRoot, HostedMaterials materials, PsychoMirrorItem weapon)
        {
            GameObject blade = CreateExtrudedPolygon(
                visualRoot,
                "Psycho Frost Runeblade",
                new[]
                {
                    new Vector2(-0.070f, -0.36f),
                    new Vector2(0.070f, -0.36f),
                    new Vector2(0.105f, 0.12f),
                    new Vector2(0.055f, 0.42f),
                    new Vector2(0.000f, 0.58f),
                    new Vector2(-0.055f, 0.42f),
                    new Vector2(-0.105f, 0.12f)
                },
                0.020f,
                materials.PlayerMetal);
            blade.name = weapon.name;
            blade.transform.localPosition = new Vector3(0.51f, 0.95f, -0.035f);
            blade.transform.localRotation = Quaternion.Euler(0f, 0f, -24f);

            GameObject fuller = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cylinder, "Runeblade Central Fuller", new Vector3(0.55f, 1.02f, -0.060f), new Vector3(0.014f, 0.39f, 0.014f), materials.LandmarkGlass, weapon);
            fuller.transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
            GameObject guard = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cylinder, "Runeblade Curved Guard", new Vector3(0.40f, 0.61f, -0.035f), new Vector3(0.030f, 0.165f, 0.030f), materials.PlayerMetal, weapon);
            guard.transform.localRotation = Quaternion.Euler(0f, 0f, 72f);
            GameObject grip = CreatePlayerPrimitive(visualRoot, PrimitiveType.Capsule, "Runeblade Leather Grip", new Vector3(0.36f, 0.49f, -0.030f), new Vector3(0.043f, 0.155f, 0.043f), materials.PlayerLeather, weapon);
            grip.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, "Runeblade Crystal Pommel", new Vector3(0.32f, 0.36f, -0.030f), new Vector3(0.058f, 0.058f, 0.058f), materials.LandmarkBanner, weapon);
        }

        private static void CreatePsychoRuneKiteShield(Transform visualRoot, HostedMaterials materials, PsychoMirrorItem shield)
        {
            GameObject shieldBody = CreateExtrudedPolygon(
                visualRoot,
                "Psycho Rune-Kite Ward",
                new[]
                {
                    new Vector2(-0.23f, 0.21f),
                    new Vector2(0.23f, 0.21f),
                    new Vector2(0.19f, -0.12f),
                    new Vector2(0.00f, -0.43f),
                    new Vector2(-0.19f, -0.12f)
                },
                0.030f,
                materials.PlayerMetal);
            shieldBody.name = shield.name;
            shieldBody.transform.localPosition = new Vector3(-0.50f, 0.84f, -0.020f);
            shieldBody.transform.localRotation = Quaternion.Euler(0f, 0f, 3f);

            GameObject boss = CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, "Rune-Kite Raised Boss", new Vector3(-0.505f, 0.85f, -0.060f), new Vector3(0.085f, 0.060f, 0.030f), materials.LandmarkBanner, shield);
            boss.transform.localRotation = Quaternion.Euler(0f, 0f, 3f);
            CreatePlayerTaperedPrism(visualRoot, "Rune-Kite Left Frost Inlay", new Vector3(-0.585f, 0.84f, -0.066f), 0.040f, 0.018f, 0.42f, 0.014f, materials.LandmarkGlass, shield).transform.localRotation = Quaternion.Euler(0f, 0f, -10f);
            CreatePlayerTaperedPrism(visualRoot, "Rune-Kite Right Frost Inlay", new Vector3(-0.425f, 0.84f, -0.066f), 0.040f, 0.018f, 0.42f, 0.014f, materials.LandmarkGlass, shield).transform.localRotation = Quaternion.Euler(0f, 0f, 10f);
        }

        private static GameObject CreateExtrudedPolygon(Transform parent, string name, Vector2[] shape, float depth, Material material)
        {
            int count = shape.Length;
            float halfDepth = depth * 0.5f;
            Vector3[] vertices = new Vector3[count * 2];
            for (int i = 0; i < count; i++)
            {
                vertices[i] = new Vector3(shape[i].x, shape[i].y, -halfDepth);
                vertices[i + count] = new Vector3(shape[i].x, shape[i].y, halfDepth);
            }

            List<int> triangles = new List<int>((count - 2) * 6 + count * 6);
            for (int i = 1; i < count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);

                triangles.Add(count);
                triangles.Add(count + i + 1);
                triangles.Add(count + i);
            }

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                triangles.Add(i);
                triangles.Add(next);
                triangles.Add(count + i);
                triangles.Add(next);
                triangles.Add(count + next);
                triangles.Add(count + i);
            }

            Mesh mesh = new Mesh { name = name + " Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject polygon = new GameObject(name);
            polygon.transform.SetParent(parent, false);
            polygon.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = polygon.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return polygon;
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

        private static GameObject CreatePlayerTaperedPrism(Transform parent, string name, Vector3 localPosition, float topWidth, float bottomWidth, float height, float depth, Material material, PsychoMirrorItem item = null)
        {
            float top = topWidth * 0.5f;
            float bottom = bottomWidth * 0.5f;
            float halfHeight = height * 0.5f;
            float halfDepth = depth * 0.5f;
            Mesh mesh = new Mesh { name = name + " Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-top, halfHeight, -halfDepth),
                new Vector3(top, halfHeight, -halfDepth),
                new Vector3(bottom, -halfHeight, -halfDepth),
                new Vector3(-bottom, -halfHeight, -halfDepth),
                new Vector3(-top, halfHeight, halfDepth),
                new Vector3(top, halfHeight, halfDepth),
                new Vector3(bottom, -halfHeight, halfDepth),
                new Vector3(-bottom, -halfHeight, halfDepth)
            };
            mesh.triangles = new[]
            {
                0, 1, 2,
                0, 2, 3,
                4, 6, 5,
                4, 7, 6,
                4, 5, 1,
                4, 1, 0,
                3, 2, 6,
                3, 6, 7,
                1, 5, 6,
                1, 6, 2,
                4, 0, 3,
                4, 3, 7
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject prism = new GameObject(item == null || string.IsNullOrWhiteSpace(item.name) ? name : $"{name} - {item.name}");
            prism.transform.SetParent(parent, false);
            prism.transform.localPosition = localPosition;
            prism.AddComponent<MeshFilter>().sharedMesh = mesh;
            prism.AddComponent<MeshRenderer>().sharedMaterial = material;
            return prism;
        }

        private static GameObject CreatePlayerHelmetDome(Transform visualRoot, Material material, PsychoMirrorItem head)
        {
            const int radialSegments = 18;
            const int verticalSegments = 5;
            const float radiusX = 0.145f;
            const float radiusY = 0.090f;
            const float radiusZ = 0.132f;

            List<Vector3> vertices = new List<Vector3> { new Vector3(0f, radiusY, 0f) };
            for (int ring = 1; ring <= verticalSegments; ring++)
            {
                float theta = (Mathf.PI * 0.5f) * ring / verticalSegments;
                float y = Mathf.Cos(theta) * radiusY;
                float ringX = Mathf.Sin(theta) * radiusX;
                float ringZ = Mathf.Sin(theta) * radiusZ;
                for (int segment = 0; segment < radialSegments; segment++)
                {
                    float angle = (Mathf.PI * 2f) * segment / radialSegments;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * ringX, y, Mathf.Sin(angle) * ringZ));
                }
            }

            List<int> triangles = new List<int>();
            for (int segment = 0; segment < radialSegments; segment++)
            {
                int next = segment == radialSegments - 1 ? 0 : segment + 1;
                triangles.Add(0);
                triangles.Add(1 + next);
                triangles.Add(1 + segment);
            }

            for (int ring = 1; ring < verticalSegments; ring++)
            {
                int current = 1 + (ring - 1) * radialSegments;
                int nextRing = current + radialSegments;
                for (int segment = 0; segment < radialSegments; segment++)
                {
                    int next = segment == radialSegments - 1 ? 0 : segment + 1;
                    triangles.Add(current + next);
                    triangles.Add(nextRing + segment);
                    triangles.Add(current + segment);
                    triangles.Add(current + next);
                    triangles.Add(nextRing + next);
                    triangles.Add(nextRing + segment);
                }
            }

            Mesh mesh = new Mesh { name = "Highland Helmet Dome Mesh" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject helmet = new GameObject(head == null || string.IsNullOrWhiteSpace(head.name) ? "Highland Helmet Dome" : $"Highland Helmet Dome - {head.name}");
            helmet.transform.SetParent(visualRoot, false);
            helmet.transform.localPosition = new Vector3(0f, 1.675f, -0.01f);
            helmet.AddComponent<MeshFilter>().sharedMesh = mesh;
            helmet.AddComponent<MeshRenderer>().sharedMaterial = material;
            return helmet;
        }

        private static void CreatePlayerAmulet(Transform visualRoot, PsychoMirrorItem amulet, HostedMaterials materials)
        {
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Left Amulet Cord", new Vector3(-0.062f, 1.315f, -0.248f), new Vector3(0.018f, 0.185f, 0.014f), materials.PlayerLeather, amulet).transform.localRotation = Quaternion.Euler(0f, 0f, -23f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Right Amulet Cord", new Vector3(0.062f, 1.315f, -0.248f), new Vector3(0.018f, 0.185f, 0.014f), materials.PlayerLeather, amulet).transform.localRotation = Quaternion.Euler(0f, 0f, 23f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cylinder, "Amulet Medallion", new Vector3(0f, 1.205f, -0.258f), new Vector3(0.055f, 0.013f, 0.055f), materials.PlayerMetal, amulet).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, "Amulet Center Stone", new Vector3(0f, 1.205f, -0.273f), new Vector3(0.027f, 0.027f, 0.012f), materials.LandmarkBanner, amulet);
        }

        private static void CreatePlayerArmorStrap(Transform visualRoot, string name, Vector3 localPosition, float zRotation, Material material)
        {
            GameObject strap = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, name, localPosition, new Vector3(0.038f, 0.37f, 0.020f), material);
            strap.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        }

        private static void CreateHighlandFurMantle(Transform visualRoot, HostedMaterials materials)
        {
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Layered Leather Gorget", new Vector3(0f, 1.31f, -0.135f), new Vector3(0.39f, 0.050f, 0.070f), materials.PlayerLeather);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Left Collar Strap", new Vector3(-0.16f, 1.28f, -0.220f), new Vector3(0.105f, 0.030f, 0.026f), materials.PlayerHair).transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Right Collar Strap", new Vector3(0.16f, 1.28f, -0.220f), new Vector3(0.105f, 0.030f, 0.026f), materials.PlayerHair).transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, "Left Weathered Collar Pad", new Vector3(-0.285f, 1.31f, 0.005f), new Vector3(0.090f, 0.038f, 0.082f), materials.PlayerHair);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, "Right Weathered Collar Pad", new Vector3(0.285f, 1.31f, 0.005f), new Vector3(0.090f, 0.038f, 0.082f), materials.PlayerHair);
        }

        private static void CreateHighlandArm(Transform visualRoot, bool left, Material sleeveMaterial, Material armorMaterial, Material gloveMaterial, Material palmMaterial, HostedMaterials materials, PsychoMirrorItem body, PsychoMirrorItem hands)
        {
            float side = left ? -1f : 1f;
            GameObject upperArm = CreatePlayerPrimitive(visualRoot, PrimitiveType.Capsule, left ? "Left Upper Arm" : "Right Upper Arm", new Vector3(side * 0.37f, 1.05f, 0.02f), new Vector3(0.066f, 0.205f, 0.066f), sleeveMaterial);
            upperArm.transform.localRotation = Quaternion.Euler(0f, 0f, side * 7f);
            GameObject forearm = CreatePlayerPrimitive(visualRoot, PrimitiveType.Capsule, left ? "Left Leather Forearm" : "Right Leather Forearm", new Vector3(side * 0.405f, 0.74f, 0.035f), new Vector3(0.062f, 0.19f, 0.062f), gloveMaterial, hands);
            forearm.transform.localRotation = Quaternion.Euler(0f, 0f, side * 4f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, left ? "Left Steel Pauldron" : "Right Steel Pauldron", new Vector3(side * 0.325f, 1.30f, -0.01f), new Vector3(0.145f, 0.085f, 0.135f), armorMaterial, body).transform.localRotation = Quaternion.Euler(0f, 0f, side * 10f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, left ? "Left Bracer Plate" : "Right Bracer Plate", new Vector3(side * 0.425f, 0.78f, -0.055f), new Vector3(0.115f, 0.150f, 0.040f), materials.PlayerMetal, hands).transform.localRotation = Quaternion.Euler(0f, 0f, side * 4f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, left ? "Left Hand" : "Right Hand", new Vector3(side * 0.425f, 0.555f, 0.04f), new Vector3(0.078f, 0.078f, 0.078f), palmMaterial, hands);
        }

        private static void CreateHighlandLeg(Transform visualRoot, bool left, Material trouserMaterial, Material legMaterial, Material bootMaterial, HostedMaterials materials, PsychoMirrorItem legs, PsychoMirrorItem feet, bool armoredLegs)
        {
            float side = left ? -1f : 1f;
            GameObject thigh = CreatePlayerPrimitive(visualRoot, PrimitiveType.Capsule, left ? "Left Mail Thigh" : "Right Mail Thigh", new Vector3(side * 0.14f, 0.55f, 0.02f), new Vector3(0.09f, 0.24f, 0.095f), trouserMaterial);
            thigh.transform.localRotation = Quaternion.Euler(0f, 0f, side * 1.5f);
            GameObject shin = CreatePlayerPrimitive(visualRoot, PrimitiveType.Capsule, left ? "Left Greave" : "Right Greave", new Vector3(side * 0.14f, 0.30f, 0.03f), new Vector3(0.08f, 0.18f, 0.085f), armoredLegs ? legMaterial : materials.PlayerLeather, legs);
            shin.transform.localRotation = Quaternion.Euler(0f, 0f, side * 1.0f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, left ? "Left Knee Plate" : "Right Knee Plate", new Vector3(side * 0.14f, 0.45f, -0.085f), new Vector3(0.16f, 0.08f, 0.04f), materials.PlayerMetal, legs);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, left ? "Left Rugged Boot" : "Right Rugged Boot", new Vector3(side * 0.15f, 0.10f, 0.055f), new Vector3(0.16f, 0.105f, 0.255f), bootMaterial, feet);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, left ? "Left Boot Toe" : "Right Boot Toe", new Vector3(side * 0.15f, 0.055f, -0.075f), new Vector3(0.15f, 0.060f, 0.105f), bootMaterial, feet);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, left ? "Left Boot Cuff" : "Right Boot Cuff", new Vector3(side * 0.15f, 0.20f, 0.015f), new Vector3(0.16f, 0.072f, 0.165f), materials.PlayerHair, feet);
        }

        private static void CreateHighlandHead(Transform visualRoot, HostedMaterials materials, PsychoMirrorItem head)
        {
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, "Weathered Head", new Vector3(0f, 1.61f, -0.01f), new Vector3(0.215f, 0.255f, 0.198f), materials.PlayerSkin);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Sphere, "Jawline Beard", new Vector3(0f, 1.50f, -0.190f), new Vector3(0.095f, 0.048f, 0.028f), materials.PlayerHair);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Nose Bridge", new Vector3(0f, 1.615f, -0.214f), new Vector3(0.034f, 0.058f, 0.047f), materials.PlayerSkin);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Brow Shadow", new Vector3(0f, 1.68f, -0.207f), new Vector3(0.145f, 0.020f, 0.014f), materials.PlayerHair);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Left Eye", new Vector3(-0.056f, 1.655f, -0.216f), new Vector3(0.020f, 0.012f, 0.011f), materials.PlayerHair);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Right Eye", new Vector3(0.056f, 1.655f, -0.216f), new Vector3(0.020f, 0.012f, 0.011f), materials.PlayerHair);

            Material helmetMaterial = head == null ? materials.PlayerMetal : MaterialForEquippedItem(head, materials, materials.PlayerMetal);
            CreatePlayerHelmetDome(visualRoot, helmetMaterial, head);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Helmet Brow Plate", new Vector3(0f, 1.675f, -0.205f), new Vector3(0.205f, 0.030f, 0.022f), helmetMaterial, head);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Helmet Nasal Guard", new Vector3(0f, 1.605f, -0.223f), new Vector3(0.017f, 0.098f, 0.014f), helmetMaterial, head);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Left Cheek Guard", new Vector3(-0.112f, 1.59f, -0.150f), new Vector3(0.020f, 0.066f, 0.016f), helmetMaterial, head).transform.localRotation = Quaternion.Euler(0f, 10f, 0f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Right Cheek Guard", new Vector3(0.112f, 1.59f, -0.150f), new Vector3(0.020f, 0.066f, 0.016f), helmetMaterial, head).transform.localRotation = Quaternion.Euler(0f, -10f, 0f);
        }

        private static void CreateCapePanel(Transform visualRoot, Material material, HostedMaterials materials, PsychoMirrorItem cape)
        {
            Mesh mesh = new Mesh { name = "Hosted Player Tapered Cape Mesh" };
            Vector3[] vertices =
            {
                new Vector3(-0.34f, 1.33f, 0.23f),
                new Vector3(0.34f, 1.33f, 0.23f),
                new Vector3(-0.27f, 0.72f, 0.35f),
                new Vector3(0.27f, 0.72f, 0.35f),
                new Vector3(-0.17f, 0.10f, 0.28f),
                new Vector3(0.17f, 0.10f, 0.28f),
                new Vector3(-0.34f, 1.33f, 0.23f),
                new Vector3(0.34f, 1.33f, 0.23f),
                new Vector3(-0.27f, 0.72f, 0.35f),
                new Vector3(0.27f, 0.72f, 0.35f),
                new Vector3(-0.17f, 0.10f, 0.28f),
                new Vector3(0.17f, 0.10f, 0.28f)
            };

            int[] triangles =
            {
                0, 2, 1,
                1, 2, 3,
                2, 4, 3,
                3, 4, 5,
                7, 8, 6,
                9, 8, 7,
                9, 10, 8,
                11, 10, 9
            };

            Vector3[] normals =
            {
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.back,
                Vector3.back,
                Vector3.back,
                Vector3.back,
                Vector3.back,
                Vector3.back
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.RecalculateBounds();

            GameObject capeObject = new GameObject(cape == null || string.IsNullOrWhiteSpace(cape.name) ? "Cape" : $"Cape - {cape.name}");
            capeObject.transform.SetParent(visualRoot, false);
            capeObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            capeObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cylinder, "Left Cloak Clasp", new Vector3(-0.18f, 1.31f, -0.20f), new Vector3(0.045f, 0.015f, 0.045f), materials.PlayerMetal, cape).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cylinder, "Right Cloak Clasp", new Vector3(0.18f, 1.31f, -0.20f), new Vector3(0.045f, 0.015f, 0.045f), materials.PlayerMetal, cape).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cube, "Cloak Leather Cross Strap", new Vector3(0f, 1.28f, -0.225f), new Vector3(0.38f, 0.030f, 0.020f), materials.PlayerLeather, cape);
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

            GameObject shieldObject = CreatePlayerPrimitive(visualRoot, PrimitiveType.Cylinder, "Shield", new Vector3(-0.47f, 0.88f, -0.02f), new Vector3(0.24f, 0.035f, 0.34f), material, shield);
            shieldObject.transform.localRotation = Quaternion.Euler(90f, 0f, 4f);
            CreatePlayerPrimitive(visualRoot, PrimitiveType.Cylinder, "Shield Boss", new Vector3(-0.51f, 0.88f, -0.02f), new Vector3(0.09f, 0.025f, 0.09f), materials.PlayerMetal, shield).transform.localRotation = Quaternion.Euler(90f, 0f, 4f);
        }

        private static void CreatePlayerNameplate(Transform visualRoot, HostedPlayerSave playerSave, float localYaw = 0f)
        {
            GameObject plate = new GameObject("Player Nameplate");
            plate.transform.SetParent(visualRoot, false);
            plate.transform.localPosition = new Vector3(0f, 2.06f, 0f);
            plate.transform.localRotation = Quaternion.Euler(0f, localYaw, 0f);
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

        private static bool IsMetalEquipment(PsychoMirrorItem item)
        {
            string materialClass = item?.materialClass == null ? string.Empty : item.materialClass.ToLowerInvariant();
            string name = item?.name == null ? string.Empty : item.name.ToLowerInvariant();
            return materialClass.Contains("metal")
                || materialClass.Contains("rune")
                || materialClass.Contains("crystal")
                || name.Contains("plate")
                || name.Contains("helm")
                || name.Contains("chain")
                || name.Contains("mail")
                || name.Contains("shield")
                || name.Contains("sirenic");
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

                PopulateHostedPlayerHudValues(save, json);
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

        private static void PopulateHostedPlayerHudValues(HostedPlayerSave save, string json)
        {
            if (save == null || string.IsNullOrEmpty(json))
            {
                return;
            }

            save.moneyPouch = ReadLongProperty(json, "money-pouch", 0L);
            save.runEnergy = ReadIntProperty(json, "run-energy", 100);
            save.currentHealth = ReadSkillArrayValue(json, "level", 3, 100);
            save.maxHealth = ReadSkillArrayValue(json, "maxLevel", 3, save.currentHealth);
            save.currentPrayer = ReadSkillArrayValue(json, "level", 5, 1);
            save.maxPrayer = ReadSkillArrayValue(json, "maxLevel", 5, save.currentPrayer);
            PopulateHostedPlayerQuestValues(save, json);
        }

        private static void PopulateHostedPlayerQuestValues(HostedPlayerSave save, string json)
        {
            int rfdWavesCompleted = Mathf.Clamp(ReadIntProperty(json, "recipe-for-disaster-wave", 0), 0, 6);
            bool rfdStarted = ReadBooleanArrayValue(json, "recipe-for-disaster", 0, false);
            bool rfdComplete = rfdWavesCompleted >= 6 || ReadBooleanArrayValue(json, "recipe-for-disaster", 8, false);
            bool nomadStarted = ReadBooleanArrayValue(json, "nomad", 0, false);
            bool nomadComplete = ReadBooleanArrayValue(json, "nomad", 1, false);
            int completed = (rfdComplete ? 1 : 0) + (nomadComplete ? 1 : 0);

            save.recipeForDisasterStatus = rfdComplete
                ? "Recipe for Disaster: Complete"
                : rfdStarted
                    ? "Recipe for Disaster: In progress (" + rfdWavesCompleted + "/6 waves)"
                    : "Recipe for Disaster: Not started";
            save.nomadStatus = nomadComplete
                ? "Nomad's Requiem: Complete"
                : nomadStarted
                    ? "Nomad's Requiem: In progress"
                    : "Nomad's Requiem: Not started";
            save.questSummary = "Quest Progress: " + completed + "/2";
        }

        private static long ReadLongProperty(string json, string propertyName, long fallback)
        {
            string key = "\"" + propertyName + "\"";
            int keyIndex = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return fallback;
            }

            int colonIndex = json.IndexOf(':', keyIndex + key.Length);
            if (colonIndex < 0)
            {
                return fallback;
            }

            int valueStart = colonIndex + 1;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
            {
                valueStart++;
            }

            int valueEnd = valueStart;
            while (valueEnd < json.Length && (char.IsDigit(json[valueEnd]) || json[valueEnd] == '-'))
            {
                valueEnd++;
            }

            return long.TryParse(json.Substring(valueStart, valueEnd - valueStart), out long value) ? value : fallback;
        }

        private static int ReadIntProperty(string json, string propertyName, int fallback)
        {
            long value = ReadLongProperty(json, propertyName, fallback);
            if (value < int.MinValue || value > int.MaxValue)
            {
                return fallback;
            }

            return (int)value;
        }

        private static int ReadSkillArrayValue(string json, string arrayName, int skillIndex, int fallback)
        {
            int skillsIndex = json.IndexOf("\"skills\"", StringComparison.OrdinalIgnoreCase);
            if (skillsIndex < 0)
            {
                return fallback;
            }

            string key = "\"" + arrayName + "\"";
            int keyIndex = json.IndexOf(key, skillsIndex, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return fallback;
            }

            int openBracket = json.IndexOf('[', keyIndex + key.Length);
            int closeBracket = json.IndexOf(']', openBracket + 1);
            if (openBracket < 0 || closeBracket < 0 || closeBracket <= openBracket)
            {
                return fallback;
            }

            string[] values = json.Substring(openBracket + 1, closeBracket - openBracket - 1).Split(',');
            if (skillIndex < 0 || skillIndex >= values.Length)
            {
                return fallback;
            }

            return int.TryParse(values[skillIndex].Trim(), out int value) ? value : fallback;
        }

        private static bool ReadBooleanArrayValue(string json, string arrayName, int valueIndex, bool fallback)
        {
            string key = "\"" + arrayName + "\"";
            int keyIndex = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return fallback;
            }

            int openBracket = json.IndexOf('[', keyIndex + key.Length);
            int closeBracket = json.IndexOf(']', openBracket + 1);
            if (openBracket < 0 || closeBracket < 0 || closeBracket <= openBracket)
            {
                return fallback;
            }

            string[] values = json.Substring(openBracket + 1, closeBracket - openBracket - 1).Split(',');
            if (valueIndex < 0 || valueIndex >= values.Length)
            {
                return fallback;
            }

            string value = values[valueIndex].Trim();
            return bool.TryParse(value, out bool parsed) ? parsed : fallback;
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
                if (IsTerrainCollider(hitCollider))
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector3 TerrainSurfacePosition(Vector3 approximatePosition, float yOffset)
        {
            Physics.SyncTransforms();
            RaycastHit[] hits = Physics.RaycastAll(
                approximatePosition + Vector3.up * 128f,
                Vector3.down,
                256f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            float bestDistance = float.MaxValue;
            float surfaceY = approximatePosition.y;
            bool foundTerrain = false;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (!IsTerrainCollider(hit.collider))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    surfaceY = hit.point.y;
                    foundTerrain = true;
                }
            }

            if (!foundTerrain)
            {
                return approximatePosition + Vector3.up * yOffset;
            }

            return new Vector3(approximatePosition.x, surfaceY + yOffset, approximatePosition.z);
        }

        private static bool IsTerrainCollider(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return false;
            }

            if (hitCollider.gameObject.name.Contains("Terrain"))
            {
                return true;
            }

            MeshCollider meshCollider = hitCollider as MeshCollider;
            return meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.name.StartsWith("psycho_map_region_");
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

        private static GameObject CreateGroundCoverPatch(string name, Vector3 position, float width, float depth, Material material, int seed)
        {
            const int segments = 18;
            Mesh mesh = new Mesh { name = name + " Mesh" };
            Vector3[] vertices = new Vector3[segments + 1];
            Vector2[] uv = new Vector2[segments + 1];
            int[] triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float wobble = 0.68f + Deterministic01(seed + i * 31) * 0.46f;
                float px = Mathf.Cos(angle) * width * 0.5f * wobble;
                float pz = Mathf.Sin(angle) * depth * 0.5f * (0.82f + Deterministic01(seed + i * 37) * 0.32f);
                vertices[i + 1] = new Vector3(px, Mathf.Sin(angle * 3.0f + seed * 0.013f) * 0.012f, pz);
                uv[i + 1] = new Vector2(0.5f + Mathf.Cos(angle) * 0.5f * wobble, 0.5f + Mathf.Sin(angle) * 0.5f * wobble);
            }

            int triangle = 0;
            for (int i = 0; i < segments; i++)
            {
                triangles[triangle++] = 0;
                triangles[triangle++] = i + 1;
                triangles[triangle++] = i == segments - 1 ? 1 : i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            GameObject patch = new GameObject(name);
            patch.isStatic = true;
            patch.transform.position = position;
            patch.transform.rotation = Quaternion.Euler(0f, Deterministic01(seed + 41) * 360f, 0f);
            patch.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = patch.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            return patch;
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
            float width = height * 0.22f;
            const int bladeCount = 4;
            Vector3[] vertices = new Vector3[bladeCount * 4];
            int[] triangles = new int[bladeCount * 12];
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

                int t = blade * 12;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + 2;
                triangles[t + 5] = v + 3;
                triangles[t + 6] = v;
                triangles[t + 7] = v + 1;
                triangles[t + 8] = v + 2;
                triangles[t + 9] = v + 1;
                triangles[t + 10] = v + 3;
                triangles[t + 11] = v + 2;
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
            MeshRenderer renderer = clump.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
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
                Grass = LoadOrCreateTexturedMaterial(GrassMaterialPath, "Grass", new Color(0.20f, 0.34f, 0.18f, 1f), 0.18f, new Vector2(11.0f, 11.0f), 0.86f),
                Flowers = LoadOrCreateTexturedMaterial(FlowerMaterialPath, "Organic", new Color(0.76f, 0.63f, 0.34f, 1f), 0.24f, new Vector2(6.4f, 6.4f), 0.62f),
                Reeds = LoadOrCreateTexturedMaterial(ReedMaterialPath, "Leaf", new Color(0.26f, 0.37f, 0.20f, 1f), 0.16f, new Vector2(4.2f, 6.6f), 0.82f),
                Water = LoadOrCreateTexturedMaterial(WaterMaterialPath, "Water", new Color(0.035f, 0.18f, 0.27f, 0.62f), 0.92f, new Vector2(2.8f, 7.8f), 1.08f),
                Hills = LoadOrCreateTexturedMaterial(HillMaterialPath, "Grass", new Color(0.18f, 0.28f, 0.18f, 1f), 0.22f, new Vector2(7.6f, 7.6f), 0.78f),
                Mountains = LoadOrCreateTexturedMaterial(MountainMaterialPath, "Mountain", new Color(0.39f, 0.40f, 0.39f, 1f), 0.42f, new Vector2(3.8f, 3.8f), 0.98f),
                Cloud = LoadOrCreateSolidMaterial(CloudMaterialPath, new Color(0.68f, 0.76f, 0.82f, 0.18f), 0.12f),
                HorizonMist = LoadOrCreateSolidMaterial(HorizonMistMaterialPath, new Color(0.54f, 0.62f, 0.70f, 0.085f), 0.08f),
                TreeCanopy = LoadOrCreateTexturedMaterial(TreeCanopyMaterialPath, "Leaf", new Color(0.13f, 0.28f, 0.17f, 1f), 0.18f, new Vector2(4.8f, 4.8f), 0.82f),
                TreeBark = LoadOrCreateTexturedMaterial(TreeBarkMaterialPath, "Wood", new Color(0.20f, 0.15f, 0.11f, 1f), 0.18f, new Vector2(3.2f, 5.6f), 0.92f),
                FrostStone = LoadOrCreateTexturedMaterial(FrostStoneMaterialPath, "Stone", new Color(0.48f, 0.50f, 0.49f, 1f), 0.36f, new Vector2(3.6f, 3.6f), 1.02f),
                Moss = LoadOrCreateTexturedMaterial(MossMaterialPath, "Organic", new Color(0.22f, 0.30f, 0.17f, 1f), 0.18f, new Vector2(7.2f, 7.2f), 0.72f),
                CliffFace = LoadOrCreateTexturedMaterial(CliffFaceMaterialPath, "Stone", new Color(0.36f, 0.31f, 0.25f, 1f), 0.44f, new Vector2(4.6f, 3.2f), 1.18f),
                WaterFoam = LoadOrCreateSolidMaterial(WaterFoamMaterialPath, new Color(0.76f, 0.88f, 0.90f, 0.30f), 0.30f),
                WaterDepth = LoadOrCreateSolidMaterial(WaterDepthMaterialPath, new Color(0.01f, 0.07f, 0.12f, 0.36f), 0.60f),
                PlayerCloth = LoadOrCreateTexturedMaterial(PlayerClothMaterialPath, "Cloth", new Color(0.28f, 0.42f, 0.56f, 1f), 0.34f, new Vector2(2.5f, 2.5f), 0.52f),
                PlayerLeather = LoadOrCreateTexturedMaterial(PlayerLeatherMaterialPath, "Leather", new Color(0.52f, 0.34f, 0.22f, 1f), 0.30f, new Vector2(2.2f, 2.2f), 0.48f),
                PlayerMetal = LoadOrCreateTexturedMaterial(PlayerMetalMaterialPath, "Metal", new Color(0.70f, 0.70f, 0.66f, 1f), 0.62f, new Vector2(2.2f, 2.2f), 0.42f),
                PlayerSkin = LoadOrCreateSolidMaterial(PlayerSkinMaterialPath, new Color(0.77f, 0.57f, 0.42f, 1f), 0.28f),
                PlayerHair = LoadOrCreateSolidMaterial(PlayerHairMaterialPath, new Color(0.23f, 0.16f, 0.09f, 1f), 0.32f),
                PlayerPaper = LoadOrCreateTexturedMaterial(PlayerPaperMaterialPath, "Paper", new Color(0.74f, 0.68f, 0.54f, 1f), 0.24f, new Vector2(1.8f, 1.8f), 0.36f),
                PlayerWood = LoadOrCreateTexturedMaterial(PlayerWoodMaterialPath, "Wood", new Color(0.38f, 0.24f, 0.12f, 1f), 0.28f, new Vector2(2.2f, 2.2f), 0.50f),
                LandmarkStone = LoadOrCreateTexturedMaterial(LandmarkStoneMaterialPath, "Stone", new Color(0.40f, 0.40f, 0.37f, 1f), 0.30f, new Vector2(3.8f, 3.8f), 0.86f),
                LandmarkRoof = LoadOrCreateTexturedMaterial(LandmarkRoofMaterialPath, "Wood", new Color(0.13f, 0.12f, 0.11f, 1f), 0.34f, new Vector2(3.0f, 3.0f), 0.82f),
                LandmarkRoad = LoadOrCreateTexturedMaterial(LandmarkRoadMaterialPath, "Stone", new Color(0.34f, 0.33f, 0.29f, 1f), 0.24f, new Vector2(5.8f, 5.8f), 0.72f),
                LandmarkBanner = LoadOrCreateTexturedMaterial(LandmarkBannerMaterialPath, "Cloth", new Color(0.27f, 0.42f, 0.63f, 1f), 0.32f, new Vector2(1.6f, 1.6f), 0.44f),
                LandmarkGlass = LoadOrCreateSolidMaterial(LandmarkGlassMaterialPath, new Color(0.40f, 0.63f, 0.72f, 0.46f), 0.72f)
            };
            ConfigureTransparent(materials.Water);
            ConfigureTransparent(materials.Cloud);
            ConfigureTransparent(materials.HorizonMist);
            ConfigureTransparent(materials.WaterFoam);
            ConfigureTransparent(materials.WaterDepth);
            ConfigureTransparent(materials.LandmarkGlass);
            ConfigureDoubleSided(materials.CliffFace);
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

            skybox.SetColor("_SkyTint", new Color(0.36f, 0.50f, 0.63f));
            skybox.SetColor("_GroundColor", new Color(0.24f, 0.28f, 0.26f));
            skybox.SetFloat("_AtmosphereThickness", 0.86f);
            skybox.SetFloat("_Exposure", 1.04f);
            skybox.SetFloat("_SunSize", 0.028f);
            skybox.SetFloat("_SunSizeConvergence", 6.2f);
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

        private static void ConfigureDoubleSided(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.SetInt("_Cull", (int)CullMode.Off);
            EditorUtility.SetDirty(material);
        }

        private static void AddWind(GameObject target, float amplitude, float speed, float gustStrength, float spatialFrequency = 1.4f, float turbulence = 0.045f)
        {
            if (windAnimatedComponentsAdded >= MaxWindAnimatedComponents)
            {
                return;
            }

            WindAnimatedFoliage wind = target.AddComponent<WindAnimatedFoliage>();
            SerializedObject windObject = new SerializedObject(wind);
            SetSerializedFloat(windObject, "amplitude", amplitude);
            SetSerializedFloat(windObject, "speed", speed);
            SetSerializedFloat(windObject, "gustStrength", gustStrength);
            SetSerializedFloat(windObject, "spatialFrequency", spatialFrequency);
            SetSerializedFloat(windObject, "turbulence", turbulence);
            windObject.ApplyModifiedPropertiesWithoutUndo();
            windAnimatedComponentsAdded++;
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

        private static void SetSerializedInt(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetSerializedLong(SerializedObject serializedObject, string propertyName, long value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.longValue = value;
            }
        }

        private static void SetSerializedString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value ?? string.Empty;
            }
        }

        private static void SetSerializedObject(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
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
            public Material HorizonMist;
            public Material TreeCanopy;
            public Material TreeBark;
            public Material FrostStone;
            public Material Moss;
            public Material CliffFace;
            public Material WaterFoam;
            public Material WaterDepth;
            public Material PlayerCloth;
            public Material PlayerLeather;
            public Material PlayerMetal;
            public Material PlayerSkin;
            public Material PlayerHair;
            public Material PlayerPaper;
            public Material PlayerWood;
            public Material LandmarkStone;
            public Material LandmarkRoof;
            public Material LandmarkRoad;
            public Material LandmarkBanner;
            public Material LandmarkGlass;
        }

        [Serializable]
        private sealed class HostedPlayerSave
        {
            public string username;
            public string gender;
            public HostedSavePosition position;
            public int[] appearance;
            public HostedSaveItem[] equipment;
            [NonSerialized] public long moneyPouch;
            [NonSerialized] public int runEnergy = 100;
            [NonSerialized] public int currentHealth = 100;
            [NonSerialized] public int maxHealth = 100;
            [NonSerialized] public int currentPrayer = 1;
            [NonSerialized] public int maxPrayer = 1;
            [NonSerialized] public string recipeForDisasterStatus = "Recipe for Disaster: Not started";
            [NonSerialized] public string nomadStatus = "Nomad's Requiem: Not started";
            [NonSerialized] public string questSummary = "Quest Progress: 0/2";
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

        private struct VertexNormalKey : IEquatable<VertexNormalKey>
        {
            private readonly int x;
            private readonly int y;
            private readonly int z;

            public VertexNormalKey(Vector3 position, float tolerance)
            {
                float safeTolerance = Mathf.Max(0.00001f, tolerance);
                x = Mathf.RoundToInt(position.x / safeTolerance);
                y = Mathf.RoundToInt(position.y / safeTolerance);
                z = Mathf.RoundToInt(position.z / safeTolerance);
            }

            public bool Equals(VertexNormalKey other)
            {
                return x == other.x && y == other.y && z == other.z;
            }

            public override bool Equals(object obj)
            {
                return obj is VertexNormalKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + x;
                    hash = hash * 31 + y;
                    hash = hash * 31 + z;
                    return hash;
                }
            }
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
            public int smoothedCharacterMeshes;
            public int visualReplacementNpcs;
            public int visualReplacementObjects;
            public int decodedObjectAccents;
            public int terrainCollisionSamples;
            public int terrainCollisionMisses;
            public int groundCoverPatches;
            public int enhancedFoliageObjects;
            public int cliffDressingObjects;
            public int waterFoamEdges;
            public int waterDepthChannels;
            public int horizonMistPanels;
            public int landmarkDressingObjects;
        }
    }
}
