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
        private const int HostedVillageNpcBaseId = 900000;
        private const int HostedWildlifeNpcBaseId = 910000;
        private const int HostedGiantNpcBaseId = 930000;
        private const int HostedMammothNpcBaseId = 940000;
        private const bool UseFlatHostedVisualBase = true;
        private const bool PlaceCacheObjectsOnFlatBase = false;
        private const bool PlaceCacheNpcsOnFlatBase = false;
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
        private const string HerbMaterialPath = GeneratedRoot + "/Psycho_Hosted_Herbs.mat";
        private const string WildflowerBlueMaterialPath = GeneratedRoot + "/Psycho_Hosted_Wildflower_Blue.mat";
        private const string WildflowerPurpleMaterialPath = GeneratedRoot + "/Psycho_Hosted_Wildflower_Purple.mat";
        private const string WildflowerGoldMaterialPath = GeneratedRoot + "/Psycho_Hosted_Wildflower_Gold.mat";
        private const string ReedMaterialPath = GeneratedRoot + "/Psycho_Hosted_Reeds.mat";
        private const string WaterMaterialPath = GeneratedRoot + "/Psycho_Hosted_Water.mat";
        private const string HillMaterialPath = GeneratedRoot + "/Psycho_Hosted_Hills.mat";
        private const string MountainMaterialPath = GeneratedRoot + "/Psycho_Hosted_Mountains.mat";
        private const string SnowMaterialPath = GeneratedRoot + "/Psycho_Hosted_Snow.mat";
        private const string AutumnCanopyMaterialPath = GeneratedRoot + "/Psycho_Hosted_Autumn_Canopy.mat";
        private const string WinterCanopyMaterialPath = GeneratedRoot + "/Psycho_Hosted_Winter_Canopy.mat";
        private const string AutumnGroundMaterialPath = GeneratedRoot + "/Psycho_Hosted_Autumn_Ground.mat";
        private const string CloudMaterialPath = GeneratedRoot + "/Psycho_Hosted_Clouds.mat";
        private const string HorizonMistMaterialPath = GeneratedRoot + "/Psycho_Hosted_Horizon_Mist.mat";
        private const string TreeCanopyMaterialPath = GeneratedRoot + "/Psycho_Hosted_Tree_Canopy.mat";
        private const string TreeCanopyFarMaterialPath = GeneratedRoot + "/Psycho_Hosted_Tree_Canopy_Far.mat";
        private const string TreeBarkMaterialPath = GeneratedRoot + "/Psycho_Hosted_Tree_Bark.mat";
        private const string TreeBarkFarMaterialPath = GeneratedRoot + "/Psycho_Hosted_Tree_Bark_Far.mat";
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
        private const string WildlifeHideMaterialPath = GeneratedRoot + "/Psycho_Hosted_Wildlife_Hide.mat";
        private const string MammothFurMaterialPath = GeneratedRoot + "/Psycho_Hosted_Mammoth_Fur.mat";
        private const string MammothTuskMaterialPath = GeneratedRoot + "/Psycho_Hosted_Mammoth_Tusk.mat";
        private const string GiantSkinMaterialPath = GeneratedRoot + "/Psycho_Hosted_Giant_Skin.mat";
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
            Debug.Log($"Hosted test world built: {ScenePath}. Regions {context.Report.loadedRegions}, flat terrain regions {context.Report.flatBaseTerrainRegions}, objects {context.Report.placedObjects}, clean-base skipped cache objects {context.Report.cleanBaseSkippedCacheObjects}, NPCs {context.Report.npcSpawns}, clean-base skipped cache NPCs {context.Report.cleanBaseSkippedCacheNpcs}, cache NPC visuals {context.Report.cacheNpcVisuals}, visual NPC replacements {context.Report.visualReplacementNpcs}, visual object replacements {context.Report.visualReplacementObjects}, decoded object accents {context.Report.decodedObjectAccents}, smoothed character meshes {context.Report.smoothedCharacterMeshes}, settlements {context.Report.hostedSettlements}, villages {context.Report.hostedVillages}, towns {context.Report.hostedTowns}, cities {context.Report.hostedCities}, settlement NPCs {context.Report.hostedSettlementNpcs}, village-cleared trees {context.Report.villageClearedObjects}, intro quest dressing {context.Report.introQuestDressingObjects}, biome regions {context.Report.seasonalBiomeRegions}, mountains {context.Report.mountainMassifs}, hills {context.Report.hillMounds}, dense forest trees {context.Report.denseForestTrees}, snow patches {context.Report.snowPatches}, seasonal dressing {context.Report.seasonalDressingObjects}, lush meadow patches {context.Report.lushMeadowPatches}, wildflower clusters {context.Report.wildflowerClusters}, herb clusters {context.Report.herbClusters}, wildlife NPCs {context.Report.wildlifeNpcs}, giant camps {context.Report.giantCamps}, giants {context.Report.giants}, mammoth companions {context.Report.mammothCompanions}, cliffs {context.Report.cliffDressingObjects}, ground cover {context.Report.groundCoverPatches}, foliage silhouettes {context.Report.enhancedFoliageObjects}, foliage LOD proxies {context.Report.foliageLodProxies}, foam edges {context.Report.waterFoamEdges}, water streaks {context.Report.waterSurfaceStreaks}.");
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
            GameObject npcRoot = GameObject.Find("Hosted Settlement Ambient NPCs") ?? GameObject.Find("Hosted NPC Spawns");
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

        [MenuItem("Psycho/Render Hosted Village Preview")]
        public static void RenderHostedVillagePreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject village = GameObject.Find("Varrock Green City")
                ?? GameObject.Find("Central Market City")
                ?? GameObject.Find("River Lum Town")
                ?? GameObject.Find("North Edgeville Farmstead");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (village == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both a generated village and camera.");
            }

            Bounds bounds = BuildVillagePreviewBounds(village);
            Vector3 focus = bounds.center + Vector3.up * 0.55f;
            float viewSize = Mathf.Max(bounds.size.x, bounds.size.z, bounds.size.y * 2.2f);
            float cameraDistance = Mathf.Clamp(viewSize * 0.78f, 9.5f, 17.5f);
            camera.transform.position = focus + new Vector3(0.72f, 0.36f, -0.78f).normalized * cameraDistance;
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 38f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-village-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedVillagePreviewBatch()
        {
            RenderHostedVillagePreview();
        }

        [MenuItem("Psycho/Render Hosted Biome Preview")]
        public static void RenderHostedBiomePreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject biome = GameObject.Find("Northern Frostspine Mountains")
                ?? GameObject.Find("Northeast Snowwood")
                ?? GameObject.Find("Western Autumnwood");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (biome == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both a generated biome region and camera.");
            }

            Bounds bounds = BuildObjectPreviewBounds(biome.transform);
            Vector3 focus = bounds.center + Vector3.up * 3.4f;
            float viewSize = Mathf.Max(bounds.size.x, bounds.size.z, bounds.size.y * 2.6f);
            float cameraDistance = Mathf.Clamp(viewSize * 0.96f, 30f, 88f);
            camera.transform.position = focus + new Vector3(0.76f, 0.42f, -0.64f).normalized * cameraDistance;
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 42f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-biome-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedBiomePreviewBatch()
        {
            RenderHostedBiomePreview();
        }

        [MenuItem("Psycho/Render Hosted Seasonal Biome Preview")]
        public static void RenderHostedSeasonalBiomePreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject biome = GameObject.Find("Western Autumnwood")
                ?? GameObject.Find("Southern Amber Hills")
                ?? GameObject.Find("Falador Rolling Meadows");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (biome == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both a generated seasonal biome region and camera.");
            }

            Bounds bounds = BuildObjectPreviewBounds(biome.transform);
            Vector3 focus = bounds.center + Vector3.up * 2.2f;
            float viewSize = Mathf.Max(bounds.size.x, bounds.size.z, bounds.size.y * 2.4f);
            float cameraDistance = Mathf.Clamp(viewSize * 0.82f, 24f, 72f);
            camera.transform.position = focus + new Vector3(-0.72f, 0.34f, -0.70f).normalized * cameraDistance;
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 40f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-seasonal-biome-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedSeasonalBiomePreviewBatch()
        {
            RenderHostedSeasonalBiomePreview();
        }

        [MenuItem("Psycho/Render Hosted Lush Meadow Preview")]
        public static void RenderHostedLushMeadowPreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject meadow = GameObject.Find("Open Vale Wildflower Preserve")
                ?? GameObject.Find("Highland Lake Alpine Flower Shelf")
                ?? GameObject.Find("Falador Lush Wildflower Field")
                ?? GameObject.Find("Western Autumn Herb Glade")
                ?? GameObject.Find("Draynor Herb Meadow")
                ?? GameObject.Find("River Lum Wildflower Bank")
                ?? GameObject.Find("Edgeville Player Start Lush Field");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (meadow == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both a lush meadow field and camera.");
            }

            Vector3 focus = meadow.transform.position + new Vector3(0.35f, 0.42f, -0.20f);
            camera.transform.position = focus + new Vector3(9.4f, 2.65f, -10.8f);
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 28f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-lush-meadow-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedLushMeadowPreviewBatch()
        {
            RenderHostedLushMeadowPreview();
        }

        [MenuItem("Psycho/Render Hosted Prison Intro Preview")]
        public static void RenderHostedPrisonIntroPreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject prison = GameObject.Find("North Edgeville Prison Yard");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (prison == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both the prison intro set and camera.");
            }

            Bounds bounds = BuildObjectPreviewBounds(prison.transform);
            Vector3 focus = bounds.center + Vector3.up * 0.92f;
            camera.transform.position = focus + new Vector3(8.4f, 4.2f, -9.6f);
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 38f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-prison-intro-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedPrisonIntroPreviewBatch()
        {
            RenderHostedPrisonIntroPreview();
        }

        [MenuItem("Psycho/Render Hosted Giant Mammoth Preview")]
        public static void RenderHostedGiantMammothPreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildHostedTestWorldScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject camp = GameObject.Find("Frostpine Giant Camp")
                ?? GameObject.Find("Autumnreach Giant Camp")
                ?? GameObject.Find("Highland Lake Giant Camp");
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camp == null || camera == null)
            {
                throw new InvalidOperationException("Hosted test world scene needs both a giant camp and camera.");
            }

            Bounds bounds = BuildObjectPreviewBounds(camp.transform);
            Vector3 focus = bounds.center + Vector3.up * 1.75f;
            float viewSize = Mathf.Max(bounds.size.x, bounds.size.z, bounds.size.y * 1.8f);
            float cameraDistance = Mathf.Clamp(viewSize * 0.64f, 13f, 34f);
            camera.transform.position = focus + new Vector3(-0.58f, 0.30f, -0.76f).normalized * cameraDistance;
            camera.transform.rotation = Quaternion.LookRotation(focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = 36f;
            camera.farClipPlane = 2400f;
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-hosted-giant-mammoth-preview.png"));
            RenderCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderHostedGiantMammothPreviewBatch()
        {
            RenderHostedGiantMammothPreview();
        }

        private static Bounds BuildVillagePreviewBounds(GameObject village)
        {
            Bounds bounds = BuildObjectPreviewBounds(village.transform);
            GameObject npcRoot = GameObject.Find("Hosted Settlement Ambient NPCs") ?? GameObject.Find("Hosted Village Ambient NPCs");
            if (npcRoot != null)
            {
                string villageMarker = "(" + village.name + ")";
                for (int i = 0; i < npcRoot.transform.childCount; i++)
                {
                    Transform child = npcRoot.transform.GetChild(i);
                    if (child.name.IndexOf(villageMarker, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    Renderer[] renderers = child.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            bounds.Expand(1.3f);
            return bounds;
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
            context.Report.cleanFlatBase = UseFlatHostedVisualBase;
            GameObject terrainRoot = new GameObject(UseFlatHostedVisualBase ? "Hosted Flat Green Terrain Regions" : "Hosted Terrain Regions");
            GameObject objectRoot = new GameObject(UseFlatHostedVisualBase ? "Hosted Cache Objects (Disabled For Clean Base)" : "Hosted Cache Objects");

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

                BuildTerrain(terrainRoot.transform, landscape, UseFlatHostedVisualBase ? hostedMaterials.Grass : terrainMaterial, context);
                if (!UseFlatHostedVisualBase || PlaceCacheObjectsOnFlatBase)
                {
                    BuildObjects(store, database, objectRoot.transform, landscape, objects, objectMaterial, hostedMaterials, context);
                }
                else
                {
                    context.Report.cleanBaseSkippedCacheObjects += objects.Placements.Count;
                }
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

        private static void BuildTerrain(Transform root, PsychoMapLandscape landscape, Material material, HostedBuildContext context)
        {
            Mesh terrainMesh = UseFlatHostedVisualBase
                ? BuildFlatHostedTerrainMesh(landscape)
                : PsychoMapMeshBuilder.BuildTerrainMesh(landscape, 0, TileScale, HeightScale);
            GameObject terrain = new GameObject($"Terrain Region {(landscape.RegionX << 8) + landscape.RegionY}");
            terrain.isStatic = true;
            terrain.transform.SetParent(root, false);
            terrain.transform.position = RegionOrigin(landscape.RegionX, landscape.RegionY);
            terrain.AddComponent<MeshFilter>().sharedMesh = terrainMesh;
            terrain.AddComponent<MeshRenderer>().sharedMaterial = material;
            terrain.AddComponent<MeshCollider>().sharedMesh = terrainMesh;
            if (UseFlatHostedVisualBase)
            {
                context.Report.flatBaseTerrainRegions++;
            }
        }

        private static Mesh BuildFlatHostedTerrainMesh(PsychoMapLandscape landscape)
        {
            const int segments = 64;
            int stride = segments + 1;
            Vector3[] vertices = new Vector3[stride * stride];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[segments * segments * 6];

            int vertex = 0;
            for (int y = 0; y <= segments; y++)
            {
                for (int x = 0; x <= segments; x++)
                {
                    vertices[vertex] = new Vector3(x * TileScale, 0f, y * TileScale);
                    uv[vertex] = new Vector2(x / (float)segments * 16f, y / (float)segments * 16f);
                    vertex++;
                }
            }

            int tri = 0;
            for (int y = 0; y < segments; y++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int i = y * stride + x;
                    triangles[tri++] = i;
                    triangles[tri++] = i + stride;
                    triangles[tri++] = i + 1;
                    triangles[tri++] = i + 1;
                    triangles[tri++] = i + stride;
                    triangles[tri++] = i + stride + 1;
                }
            }

            Mesh mesh = new Mesh { name = $"psycho_flat_base_region_{(landscape.RegionX << 8) + landscape.RegionY}" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
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
                int worldX = landscape.RegionX * 64 + placement.LocalX;
                int worldY = landscape.RegionY * 64 + placement.LocalY;
                if (hasDefinition && IsTreeLikeObject(definition) && IsHostedVillageClearingTile(worldX, worldY))
                {
                    context.Report.skippedObjects++;
                    context.Report.villageClearedObjects++;
                    continue;
                }

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
                        context.Report.foliageLodProxies++;
                        addedWind = true;
                    }
                }
                else if (!usedArtReplacement && windResponsive && ShouldAddFoliageSilhouette(placed.transform, definition))
                {
                    AddFoliageSilhouette(placed.transform, definition, placement, hostedMaterials);
                    context.Report.enhancedFoliageObjects++;
                    context.Report.foliageLodProxies++;
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
            float trunkRadius = 0.16f + footprint * 0.030f;
            float canopyWidth = 1.58f + footprint * 0.42f;
            float canopyHeight = 1.18f + footprint * 0.20f;
            float seed = placement.ObjectId * 0.071f + placement.LocalX * 0.19f + placement.LocalY * 0.13f;
            string name = definition.name == null ? string.Empty : definition.name.ToLowerInvariant();
            bool broadleaf = name.Contains("oak") || name.Contains("willow") || name.Contains("maple") || name.Contains("dead");

            GameObject tree = new GameObject("Hosted Foliage Silhouette LOD");
            tree.transform.SetParent(parent, false);

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Detailed Bark Trunk Silhouette";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight * 0.46f, 0f);
            trunk.transform.localRotation = Quaternion.Euler(0f, seed * 73f, Mathf.Sin(seed) * 2.5f);
            trunk.transform.localScale = new Vector3(trunkRadius, trunkHeight * 0.46f, trunkRadius);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = materials.TreeBark;
            RemoveCollider(trunk);

            for (int i = 0; i < 4; i++)
            {
                float angle = seed + i * Mathf.PI * 0.5f;
                Vector3 start = new Vector3(0f, trunkHeight * (0.46f + i * 0.10f), 0f);
                Vector3 end = start + new Vector3(Mathf.Cos(angle), 0.18f + i * 0.03f, Mathf.Sin(angle)) * (0.42f + footprint * 0.07f);
                CreateFoliageBranch(tree.transform, $"Wind Branch Silhouette {i + 1}", start, end, 0.040f + footprint * 0.006f, materials.TreeBark);
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
                    canopy.transform.SetParent(tree.transform, false);
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

                Renderer[] highRenderers = parent.GetComponentsInChildren<Renderer>(true);
                Renderer[] farRenderers = CreateBroadleafFarLodProxy(tree.transform, trunkHeight, canopyWidth, canopyHeight, materials.TreeBarkFar, materials.TreeCanopyFar, placement.ObjectId);
                AddGeneratedFoliageLodGroup(parent.gameObject, highRenderers, farRenderers);
                return;
            }

            int layerCount = 6;
            for (int i = 0; i < layerCount; i++)
            {
                float t = i / Mathf.Max(1f, layerCount - 1f);
                float layerRadius = canopyWidth * Mathf.Lerp(0.54f, 0.14f, t);
                float layerHeight = canopyHeight * Mathf.Lerp(0.32f, 0.20f, t);
                GameObject canopy = new GameObject($"Wind Conifer Bough Silhouette {i + 1}");
                canopy.transform.SetParent(tree.transform, false);
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
                        tree.transform,
                        $"Wind Conifer Needle Spray {i + 1}.{arm + 1}",
                        canopy.transform.localPosition + sprayOffset,
                        new Vector3(layerRadius * 0.54f, layerHeight * 0.115f, layerRadius * 0.20f),
                        branchAngle * Mathf.Rad2Deg,
                        materials.TreeCanopy,
                        0.030f + i * 0.006f,
                        0.78f + i * 0.09f);
                }
            }

            Renderer[] coniferHighRenderers = parent.GetComponentsInChildren<Renderer>(true);
            Renderer[] coniferFarRenderers = CreateConiferFarLodProxy(tree.transform, Mathf.Max(trunkHeight + canopyHeight, canopyHeight * 2.1f), trunkRadius, materials.TreeBarkFar, materials.TreeCanopyFar, placement.ObjectId);
            AddGeneratedFoliageLodGroup(parent.gameObject, coniferHighRenderers, coniferFarRenderers);
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
            if (UseFlatHostedVisualBase && !PlaceCacheNpcsOnFlatBase)
            {
                GameObject disabledRoot = new GameObject("Hosted Cache NPC Spawns (Disabled For Clean Base)");
                disabledRoot.SetActive(false);
                foreach (PsychoMirrorNpcSpawn spawn in database.NpcSpawns)
                {
                    if (IsWorldTileInsideHostedBounds(spawn.x, spawn.y))
                    {
                        context.Report.cleanBaseSkippedCacheNpcs++;
                    }
                }

                return;
            }

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
            if (!UseFlatHostedVisualBase)
            {
                BuildGrassField(context, materials);
            }

            BuildGroundCoverPatches(context, materials);
            BuildWaterways(context, materials);
            BuildWorldLandmarks(context, materials);
            BuildHostedVillageNetwork(context, materials);
            BuildPrisonBreakIntroDressing(context, materials);
            BuildRegionalBiomeDressing(context, materials);
            BuildLushWildflowerMeadows(context, materials);
            BuildWildlifeAndGiantEcology(context, materials);
            BuildHighlandForestDressing(context, materials);
            BuildRockOutcropDressing(context, materials);
            BuildHighlandCliffDressing(context, materials);
            BuildDistantVista(context, materials);
            if (!UseFlatHostedVisualBase)
            {
                BuildHorizonMist(context, materials);
            }

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
                if (!UseFlatHostedVisualBase && (overlay != 0 || (flags & 1) == 1))
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
                    if (IsHostedVillageClearingTile(worldX, worldY))
                    {
                        continue;
                    }

                    if (!TryGetNaturalDressingPosition(context, worldX, worldY, seed, out Vector3 position))
                    {
                        continue;
                    }

                    if (!TryGetLandscapeTile(context, worldX, worldY, out PsychoMapLandscape landscape, out int localX, out int localY))
                    {
                        continue;
                    }

                    byte overlay = landscape.OverlayIds[0, localX, localY];
                    if (!UseFlatHostedVisualBase && overlay != 0 && Deterministic01(seed + 7) < 0.78f)
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
            CreateWaterDepthRibbon(harborWater.transform, "Harbor Deep Tidal Pocket", 10.2f, 5.6f, materials.WaterDepth);
            CreateWaterFoamRibbon(harborWater.transform, "Harbor Western Wash Foam", -7.12f, 5.4f, materials.WaterFoam, 0.29f);
            CreateWaterFoamRibbon(harborWater.transform, "Harbor Eastern Wash Foam", 7.12f, 5.4f, materials.WaterFoam, 0.73f);
            context.Report.waterDepthChannels++;
            context.Report.waterFoamEdges += 2;
            context.Report.waterSurfaceStreaks += CreateWaterSurfaceStreaks(harborWater.transform, "Harbor Tidal Surface Current", 14.0f, 5.2f, materials.WaterFoam, 4, 12043);
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

        private static void BuildHostedVillageNetwork(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject villageRoot = new GameObject("Generated Hosted Settlement Network");
            GameObject npcRoot = new GameObject("Hosted Settlement Ambient NPCs");
            GameObject factoryObject = new GameObject("Psycho Settlement Visual Factory");
            PsychoVisualFactory factory = factoryObject.AddComponent<PsychoVisualFactory>();
            factory.SetNpcPreviewAnimation(false);

            HostedVillageSpec[] villages = CreateHostedVillageSpecs();
            for (int i = 0; i < villages.Length; i++)
            {
                CreateHostedVillage(villageRoot.transform, npcRoot.transform, context, materials, factory, villages[i], i);
            }
        }

        private static HostedVillageSpec[] CreateHostedVillageSpecs()
        {
            return new[]
            {
                new HostedVillageSpec("North Edgeville Farmstead", 3086, 3522, 5, 14, 8f, 0.96f, HostedSettlementTier.Village),
                new HostedVillageSpec("River Lum Town", 3148, 3474, 6, 18, -18f, 1.08f, HostedSettlementTier.Town),
                new HostedVillageSpec("Varrock Green City", 3202, 3422, 16, 44, 80f, 1.42f, HostedSettlementTier.City),
                new HostedVillageSpec("East Highland Sheepfold", 3264, 3446, 5, 14, -36f, 0.96f, HostedSettlementTier.Village),
                new HostedVillageSpec("Northern Ridge Watch", 3130, 3610, 6, 18, 28f, 1.00f, HostedSettlementTier.Village),
                new HostedVillageSpec("Western Autumnwood Village", 2940, 3496, 6, 16, -64f, 0.98f, HostedSettlementTier.Village),
                new HostedVillageSpec("Falador Meadow Town", 2975, 3370, 10, 26, 12f, 1.18f, HostedSettlementTier.Town),
                new HostedVillageSpec("Draynor Spring Town", 3090, 3262, 9, 24, 92f, 1.14f, HostedSettlementTier.Town),
                new HostedVillageSpec("Port Sarim Harbor Town", 3040, 3226, 8, 24, -8f, 1.12f, HostedSettlementTier.Town),
                new HostedVillageSpec("Southern Trade Yard", 3155, 3236, 7, 18, 42f, 1.02f, HostedSettlementTier.Village),
                new HostedVillageSpec("Northeast Snowwood Village", 3330, 3628, 6, 16, -28f, 0.96f, HostedSettlementTier.Village),
                new HostedVillageSpec("Southwest Stonecroft", 2878, 3248, 6, 16, 66f, 0.98f, HostedSettlementTier.Village),
                new HostedVillageSpec("Frostgate Town", 3182, 3650, 9, 24, -16f, 1.16f, HostedSettlementTier.Town),
                new HostedVillageSpec("Northwatch City", 3060, 3634, 14, 38, 34f, 1.34f, HostedSettlementTier.City),
                new HostedVillageSpec("Central Market City", 3158, 3490, 15, 42, 4f, 1.38f, HostedSettlementTier.City),
                new HostedVillageSpec("Autumnreach Town", 2915, 3440, 9, 24, -44f, 1.14f, HostedSettlementTier.Town),
                new HostedVillageSpec("Highland Lake Village", 3298, 3520, 6, 16, 54f, 1.00f, HostedSettlementTier.Village),
                new HostedVillageSpec("South Meadow Village", 3190, 3294, 6, 16, -12f, 0.98f, HostedSettlementTier.Village)
            };
        }

        private static bool IsHostedVillageClearingTile(int worldX, int worldY)
        {
            HostedVillageSpec[] villages = CreateHostedVillageSpecs();
            for (int i = 0; i < villages.Length; i++)
            {
                HostedVillageSpec village = villages[i];
                float dx = worldX - village.WorldX;
                float dy = worldY - village.WorldY;
                float radius = HostedSettlementClearingRadius(village);
                if (dx * dx + dy * dy <= radius * radius)
                {
                    return true;
                }
            }

            return false;
        }

        private static float HostedSettlementClearingRadius(HostedVillageSpec settlement)
        {
            float tierRadius = settlement.Tier == HostedSettlementTier.City
                ? 16.0f
                : settlement.Tier == HostedSettlementTier.Town
                    ? 11.5f
                    : 7.5f;
            return tierRadius + settlement.Scale * (settlement.HouseCount * 0.92f + 2.8f);
        }

        private static float SettlementTierScale(HostedSettlementTier tier)
        {
            if (tier == HostedSettlementTier.City)
            {
                return 1.36f;
            }

            if (tier == HostedSettlementTier.Town)
            {
                return 1.16f;
            }

            return 1f;
        }

        private static void CreateHostedVillage(
            Transform villageParent,
            Transform npcParent,
            HostedBuildContext context,
            HostedMaterials materials,
            PsychoVisualFactory factory,
            HostedVillageSpec village,
            int villageIndex)
        {
            if (!TryCreateVillageRoot(villageParent, village, context, out Transform root))
            {
                return;
            }

            root.localRotation = Quaternion.Euler(0f, village.Yaw, 0f);
            float scale = village.Scale;
            float tierScale = SettlementTierScale(village.Tier);
            int dressing = 0;

            CreateGroundPlate(root, "Packed Settlement Green", Vector3.zero, scale * tierScale * (8.6f + village.HouseCount * 0.58f), scale * tierScale * 7.4f, materials.LandmarkRoad);
            CreateGroundPlate(root, "Cross Settlement Lane", new Vector3(0f, 0.012f, 0f), scale * tierScale * 3.0f, scale * tierScale * 11.4f, materials.LandmarkRoad);
            CreateLandmarkPaverBands(root, "Settlement Lane", scale * tierScale * (7.2f + village.HouseCount * 0.45f), scale * tierScale * 6.3f, materials.FrostStone);
            dressing += 16;

            for (int house = 0; house < village.HouseCount; house++)
            {
                int seed = 51000 + villageIndex * 997 + house * 131;
                float laneOffset = (house - (village.HouseCount - 1) * 0.5f) * scale * 2.65f;
                float side = house % 2 == 0 ? scale * 3.0f : -scale * 3.0f;
                float width = scale * (2.25f + Deterministic01(seed + 3) * 0.62f);
                float depth = scale * (2.05f + Deterministic01(seed + 7) * 0.58f);
                float yaw = house % 2 == 0 ? 0f : 180f;
                yaw += (Deterministic01(seed + 11) - 0.5f) * 18f;
                dressing += CreateHostedVillageHouse(root, materials, $"Village Cottage {house + 1}", new Vector3(laneOffset, 0f, side), yaw, width, depth, seed);
            }

            dressing += CreateHostedVillageMarket(root, materials, scale, villageIndex);
            dressing += CreateHostedVillageWell(root, materials, scale);
            dressing += CreateHostedVillageFencing(root, materials, scale, villageIndex);
            dressing += CreateHostedVillageProps(root, materials, scale, villageIndex);
            dressing += CreateHostedSettlementTierDressing(root, materials, scale, villageIndex, village);
            CreateHostedVillageNpcCluster(npcParent, root, context, factory, village, villageIndex);

            context.Report.hostedSettlements++;
            if (village.Tier == HostedSettlementTier.City)
            {
                context.Report.hostedCities++;
            }
            else if (village.Tier == HostedSettlementTier.Town)
            {
                context.Report.hostedTowns++;
            }
            else
            {
                context.Report.hostedVillages++;
            }

            context.Report.landmarkDressingObjects += dressing;
        }

        private static bool TryCreateVillageRoot(Transform parent, HostedVillageSpec village, HostedBuildContext context, out Transform root)
        {
            root = null;
            if (!TryFindNearbyVillageAnchor(context, village.WorldX, village.WorldY, out Vector3 position))
            {
                return false;
            }

            GameObject villageObject = new GameObject(village.Name);
            villageObject.transform.SetParent(parent, false);
            villageObject.transform.position = position;
            root = villageObject.transform;
            return true;
        }

        private static bool TryFindNearbyVillageAnchor(HostedBuildContext context, int anchorX, int anchorY, out Vector3 position)
        {
            position = default;
            for (int radius = 0; radius <= 32; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (radius > 0 && Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        {
                            continue;
                        }

                        int worldX = anchorX + dx;
                        int worldY = anchorY + dy;
                        if (!TryGetLandscapeTile(context, worldX, worldY, out PsychoMapLandscape landscape, out int localX, out int localY))
                        {
                            continue;
                        }

                        byte flags = landscape.RenderFlags[0, localX, localY];
                        if (!UseFlatHostedVisualBase && (flags & 1) == 1)
                        {
                            continue;
                        }

                        position = TerrainSurfacePosition(WorldTilePosition(context, worldX, worldY), 0.045f);
                        return true;
                    }
                }
            }

            return false;
        }

        private static int CreateHostedVillageHouse(Transform parent, HostedMaterials materials, string prefix, Vector3 localPosition, float yaw, float width, float depth, int seed)
        {
            GameObject house = new GameObject(prefix);
            house.transform.SetParent(parent, false);
            house.transform.localPosition = localPosition;
            house.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            CreateLandmarkBox(house.transform, "Warm Stone Walls", new Vector3(0f, 0.82f, 0f), new Vector3(width, 1.64f, depth), materials.LandmarkStone);
            CreateLandmarkBox(house.transform, "Raised Stone Footing", new Vector3(0f, 0.18f, 0f), new Vector3(width * 1.08f, 0.20f, depth * 1.08f), materials.LandmarkRoad);
            CreateLandmarkGabledRoof(house.transform, "Pitched Timber Roof", new Vector3(0f, 2.02f, 0f), width * 1.22f, depth * 1.20f, 0.72f, materials.LandmarkRoof);
            CreateHouseDetailSet(house.transform, prefix, Vector3.zero, width, depth, materials);
            CreateLandmarkBox(house.transform, "Front Door", new Vector3(0f, 0.58f, -depth * 0.525f), new Vector3(width * 0.24f, 1.02f, 0.12f), materials.TreeBark);
            CreateLandmarkBox(house.transform, "Window Left", new Vector3(-width * 0.30f, 1.10f, -depth * 0.535f), new Vector3(width * 0.18f, 0.34f, 0.08f), materials.LandmarkGlass);
            CreateLandmarkBox(house.transform, "Window Right", new Vector3(width * 0.30f, 1.10f, -depth * 0.535f), new Vector3(width * 0.18f, 0.34f, 0.08f), materials.LandmarkGlass);
            if (Deterministic01(seed + 17) > 0.48f)
            {
                CreateLandmarkChimney(house.transform, "Cottage Chimney", new Vector3(width * 0.24f, 2.34f, depth * 0.08f), materials.FrostStone, materials.TreeBark);
            }

            return 19;
        }

        private static int CreateHostedVillageMarket(Transform root, HostedMaterials materials, float scale, int villageIndex)
        {
            int created = 0;
            for (int stall = 0; stall < 2; stall++)
            {
                float side = stall == 0 ? -1f : 1f;
                Vector3 position = new Vector3(side * scale * 2.15f, 0.42f, scale * 0.55f);
                GameObject table = CreateLandmarkBox(root, $"Village Market Table {stall + 1}", position, new Vector3(scale * 1.34f, scale * 0.34f, scale * 0.72f), materials.TreeBark);
                table.transform.localRotation = Quaternion.Euler(0f, side * 8f, 0f);
                CreateLandmarkDetailBox(root, $"Village Market Cloth {stall + 1}", position + Vector3.up * scale * 0.26f, new Vector3(scale * 1.48f, scale * 0.06f, scale * 0.84f), stall == 0 ? materials.LandmarkBanner : materials.PlayerPaper);
                CreateLandmarkDetailBox(root, $"Village Crate Stack {stall + 1}", new Vector3(side * scale * 3.05f, scale * 0.25f, -scale * 0.38f), new Vector3(scale * 0.46f, scale * 0.50f, scale * 0.46f), materials.PlayerWood);
                created += 3;
            }

            return created;
        }

        private static int CreateHostedVillageWell(Transform root, HostedMaterials materials, float scale)
        {
            CreateLandmarkCylinder(root, "Village Stone Well", new Vector3(0f, scale * 0.32f, -scale * 1.58f), new Vector3(scale * 0.52f, scale * 0.32f, scale * 0.52f), materials.FrostStone);
            CreateLandmarkCylinder(root, "Village Well Water", new Vector3(0f, scale * 0.66f, -scale * 1.58f), new Vector3(scale * 0.42f, scale * 0.035f, scale * 0.42f), materials.Water);
            CreateLandmarkBox(root, "Village Well Beam", new Vector3(0f, scale * 1.38f, -scale * 1.58f), new Vector3(scale * 1.16f, scale * 0.12f, scale * 0.16f), materials.TreeBark);
            CreateLandmarkCylinder(root, "Village Well Left Post", new Vector3(-scale * 0.48f, scale * 0.98f, -scale * 1.58f), new Vector3(scale * 0.07f, scale * 0.72f, scale * 0.07f), materials.TreeBark);
            CreateLandmarkCylinder(root, "Village Well Right Post", new Vector3(scale * 0.48f, scale * 0.98f, -scale * 1.58f), new Vector3(scale * 0.07f, scale * 0.72f, scale * 0.07f), materials.TreeBark);
            return 5;
        }

        private static int CreateHostedVillageFencing(Transform root, HostedMaterials materials, float scale, int villageIndex)
        {
            int created = 0;
            float width = scale * 6.2f;
            float depth = scale * 5.0f;
            for (int i = -3; i <= 3; i++)
            {
                float x = i * width / 6f;
                CreateLandmarkDetailBox(root, $"Village North Fence Rail {i + 4}", new Vector3(x, scale * 0.55f, depth), new Vector3(scale * 0.86f, scale * 0.10f, scale * 0.08f), materials.TreeBark);
                CreateLandmarkDetailBox(root, $"Village South Fence Rail {i + 4}", new Vector3(x, scale * 0.55f, -depth), new Vector3(scale * 0.86f, scale * 0.10f, scale * 0.08f), materials.TreeBark);
                created += 2;
            }

            for (int i = -2; i <= 2; i++)
            {
                float z = i * depth / 4f;
                CreateLandmarkDetailBox(root, $"Village East Fence Rail {i + 3}", new Vector3(width, scale * 0.55f, z), new Vector3(scale * 0.08f, scale * 0.10f, scale * 0.92f), materials.TreeBark);
                CreateLandmarkDetailBox(root, $"Village West Fence Rail {i + 3}", new Vector3(-width, scale * 0.55f, z), new Vector3(scale * 0.08f, scale * 0.10f, scale * 0.92f), materials.TreeBark);
                created += 2;
            }

            return created;
        }

        private static int CreateHostedVillageProps(Transform root, HostedMaterials materials, float scale, int villageIndex)
        {
            int created = 0;
            for (int i = 0; i < 8; i++)
            {
                int seed = 62000 + villageIndex * 503 + i * 41;
                float angle = Deterministic01(seed + 3) * Mathf.PI * 2f;
                float radius = scale * (2.2f + Deterministic01(seed + 7) * 3.8f);
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, scale * 0.22f, Mathf.Sin(angle) * radius);
                if (i % 3 == 0)
                {
                    CreateLandmarkCylinder(root, $"Village Barrel {i + 1}", position, new Vector3(scale * 0.18f, scale * 0.28f, scale * 0.18f), materials.TreeBark);
                }
                else
                {
                    GameObject crate = CreateLandmarkBox(root, $"Village Supply Crate {i + 1}", position, new Vector3(scale * 0.42f, scale * 0.40f, scale * 0.42f), materials.PlayerWood);
                    crate.transform.localRotation = Quaternion.Euler(0f, Deterministic01(seed + 11) * 180f, 0f);
                }

                created++;
            }

            CreateLandmarkBox(root, "Village Notice Sign", new Vector3(0f, scale * 0.82f, scale * 2.2f), new Vector3(scale * 0.76f, scale * 0.42f, scale * 0.08f), materials.PlayerPaper);
            CreateLandmarkCylinder(root, "Village Notice Post", new Vector3(0f, scale * 0.42f, scale * 2.2f), new Vector3(scale * 0.055f, scale * 0.42f, scale * 0.055f), materials.TreeBark);
            return created + 2;
        }

        private static int CreateHostedSettlementTierDressing(Transform root, HostedMaterials materials, float scale, int villageIndex, HostedVillageSpec settlement)
        {
            if (settlement.Tier == HostedSettlementTier.Village)
            {
                return 0;
            }

            int created = 0;
            float tierScale = SettlementTierScale(settlement.Tier);
            float plazaWidth = scale * tierScale * (settlement.Tier == HostedSettlementTier.City ? 8.4f : 5.8f);
            float plazaDepth = scale * tierScale * (settlement.Tier == HostedSettlementTier.City ? 6.8f : 4.6f);
            CreateGroundPlate(root, $"{settlement.Tier} Stone Market Square", new Vector3(0f, 0.020f, 0f), plazaWidth, plazaDepth, materials.FrostStone);
            created++;

            int stallCount = settlement.Tier == HostedSettlementTier.City ? 6 : 4;
            for (int i = 0; i < stallCount; i++)
            {
                int seed = 73500 + villageIndex * 409 + i * 61;
                float side = i % 2 == 0 ? -1f : 1f;
                float row = i / 2;
                Vector3 position = new Vector3(side * scale * tierScale * 2.45f, scale * 0.45f, (row - 1f) * scale * tierScale * 1.25f);
                GameObject stall = CreateLandmarkBox(root, $"{settlement.Tier} Market Stall {i + 1}", position, new Vector3(scale * 1.30f, scale * 0.34f, scale * 0.78f), materials.TreeBark);
                stall.transform.localRotation = Quaternion.Euler(0f, side * (8f + Deterministic01(seed + 3) * 6f), 0f);
                CreateLandmarkDetailBox(root, $"{settlement.Tier} Stall Cloth {i + 1}", position + Vector3.up * scale * 0.31f, new Vector3(scale * 1.44f, scale * 0.06f, scale * 0.86f), i % 3 == 0 ? materials.LandmarkBanner : materials.PlayerPaper);
                created += 2;
            }

            int bannerCount = settlement.Tier == HostedSettlementTier.City ? 8 : 4;
            float bannerRadius = scale * tierScale * (settlement.Tier == HostedSettlementTier.City ? 5.4f : 3.9f);
            for (int i = 0; i < bannerCount; i++)
            {
                float angle = i * Mathf.PI * 2f / bannerCount;
                Vector3 post = new Vector3(Mathf.Cos(angle) * bannerRadius, scale * 1.08f, Mathf.Sin(angle) * bannerRadius);
                CreateLandmarkCylinder(root, $"{settlement.Tier} Banner Post {i + 1}", post, new Vector3(scale * 0.055f, scale * 1.08f, scale * 0.055f), materials.TreeBark);
                CreateLandmarkDetailBox(root, $"{settlement.Tier} Hanging Banner {i + 1}", post + new Vector3(Mathf.Cos(angle) * scale * 0.12f, scale * 0.42f, Mathf.Sin(angle) * scale * 0.12f), new Vector3(scale * 0.06f, scale * 0.70f, scale * 0.32f), materials.LandmarkBanner);
                created += 2;
            }

            if (settlement.Tier == HostedSettlementTier.City)
            {
                float towerRadius = scale * tierScale * 6.1f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = Mathf.PI * 0.25f + i * Mathf.PI * 0.5f;
                    Vector3 tower = new Vector3(Mathf.Cos(angle) * towerRadius, scale * 1.35f, Mathf.Sin(angle) * towerRadius);
                    CreateLandmarkCylinder(root, $"City Watch Tower {i + 1}", tower, new Vector3(scale * 0.44f, scale * 1.35f, scale * 0.44f), materials.LandmarkStone);
                    CreateLandmarkGabledRoof(root, $"City Watch Tower Roof {i + 1}", tower + Vector3.up * scale * 1.50f, scale * 1.12f, scale * 1.12f, scale * 0.52f, materials.LandmarkRoof);
                    created += 2;
                }

                CreateLandmarkCylinder(root, "City Central Fountain Basin", new Vector3(0f, scale * 0.18f, 0f), new Vector3(scale * 0.88f, scale * 0.18f, scale * 0.88f), materials.FrostStone);
                CreateLandmarkCylinder(root, "City Central Fountain Water", new Vector3(0f, scale * 0.39f, 0f), new Vector3(scale * 0.70f, scale * 0.035f, scale * 0.70f), materials.Water);
                created += 2;
            }

            return created;
        }

        private static int CreateHostedVillageNpcCluster(
            Transform npcParent,
            Transform villageRoot,
            HostedBuildContext context,
            PsychoVisualFactory factory,
            HostedVillageSpec village,
            int villageIndex)
        {
            int created = 0;
            for (int i = 0; i < village.NpcCount; i++)
            {
                int seed = 70000 + villageIndex * 1543 + i * 97;
                float angle = Deterministic01(seed + 3) * Mathf.PI * 2f;
                float maxRadius = village.Tier == HostedSettlementTier.City
                    ? 8.4f
                    : village.Tier == HostedSettlementTier.Town
                        ? 5.8f
                        : 3.6f;
                float radius = village.Scale * (0.80f + Mathf.Pow(Deterministic01(seed + 7), 0.62f) * maxRadius);
                Vector3 local = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                string role = VillageNpcRole(i, villageIndex, village.Tier);
                PsychoMirrorNpc npc = CreateHostedVillageNpc(HostedVillageNpcBaseId + villageIndex * 100 + i, role, village.Tier, seed);
                GameObject npcObject = factory.CreateNpcVisual(npc);
                npcObject.name = $"Settlement NPC {npc.id} - {npc.name} ({village.Name})";
                npcObject.transform.SetParent(npcParent, true);
                npcObject.transform.position = TerrainSurfacePosition(villageRoot.TransformPoint(local), 0.035f);
                npcObject.transform.rotation = Quaternion.Euler(0f, village.Yaw + Deterministic01(seed + 13) * 360f, 0f);

                PsychoInteractable interactable = npcObject.AddComponent<PsychoInteractable>();
                interactable.Configure(npc.id, npc.name, VillageNpcActions(role));
                npcObject.AddComponent<PsychoGroundFollower>();
                PrototypeNpcWander wander = npcObject.AddComponent<PrototypeNpcWander>();
                SerializedObject wanderObject = new SerializedObject(wander);
                wanderObject.FindProperty("wanderRadius").floatValue = village.Scale * Mathf.Lerp(1.65f, maxRadius * 0.75f, Deterministic01(seed + 17));
                wanderObject.FindProperty("speed").floatValue = role == "Guard" ? 1.42f : role == "Merchant" ? 1.16f : 1.02f;
                wanderObject.FindProperty("pauseDuration").floatValue = 0.78f + Deterministic01(seed + 19) * 0.82f;
                SetSerializedFloat(wanderObject, "turnSpeed", role == "Guard" ? 5.4f : 4.6f);
                SetSerializedFloat(wanderObject, "acceleration", 3.1f);
                SetSerializedFloat(wanderObject, "visualStrideBob", 0.0022f);
                SetSerializedFloat(wanderObject, "visualStrideSway", 0.28f);
                wanderObject.ApplyModifiedPropertiesWithoutUndo();
                created++;
            }

            context.Report.hostedVillageNpcs += created;
            context.Report.hostedSettlementNpcs += created;
            return created;
        }

        private static PsychoMirrorNpc CreateHostedVillageNpc(int id, string role, HostedSettlementTier tier, int seed)
        {
            string visualClass = role;
            string tierName = tier == HostedSettlementTier.City ? "City" : tier == HostedSettlementTier.Town ? "Town" : "Village";
            string displayName = role == "Merchant"
                ? $"{tierName} Merchant"
                : role == "Guard"
                    ? $"{tierName} Watch"
                    : role == "Banker"
                        ? $"{tierName} Clerk"
                        : $"{tierName} Resident";
            return new PsychoMirrorNpc
            {
                id = id,
                name = displayName,
                examine = "A hosted Unity settlement resident.",
                combat = role == "Guard" ? 24 : 0,
                size = 1,
                attackable = role == "Guard",
                aggressive = false,
                retreats = false,
                poisonous = false,
                respawn = 30,
                hitpoints = role == "Guard" ? 45 : 12,
                visualClass = visualClass,
                materialClass = role == "Guard" ? "Metal" : role == "Merchant" ? "Cloth" : "Leather",
                scale = 0.98f + Deterministic01(seed + 31) * 0.08f
            };
        }

        private static string VillageNpcRole(int index, int villageIndex, HostedSettlementTier tier)
        {
            int roll = (index + villageIndex * 3) % (tier == HostedSettlementTier.City ? 9 : 7);
            if (roll == 0)
            {
                return "Merchant";
            }

            if (roll == 3 || (tier == HostedSettlementTier.City && roll == 7))
            {
                return "Guard";
            }

            if (roll == 5 || (tier != HostedSettlementTier.Village && roll == 8))
            {
                return "Banker";
            }

            return "Citizen";
        }

        private static string[] VillageNpcActions(string role)
        {
            if (role == "Merchant")
            {
                return new[] { "Talk-to", "Trade", "Examine" };
            }

            if (role == "Guard")
            {
                return new[] { "Talk-to", "Ask-for-directions", "Examine" };
            }

            if (role == "Banker")
            {
                return new[] { "Talk-to", "Bank", "Examine" };
            }

            return new[] { "Talk-to", "Examine" };
        }

        private static void BuildPrisonBreakIntroDressing(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject rootObject = new GameObject("Gallows Dawn Prison Intro Set");
            if (!TryCreateLandmarkRoot(rootObject.transform, "North Edgeville Prison Yard", context, 3087, 3491, out Transform root))
            {
                return;
            }

            root.localRotation = Quaternion.Euler(0f, -18f, 0f);
            int created = 0;
            CreateGroundPlate(root, "Packed Mud Prison Yard", Vector3.zero, 16.2f, 12.4f, materials.AutumnGround);
            CreateGroundPlate(root, "Worn Road To Village", new Vector3(0f, 0.012f, -8.8f), 4.2f, 9.0f, materials.LandmarkRoad);
            created += 2;

            if (TryInstantiateStarterPrefabLocal(root, "KayKit_Barracks", "Authored Prison Keep Barracks", new Vector3(-4.15f, 0.02f, 1.3f), Quaternion.Euler(0f, 180f, 0f), new Vector3(1.42f, 1.28f, 1.42f), out _))
            {
                created++;
            }

            if (TryInstantiateStarterPrefabLocal(root, "KayKit_WallGateClosed", "Authored Prison Portcullis Gate", new Vector3(0f, 0.03f, -6.15f), Quaternion.Euler(0f, 180f, 0f), new Vector3(0.94f, 1.05f, 0.94f), out _, keepColliders: true))
            {
                created++;
            }

            if (TryInstantiateStarterPrefabLocal(root, "KayKit_Watchtower", "Authored Prison Watchtower", new Vector3(7.15f, 0.02f, 5.15f), Quaternion.Euler(0f, -30f, 0f), new Vector3(0.82f, 0.92f, 0.82f), out _, keepColliders: true))
            {
                created++;
            }

            for (int i = 0; i < 4; i++)
            {
                float z = i < 2 ? -6.2f : 6.2f;
                float x = i % 2 == 0 ? -8.1f : 8.1f;
                CreateLandmarkBox(root, $"Prison Yard Wall {i + 1}", new Vector3(i < 2 ? 0f : x, 0.78f, i < 2 ? z : 0f), i < 2 ? new Vector3(16.4f, 1.56f, 0.34f) : new Vector3(0.34f, 1.56f, 12.4f), materials.LandmarkStone);
                created++;
            }

            CreateLandmarkBox(root, "Prison Gate Left Post", new Vector3(-1.05f, 1.18f, -6.32f), new Vector3(0.28f, 2.36f, 0.34f), materials.TreeBark);
            CreateLandmarkBox(root, "Prison Gate Right Post", new Vector3(1.05f, 1.18f, -6.32f), new Vector3(0.28f, 2.36f, 0.34f), materials.TreeBark);
            CreateLandmarkBox(root, "Prison Gate Open Leaf", new Vector3(1.95f, 0.92f, -6.12f), new Vector3(1.35f, 1.84f, 0.18f), materials.TreeBark).transform.localRotation = Quaternion.Euler(0f, -34f, 0f);
            created += 3;

            CreateLandmarkBox(root, "Prison Cell Stone Room", new Vector3(-4.15f, 1.08f, 1.3f), new Vector3(4.8f, 2.16f, 4.2f), materials.LandmarkStone);
            CreateLandmarkGabledRoof(root, "Prison Cell Heavy Roof", new Vector3(-4.15f, 2.46f, 1.3f), 5.4f, 4.8f, 0.72f, materials.LandmarkRoof);
            CreateLandmarkBox(root, "Cell Door Bars", new Vector3(-4.15f, 0.98f, -0.88f), new Vector3(1.25f, 1.82f, 0.10f), materials.PlayerMetal);
            CreateLandmarkBox(root, "Cell Straw Bed", new Vector3(-5.46f, 0.19f, 2.28f), new Vector3(1.55f, 0.24f, 0.82f), materials.Flowers);
            created += 4;

            CreateLandmarkStoneCourses(root, "Prison Cell Cut Stone Course", 4.9f, 4.3f, 0.44f, 2.12f, materials.FrostStone);
            CreateLandmarkTimberBracing(root, "Prison Cell Black Timber Frame", 4.9f, 4.3f, 1.22f, 1.42f, materials.TreeBark);
            CreateLandmarkRoofShingles(root, "Prison Cell Roof Shingles", 5.6f, 4.9f, 2.20f, 0.72f, materials.TreeBark);
            CreateLandmarkDetailBox(root, "Cell Lock Plate", new Vector3(-4.15f, 1.05f, -0.965f), new Vector3(0.52f, 0.22f, 0.04f), materials.PlayerMetal);
            CreateLandmarkDetailBox(root, "Cell Door Crossbar", new Vector3(-4.15f, 1.36f, -0.980f), new Vector3(1.46f, 0.12f, 0.08f), materials.TreeBark);
            created += 5;

            CreateLandmarkBox(root, "Gallows Platform", new Vector3(3.9f, 0.36f, 2.4f), new Vector3(3.2f, 0.34f, 2.4f), materials.TreeBark);
            CreateLandmarkCylinder(root, "Gallows Left Upright", new Vector3(2.9f, 1.88f, 2.1f), new Vector3(0.10f, 1.62f, 0.10f), materials.TreeBark);
            CreateLandmarkCylinder(root, "Gallows Right Upright", new Vector3(4.9f, 1.88f, 2.1f), new Vector3(0.10f, 1.62f, 0.10f), materials.TreeBark);
            CreateLandmarkBox(root, "Gallows Crossbeam", new Vector3(3.9f, 3.46f, 2.1f), new Vector3(2.45f, 0.16f, 0.16f), materials.TreeBark);
            CreateLandmarkCylinder(root, "Morning Rope", new Vector3(3.9f, 2.70f, 2.1f), new Vector3(0.026f, 0.68f, 0.026f), materials.PlayerPaper);
            created += 5;

            CreateLandmarkDetailBox(root, "Gallows Trapdoor Split", new Vector3(3.9f, 0.56f, 2.4f), new Vector3(0.06f, 0.035f, 2.04f), materials.LandmarkRoad);
            CreateLandmarkDetailBox(root, "Gallows Warning Placard", new Vector3(2.42f, 1.02f, 1.14f), new Vector3(0.62f, 0.42f, 0.05f), materials.PlayerPaper).transform.localRotation = Quaternion.Euler(0f, -12f, 0f);
            created += 2;

            if (TryInstantiateStarterPrefabLocal(root, "Starter_Barrel", "Prison Yard Water Barrel", new Vector3(5.85f, 0.05f, -3.05f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 0.82f, out _))
            {
                created++;
            }

            if (TryInstantiateStarterPrefabLocal(root, "Starter_Crate", "Prison Evidence Crate", new Vector3(-6.55f, 0.05f, -4.55f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.78f, out _))
            {
                created++;
            }

            if (TryInstantiateStarterPrefabLocal(root, "Starter_Chest", "Confiscated Gear Chest", new Vector3(-5.85f, 0.05f, -4.05f), Quaternion.Euler(0f, 10f, 0f), Vector3.one * 0.72f, out _))
            {
                created++;
            }

            if (TryInstantiateStarterPrefabLocal(root, "KayKit_Well", "Roadside Prison Well", new Vector3(6.18f, 0.04f, -1.20f), Quaternion.Euler(0f, 24f, 0f), Vector3.one * 0.62f, out _))
            {
                created++;
            }

            CreateIntroPointLight(root, "Gate Lantern Glow", new Vector3(0f, 2.35f, -5.85f), new Color(1.0f, 0.58f, 0.28f, 1f), 4.8f, 1.75f);
            CreateIntroPointLight(root, "Cell Torch Glow", new Vector3(-2.28f, 1.70f, -1.05f), new Color(1.0f, 0.46f, 0.20f, 1f), 3.9f, 1.15f);
            CreateIntroPointLight(root, "Road Escape Lantern", new Vector3(0.15f, 1.35f, -9.55f), new Color(0.92f, 0.68f, 0.34f, 1f), 4.2f, 1.05f);
            created += 3;

            CreateQuestMarkerDressing(root, "Intake Quest Marker", new Vector3(-0.6f, 1.92f, -3.8f), materials.WildflowerGold);
            CreateQuestMarkerDressing(root, "Cell Quest Marker", new Vector3(-4.15f, 2.34f, -0.88f), materials.WildflowerGold);
            CreateQuestMarkerDressing(root, "Escape Road Quest Marker", new Vector3(0f, 1.55f, -9.80f), materials.WildflowerGold);
            created += 3;

            CreateProceduralStoryNpc(root, "Rough-Cloth Priest Intake Clerk", new Vector3(-0.6f, 0.06f, -3.8f), 162f, materials.PlayerPaper, materials.PlayerSkin, materials.PlayerHair, 950001, new[] { "Talk-to", "Examine" }, "Monk_Citizen", new Vector3(1.04f, 1.04f, 1.04f));
            CreateProceduralStoryNpc(root, "Sleeping Prison Keep", new Vector3(2.6f, 0.06f, -3.2f), -110f, materials.PlayerLeather, materials.PlayerSkin, materials.PlayerHair, 950002, new[] { "Wake", "Examine" }, "Warrior_Player", new Vector3(1.06f, 1.06f, 1.06f));
            CreateProceduralStoryNpc(root, "Breakout Stranger", new Vector3(1.30f, 0.06f, -5.35f), -6f, materials.LandmarkBanner, materials.PlayerSkin, materials.PlayerHair, 950003, new[] { "Talk-to", "Examine" }, "Rogue_Merchant", new Vector3(1.02f, 1.02f, 1.02f));
            created += 3;

            context.Report.introQuestDressingObjects += created;
            context.Report.landmarkDressingObjects += created;
        }

        private static void CreateProceduralStoryNpc(Transform parent, string name, Vector3 localPosition, float yaw, Material cloth, Material skin, Material hair, int id, string[] actions, string authoredPrefab = null, Vector3 authoredScale = default)
        {
            GameObject npc = new GameObject(name);
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition = localPosition;
            npc.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            bool hasAuthoredVisual = !string.IsNullOrWhiteSpace(authoredPrefab)
                && TryInstantiateStarterPrefabLocal(npc.transform, authoredPrefab, "Authored Story NPC Visual", Vector3.zero, Quaternion.identity, authoredScale == default ? Vector3.one : authoredScale, out _);
            if (!hasAuthoredVisual)
            {
                GameObject visual = new GameObject("Story NPC Visual");
                visual.transform.SetParent(npc.transform, false);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Robed Body", new Vector3(0f, 0.88f, 0f), new Vector3(0.20f, 0.45f, 0.16f), cloth);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 1.44f, 0f), new Vector3(0.15f, 0.18f, 0.15f), skin);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Hair", new Vector3(0f, 1.56f, 0f), new Vector3(0.16f, 0.06f, 0.15f), hair);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Left Arm", new Vector3(-0.22f, 0.88f, 0.02f), new Vector3(0.045f, 0.24f, 0.045f), cloth).transform.localRotation = Quaternion.Euler(0f, 0f, -8f);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Right Arm", new Vector3(0.22f, 0.88f, 0.02f), new Vector3(0.045f, 0.24f, 0.045f), cloth).transform.localRotation = Quaternion.Euler(0f, 0f, 8f);
            }

            CapsuleCollider collider = npc.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.82f, 0f);
            collider.height = 1.7f;
            collider.radius = 0.25f;
            npc.AddComponent<PsychoInteractable>().Configure(id, name, actions);
        }

        private static void CreateIntroPointLight(Transform parent, string name, Vector3 localPosition, Color color, float range, float intensity)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = localPosition;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
        }

        private static void CreateQuestMarkerDressing(Transform parent, string name, Vector3 localPosition, Material material)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = localPosition;
            CreateLandmarkCylinder(marker.transform, "Marker Stem", new Vector3(0f, -0.38f, 0f), new Vector3(0.025f, 0.38f, 0.025f), material);
            GameObject diamond = CreateLandmarkBox(marker.transform, "Marker Diamond", Vector3.zero, new Vector3(0.28f, 0.28f, 0.28f), material);
            diamond.transform.localRotation = Quaternion.Euler(42f, 45f, 0f);
        }

        private static void BuildLushWildflowerMeadows(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Lush Wildflower Meadow Ecology");
            HostedMeadowSpec[] meadows = CreateHostedMeadowSpecs();
            for (int i = 0; i < meadows.Length; i++)
            {
                CreateLushWildflowerMeadow(root.transform, context, materials, meadows[i], i);
            }
        }

        private static HostedMeadowSpec[] CreateHostedMeadowSpecs()
        {
            return new[]
            {
                new HostedMeadowSpec("Open Vale Wildflower Preserve", 3030, 3425, 28f, 50, 82, 46),
                new HostedMeadowSpec("Edgeville Player Start Lush Field", 3087, 3491, 18f, 42, 64, 34),
                new HostedMeadowSpec("Falador Lush Wildflower Field", 2968, 3378, 25f, 42, 58, 32),
                new HostedMeadowSpec("Draynor Herb Meadow", 3078, 3270, 23f, 36, 42, 38),
                new HostedMeadowSpec("River Lum Wildflower Bank", 3144, 3478, 21f, 34, 48, 26),
                new HostedMeadowSpec("South Meadow Village Flowering Common", 3192, 3298, 22f, 34, 44, 30),
                new HostedMeadowSpec("Western Autumn Herb Glade", 2922, 3448, 24f, 38, 36, 34),
                new HostedMeadowSpec("North Edgeville Wayside Meadow", 3082, 3516, 20f, 30, 36, 24),
                new HostedMeadowSpec("Highland Lake Alpine Flower Shelf", 3292, 3516, 19f, 28, 30, 28)
            };
        }

        private static void CreateLushWildflowerMeadow(Transform parent, HostedBuildContext context, HostedMaterials materials, HostedMeadowSpec meadow, int meadowIndex)
        {
            if (!TryFindNearbyNaturalAnchor(context, meadow.WorldX, meadow.WorldY, 24, out Vector3 anchor))
            {
                return;
            }

            GameObject meadowRoot = new GameObject(meadow.Name);
            meadowRoot.transform.SetParent(parent, false);
            meadowRoot.transform.position = anchor;

            int patches = 0;
            int flowers = 0;
            int herbs = 0;
            for (int i = 0; i < meadow.PatchCount; i++)
            {
                int seed = 122000 + meadowIndex * 3571 + i * 131;
                if (!TryGetRadialNaturalPosition(context, meadow.WorldX, meadow.WorldY, meadow.Radius, seed, 0.76f, out int worldX, out int worldY, out Vector3 position))
                {
                    continue;
                }

                if (IsHostedSettlementCoreTile(worldX, worldY))
                {
                    continue;
                }

                float roll = Deterministic01(seed + 19);
                Material material = roll > 0.72f
                    ? materials.Flowers
                    : roll > 0.48f
                        ? materials.Herbs
                        : materials.Grass;
                float width = Mathf.Lerp(2.2f, 6.8f, Deterministic01(seed + 23));
                float depth = Mathf.Lerp(1.4f, 4.8f, Deterministic01(seed + 29));
                GameObject patch = CreateGroundCoverPatch($"{meadow.Name} Lush Ground Blend {patches + 1}", position + Vector3.up * (0.018f + patches * 0.00008f), width, depth, material, seed);
                patch.transform.SetParent(meadowRoot.transform, true);
                patches++;
            }

            for (int i = 0; i < meadow.WildflowerCount; i++)
            {
                int seed = 132000 + meadowIndex * 4027 + i * 149;
                if (!TryGetRadialNaturalPosition(context, meadow.WorldX, meadow.WorldY, meadow.Radius * 0.92f, seed, 0.82f, out int worldX, out int worldY, out Vector3 position))
                {
                    continue;
                }

                if (IsHostedSettlementCoreTile(worldX, worldY))
                {
                    continue;
                }

                Material petalMaterial = WildflowerMaterialForSeed(materials, seed);
                float radius = Mathf.Lerp(0.34f, 0.86f, Deterministic01(seed + 31));
                float height = Mathf.Lerp(0.16f, 0.34f, Deterministic01(seed + 37));
                string prefabName = Deterministic01(seed + 53) > 0.36f ? "Bush_Common_Flowers" : "Grass_Common_Tall";
                Vector3 scale = Vector3.one * Mathf.Lerp(0.34f, 0.76f, Deterministic01(seed + 59));
                if (TryInstantiateStarterPrefab(meadowRoot.transform, prefabName, $"{meadow.Name} Authored Wildflower Cluster {flowers + 1}", position + Vector3.up * 0.030f, Quaternion.Euler(0f, Deterministic01(seed + 61) * 360f, 0f), scale))
                {
                    CreateWildflowerCluster(meadowRoot.transform, $"{meadow.Name} Petal Detail {flowers + 1}", position + Vector3.up * 0.070f, radius * 0.72f, height * 1.10f, petalMaterial, seed + 503);
                }
                else
                {
                    CreateWildflowerCluster(meadowRoot.transform, $"{meadow.Name} Wildflower Cluster {flowers + 1}", position + Vector3.up * 0.052f, radius, height, petalMaterial, seed);
                }

                flowers++;
            }

            for (int i = 0; i < meadow.HerbCount; i++)
            {
                int seed = 142000 + meadowIndex * 3769 + i * 157;
                if (!TryGetRadialNaturalPosition(context, meadow.WorldX, meadow.WorldY, meadow.Radius * 0.88f, seed, 0.70f, out int worldX, out int worldY, out Vector3 position))
                {
                    continue;
                }

                if (IsHostedSettlementCoreTile(worldX, worldY))
                {
                    continue;
                }

                float radius = Mathf.Lerp(0.30f, 0.74f, Deterministic01(seed + 41));
                float height = Mathf.Lerp(0.18f, 0.42f, Deterministic01(seed + 43));
                string prefabName = Deterministic01(seed + 47) > 0.46f ? "Grass_Wispy_Tall" : "Bush_Common";
                Vector3 scale = Vector3.one * Mathf.Lerp(0.28f, 0.68f, Deterministic01(seed + 53));
                if (TryInstantiateStarterPrefab(meadowRoot.transform, prefabName, $"{meadow.Name} Authored Herb Cluster {herbs + 1}", position + Vector3.up * 0.024f, Quaternion.Euler(0f, Deterministic01(seed + 57) * 360f, 0f), scale))
                {
                    CreateHerbCluster(meadowRoot.transform, $"{meadow.Name} Herb Detail {herbs + 1}", position + Vector3.up * 0.058f, radius * 0.68f, height * 1.10f, materials.Herbs, seed + 607);
                }
                else
                {
                    CreateHerbCluster(meadowRoot.transform, $"{meadow.Name} Herb Cluster {herbs + 1}", position + Vector3.up * 0.048f, radius, height, materials.Herbs, seed);
                }

                herbs++;
            }

            context.Report.lushMeadowPatches += patches;
            context.Report.wildflowerClusters += flowers;
            context.Report.herbClusters += herbs;
            context.Report.groundCoverPatches += patches;
            context.Report.seasonalDressingObjects += patches + flowers + herbs;
            context.Report.windAnimatedObjects += flowers + herbs;

            if (meadow.Name == "Open Vale Wildflower Preserve")
            {
                CreateOpenValeNatureFrame(meadowRoot.transform, context, materials, meadow, meadowIndex);
            }
        }

        private static Material WildflowerMaterialForSeed(HostedMaterials materials, int seed)
        {
            float roll = Deterministic01(seed + 11);
            if (roll > 0.66f)
            {
                return materials.WildflowerBlue;
            }

            if (roll > 0.33f)
            {
                return materials.WildflowerPurple;
            }

            return materials.WildflowerGold;
        }

        private static void CreateOpenValeNatureFrame(Transform parent, HostedBuildContext context, HostedMaterials materials, HostedMeadowSpec meadow, int meadowIndex)
        {
            int created = 0;
            string[] treePrefabs = { "CommonTree_1", "CommonTree_2", "CommonTree_3", "TwistedTree_1", "Pine_1", "Pine_2" };
            for (int i = 0; i < 28; i++)
            {
                int seed = 172000 + meadowIndex * 1901 + i * 211;
                float angle = i / 28f * Mathf.PI * 2f + Mathf.Lerp(-0.16f, 0.16f, Deterministic01(seed + 5));
                float distance = Mathf.Lerp(meadow.Radius * 0.74f, meadow.Radius * 1.08f, Deterministic01(seed + 11));
                int worldX = meadow.WorldX + Mathf.RoundToInt(Mathf.Cos(angle) * distance);
                int worldY = meadow.WorldY + Mathf.RoundToInt(Mathf.Sin(angle) * distance);
                if (!IsWorldTileInsideHostedBounds(worldX, worldY) || IsHostedSettlementCoreTile(worldX, worldY))
                {
                    continue;
                }

                if (!TryGetNaturalDressingPosition(context, worldX, worldY, seed, out Vector3 position))
                {
                    continue;
                }

                string prefab = treePrefabs[Mathf.FloorToInt(Deterministic01(seed + 17) * treePrefabs.Length) % treePrefabs.Length];
                float scale = Mathf.Lerp(0.86f, 1.38f, Deterministic01(seed + 23));
                if (TryInstantiateStarterPrefab(parent, prefab, $"Open Vale Authored Treeline {created + 1}", position, Quaternion.Euler(0f, Deterministic01(seed + 29) * 360f, 0f), Vector3.one * scale, keepColliders: true))
                {
                    created++;
                }
            }

            for (int i = 0; i < 18; i++)
            {
                int seed = 177000 + meadowIndex * 1709 + i * 197;
                if (!TryGetRadialNaturalPosition(context, meadow.WorldX, meadow.WorldY, meadow.Radius * 0.86f, seed, 0.72f, out _, out _, out Vector3 position))
                {
                    continue;
                }

                Material material = i % 3 == 0 ? materials.WildflowerGold : i % 3 == 1 ? materials.WildflowerBlue : materials.WildflowerPurple;
                float width = Mathf.Lerp(2.8f, 6.2f, Deterministic01(seed + 31));
                float depth = Mathf.Lerp(1.5f, 4.2f, Deterministic01(seed + 37));
                GameObject bloom = CreateGroundCoverPatch($"Open Vale Color Bloom Carpet {i + 1}", position + Vector3.up * (0.034f + i * 0.0002f), width, depth, material, seed);
                bloom.transform.SetParent(parent, true);
                created++;
            }

            for (int i = 0; i < 14; i++)
            {
                int seed = 181000 + meadowIndex * 1601 + i * 181;
                if (!TryGetRadialNaturalPosition(context, meadow.WorldX, meadow.WorldY, meadow.Radius * 0.95f, seed, 0.62f, out _, out _, out Vector3 position))
                {
                    continue;
                }

                string rockPrefab = Deterministic01(seed + 19) > 0.5f ? "Rock_Medium_2" : "Rock_Medium_3";
                TryInstantiateStarterPrefab(parent, rockPrefab, $"Open Vale Moss Rock {i + 1}", position, Quaternion.Euler(0f, Deterministic01(seed + 41) * 360f, 0f), Vector3.one * Mathf.Lerp(0.52f, 0.92f, Deterministic01(seed + 43)), keepColliders: true);
                created++;
            }

            context.Report.seasonalDressingObjects += created;
            context.Report.enhancedFoliageObjects += created;
            context.Report.foliageLodProxies += created;
            context.Report.windAnimatedObjects += created;
        }

        private static bool TryGetRadialNaturalPosition(
            HostedBuildContext context,
            int anchorX,
            int anchorY,
            float radius,
            int seed,
            float radiusPower,
            out int worldX,
            out int worldY,
            out Vector3 position)
        {
            float angle = Deterministic01(seed + 3) * Mathf.PI * 2f;
            float distance = Mathf.Pow(Deterministic01(seed + 5), radiusPower) * radius;
            worldX = anchorX + Mathf.RoundToInt(Mathf.Cos(angle) * distance);
            worldY = anchorY + Mathf.RoundToInt(Mathf.Sin(angle) * distance);
            if (!IsWorldTileInsideHostedBounds(worldX, worldY))
            {
                position = default;
                return false;
            }

            return TryGetNaturalDressingPosition(context, worldX, worldY, seed, out position);
        }

        private static bool IsHostedSettlementCoreTile(int worldX, int worldY)
        {
            HostedVillageSpec[] villages = CreateHostedVillageSpecs();
            for (int i = 0; i < villages.Length; i++)
            {
                HostedVillageSpec village = villages[i];
                float dx = worldX - village.WorldX;
                float dy = worldY - village.WorldY;
                float tierRadius = village.Tier == HostedSettlementTier.City
                    ? 10.5f
                    : village.Tier == HostedSettlementTier.Town
                        ? 7.4f
                        : 5.2f;
                float radius = tierRadius + village.Scale * village.HouseCount * 0.34f;
                if (dx * dx + dy * dy <= radius * radius)
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject CreateWildflowerCluster(Transform parent, string name, Vector3 position, float radius, float height, Material material, int seed)
        {
            int blossomCount = 6 + Mathf.FloorToInt(Deterministic01(seed + 17) * 7f);
            Mesh mesh = new Mesh { name = name + " Mesh" };
            Vector3[] vertices = new Vector3[blossomCount * 5];
            int[] triangles = new int[blossomCount * 12];
            for (int i = 0; i < blossomCount; i++)
            {
                float angle = Deterministic01(seed + i * 31) * Mathf.PI * 2f;
                float distance = Mathf.Sqrt(Deterministic01(seed + i * 37)) * radius;
                Vector3 center = new Vector3(Mathf.Cos(angle) * distance, height * (0.42f + Deterministic01(seed + i * 41) * 0.40f), Mathf.Sin(angle) * distance);
                float yaw = Deterministic01(seed + i * 43) * Mathf.PI * 2f;
                float petal = Mathf.Lerp(0.026f, 0.048f, Deterministic01(seed + i * 47));
                Vector3 axisA = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw)) * petal;
                Vector3 axisB = Vector3.up * petal * 1.22f;
                int v = i * 5;
                vertices[v] = center;
                vertices[v + 1] = center + axisB;
                vertices[v + 2] = center + axisA;
                vertices[v + 3] = center - axisB * 0.52f;
                vertices[v + 4] = center - axisA;
                int t = i * 12;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;
                triangles[t + 3] = v;
                triangles[t + 4] = v + 2;
                triangles[t + 5] = v + 3;
                triangles[t + 6] = v;
                triangles[t + 7] = v + 3;
                triangles[t + 8] = v + 4;
                triangles[t + 9] = v;
                triangles[t + 10] = v + 4;
                triangles[t + 11] = v + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject cluster = new GameObject(name);
            cluster.transform.SetParent(parent, true);
            cluster.transform.position = position;
            MeshFilter filter = cluster.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = cluster.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            AddWind(cluster, 0.035f, 1.45f, 0.22f, 1.05f, 0.026f);
            return cluster;
        }

        private static GameObject CreateHerbCluster(Transform parent, string name, Vector3 position, float radius, float height, Material material, int seed)
        {
            int leafCount = 7 + Mathf.FloorToInt(Deterministic01(seed + 13) * 8f);
            Mesh mesh = new Mesh { name = name + " Mesh" };
            Vector3[] vertices = new Vector3[leafCount * 4];
            int[] triangles = new int[leafCount * 6];
            for (int i = 0; i < leafCount; i++)
            {
                float angle = Deterministic01(seed + i * 19) * Mathf.PI * 2f;
                float distance = Deterministic01(seed + i * 23) * radius;
                Vector3 root = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
                Vector3 side = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * Mathf.Lerp(0.024f, 0.052f, Deterministic01(seed + i * 29));
                Vector3 lean = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * height * Mathf.Lerp(0.10f, 0.28f, Deterministic01(seed + i * 31));
                Vector3 tip = root + Vector3.up * (height * Mathf.Lerp(0.58f, 1.0f, Deterministic01(seed + i * 37))) + lean;
                int v = i * 4;
                vertices[v] = root - side;
                vertices[v + 1] = root + side;
                vertices[v + 2] = tip - side * 0.18f;
                vertices[v + 3] = tip + side * 0.18f;
                int t = i * 6;
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

            GameObject cluster = new GameObject(name);
            cluster.transform.SetParent(parent, true);
            cluster.transform.position = position;
            cluster.transform.rotation = Quaternion.Euler(0f, Deterministic01(seed + 59) * 360f, 0f);
            MeshFilter filter = cluster.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = cluster.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            AddWind(cluster, 0.045f, 1.18f, 0.26f, 0.92f, 0.028f);
            return cluster;
        }

        private static bool TryInstantiateStarterPrefab(Transform parent, string prefabName, string instanceName, Vector3 position, Quaternion rotation, Vector3 scale, bool keepColliders = false)
        {
            string prefabPath = $"Assets/Resources/PsychoArt/Prefabs/Starter/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return false;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return false;
            }

            instance.name = instanceName;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = Vector3.Scale(instance.transform.localScale, scale);
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = ShadowCastingMode.On;
                renderers[i].receiveShadows = true;
            }

            if (!keepColliders)
            {
                Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < colliders.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(colliders[i]);
                }
            }

            AddWind(instance, 0.040f, 0.96f, 0.24f, 0.82f, 0.030f);
            return true;
        }

        private static bool TryInstantiateStarterPrefabLocal(Transform parent, string prefabName, string instanceName, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, out GameObject instance, Material materialOverride = null, bool keepColliders = false)
        {
            instance = null;
            string prefabPath = $"Assets/Resources/PsychoArt/Prefabs/Starter/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return false;
            }

            instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return false;
            }

            instance.name = instanceName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = Vector3.Scale(instance.transform.localScale, localScale);

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = ShadowCastingMode.On;
                renderers[i].receiveShadows = true;
                if (materialOverride != null)
                {
                    renderers[i].sharedMaterial = materialOverride;
                }
            }

            if (!keepColliders)
            {
                Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < colliders.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(colliders[i]);
                }
            }

            return true;
        }

        private static void BuildWildlifeAndGiantEcology(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Hosted Wildlife And Giant Ecology");
            BuildWildlifeHerds(root.transform, context, materials);
            BuildGiantMammothCamps(root.transform, context, materials);
        }

        private static void BuildWildlifeHerds(Transform parent, HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Hosted Wildlife Herds");
            root.transform.SetParent(parent, false);
            HostedWildlifeSpec[] specs =
            {
                new HostedWildlifeSpec("Falador Meadow Deer", "Deer", 2970, 3388, 9, 20f),
                new HostedWildlifeSpec("Draynor Meadow Hares", "Hare", 3070, 3266, 12, 18f),
                new HostedWildlifeSpec("River Lum Red Foxes", "Fox", 3148, 3468, 7, 17f),
                new HostedWildlifeSpec("Western Autumn Stags", "Deer", 2928, 3460, 8, 22f),
                new HostedWildlifeSpec("Highland Lake Elk", "Deer", 3298, 3524, 7, 18f),
                new HostedWildlifeSpec("South Meadow Hares", "Hare", 3194, 3298, 12, 19f),
                new HostedWildlifeSpec("North Pine Foxes", "Fox", 3096, 3548, 6, 18f)
            };

            int created = 0;
            for (int group = 0; group < specs.Length; group++)
            {
                HostedWildlifeSpec spec = specs[group];
                for (int i = 0; i < spec.Count; i++)
                {
                    int seed = 152000 + group * 2221 + i * 173;
                    if (!TryGetRadialNaturalPosition(context, spec.WorldX, spec.WorldY, spec.Radius, seed, 0.64f, out int worldX, out int worldY, out Vector3 position))
                    {
                        continue;
                    }

                    if (IsHostedSettlementCoreTile(worldX, worldY))
                    {
                        continue;
                    }

                    GameObject wildlife = CreateProceduralWildlife(root.transform, $"{spec.Kind} {created + 1} ({spec.Name})", spec.Kind, position, Deterministic01(seed + 3) * 360f, materials, seed);
                    PsychoInteractable interactable = wildlife.AddComponent<PsychoInteractable>();
                    interactable.Configure(HostedWildlifeNpcBaseId + created, spec.Kind, new[] { "Observe", "Examine" });
                    wildlife.AddComponent<PsychoGroundFollower>();
                    ConfigureWander(wildlife, spec.Kind == "Hare" ? 2.2f : 4.8f, spec.Kind == "Hare" ? 1.55f : 1.18f, 0.55f, 0.0018f, 0.18f);
                    created++;
                }
            }

            context.Report.wildlifeNpcs += created;
            context.Report.visualReplacementNpcs += created;
        }

        private static GameObject CreateProceduralWildlife(Transform parent, string name, string kind, Vector3 position, float yaw, HostedMaterials materials, int seed)
        {
            GameObject animal = new GameObject(name);
            animal.transform.SetParent(parent, true);
            animal.transform.position = position;
            animal.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            GameObject visual = new GameObject($"{kind} Visual");
            visual.transform.SetParent(animal.transform, false);

            if (kind == "Hare")
            {
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Compact Hare Body", new Vector3(0f, 0.17f, 0f), new Vector3(0.16f, 0.10f, 0.24f), materials.WildlifeHide);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Hare Chest", new Vector3(0f, 0.22f, 0.17f), new Vector3(0.13f, 0.11f, 0.12f), materials.WildlifeHide);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Hare Head", new Vector3(0f, 0.30f, 0.29f), new Vector3(0.10f, 0.09f, 0.10f), materials.WildlifeHide);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Left Hare Ear", new Vector3(-0.045f, 0.45f, 0.30f), new Vector3(0.020f, 0.12f, 0.020f), materials.WildlifeHide).transform.localRotation = Quaternion.Euler(-12f, 0f, -9f);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Right Hare Ear", new Vector3(0.045f, 0.45f, 0.30f), new Vector3(0.020f, 0.12f, 0.020f), materials.WildlifeHide).transform.localRotation = Quaternion.Euler(-12f, 0f, 9f);
                SphereCollider collider = animal.AddComponent<SphereCollider>();
                collider.center = new Vector3(0f, 0.22f, 0.07f);
                collider.radius = 0.32f;
                return animal;
            }

            if (kind == "Fox")
            {
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Lean Fox Body", new Vector3(0f, 0.36f, 0f), new Vector3(0.16f, 0.33f, 0.16f), materials.WildlifeHide).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Fox Head", new Vector3(0f, 0.43f, 0.41f), new Vector3(0.12f, 0.10f, 0.13f), materials.WildlifeHide);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Cube, "Fox Muzzle", new Vector3(0f, 0.40f, 0.54f), new Vector3(0.09f, 0.055f, 0.13f), materials.MammothTusk);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Fox Tail", new Vector3(0f, 0.40f, -0.47f), new Vector3(0.065f, 0.32f, 0.065f), materials.WildlifeHide).transform.localRotation = Quaternion.Euler(58f, 0f, 0f);
                for (int leg = 0; leg < 4; leg++)
                {
                    float side = leg % 2 == 0 ? -1f : 1f;
                    float z = leg < 2 ? 0.22f : -0.22f;
                    CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, $"Fox Leg {leg + 1}", new Vector3(side * 0.10f, 0.16f, z), new Vector3(0.030f, 0.14f, 0.030f), materials.WildlifeHide);
                }

                CapsuleCollider collider = animal.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0f, 0.36f, 0.02f);
                collider.height = 0.78f;
                collider.radius = 0.22f;
                return animal;
            }

            float bodyScale = 0.92f + Deterministic01(seed + 71) * 0.20f;
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Highland Deer Body", new Vector3(0f, 0.58f, 0f), new Vector3(0.24f * bodyScale, 0.48f * bodyScale, 0.24f * bodyScale), materials.WildlifeHide).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Deer Chest", new Vector3(0f, 0.66f, 0.34f), new Vector3(0.19f, 0.22f, 0.18f), materials.WildlifeHide);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Deer Head", new Vector3(0f, 0.96f, 0.56f), new Vector3(0.13f, 0.16f, 0.12f), materials.WildlifeHide);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Deer Neck", new Vector3(0f, 0.82f, 0.42f), new Vector3(0.070f, 0.24f, 0.070f), materials.WildlifeHide).transform.localRotation = Quaternion.Euler(-28f, 0f, 0f);
            for (int leg = 0; leg < 4; leg++)
            {
                float side = leg % 2 == 0 ? -1f : 1f;
                float z = leg < 2 ? 0.28f : -0.28f;
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, $"Deer Leg {leg + 1}", new Vector3(side * 0.14f, 0.29f, z), new Vector3(0.040f, 0.31f, 0.040f), materials.WildlifeHide);
            }

            CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Left Antler Stem", new Vector3(-0.055f, 1.14f, 0.58f), new Vector3(0.018f, 0.14f, 0.018f), materials.MammothTusk).transform.localRotation = Quaternion.Euler(-15f, 0f, -28f);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Right Antler Stem", new Vector3(0.055f, 1.14f, 0.58f), new Vector3(0.018f, 0.14f, 0.018f), materials.MammothTusk).transform.localRotation = Quaternion.Euler(-15f, 0f, 28f);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Left Antler Branch", new Vector3(-0.115f, 1.20f, 0.61f), new Vector3(0.014f, 0.09f, 0.014f), materials.MammothTusk).transform.localRotation = Quaternion.Euler(10f, 0f, -58f);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Right Antler Branch", new Vector3(0.115f, 1.20f, 0.61f), new Vector3(0.014f, 0.09f, 0.014f), materials.MammothTusk).transform.localRotation = Quaternion.Euler(10f, 0f, 58f);
            CapsuleCollider deerCollider = animal.AddComponent<CapsuleCollider>();
            deerCollider.center = new Vector3(0f, 0.58f, 0.06f);
            deerCollider.height = 1.28f;
            deerCollider.radius = 0.34f;
            return animal;
        }

        private static void BuildGiantMammothCamps(Transform parent, HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Hosted Giant Mammoth Camps");
            root.transform.SetParent(parent, false);
            HostedGiantCampSpec[] camps =
            {
                new HostedGiantCampSpec("Frostpine Giant Camp", 3136, 3588, 2, 3, 18f),
                new HostedGiantCampSpec("Autumnreach Giant Camp", 2924, 3468, 2, 2, 17f),
                new HostedGiantCampSpec("Highland Lake Giant Camp", 3290, 3528, 2, 2, 16f),
                new HostedGiantCampSpec("Southern Amber Giant Camp", 3180, 3268, 1, 2, 15f),
                new HostedGiantCampSpec("Snowwood Mammoth Camp", 3320, 3606, 2, 3, 18f)
            };

            int giantCount = 0;
            int mammothCount = 0;
            for (int campIndex = 0; campIndex < camps.Length; campIndex++)
            {
                HostedGiantCampSpec camp = camps[campIndex];
                if (!TryFindNearbyNaturalAnchor(context, camp.WorldX, camp.WorldY, 28, out Vector3 anchor))
                {
                    continue;
                }

                GameObject campRoot = new GameObject(camp.Name);
                campRoot.transform.SetParent(root.transform, false);
                campRoot.transform.position = anchor;
                CreateGiantCampDressing(campRoot.transform, materials, camp, campIndex);
                context.Report.giantCamps++;

                List<GameObject> giants = new List<GameObject>(camp.GiantCount);
                for (int i = 0; i < camp.GiantCount; i++)
                {
                    int seed = 172000 + campIndex * 3541 + i * 211;
                    Vector3 local = CampLocalPosition(camp.Radius * 0.34f, seed);
                    GameObject giant = CreateProceduralGiant(campRoot.transform, $"Hosted Giant {giantCount + 1} ({camp.Name})", local, Deterministic01(seed + 7) * 360f, materials, seed);
                    PsychoInteractable interactable = giant.AddComponent<PsychoInteractable>();
                    interactable.Configure(HostedGiantNpcBaseId + giantCount, "Giant", new[] { "Provoke", "Talk-to", "Examine" });
                    giant.AddComponent<PsychoGroundFollower>();
                    ConfigureWander(giant, 2.8f, 0.82f, 1.2f, 0.0020f, 0.14f);
                    giants.Add(giant);
                    giantCount++;
                }

                List<PsychoMammothCompanion> companions = new List<PsychoMammothCompanion>(camp.MammothCount);
                for (int i = 0; i < camp.MammothCount; i++)
                {
                    int seed = 182000 + campIndex * 4021 + i * 223;
                    Vector3 local = CampLocalPosition(camp.Radius * 0.46f, seed);
                    GameObject mammoth = CreateProceduralMammoth(campRoot.transform, $"Mammoth Companion {mammothCount + 1} ({camp.Name})", local, Deterministic01(seed + 7) * 360f, materials, seed);
                    PsychoInteractable interactable = mammoth.AddComponent<PsychoInteractable>();
                    interactable.Configure(HostedMammothNpcBaseId + mammothCount, "Mammoth Companion", new[] { "Observe", "Examine" });
                    mammoth.AddComponent<PsychoGroundFollower>();
                    ConfigureWander(mammoth, 3.8f, 0.72f, 1.0f, 0.0014f, 0.10f);
                    PsychoMammothCompanion companion = mammoth.AddComponent<PsychoMammothCompanion>();
                    Transform leader = giants.Count == 0 ? campRoot.transform : giants[i % giants.Count].transform;
                    float side = i % 2 == 0 ? -1f : 1f;
                    companion.Configure(leader, new Vector3(side * 3.1f, 0f, -2.6f - i * 0.35f), 2.2f, 5.8f, 1.18f, 3.25f, 8.0f + Deterministic01(seed + 31) * 3.5f);
                    companions.Add(companion);
                    mammothCount++;
                }

                PsychoMammothCompanion[] sharedCompanions = companions.ToArray();
                for (int i = 0; i < giants.Count; i++)
                {
                    PsychoGiantProvocation provocation = giants[i].AddComponent<PsychoGiantProvocation>();
                    provocation.Configure(sharedCompanions, 0.52f + Deterministic01(192000 + campIndex * 73 + i * 17) * 0.26f, camp.Name);
                }
            }

            context.Report.giants += giantCount;
            context.Report.mammothCompanions += mammothCount;
            context.Report.visualReplacementNpcs += giantCount + mammothCount;
        }

        private static Vector3 CampLocalPosition(float radius, int seed)
        {
            float angle = Deterministic01(seed + 3) * Mathf.PI * 2f;
            float distance = Mathf.Lerp(radius * 0.28f, radius, Deterministic01(seed + 5));
            return new Vector3(Mathf.Cos(angle) * distance, 0.055f, Mathf.Sin(angle) * distance);
        }

        private static void CreateGiantCampDressing(Transform root, HostedMaterials materials, HostedGiantCampSpec camp, int campIndex)
        {
            CreateGroundCoverPatch("Trampled Giant Camp Ground", root.position + Vector3.up * 0.022f, camp.Radius * 0.95f, camp.Radius * 0.72f, materials.AutumnGround, 202000 + campIndex * 97).transform.SetParent(root, true);
            CreateLandmarkCylinder(root, "Stone Fire Ring", new Vector3(0f, 0.10f, 0f), new Vector3(1.20f, 0.10f, 1.20f), materials.FrostStone);
            CreateLandmarkCylinder(root, "Low Camp Ember Bed", new Vector3(0f, 0.18f, 0f), new Vector3(0.86f, 0.035f, 0.86f), materials.LandmarkBanner);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Vector3 rock = new Vector3(Mathf.Cos(angle) * 1.15f, 0.22f, Mathf.Sin(angle) * 1.15f);
                CreateLandmarkDetailBox(root, $"Camp Ring Stone {i + 1}", rock, new Vector3(0.34f, 0.26f, 0.28f), materials.FrostStone).transform.localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
            }

            for (int i = 0; i < 4; i++)
            {
                int seed = 203000 + campIndex * 191 + i * 29;
                Vector3 log = CampLocalPosition(camp.Radius * 0.32f, seed) + Vector3.up * 0.22f;
                GameObject bench = CreateLandmarkBox(root, $"Giant Camp Log Seat {i + 1}", log, new Vector3(1.65f, 0.28f, 0.32f), materials.TreeBark);
                bench.transform.localRotation = Quaternion.Euler(0f, Deterministic01(seed + 7) * 360f, 0f);
            }
        }

        private static GameObject CreateProceduralGiant(Transform parent, string name, Vector3 localPosition, float yaw, HostedMaterials materials, int seed)
        {
            GameObject giant = new GameObject(name);
            giant.transform.SetParent(parent, false);
            giant.transform.localPosition = localPosition;
            giant.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            GameObject visual = new GameObject("Authored Optimized Giant Visual");
            visual.transform.SetParent(giant.transform, false);

            float heightScale = 1.0f + Deterministic01(seed + 11) * 0.16f;
            string giantPrefab = Deterministic01(seed + 43) > 0.48f ? "Warrior_Dwarf" : "Warrior_Player";
            bool hasAuthoredGiant = TryInstantiateStarterPrefabLocal(visual.transform, giantPrefab, "CC0 Authored Giant Base", Vector3.zero, Quaternion.identity, new Vector3(2.18f * heightScale, 2.45f * heightScale, 2.18f * heightScale), out _);
            if (!hasAuthoredGiant)
            {
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Giant Hide Torso", new Vector3(0f, 1.95f * heightScale, 0f), new Vector3(0.50f, 0.78f * heightScale, 0.35f), materials.GiantSkin);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Giant Heavy Chest", new Vector3(0f, 2.35f * heightScale, -0.05f), new Vector3(0.46f, 0.34f, 0.30f), materials.PlayerLeather);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Giant Head", new Vector3(0f, 3.05f * heightScale, 0.02f), new Vector3(0.26f, 0.31f, 0.24f), materials.GiantSkin);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Cube, "Giant Brow", new Vector3(0f, 3.12f * heightScale, -0.23f), new Vector3(0.25f, 0.055f, 0.035f), materials.PlayerHair);
                CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Giant Beard", new Vector3(0f, 2.88f * heightScale, -0.21f), new Vector3(0.18f, 0.12f, 0.06f), materials.PlayerHair);
                for (int sideIndex = 0; sideIndex < 2; sideIndex++)
                {
                    float side = sideIndex == 0 ? -1f : 1f;
                    CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, side < 0f ? "Left Giant Arm" : "Right Giant Arm", new Vector3(side * 0.52f, 2.03f * heightScale, 0.02f), new Vector3(0.105f, 0.58f * heightScale, 0.105f), materials.GiantSkin).transform.localRotation = Quaternion.Euler(0f, 0f, side * 13f);
                    CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, side < 0f ? "Left Giant Hand" : "Right Giant Hand", new Vector3(side * 0.62f, 1.32f * heightScale, 0.03f), new Vector3(0.13f, 0.11f, 0.12f), materials.GiantSkin);
                    CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, side < 0f ? "Left Giant Leg" : "Right Giant Leg", new Vector3(side * 0.20f, 0.86f * heightScale, 0.02f), new Vector3(0.14f, 0.56f * heightScale, 0.13f), materials.PlayerLeather);
                    CreatePlayerPrimitive(visual.transform, PrimitiveType.Cube, side < 0f ? "Left Giant Foot" : "Right Giant Foot", new Vector3(side * 0.20f, 0.16f, -0.08f), new Vector3(0.24f, 0.13f, 0.38f), materials.PlayerLeather);
                }
            }

            CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Giant Fur Shoulder Mass", new Vector3(0f, 2.34f * heightScale, -0.04f), new Vector3(0.72f, 0.26f, 0.38f), materials.PlayerLeather);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Cube, "Giant Hide Apron", new Vector3(0f, 1.36f * heightScale, -0.20f), new Vector3(0.62f, 0.62f, 0.06f), materials.PlayerLeather);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Giant Braided Beard", new Vector3(0f, 2.90f * heightScale, -0.23f), new Vector3(0.20f, 0.18f, 0.08f), materials.PlayerHair);
            GameObject club = CreatePlayerPrimitive(visual.transform, PrimitiveType.Capsule, "Giant Pine Club", new Vector3(0.72f, 1.72f, -0.12f), new Vector3(0.10f, 0.72f, 0.10f), materials.TreeBark);
            club.transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Sphere, "Club Knotted Head", new Vector3(0.88f, 2.48f, -0.14f), new Vector3(0.22f, 0.26f, 0.20f), materials.TreeBark);
            CreatePlayerPrimitive(visual.transform, PrimitiveType.Cube, "Club Iron Band", new Vector3(0.82f, 2.18f, -0.13f), new Vector3(0.21f, 0.045f, 0.21f), materials.PlayerMetal).transform.localRotation = Quaternion.Euler(0f, 0f, -24f);

            CapsuleCollider collider = giant.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1.58f * heightScale, 0f);
            collider.height = 3.25f * heightScale;
            collider.radius = 0.54f;
            return giant;
        }

        private static GameObject CreateProceduralMammoth(Transform parent, string name, Vector3 localPosition, float yaw, HostedMaterials materials, int seed)
        {
            GameObject mammoth = new GameObject(name);
            mammoth.transform.SetParent(parent, false);
            mammoth.transform.localPosition = localPosition;
            mammoth.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            GameObject visual = new GameObject("Authored Optimized Mammoth Visual");
            visual.transform.SetParent(mammoth.transform, false);

            float scale = 0.96f + Deterministic01(seed + 13) * 0.14f;
            if (!TryInstantiateStarterPrefabLocal(visual.transform, "Starter_Cow", "CC0 Authored Mammoth Body Base", Vector3.zero, Quaternion.identity, new Vector3(1.92f * scale, 1.55f * scale, 2.42f * scale), out _, materials.MammothFur))
            {
                CreateOptimizedMammothPart(visual.transform, "Fallback Mammoth Barrel Body", CreateEllipsoidMesh("Mammoth Body Mesh", 18, 9), new Vector3(0f, 1.10f * scale, 0f), Quaternion.identity, new Vector3(0.86f * scale, 0.58f * scale, 1.18f * scale), materials.MammothFur);
                CreateOptimizedMammothPart(visual.transform, "Fallback Mammoth Head", CreateEllipsoidMesh("Mammoth Head Mesh", 14, 7), new Vector3(0f, 1.22f * scale, 1.10f * scale), Quaternion.identity, new Vector3(0.44f * scale, 0.38f * scale, 0.42f * scale), materials.MammothFur);
                for (int leg = 0; leg < 4; leg++)
                {
                    float side = leg % 2 == 0 ? -1f : 1f;
                    float z = leg < 2 ? 0.54f : -0.54f;
                    CreateOptimizedMammothPart(visual.transform, $"Fallback Mammoth Pillar Leg {leg + 1}", CreateTaperedTubeMesh("Mammoth Leg Mesh", 10, 4, 1.0f, 0.16f, 0.13f, 0.0f), new Vector3(side * 0.42f * scale, 0.48f * scale, z * scale), Quaternion.identity, new Vector3(scale, scale * 0.82f, scale), materials.MammothFur);
                }
            }

            CreateOptimizedMammothPart(visual.transform, "Mammoth Shoulder Fur Hump", CreateEllipsoidMesh("Mammoth Hump Mesh", 16, 8), new Vector3(0f, 1.50f * scale, 0.18f * scale), Quaternion.identity, new Vector3(0.68f * scale, 0.38f * scale, 0.58f * scale), materials.MammothFur);
            CreateOptimizedMammothPart(visual.transform, "Mammoth Heavy Forehead", CreateEllipsoidMesh("Mammoth Brow Mesh", 14, 7), new Vector3(0f, 1.32f * scale, 1.08f * scale), Quaternion.identity, new Vector3(0.42f * scale, 0.26f * scale, 0.34f * scale), materials.MammothFur);
            CreateOptimizedMammothPart(visual.transform, "Mammoth Hanging Trunk", CreateTaperedTubeMesh("Mammoth Trunk Mesh", 14, 9, 1.18f, 0.16f, 0.065f, 0.24f), new Vector3(0f, 1.03f * scale, 1.34f * scale), Quaternion.Euler(18f, 0f, 0f), new Vector3(scale, scale, scale), materials.MammothFur);
            CreateOptimizedMammothPart(visual.transform, "Left Mammoth Ear", CreateFlattenedLeafMesh("Mammoth Ear Mesh", 12), new Vector3(-0.48f * scale, 1.28f * scale, 1.02f * scale), Quaternion.Euler(0f, -24f, 10f), new Vector3(0.44f * scale, 0.48f * scale, 0.11f * scale), materials.MammothFur);
            CreateOptimizedMammothPart(visual.transform, "Right Mammoth Ear", CreateFlattenedLeafMesh("Mammoth Ear Mesh", 12), new Vector3(0.48f * scale, 1.28f * scale, 1.02f * scale), Quaternion.Euler(0f, 24f, -10f), new Vector3(0.44f * scale, 0.48f * scale, 0.11f * scale), materials.MammothFur);
            CreateOptimizedMammothPart(visual.transform, "Left Mammoth Swept Tusk", CreateTaperedTubeMesh("Mammoth Tusk Mesh", 12, 9, 1.22f, 0.062f, 0.014f, 0.42f), new Vector3(-0.28f * scale, 0.98f * scale, 1.42f * scale), Quaternion.Euler(70f, -7f, -30f), new Vector3(scale, scale, scale), materials.MammothTusk);
            CreateOptimizedMammothPart(visual.transform, "Right Mammoth Swept Tusk", CreateTaperedTubeMesh("Mammoth Tusk Mesh", 12, 9, 1.22f, 0.062f, 0.014f, 0.42f), new Vector3(0.28f * scale, 0.98f * scale, 1.42f * scale), Quaternion.Euler(70f, 7f, 30f), new Vector3(scale, scale, scale), materials.MammothTusk);
            CreateOptimizedMammothPart(visual.transform, "Left Shaggy Belly Fur", CreateFlattenedLeafMesh("Mammoth Fur Fringe Mesh", 10), new Vector3(-0.46f * scale, 0.88f * scale, -0.06f * scale), Quaternion.Euler(0f, -92f, 180f), new Vector3(1.08f * scale, 0.70f * scale, 0.08f * scale), materials.MammothFur);
            CreateOptimizedMammothPart(visual.transform, "Right Shaggy Belly Fur", CreateFlattenedLeafMesh("Mammoth Fur Fringe Mesh", 10), new Vector3(0.46f * scale, 0.88f * scale, -0.06f * scale), Quaternion.Euler(0f, 92f, 180f), new Vector3(1.08f * scale, 0.70f * scale, 0.08f * scale), materials.MammothFur);

            BoxCollider collider = mammoth.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 1.03f * scale, 0.18f * scale);
            collider.size = new Vector3(1.85f * scale, 1.62f * scale, 2.72f * scale);
            return mammoth;
        }

        private static GameObject CreateOptimizedMammothPart(Transform parent, string name, Mesh mesh, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return part;
        }

        private static Mesh CreateEllipsoidMesh(string name, int segments, int rings)
        {
            Mesh mesh = new Mesh { name = name };
            List<Vector3> vertices = new List<Vector3>((segments + 1) * (rings + 1));
            List<int> triangles = new List<int>(segments * rings * 6);
            for (int ring = 0; ring <= rings; ring++)
            {
                float v = ring / (float)rings;
                float theta = v * Mathf.PI;
                float y = Mathf.Cos(theta);
                float radius = Mathf.Sin(theta);
                for (int segment = 0; segment <= segments; segment++)
                {
                    float u = segment / (float)segments;
                    float angle = u * Mathf.PI * 2f;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius));
                }
            }

            for (int ring = 0; ring < rings; ring++)
            {
                for (int segment = 0; segment < segments; segment++)
                {
                    int current = ring * (segments + 1) + segment;
                    int next = current + segments + 1;
                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);
                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTaperedTubeMesh(string name, int segments, int rings, float length, float baseRadius, float tipRadius, float forwardCurve)
        {
            Mesh mesh = new Mesh { name = name };
            List<Vector3> vertices = new List<Vector3>((segments + 1) * (rings + 1));
            List<int> triangles = new List<int>(segments * rings * 6);
            for (int ring = 0; ring <= rings; ring++)
            {
                float t = ring / (float)rings;
                float radius = Mathf.Lerp(baseRadius, tipRadius, t);
                float y = -length * t;
                float zCurve = Mathf.Sin(t * Mathf.PI) * forwardCurve;
                for (int segment = 0; segment <= segments; segment++)
                {
                    float angle = segment / (float)segments * Mathf.PI * 2f;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius + zCurve));
                }
            }

            for (int ring = 0; ring < rings; ring++)
            {
                for (int segment = 0; segment < segments; segment++)
                {
                    int current = ring * (segments + 1) + segment;
                    int next = current + segments + 1;
                    triangles.Add(current);
                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(current + 1);
                    triangles.Add(next + 1);
                    triangles.Add(next);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateFlattenedLeafMesh(string name, int segments)
        {
            Mesh mesh = new Mesh { name = name };
            Vector3[] vertices = new Vector3[segments + 2];
            int[] triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(-Mathf.PI * 0.82f, Mathf.PI * 0.82f, t);
                vertices[i + 1] = new Vector3(Mathf.Sin(angle) * 0.5f, Mathf.Cos(angle) * 0.5f, 0f);
                if (i < segments)
                {
                    int tri = i * 3;
                    triangles[tri] = 0;
                    triangles[tri + 1] = i + 1;
                    triangles[tri + 2] = i + 2;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void ConfigureWander(GameObject target, float radius, float speed, float pause, float strideBob, float strideSway)
        {
            PrototypeNpcWander wander = target.AddComponent<PrototypeNpcWander>();
            SerializedObject wanderObject = new SerializedObject(wander);
            wanderObject.FindProperty("wanderRadius").floatValue = radius;
            wanderObject.FindProperty("speed").floatValue = speed;
            wanderObject.FindProperty("pauseDuration").floatValue = pause;
            SetSerializedFloat(wanderObject, "turnSpeed", 4.5f);
            SetSerializedFloat(wanderObject, "acceleration", 2.4f);
            SetSerializedFloat(wanderObject, "visualStrideBob", strideBob);
            SetSerializedFloat(wanderObject, "visualStrideSway", strideSway);
            wanderObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildRegionalBiomeDressing(HostedBuildContext context, HostedMaterials materials)
        {
            GameObject root = new GameObject("Generated Regional Biome Dressing");
            HostedBiomeSpec[] biomes = CreateHostedBiomeSpecs();
            for (int i = 0; i < biomes.Length; i++)
            {
                CreateRegionalBiome(root.transform, context, materials, biomes[i], i);
            }
        }

        private static HostedBiomeSpec[] CreateHostedBiomeSpecs()
        {
            return new[]
            {
                new HostedBiomeSpec("Northern Frostspine Mountains", 3138, 3655, 43f, HostedSeasonBiome.WinterHighland, 11, 8, 55, 40, 12f),
                new HostedBiomeSpec("Northeast Snowwood", 3314, 3618, 37f, HostedSeasonBiome.Snowfield, 6, 6, 70, 46, -24f),
                new HostedBiomeSpec("Western Autumnwood", 2918, 3488, 34f, HostedSeasonBiome.AutumnWoodland, 3, 9, 70, 0, 64f),
                new HostedBiomeSpec("North Edgeville Pine Belt", 3078, 3534, 31f, HostedSeasonBiome.SummerForest, 2, 7, 60, 4, -8f),
                new HostedBiomeSpec("Falador Rolling Meadows", 2962, 3372, 32f, HostedSeasonBiome.SpringMeadow, 0, 12, 40, 0, 22f),
                new HostedBiomeSpec("Draynor Springwood", 3068, 3268, 30f, HostedSeasonBiome.SpringMeadow, 0, 10, 42, 0, 86f),
                new HostedBiomeSpec("Southern Amber Hills", 3150, 3230, 33f, HostedSeasonBiome.AutumnWoodland, 2, 14, 48, 0, 36f),
                new HostedBiomeSpec("Eastern Highland Ridges", 3265, 3442, 35f, HostedSeasonBiome.SummerForest, 7, 10, 55, 10, -42f)
            };
        }

        private static void CreateRegionalBiome(Transform parent, HostedBuildContext context, HostedMaterials materials, HostedBiomeSpec biome, int biomeIndex)
        {
            if (!TryFindNearbyNaturalAnchor(context, biome.WorldX, biome.WorldY, 36, out Vector3 anchor))
            {
                return;
            }

            GameObject biomeObject = new GameObject(biome.Name);
            biomeObject.transform.SetParent(parent, false);
            biomeObject.transform.position = anchor;

            int seasonalObjects = 0;
            seasonalObjects += CreateBiomeGroundWash(biomeObject.transform, context, materials, biome, biomeIndex);
            int mountains = CreateBiomeMountainRidge(biomeObject.transform, context, materials, biome, biomeIndex);
            int hills = CreateBiomeHillField(biomeObject.transform, context, materials, biome, biomeIndex);
            int denseTrees = CreateBiomeDenseForest(biomeObject.transform, context, materials, biome, biomeIndex);
            int snowPatches = CreateBiomeSnowPockets(biomeObject.transform, context, materials, biome, biomeIndex);

            context.Report.seasonalBiomeRegions++;
            context.Report.mountainMassifs += mountains;
            context.Report.hillMounds += hills;
            context.Report.denseForestTrees += denseTrees;
            context.Report.snowPatches += snowPatches;
            context.Report.seasonalDressingObjects += seasonalObjects + snowPatches;
            context.Report.enhancedFoliageObjects += denseTrees;
            context.Report.foliageLodProxies += denseTrees;
            context.Report.windAnimatedObjects += denseTrees;
        }

        private static bool TryFindNearbyNaturalAnchor(HostedBuildContext context, int anchorX, int anchorY, int maxRadius, out Vector3 position)
        {
            position = default;
            for (int radius = 0; radius <= maxRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (radius > 0 && Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        {
                            continue;
                        }

                        int worldX = anchorX + dx;
                        int worldY = anchorY + dy;
                        if (!TryGetLandscapeTile(context, worldX, worldY, out PsychoMapLandscape landscape, out int localX, out int localY))
                        {
                            continue;
                        }

                        byte flags = landscape.RenderFlags[0, localX, localY];
                        if (!UseFlatHostedVisualBase && (flags & 1) == 1)
                        {
                            continue;
                        }

                        position = TerrainSurfacePosition(WorldTilePosition(context, worldX, worldY), 0.035f);
                        return true;
                    }
                }
            }

            return false;
        }

        private static int CreateBiomeGroundWash(Transform root, HostedBuildContext context, HostedMaterials materials, HostedBiomeSpec biome, int biomeIndex)
        {
            int created = 0;
            int patchCount = Mathf.Clamp(Mathf.RoundToInt(biome.Radius * 0.38f), 8, 18);
            Material material = SeasonalGroundMaterial(biome.Season, materials);
            for (int i = 0; i < patchCount; i++)
            {
                int seed = 91000 + biomeIndex * 2003 + i * 137;
                if (!TryGetBiomeScatterPosition(context, biome, seed, biome.Radius * 0.74f, 0.72f, out int worldX, out int worldY, out Vector3 position))
                {
                    continue;
                }

                if (IsHostedVillageClearingTile(worldX, worldY))
                {
                    continue;
                }

                bool snowBiome = biome.Season == HostedSeasonBiome.WinterHighland || biome.Season == HostedSeasonBiome.Snowfield;
                float width = snowBiome ? Mathf.Lerp(10.0f, 22.0f, Deterministic01(seed + 31)) : Mathf.Lerp(5.2f, 10.8f, Deterministic01(seed + 31));
                float depth = snowBiome ? Mathf.Lerp(8.0f, 18.0f, Deterministic01(seed + 37)) : Mathf.Lerp(3.8f, 8.6f, Deterministic01(seed + 37));
                GameObject patch = CreateGroundCoverPatch($"{biome.Name} Seasonal Ground Wash {i + 1}", position + Vector3.up * (0.018f + i * 0.00035f), width, depth, material, seed);
                patch.transform.SetParent(root, true);
                created++;
            }

            return created;
        }

        private static int CreateBiomeMountainRidge(Transform root, HostedBuildContext context, HostedMaterials materials, HostedBiomeSpec biome, int biomeIndex)
        {
            int created = 0;
            float yaw = biome.Yaw * Mathf.Deg2Rad;
            Vector2 ridge = new Vector2(Mathf.Cos(yaw), Mathf.Sin(yaw));
            Vector2 cross = new Vector2(-ridge.y, ridge.x);

            for (int i = 0; i < biome.MountainCount; i++)
            {
                int seed = 93000 + biomeIndex * 2027 + i * 173;
                float t = biome.MountainCount <= 1 ? 0f : (i / (float)(biome.MountainCount - 1) - 0.5f);
                float along = t * biome.Radius * 1.48f + (Deterministic01(seed + 3) - 0.5f) * 6.0f;
                float lateral = (Deterministic01(seed + 7) - 0.5f) * biome.Radius * 0.36f;
                int worldX = biome.WorldX + Mathf.RoundToInt(ridge.x * along + cross.x * lateral);
                int worldY = biome.WorldY + Mathf.RoundToInt(ridge.y * along + cross.y * lateral);
                if (IsHostedVillageClearingTile(worldX, worldY))
                {
                    continue;
                }

                if (!TryGetNaturalDressingPosition(context, worldX, worldY, seed, out Vector3 position))
                {
                    continue;
                }

                float width = Mathf.Lerp(8.0f, 16.4f, Deterministic01(seed + 13));
                float height = Mathf.Lerp(5.8f, 14.2f, Deterministic01(seed + 17));
                float depth = Mathf.Lerp(6.8f, 13.6f, Deterministic01(seed + 19));
                bool snowCap = biome.Season == HostedSeasonBiome.WinterHighland
                    || biome.Season == HostedSeasonBiome.Snowfield
                    || Deterministic01(seed + 23) > 0.74f;
                CreateRegionalMountainMassif(root, position, width, height, depth, materials.Mountains, materials.Snow, seed, snowCap);
                created++;
            }

            return created;
        }

        private static int CreateBiomeHillField(Transform root, HostedBuildContext context, HostedMaterials materials, HostedBiomeSpec biome, int biomeIndex)
        {
            int created = 0;
            for (int i = 0; i < biome.HillCount; i++)
            {
                int seed = 95000 + biomeIndex * 2113 + i * 149;
                if (!TryGetBiomeScatterPosition(context, biome, seed, biome.Radius * 0.84f, 0.56f, out int worldX, out int worldY, out Vector3 position))
                {
                    continue;
                }

                if (IsHostedVillageClearingTile(worldX, worldY))
                {
                    continue;
                }

                float width = Mathf.Lerp(8.2f, 18.6f, Deterministic01(seed + 29));
                float depth = Mathf.Lerp(6.2f, 14.8f, Deterministic01(seed + 31));
                float height = Mathf.Lerp(0.58f, 2.25f, Deterministic01(seed + 37));
                GameObject hill = CreateRegionalHillMound($"{biome.Name} Rolling Hill {i + 1}", position, width, depth, height, SeasonalHillMaterial(biome.Season, materials), seed);
                hill.transform.SetParent(root, true);
                created++;
            }

            return created;
        }

        private static int CreateBiomeDenseForest(Transform root, HostedBuildContext context, HostedMaterials materials, HostedBiomeSpec biome, int biomeIndex)
        {
            int created = 0;
            for (int i = 0; i < biome.ForestTreeCount; i++)
            {
                int seed = 97000 + biomeIndex * 2213 + i * 167;
                if (!TryGetBiomeScatterPosition(context, biome, seed, biome.Radius * 0.76f, 1.18f, out int worldX, out int worldY, out Vector3 position))
                {
                    continue;
                }

                if (IsHostedVillageClearingTile(worldX, worldY))
                {
                    continue;
                }

                bool conifer = biome.Season == HostedSeasonBiome.WinterHighland
                    || biome.Season == HostedSeasonBiome.Snowfield
                    || biome.Season == HostedSeasonBiome.SummerForest
                    || Deterministic01(seed + 17) > 0.42f;
                float height = Mathf.Lerp(2.6f, 5.6f, Deterministic01(seed + 23));
                Material canopy = SeasonalCanopyMaterial(biome.Season, materials);
                Material farCanopy = SeasonalFarCanopyMaterial(biome.Season, materials);
                if (conifer)
                {
                    CreateHighlandConifer(root, position, height, materials.TreeBark, canopy, materials.TreeBarkFar, farCanopy, seed);
                }
                else
                {
                    CreateSeasonalBroadleaf(root, position, height, materials.TreeBark, canopy, materials.TreeBarkFar, farCanopy, seed);
                }

                if ((biome.Season == HostedSeasonBiome.WinterHighland || biome.Season == HostedSeasonBiome.Snowfield) && i % 4 == 0)
                {
                    GameObject snow = CreateGroundCoverPatch($"{biome.Name} Tree Snow Dusting {i + 1}", position + Vector3.up * 0.028f, 1.6f, 1.2f, materials.Snow, seed + 89);
                    snow.transform.SetParent(root, true);
                    context.Report.snowPatches++;
                }

                created++;
            }

            return created;
        }

        private static int CreateBiomeSnowPockets(Transform root, HostedBuildContext context, HostedMaterials materials, HostedBiomeSpec biome, int biomeIndex)
        {
            int created = 0;
            for (int i = 0; i < biome.SnowPatchCount; i++)
            {
                int seed = 99000 + biomeIndex * 2297 + i * 181;
                if (!TryGetBiomeScatterPosition(context, biome, seed, biome.Radius * 0.92f, 0.60f, out int worldX, out int worldY, out Vector3 position))
                {
                    continue;
                }

                if (IsHostedVillageClearingTile(worldX, worldY))
                {
                    continue;
                }

                float width = Mathf.Lerp(2.6f, 7.8f, Deterministic01(seed + 11));
                float depth = Mathf.Lerp(2.0f, 6.2f, Deterministic01(seed + 13));
                GameObject patch = CreateGroundCoverPatch($"{biome.Name} Snow Pocket {i + 1}", position + Vector3.up * (0.026f + i * 0.0002f), width, depth, materials.Snow, seed);
                patch.transform.SetParent(root, true);

                if (Deterministic01(seed + 17) > 0.54f)
                {
                    CreateHighlandRockOutcrop(root, position + new Vector3(0f, 0.035f, 0f), Mathf.Lerp(0.34f, 0.74f, Deterministic01(seed + 19)), materials.FrostStone, materials.Snow, seed + 47);
                }

                created++;
            }

            return created;
        }

        private static bool TryGetBiomeScatterPosition(
            HostedBuildContext context,
            HostedBiomeSpec biome,
            int seed,
            float maxRadius,
            float radiusPower,
            out int worldX,
            out int worldY,
            out Vector3 position)
        {
            float angle = Deterministic01(seed + 3) * Mathf.PI * 2f;
            float radius = Mathf.Pow(Deterministic01(seed + 5), radiusPower) * maxRadius;
            worldX = biome.WorldX + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
            worldY = biome.WorldY + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
            return TryGetNaturalDressingPosition(context, worldX, worldY, seed, out position);
        }

        private static Material SeasonalGroundMaterial(HostedSeasonBiome season, HostedMaterials materials)
        {
            if (season == HostedSeasonBiome.WinterHighland || season == HostedSeasonBiome.Snowfield)
            {
                return materials.Snow;
            }

            if (season == HostedSeasonBiome.AutumnWoodland)
            {
                return materials.AutumnGround;
            }

            if (season == HostedSeasonBiome.SpringMeadow)
            {
                return materials.Flowers;
            }

            return materials.Moss;
        }

        private static Material SeasonalHillMaterial(HostedSeasonBiome season, HostedMaterials materials)
        {
            if (season == HostedSeasonBiome.WinterHighland || season == HostedSeasonBiome.Snowfield)
            {
                return materials.FrostStone;
            }

            if (season == HostedSeasonBiome.AutumnWoodland)
            {
                return materials.AutumnGround;
            }

            return materials.Hills;
        }

        private static Material SeasonalCanopyMaterial(HostedSeasonBiome season, HostedMaterials materials)
        {
            if (season == HostedSeasonBiome.WinterHighland || season == HostedSeasonBiome.Snowfield)
            {
                return materials.WinterCanopy;
            }

            if (season == HostedSeasonBiome.AutumnWoodland)
            {
                return materials.AutumnCanopy;
            }

            return materials.TreeCanopy;
        }

        private static Material SeasonalFarCanopyMaterial(HostedSeasonBiome season, HostedMaterials materials)
        {
            if (season == HostedSeasonBiome.WinterHighland || season == HostedSeasonBiome.Snowfield)
            {
                return materials.WinterCanopy;
            }

            if (season == HostedSeasonBiome.AutumnWoodland)
            {
                return materials.AutumnCanopy;
            }

            return materials.TreeCanopyFar;
        }

        private static void CreateRegionalMountainMassif(Transform parent, Vector3 position, float width, float height, float depth, Material mountainMaterial, Material snowMaterial, int seed, bool snowCap)
        {
            GameObject mountain = CreateMountain("Regional Mountain Massif", position, width, height, depth, mountainMaterial);
            mountain.transform.SetParent(parent, true);
            mountain.transform.localRotation *= Quaternion.Euler(0f, (Deterministic01(seed + 29) - 0.5f) * 28f, 0f);
            MeshRenderer mountainRenderer = mountain.GetComponent<MeshRenderer>();
            if (mountainRenderer != null)
            {
                mountainRenderer.shadowCastingMode = ShadowCastingMode.On;
                mountainRenderer.receiveShadows = true;
            }

            if (!snowCap)
            {
                return;
            }

            GameObject cap = CreateMountainSnowCap("Snow Crown", position, width, height, depth, snowMaterial, seed);
            cap.transform.SetParent(mountain.transform, true);
        }

        private static GameObject CreateMountainSnowCap(string name, Vector3 position, float width, float height, float depth, Material material, int seed)
        {
            Mesh mesh = new Mesh { name = name + " Mesh" };
            float lowerY = height * Mathf.Lerp(0.40f, 0.52f, Deterministic01(seed + 31));
            float halfWidth = width * 0.34f;
            float halfDepth = depth * 0.28f;
            mesh.vertices = new[]
            {
                new Vector3(-halfWidth, lowerY, -halfDepth),
                new Vector3(halfWidth * 0.92f, lowerY * 0.98f, -halfDepth * 0.88f),
                new Vector3(halfWidth * 0.76f, lowerY * 0.96f, halfDepth),
                new Vector3(-halfWidth * 0.88f, lowerY * 1.01f, halfDepth * 0.82f),
                new Vector3(-width * 0.08f, height * 0.76f, -depth * 0.03f),
                new Vector3(width * 0.09f, height, depth * 0.05f)
            };
            mesh.triangles = new[]
            {
                0, 4, 1,
                1, 4, 5,
                1, 5, 2,
                2, 5, 3,
                3, 5, 4,
                3, 4, 0
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject cap = new GameObject(name);
            cap.transform.position = position + Vector3.up * 0.018f;
            cap.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = cap.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return cap;
        }

        private static GameObject CreateRegionalHillMound(string name, Vector3 position, float width, float depth, float height, Material material, int seed)
        {
            GameObject hill = new GameObject(name);
            hill.isStatic = true;
            hill.transform.position = position + Vector3.up * 0.012f;
            hill.transform.rotation = Quaternion.Euler(0f, Deterministic01(seed + 43) * 360f, 0f);
            hill.AddComponent<MeshFilter>().sharedMesh = CreateRegionalHillMesh(name + " Mesh", width, depth, height, seed);
            MeshRenderer renderer = hill.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return hill;
        }

        private static Mesh CreateRegionalHillMesh(string name, float width, float depth, float height, int seed)
        {
            const int segments = 14;
            int stride = segments + 1;
            Vector3[] vertices = new Vector3[stride * stride];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[segments * segments * 6];

            int vertex = 0;
            for (int z = 0; z <= segments; z++)
            {
                float vz = (float)z / segments;
                for (int x = 0; x <= segments; x++)
                {
                    float vx = (float)x / segments;
                    float px = (vx - 0.5f) * width;
                    float pz = (vz - 0.5f) * depth;
                    float nx = (vx - 0.5f) * 2f;
                    float nz = (vz - 0.5f) * 2f;
                    float dist = Mathf.Sqrt(nx * nx + nz * nz);
                    float dome = Mathf.Clamp01(1f - dist);
                    dome = Mathf.SmoothStep(0f, 1f, dome);
                    float ripple = Mathf.Sin((vx * 4.9f + seed * 0.013f) * Mathf.PI) * Mathf.Sin((vz * 3.7f + seed * 0.017f) * Mathf.PI) * height * 0.055f;
                    vertices[vertex] = new Vector3(px, dome * height + ripple, pz);
                    uv[vertex] = new Vector2(vx * Mathf.Max(1f, width * 0.16f), vz * Mathf.Max(1f, depth * 0.16f));
                    vertex++;
                }
            }

            int tri = 0;
            for (int z = 0; z < segments; z++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int i = z * stride + x;
                    triangles[tri++] = i;
                    triangles[tri++] = i + stride;
                    triangles[tri++] = i + 1;
                    triangles[tri++] = i + 1;
                    triangles[tri++] = i + stride;
                    triangles[tri++] = i + stride + 1;
                }
            }

            Mesh mesh = new Mesh { name = name };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void CreateSeasonalBroadleaf(Transform parent, Vector3 position, float height, Material barkMaterial, Material canopyMaterial, Material farBarkMaterial, Material farCanopyMaterial, int seed)
        {
            GameObject tree = new GameObject("Seasonal Dense Broadleaf Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = position;
            tree.transform.rotation = Quaternion.Euler(0f, Deterministic01(seed + 5) * 360f, 0f);

            float trunkHeight = height * Mathf.Lerp(0.42f, 0.54f, Deterministic01(seed + 9));
            float trunkRadius = height * Mathf.Lerp(0.030f, 0.046f, Deterministic01(seed + 11));
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Seasonal Broadleaf Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight * 0.45f, 0f);
            trunk.transform.localRotation = Quaternion.Euler(0f, Deterministic01(seed + 13) * 360f, (Deterministic01(seed + 17) - 0.5f) * 3.2f);
            trunk.transform.localScale = new Vector3(trunkRadius, trunkHeight * 0.46f, trunkRadius);
            MeshRenderer trunkRenderer = trunk.GetComponent<MeshRenderer>();
            trunkRenderer.sharedMaterial = barkMaterial;
            trunkRenderer.shadowCastingMode = ShadowCastingMode.On;
            trunkRenderer.receiveShadows = true;
            RemoveCollider(trunk);

            int branches = 5;
            for (int i = 0; i < branches; i++)
            {
                float angle = i * Mathf.PI * 2f / branches + Deterministic01(seed + i * 23) * 0.36f;
                Vector3 start = new Vector3(0f, trunkHeight * Mathf.Lerp(0.55f, 0.86f, i / (float)Mathf.Max(1, branches - 1)), 0f);
                Vector3 end = start + new Vector3(Mathf.Cos(angle), 0.32f + Deterministic01(seed + i * 31) * 0.22f, Mathf.Sin(angle)) * height * Mathf.Lerp(0.20f, 0.32f, Deterministic01(seed + i * 37));
                CreateFoliageBranch(tree.transform, $"Seasonal Crown Branch {i + 1}", start, end, trunkRadius * 0.42f, barkMaterial);
            }

            float canopyWidth = height * Mathf.Lerp(0.72f, 0.98f, Deterministic01(seed + 41));
            float canopyHeight = height * Mathf.Lerp(0.34f, 0.46f, Deterministic01(seed + 43));
            int lobes = 7;
            for (int i = 0; i < lobes; i++)
            {
                float angle = i * Mathf.PI * 2f / lobes + Deterministic01(seed + i * 47) * 0.32f;
                float radius = i == 0 ? 0f : canopyWidth * Mathf.Lerp(0.18f, 0.42f, Deterministic01(seed + i * 53));
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, trunkHeight + canopyHeight * Mathf.Lerp(0.08f, 0.54f, Deterministic01(seed + i * 59)), Mathf.Sin(angle) * radius);
                Vector3 scale = new Vector3(
                    canopyWidth * Mathf.Lerp(0.34f, 0.54f, Deterministic01(seed + i * 61)),
                    canopyHeight * Mathf.Lerp(0.32f, 0.48f, Deterministic01(seed + i * 67)),
                    canopyWidth * Mathf.Lerp(0.30f, 0.52f, Deterministic01(seed + i * 71)));
                CreateFoliageSpray(
                    tree.transform,
                    $"Seasonal Leaf Mass {i + 1}",
                    offset,
                    scale,
                    angle * Mathf.Rad2Deg,
                    canopyMaterial,
                    0.044f + i * 0.004f,
                    0.76f + i * 0.055f);
            }

            Renderer[] highRenderers = tree.GetComponentsInChildren<Renderer>(true);
            Renderer[] farRenderers = CreateBroadleafFarLodProxy(tree.transform, trunkHeight, canopyWidth, canopyHeight, farBarkMaterial, farCanopyMaterial, seed);
            AddGeneratedFoliageLodGroup(tree, highRenderers, farRenderers);
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
                    if (IsHostedVillageClearingTile(worldX, worldY))
                    {
                        continue;
                    }

                    if (!TryGetNaturalDressingPosition(context, worldX, worldY, seed, out Vector3 position))
                    {
                        continue;
                    }

                    float height = 2.45f + Deterministic01(seed + 37) * 2.35f;
                    CreateHighlandConifer(root.transform, position, height, materials.TreeBark, materials.TreeCanopy, materials.TreeBarkFar, materials.TreeCanopyFar, seed);
                    created++;
                }
            }

            context.Report.enhancedFoliageObjects += created;
            context.Report.foliageLodProxies += created;
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
            if (!UseFlatHostedVisualBase && (flags & 1) == 1)
            {
                return false;
            }

            float scatterX = (Deterministic01(seed + 101) - 0.5f) * TileScale * 0.86f;
            float scatterZ = (Deterministic01(seed + 103) - 0.5f) * TileScale * 0.86f;
            position = WorldTilePosition(context, worldX, worldY) + new Vector3(scatterX, 0.045f, scatterZ);
            return true;
        }

        private static void CreateHighlandConifer(Transform parent, Vector3 position, float height, Material barkMaterial, Material canopyMaterial, Material farBarkMaterial, Material farCanopyMaterial, int seed)
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

            Renderer[] highRenderers = tree.GetComponentsInChildren<Renderer>(true);
            Renderer[] farRenderers = CreateConiferFarLodProxy(tree.transform, height, trunkRadius, farBarkMaterial, farCanopyMaterial, seed);
            AddGeneratedFoliageLodGroup(tree, highRenderers, farRenderers);
        }

        private static Renderer[] CreateConiferFarLodProxy(Transform parent, float height, float trunkRadius, Material barkMaterial, Material canopyMaterial, int seed)
        {
            List<Renderer> renderers = new List<Renderer>(4);
            GameObject root = new GameObject("Far Conifer Silhouette Proxy");
            root.transform.SetParent(parent, false);

            float trunkHeight = height * 0.62f;
            renderers.Add(CreateFoliageLodPrimitive(
                root.transform,
                PrimitiveType.Cylinder,
                "Far Readable Trunk",
                barkMaterial,
                new Vector3(0f, trunkHeight * 0.45f, 0f),
                Quaternion.identity,
                new Vector3(Mathf.Max(0.030f, trunkRadius * 1.35f), trunkHeight * 0.45f, Mathf.Max(0.030f, trunkRadius * 1.35f))));

            for (int layer = 0; layer < 3; layer++)
            {
                float t = layer / 2f;
                float radius = height * Mathf.Lerp(0.34f, 0.13f, t);
                float layerHeight = height * Mathf.Lerp(0.30f, 0.20f, t);
                Vector3 localPosition = new Vector3(
                    (Deterministic01(seed + layer * 31) - 0.5f) * height * 0.018f,
                    height * Mathf.Lerp(0.22f, 0.74f, t),
                    (Deterministic01(seed + layer * 37) - 0.5f) * height * 0.018f);
                renderers.Add(CreateFoliageLodMesh(
                    root.transform,
                    $"Far Conifer Mass {layer + 1}",
                    CreateTaperedConeMesh($"Far Conifer Proxy Mesh {seed}_{layer}", 8, radius, radius * 0.16f, layerHeight, seed + layer * 43),
                    canopyMaterial,
                    localPosition,
                    Quaternion.Euler(0f, layer * 41f + Deterministic01(seed + layer * 47) * 22f, 0f),
                    Vector3.one));
            }

            return renderers.ToArray();
        }

        private static Renderer[] CreateBroadleafFarLodProxy(Transform parent, float trunkHeight, float canopyWidth, float canopyHeight, Material barkMaterial, Material canopyMaterial, int seed)
        {
            List<Renderer> renderers = new List<Renderer>(4);
            GameObject root = new GameObject("Far Broadleaf Silhouette Proxy");
            root.transform.SetParent(parent, false);

            renderers.Add(CreateFoliageLodPrimitive(
                root.transform,
                PrimitiveType.Cylinder,
                "Far Broadleaf Trunk",
                barkMaterial,
                new Vector3(0f, trunkHeight * 0.43f, 0f),
                Quaternion.identity,
                new Vector3(0.075f + canopyWidth * 0.018f, trunkHeight * 0.43f, 0.075f + canopyWidth * 0.018f)));

            renderers.Add(CreateFoliageLodPrimitive(
                root.transform,
                PrimitiveType.Sphere,
                "Far Broadleaf Crown",
                canopyMaterial,
                new Vector3(0f, trunkHeight + canopyHeight * 0.18f, 0f),
                Quaternion.Euler(0f, Deterministic01(seed + 17) * 360f, 0f),
                new Vector3(canopyWidth * 0.66f, canopyHeight * 0.48f, canopyWidth * 0.60f)));
            renderers.Add(CreateFoliageLodPrimitive(
                root.transform,
                PrimitiveType.Sphere,
                "Far Broadleaf Side Crown",
                canopyMaterial,
                new Vector3(canopyWidth * 0.18f, trunkHeight + canopyHeight * 0.04f, -canopyWidth * 0.06f),
                Quaternion.Euler(0f, Deterministic01(seed + 23) * 360f, 0f),
                new Vector3(canopyWidth * 0.45f, canopyHeight * 0.34f, canopyWidth * 0.42f)));

            return renderers.ToArray();
        }

        private static Renderer CreateFoliageLodPrimitive(Transform parent, PrimitiveType primitiveType, string name, Material material, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.isStatic = true;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            Renderer renderer = ConfigureFarFoliageRenderer(part, material);
            RemoveCollider(part);
            return renderer;
        }

        private static Renderer CreateFoliageLodMesh(Transform parent, string name, Mesh mesh, Material material, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject part = new GameObject(name);
            part.isStatic = true;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            return ConfigureFarFoliageRenderer(part, material);
        }

        private static Renderer ConfigureFarFoliageRenderer(GameObject part, Material material)
        {
            MeshRenderer renderer = part.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = part.AddComponent<MeshRenderer>();
            }

            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return renderer;
        }

        private static void AddGeneratedFoliageLodGroup(GameObject root, Renderer[] highRenderers, Renderer[] farRenderers)
        {
            if (highRenderers == null || highRenderers.Length == 0 || farRenderers == null || farRenderers.Length == 0)
            {
                return;
            }

            LODGroup lodGroup = root.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = root.AddComponent<LODGroup>();
            }

            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;
            lodGroup.SetLODs(new[]
            {
                new LOD(0.10f, highRenderers),
                new LOD(0.026f, farRenderers)
            });
            lodGroup.RecalculateBounds();
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
            BuildIntroQuestController(context, player.transform);
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

        private static void BuildIntroQuestController(HostedBuildContext context, Transform player)
        {
            GameObject questObject = new GameObject("Gallows Dawn Intro Quest");
            PsychoIntroQuestController quest = questObject.AddComponent<PsychoIntroQuestController>();
            Vector3 intake = TerrainSurfacePosition(WorldTilePosition(context, 3087, 3490), 0.08f);
            Vector3 cell = TerrainSurfacePosition(WorldTilePosition(context, 3083, 3492), 0.08f);
            Vector3 release = TerrainSurfacePosition(WorldTilePosition(context, 3087, 3483), 0.08f);
            Vector3 village = TerrainSurfacePosition(WorldTilePosition(context, 3086, 3522), 0.08f);
            quest.Configure(player, intake, cell, release, village, 9.0f);
        }

        private static void BuildPlayerVisual(Transform parent, HostedMaterials materials, PsychoMirrorDatabase database, HostedPlayerSave playerSave)
        {
            if (PsychoArtAssetResolver.TryInstantiatePlayer(PlayerDisplayName(playerSave), parent, out GameObject artPlayer))
            {
                artPlayer.name = "Psycho Hero Art Prefab";
                BuildResolvedHeroGearOverlay(parent, materials, playerSave);
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

        private static void BuildResolvedHeroGearOverlay(Transform parent, HostedMaterials materials, HostedPlayerSave playerSave)
        {
            GameObject overlay = new GameObject("Psycho Hero Gear Overlay");
            overlay.transform.SetParent(parent, false);

            PsychoMirrorItem heroCape = CreateHostedHeroItem("Psycho Highland Cloak", "Cape", "Cloth");
            PsychoMirrorItem heroAmulet = CreateHostedHeroItem("Psycho Dawn Crystal Amulet", "Amulet", "Crystal");
            PsychoMirrorItem heroBlade = CreateHostedHeroItem("Psycho Frost Runeblade", "TwoHandedWeapon", "Metal");
            PsychoMirrorItem heroShield = CreateHostedHeroItem("Psycho Rune-Kite Ward", "Shield", "Metal");

            CreateHighlandFurMantle(overlay.transform, materials);
            CreatePlayerAmulet(overlay.transform, heroAmulet, materials);
            CreateCapePanel(overlay.transform, materials.PlayerCloth, materials, heroCape);
            CreatePsychoRuneblade(overlay.transform, materials, heroBlade);
            CreatePsychoRuneKiteShield(overlay.transform, materials, heroShield);
            CreatePlayerNameplate(overlay.transform, playerSave);

            PsychoHumanoidPresentationRig overlayRig = overlay.AddComponent<PsychoHumanoidPresentationRig>();
            overlayRig.Configure(parent, 0.0035f, 0.010f, 0.80f, 0.40f, 0.65f);
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
            float height = UseFlatHostedVisualBase ? 0f : -landscape.Heights[0, x, y] * HeightScale;
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
            int streaks = CreateWaterSurfaceStreaks(water.transform, "River Surface Current", width, depth, foamMaterial, 6, 9021);
            buildContext.Report.waterDepthChannels++;
            buildContext.Report.waterFoamEdges += 2;
            buildContext.Report.waterSurfaceStreaks += streaks;

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

        private static int CreateWaterSurfaceStreaks(Transform parent, string name, float width, float depth, Material material, int count, int seedBase)
        {
            int created = 0;
            for (int i = 0; i < count; i++)
            {
                float xOffset = Mathf.Lerp(-width * 0.22f, width * 0.22f, Deterministic01(seedBase + i * 37));
                float zOffset = Mathf.Lerp(-depth * 0.32f, depth * 0.32f, Deterministic01(seedBase + i * 41));
                float streakWidth = Mathf.Lerp(width * 0.055f, width * 0.12f, Deterministic01(seedBase + i * 43));
                float streakDepth = Mathf.Lerp(depth * 0.18f, depth * 0.36f, Deterministic01(seedBase + i * 47));
                float yaw = Mathf.Lerp(-8f, 8f, Deterministic01(seedBase + i * 53));
                CreateWaterStreakRibbon(parent, $"{name} {i + 1}", new Vector3(xOffset, 0.041f + i * 0.0015f, zOffset), streakWidth, streakDepth, yaw, material, seedBase + i * 59);
                created++;
            }

            return created;
        }

        private static void CreateWaterStreakRibbon(Transform parent, string name, Vector3 localPosition, float width, float depth, float yaw, Material material, int seed)
        {
            const int segments = 14;
            Mesh mesh = new Mesh { name = name + " Mesh" };
            Vector3[] vertices = new Vector3[(segments + 1) * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[segments * 6];

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float z = (t - 0.5f) * depth;
                float wobble = Mathf.Sin(t * Mathf.PI * 3.0f + seed * 0.11f) * width * 0.18f
                    + Mathf.Sin(t * Mathf.PI * 9.0f + seed * 0.07f) * width * 0.06f;
                float taper = Mathf.Sin(t * Mathf.PI);
                float halfWidth = width * (0.22f + taper * 0.78f);
                int v = i * 2;
                vertices[v] = new Vector3(wobble - halfWidth, 0f, z);
                vertices[v + 1] = new Vector3(wobble + halfWidth, 0f, z);
                uv[v] = new Vector2(0f, t * 2.2f);
                uv[v + 1] = new Vector2(1f, t * 2.2f);
                colors[v] = new Color(1f, 1f, 1f, taper);
                colors[v + 1] = new Color(1f, 1f, 1f, taper);
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
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject streak = new GameObject(name);
            streak.transform.SetParent(parent, false);
            streak.transform.localPosition = localPosition;
            streak.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            streak.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = streak.AddComponent<MeshRenderer>();
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
                Grass = LoadOrCreateTexturedMaterial(GrassMaterialPath, "Grass", new Color(0.28f, 0.47f, 0.24f, 1f), 0.18f, new Vector2(11.0f, 11.0f), 0.86f),
                Flowers = LoadOrCreateTexturedMaterial(FlowerMaterialPath, "Organic", new Color(0.76f, 0.63f, 0.34f, 1f), 0.24f, new Vector2(6.4f, 6.4f), 0.62f),
                Herbs = LoadOrCreateTexturedMaterial(HerbMaterialPath, "Leaf", new Color(0.34f, 0.53f, 0.27f, 1f), 0.20f, new Vector2(5.6f, 5.6f), 0.72f),
                WildflowerBlue = LoadOrCreateSolidMaterial(WildflowerBlueMaterialPath, new Color(0.38f, 0.52f, 0.88f, 1f), 0.26f),
                WildflowerPurple = LoadOrCreateSolidMaterial(WildflowerPurpleMaterialPath, new Color(0.62f, 0.42f, 0.82f, 1f), 0.28f),
                WildflowerGold = LoadOrCreateSolidMaterial(WildflowerGoldMaterialPath, new Color(0.96f, 0.75f, 0.28f, 1f), 0.22f),
                Reeds = LoadOrCreateTexturedMaterial(ReedMaterialPath, "Leaf", new Color(0.26f, 0.37f, 0.20f, 1f), 0.16f, new Vector2(4.2f, 6.6f), 0.82f),
                Water = LoadOrCreateTexturedMaterial(WaterMaterialPath, "Water", new Color(0.035f, 0.18f, 0.27f, 0.62f), 0.92f, new Vector2(2.8f, 7.8f), 1.08f),
                Hills = LoadOrCreateTexturedMaterial(HillMaterialPath, "Grass", new Color(0.24f, 0.38f, 0.22f, 1f), 0.22f, new Vector2(7.6f, 7.6f), 0.78f),
                Mountains = LoadOrCreateTexturedMaterial(MountainMaterialPath, "Mountain", new Color(0.39f, 0.40f, 0.39f, 1f), 0.42f, new Vector2(3.8f, 3.8f), 0.98f),
                Snow = LoadOrCreatePlainMaterial(SnowMaterialPath, new Color(0.92f, 0.97f, 1.00f, 1f), 0.64f),
                AutumnCanopy = LoadOrCreateTexturedMaterial(AutumnCanopyMaterialPath, "Leaf", new Color(0.70f, 0.42f, 0.17f, 1f), 0.18f, new Vector2(4.0f, 4.0f), 0.52f),
                WinterCanopy = LoadOrCreateTexturedMaterial(WinterCanopyMaterialPath, "Leaf", new Color(0.36f, 0.48f, 0.44f, 1f), 0.16f, new Vector2(3.8f, 3.8f), 0.44f),
                AutumnGround = LoadOrCreateTexturedMaterial(AutumnGroundMaterialPath, "Organic", new Color(0.44f, 0.30f, 0.16f, 1f), 0.20f, new Vector2(6.8f, 6.8f), 0.66f),
                Cloud = LoadOrCreateSolidMaterial(CloudMaterialPath, new Color(0.68f, 0.76f, 0.82f, 0.18f), 0.12f),
                HorizonMist = LoadOrCreateSolidMaterial(HorizonMistMaterialPath, new Color(0.54f, 0.62f, 0.70f, 0.085f), 0.08f),
                TreeCanopy = LoadOrCreateTexturedMaterial(TreeCanopyMaterialPath, "Leaf", new Color(0.20f, 0.36f, 0.19f, 1f), 0.14f, new Vector2(4.8f, 4.8f), 0.58f),
                TreeCanopyFar = LoadOrCreateTexturedMaterial(TreeCanopyFarMaterialPath, "Leaf", new Color(0.24f, 0.43f, 0.22f, 1f), 0.10f, new Vector2(3.2f, 3.2f), 0.28f),
                TreeBark = LoadOrCreateTexturedMaterial(TreeBarkMaterialPath, "Wood", new Color(0.26f, 0.18f, 0.12f, 1f), 0.16f, new Vector2(3.2f, 5.6f), 0.74f),
                TreeBarkFar = LoadOrCreateTexturedMaterial(TreeBarkFarMaterialPath, "Wood", new Color(0.34f, 0.25f, 0.17f, 1f), 0.10f, new Vector2(2.4f, 4.0f), 0.24f),
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
                LandmarkGlass = LoadOrCreateSolidMaterial(LandmarkGlassMaterialPath, new Color(0.40f, 0.63f, 0.72f, 0.46f), 0.72f),
                WildlifeHide = LoadOrCreateTexturedMaterial(WildlifeHideMaterialPath, "Leather", new Color(0.58f, 0.38f, 0.20f, 1f), 0.22f, new Vector2(2.4f, 2.4f), 0.42f),
                MammothFur = LoadOrCreateTexturedMaterial(MammothFurMaterialPath, "Leather", new Color(0.27f, 0.20f, 0.15f, 1f), 0.34f, new Vector2(2.6f, 3.4f), 0.60f),
                MammothTusk = LoadOrCreateSolidMaterial(MammothTuskMaterialPath, new Color(0.82f, 0.76f, 0.62f, 1f), 0.35f),
                GiantSkin = LoadOrCreateSolidMaterial(GiantSkinMaterialPath, new Color(0.62f, 0.47f, 0.34f, 1f), 0.24f)
            };
            ConfigureTransparent(materials.Water);
            ConfigureTransparent(materials.Cloud);
            ConfigureTransparent(materials.HorizonMist);
            ConfigureTransparent(materials.WaterFoam);
            ConfigureTransparent(materials.WaterDepth);
            ConfigureTransparent(materials.LandmarkGlass);
            ConfigureDoubleSided(materials.CliffFace);
            ConfigureDoubleSided(materials.Snow);
            ConfigureDoubleSided(materials.Flowers);
            ConfigureDoubleSided(materials.Herbs);
            ConfigureDoubleSided(materials.WildflowerBlue);
            ConfigureDoubleSided(materials.WildflowerPurple);
            ConfigureDoubleSided(materials.WildflowerGold);
            ConfigureDoubleSided(materials.MammothFur);
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

        private static Material LoadOrCreatePlainMaterial(string assetPath, Color color, float smoothness)
        {
            Material material = LoadOrCreateSolidMaterial(assetPath, color, smoothness);
            material.mainTexture = null;
            if (material.HasProperty("_BumpMap"))
            {
                material.SetTexture("_BumpMap", null);
                material.DisableKeyword("_NORMALMAP");
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
            public Material Herbs;
            public Material WildflowerBlue;
            public Material WildflowerPurple;
            public Material WildflowerGold;
            public Material Reeds;
            public Material Water;
            public Material Hills;
            public Material Mountains;
            public Material Snow;
            public Material AutumnCanopy;
            public Material WinterCanopy;
            public Material AutumnGround;
            public Material Cloud;
            public Material HorizonMist;
            public Material TreeCanopy;
            public Material TreeCanopyFar;
            public Material TreeBark;
            public Material TreeBarkFar;
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
            public Material WildlifeHide;
            public Material MammothFur;
            public Material MammothTusk;
            public Material GiantSkin;
        }

        private enum HostedSeasonBiome
        {
            SpringMeadow,
            SummerForest,
            AutumnWoodland,
            WinterHighland,
            Snowfield
        }

        private enum HostedSettlementTier
        {
            Village,
            Town,
            City
        }

        private readonly struct HostedMeadowSpec
        {
            public readonly string Name;
            public readonly int WorldX;
            public readonly int WorldY;
            public readonly float Radius;
            public readonly int PatchCount;
            public readonly int WildflowerCount;
            public readonly int HerbCount;

            public HostedMeadowSpec(string name, int worldX, int worldY, float radius, int patchCount, int wildflowerCount, int herbCount)
            {
                Name = name;
                WorldX = worldX;
                WorldY = worldY;
                Radius = radius;
                PatchCount = patchCount;
                WildflowerCount = wildflowerCount;
                HerbCount = herbCount;
            }
        }

        private readonly struct HostedWildlifeSpec
        {
            public readonly string Name;
            public readonly string Kind;
            public readonly int WorldX;
            public readonly int WorldY;
            public readonly int Count;
            public readonly float Radius;

            public HostedWildlifeSpec(string name, string kind, int worldX, int worldY, int count, float radius)
            {
                Name = name;
                Kind = kind;
                WorldX = worldX;
                WorldY = worldY;
                Count = count;
                Radius = radius;
            }
        }

        private readonly struct HostedGiantCampSpec
        {
            public readonly string Name;
            public readonly int WorldX;
            public readonly int WorldY;
            public readonly int GiantCount;
            public readonly int MammothCount;
            public readonly float Radius;

            public HostedGiantCampSpec(string name, int worldX, int worldY, int giantCount, int mammothCount, float radius)
            {
                Name = name;
                WorldX = worldX;
                WorldY = worldY;
                GiantCount = giantCount;
                MammothCount = mammothCount;
                Radius = radius;
            }
        }

        private readonly struct HostedBiomeSpec
        {
            public readonly string Name;
            public readonly int WorldX;
            public readonly int WorldY;
            public readonly float Radius;
            public readonly HostedSeasonBiome Season;
            public readonly int MountainCount;
            public readonly int HillCount;
            public readonly int ForestTreeCount;
            public readonly int SnowPatchCount;
            public readonly float Yaw;

            public HostedBiomeSpec(string name, int worldX, int worldY, float radius, HostedSeasonBiome season, int mountainCount, int hillCount, int forestTreeCount, int snowPatchCount, float yaw)
            {
                Name = name;
                WorldX = worldX;
                WorldY = worldY;
                Radius = radius;
                Season = season;
                MountainCount = mountainCount;
                HillCount = hillCount;
                ForestTreeCount = forestTreeCount;
                SnowPatchCount = snowPatchCount;
                Yaw = yaw;
            }
        }

        private readonly struct HostedVillageSpec
        {
            public readonly string Name;
            public readonly int WorldX;
            public readonly int WorldY;
            public readonly int HouseCount;
            public readonly int NpcCount;
            public readonly float Yaw;
            public readonly float Scale;
            public readonly HostedSettlementTier Tier;

            public HostedVillageSpec(string name, int worldX, int worldY, int houseCount, int npcCount, float yaw, float scale, HostedSettlementTier tier)
            {
                Name = name;
                WorldX = worldX;
                WorldY = worldY;
                HouseCount = houseCount;
                NpcCount = npcCount;
                Yaw = yaw;
                Scale = scale;
                Tier = tier;
            }
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
            public bool cleanFlatBase;
            public int loadedRegions;
            public int missingRegions;
            public int decodedObjectPlacements;
            public int placedObjects;
            public int skippedObjects;
            public int flatBaseTerrainRegions;
            public int cleanBaseSkippedCacheObjects;
            public int cleanBaseSkippedCacheNpcs;
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
            public int hostedSettlements;
            public int hostedVillages;
            public int hostedTowns;
            public int hostedCities;
            public int hostedVillageNpcs;
            public int hostedSettlementNpcs;
            public int villageClearedObjects;
            public int introQuestDressingObjects;
            public int seasonalBiomeRegions;
            public int mountainMassifs;
            public int hillMounds;
            public int denseForestTrees;
            public int snowPatches;
            public int seasonalDressingObjects;
            public int lushMeadowPatches;
            public int wildflowerClusters;
            public int herbClusters;
            public int wildlifeNpcs;
            public int giantCamps;
            public int giants;
            public int mammothCompanions;
            public int terrainCollisionSamples;
            public int terrainCollisionMisses;
            public int groundCoverPatches;
            public int enhancedFoliageObjects;
            public int foliageLodProxies;
            public int cliffDressingObjects;
            public int waterFoamEdges;
            public int waterDepthChannels;
            public int waterSurfaceStreaks;
            public int horizonMistPanels;
            public int landmarkDressingObjects;
        }
    }
}
