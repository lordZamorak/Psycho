using System;
using System.Collections.Generic;
using System.IO;
using Psycho.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Psycho.Editor
{
    public static class PsychoStarterArtManifestBuilder
    {
        private const string ManifestPath = "Assets/Resources/PsychoArt/PsychoArtAssetManifest.asset";
        private const string PrefabRoot = "Assets/Resources/PsychoArt/Prefabs/Starter";
        private const string MaterialRoot = "Assets/Resources/PsychoArt/Materials/Starter";
        private const string CharacterFbxRoot = "Assets/PsychoArtSource/Characters/QuaterniusRpgCharacters/FBX";
        private const string CharacterTextureRoot = "Assets/PsychoArtSource/Characters/QuaterniusRpgCharacters/Textures";
        private const string FoliageFbxRoot = "Assets/PsychoArtSource/Foliage/QuaterniusStylizedNature/FBX";
        private const string BuildingFbxRoot = "Assets/PsychoArtSource/Environment/KayKitMedievalBuilder/FBX";

        [MenuItem("Psycho/Art Pipeline/Build Starter CC0 Manifest")]
        public static void BuildStarterManifest()
        {
            PsychoArtPipelineBootstrap.CreateDefaultManifest();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            ConfigureModelImports();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            EnsureFolder("Assets/Resources/PsychoArt", "Prefabs");
            EnsureFolder("Assets/Resources/PsychoArt/Prefabs", "Starter");
            EnsureFolder("Assets/Resources/PsychoArt", "Materials");
            EnsureFolder("Assets/Resources/PsychoArt/Materials", "Starter");

            Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            BuildCharacterPrefab(prefabs, "Warrior", "Warrior_Player", 1.85f);
            BuildCharacterPrefab(prefabs, "Monk", "Monk_Citizen", 1.72f);
            BuildCharacterPrefab(prefabs, "Rogue", "Rogue_Merchant", 1.72f);
            BuildCharacterPrefab(prefabs, "Cleric", "Cleric_Banker", 1.72f);
            BuildCharacterPrefab(prefabs, "Ranger", "Ranger_Citizen", 1.72f);
            BuildCharacterPrefab(prefabs, "Wizard", "Wizard_Citizen", 1.72f);

            BuildStaticPrefab(prefabs, FoliageFbxRoot, "CommonTree_1", "CommonTree_1", 3.4f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "CommonTree_2", "CommonTree_2", 3.7f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "CommonTree_3", "CommonTree_3", 3.2f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Pine_1", "Pine_1", 4.2f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Pine_2", "Pine_2", 4.5f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Pine_3", "Pine_3", 4.0f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "TwistedTree_1", "TwistedTree_1", 3.8f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "DeadTree_1", "DeadTree_1", 3.3f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Rock_Medium_1", "Rock_Medium_1", 0.75f, false);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Rock_Medium_2", "Rock_Medium_2", 0.68f, false);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Rock_Medium_3", "Rock_Medium_3", 0.82f, false);

            BuildStaticPrefab(prefabs, BuildingFbxRoot, "house", "KayKit_House", 2.9f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "market", "KayKit_Market", 2.7f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "barracks", "KayKit_Barracks", 3.1f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "castle", "KayKit_Castle", 4.4f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "wall_gate", "KayKit_WallGate", 2.7f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "wall_straight", "KayKit_Wall", 1.7f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "watchtower", "KayKit_Watchtower", 4.1f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "well", "KayKit_Well", 1.15f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "bridge", "KayKit_Bridge", 0.9f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "mine", "KayKit_Mine", 2.2f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "lumbermill", "KayKit_Lumbermill", 2.8f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "watermill", "KayKit_Watermill", 3.1f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "mill", "KayKit_Mill", 3.3f, false);

            PsychoArtAssetManifest manifest = AssetDatabase.LoadAssetAtPath<PsychoArtAssetManifest>(ManifestPath);
            if (manifest == null)
            {
                throw new InvalidOperationException($"Missing art manifest at {ManifestPath}.");
            }

            manifest.SetEntriesForEditor(BuildManifestEntries(prefabs).ToArray());
            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Built starter CC0 art manifest with {manifest.Entries.Length} entries and {prefabs.Count} prefabs.");
        }

        public static void BuildStarterManifestBatch()
        {
            BuildStarterManifest();
        }

        private static List<PsychoArtAssetEntry> BuildManifestEntries(Dictionary<string, GameObject> prefabs)
        {
            List<PsychoArtAssetEntry> entries = new List<PsychoArtAssetEntry>();

            Add(entries, prefabs, "Warrior_Player", PsychoArtAssetKind.Player, "Psycho Warrior Player", targetHeight: 1.85f, matchAny: true, localEuler: new Vector3(0f, 180f, 0f));

            Add(entries, prefabs, "Monk_Citizen", PsychoArtAssetKind.Npc, "Citizen Monk", visualClass: "Citizen", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Ranger_Citizen", PsychoArtAssetKind.Npc, "Citizen Ranger", visualClass: "Citizen", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Wizard_Citizen", PsychoArtAssetKind.Npc, "Citizen Wizard", visualClass: "Citizen", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Rogue_Merchant", PsychoArtAssetKind.Npc, "Merchant Rogue", visualClass: "Merchant", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Cleric_Banker", PsychoArtAssetKind.Npc, "Banker Cleric", visualClass: "Banker", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Warrior_Player", PsychoArtAssetKind.Npc, "Guard Warrior", visualClass: "Guard", targetHeight: 1.78f, localEuler: new Vector3(0f, 180f, 0f));

            Add(entries, prefabs, "CommonTree_1", PsychoArtAssetKind.Object, "Common Tree A", visualClass: "Tree", targetHeight: 3.4f, markStatic: true, cullHeight: 0.012f);
            Add(entries, prefabs, "CommonTree_2", PsychoArtAssetKind.Object, "Common Tree B", visualClass: "Tree", targetHeight: 3.7f, markStatic: true, cullHeight: 0.012f);
            Add(entries, prefabs, "CommonTree_3", PsychoArtAssetKind.Object, "Common Tree C", visualClass: "Tree", targetHeight: 3.2f, markStatic: true, cullHeight: 0.012f);
            Add(entries, prefabs, "Pine_1", PsychoArtAssetKind.Object, "Pine Tree A", visualClass: "Tree", nameFragments: new[] { "pine", "conifer", "evergreen" }, targetHeight: 4.2f, markStatic: true, cullHeight: 0.010f);
            Add(entries, prefabs, "Pine_2", PsychoArtAssetKind.Object, "Pine Tree B", visualClass: "Tree", nameFragments: new[] { "pine", "conifer", "evergreen" }, targetHeight: 4.5f, markStatic: true, cullHeight: 0.010f);
            Add(entries, prefabs, "TwistedTree_1", PsychoArtAssetKind.Object, "Twisted Tree", visualClass: "Tree", nameFragments: new[] { "dead tree", "dying tree", "twisted tree" }, targetHeight: 3.8f, markStatic: true, cullHeight: 0.010f);
            Add(entries, prefabs, "DeadTree_1", PsychoArtAssetKind.Object, "Dead Tree", visualClass: "Tree", nameFragments: new[] { "dead tree", "burnt tree", "hollow tree" }, targetHeight: 3.3f, markStatic: true, cullHeight: 0.010f);

            Add(entries, prefabs, "Rock_Medium_1", PsychoArtAssetKind.Object, "Rock A", visualClass: "Rock", targetHeight: 0.75f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "Rock_Medium_2", PsychoArtAssetKind.Object, "Rock B", visualClass: "Rock", targetHeight: 0.68f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "Rock_Medium_3", PsychoArtAssetKind.Object, "Rock C", visualClass: "Rock", targetHeight: 0.82f, markStatic: true, cullHeight: 0.020f);

            Add(entries, prefabs, "KayKit_House", PsychoArtAssetKind.Object, "Medieval House", nameFragments: new[] { "house", "home", "hut" }, targetHeight: 2.9f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Market", PsychoArtAssetKind.Object, "Medieval Market", nameFragments: new[] { "market", "stall", "shop" }, targetHeight: 2.7f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Barracks", PsychoArtAssetKind.Object, "Barracks", nameFragments: new[] { "barracks", "guild", "hall" }, targetHeight: 3.1f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Castle", PsychoArtAssetKind.Object, "Castle", nameFragments: new[] { "castle", "keep", "fortress" }, targetHeight: 4.4f, markStatic: true, cullHeight: 0.014f);
            Add(entries, prefabs, "KayKit_WallGate", PsychoArtAssetKind.Object, "Wall Gate", nameFragments: new[] { "gate" }, targetHeight: 2.7f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Wall", PsychoArtAssetKind.Object, "Wall", nameFragments: new[] { "wall" }, targetHeight: 1.7f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "KayKit_Watchtower", PsychoArtAssetKind.Object, "Watchtower", nameFragments: new[] { "watchtower", "tower" }, targetHeight: 4.1f, markStatic: true, cullHeight: 0.014f);
            Add(entries, prefabs, "KayKit_Well", PsychoArtAssetKind.Object, "Well", nameFragments: new[] { "well" }, targetHeight: 1.15f, markStatic: true, cullHeight: 0.024f);
            Add(entries, prefabs, "KayKit_Bridge", PsychoArtAssetKind.Object, "Bridge", nameFragments: new[] { "bridge" }, targetHeight: 0.9f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "KayKit_Mine", PsychoArtAssetKind.Object, "Mine", nameFragments: new[] { "mine", "mining" }, targetHeight: 2.2f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Lumbermill", PsychoArtAssetKind.Object, "Lumbermill", nameFragments: new[] { "lumber", "sawmill" }, targetHeight: 2.8f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Watermill", PsychoArtAssetKind.Object, "Watermill", nameFragments: new[] { "watermill" }, targetHeight: 3.1f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Mill", PsychoArtAssetKind.Object, "Mill", nameFragments: new[] { "mill" }, targetHeight: 3.3f, markStatic: true, cullHeight: 0.018f);

            return entries;
        }

        private static void Add(
            List<PsychoArtAssetEntry> entries,
            Dictionary<string, GameObject> prefabs,
            string prefabKey,
            PsychoArtAssetKind kind,
            string displayName,
            int[] ids = null,
            string visualClass = null,
            string[] nameFragments = null,
            float targetHeight = 0f,
            bool matchAny = false,
            bool markStatic = false,
            float cullHeight = 0.018f,
            Vector3 localEuler = default)
        {
            if (!prefabs.TryGetValue(prefabKey, out GameObject prefab) || prefab == null)
            {
                Debug.LogWarning($"Starter art prefab '{prefabKey}' was not built; skipping manifest entry '{displayName}'.");
                return;
            }

            entries.Add(new PsychoArtAssetEntry
            {
                kind = kind,
                displayName = displayName,
                prefab = prefab,
                ids = ids ?? Array.Empty<int>(),
                visualClass = visualClass,
                nameFragments = nameFragments ?? Array.Empty<string>(),
                matchAny = matchAny,
                targetHeight = targetHeight,
                localEuler = localEuler,
                localScale = Vector3.one,
                markStatic = markStatic,
                ensureCullLod = true,
                cullScreenRelativeHeight = cullHeight,
                receiveShadows = true,
                castShadows = true
            });
        }

        private static void BuildCharacterPrefab(Dictionary<string, GameObject> prefabs, string modelName, string prefabName, float targetHeight)
        {
            string modelPath = $"{CharacterFbxRoot}/{modelName}.fbx";
            Material overrideMaterial = LoadOrCreateCharacterMaterial(modelName);
            GameObject prefab = BuildPrefab(modelPath, prefabName, targetHeight, false, true, false, overrideMaterial);
            if (prefab != null)
            {
                prefabs[prefabName] = prefab;
            }
        }

        private static void BuildStaticPrefab(Dictionary<string, GameObject> prefabs, string root, string modelName, string prefabName, float targetHeight, bool foliage)
        {
            string modelPath = $"{root}/{modelName}.fbx";
            GameObject prefab = BuildPrefab(modelPath, prefabName, targetHeight, true, false, foliage);
            if (prefab != null)
            {
                prefabs[prefabName] = prefab;
            }
        }

        private static GameObject BuildPrefab(string modelPath, string prefabName, float targetHeight, bool addCollider, bool humanoid, bool foliage = false, Material overrideMaterial = null)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                Debug.LogWarning($"Starter art model missing or failed to import: {modelPath}");
                return null;
            }

            string prefabPath = $"{PrefabRoot}/{prefabName}.prefab";
            GameObject root = new GameObject(prefabName);
            GameObject model = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (model == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                Debug.LogWarning($"Could not instantiate starter art model: {modelPath}");
                return null;
            }

            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            NormalizeHeight(root.transform, targetHeight);
            AlignVisualBottomToRootGround(root.transform);
            if (overrideMaterial != null)
            {
                AssignMaterial(root, overrideMaterial);
            }

            ConfigureRenderers(root);
            AddCullLod(root, foliage ? 0.010f : 0.018f);

            if (addCollider)
            {
                AddBoundsCollider(root);
            }

            if (humanoid)
            {
                CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0f, targetHeight * 0.5f, 0f);
                collider.height = targetHeight;
                collider.radius = 0.32f;
            }

            if (foliage)
            {
                MarkStatic(root);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AlignVisualBottomToRootGround(Transform root)
        {
            if (root == null || !TryGetRendererBounds(root, out Bounds bounds))
            {
                return;
            }

            float bottomY = root.InverseTransformPoint(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)).y;
            if (Mathf.Abs(bottomY) <= 0.0001f)
            {
                return;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                child.localPosition -= Vector3.up * bottomY;
            }
        }

        private static Material LoadOrCreateCharacterMaterial(string modelName)
        {
            string texturePath = $"{CharacterTextureRoot}/{modelName}_Texture.png";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                Debug.LogWarning($"Character texture missing for starter art model {modelName}: {texturePath}");
                return null;
            }

            string materialPath = $"{MaterialRoot}/{modelName}_Textured.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.name = $"{modelName}_Textured";
            material.mainTexture = texture;
            material.color = Color.white;
            material.enableInstancing = true;
            material.SetFloat("_Glossiness", 0.34f);
            material.SetFloat("_Metallic", 0.02f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureModelImports()
        {
            foreach (string modelPath in Directory.GetFiles("Assets/PsychoArtSource", "*.fbx", SearchOption.AllDirectories))
            {
                string assetPath = modelPath.Replace('\\', '/');
                ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.importCameras = false;
                importer.importLights = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                importer.meshCompression = ModelImporterMeshCompression.Medium;
                importer.isReadable = false;
                importer.optimizeMeshPolygons = true;
                importer.optimizeMeshVertices = true;
                importer.animationType = assetPath.Contains("/Characters/", StringComparison.OrdinalIgnoreCase)
                    ? ModelImporterAnimationType.Generic
                    : ModelImporterAnimationType.None;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureRenderers(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;

                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        materials[i].enableInstancing = true;
                    }
                }
            }
        }

        private static void AssignMaterial(GameObject root, Material material)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material[] replacement = new Material[Mathf.Max(1, renderer.sharedMaterials.Length)];
                for (int i = 0; i < replacement.Length; i++)
                {
                    replacement[i] = material;
                }

                renderer.sharedMaterials = replacement;
            }
        }

        private static void AddCullLod(GameObject root, float cullHeight)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            LODGroup lodGroup = root.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = root.AddComponent<LODGroup>();
            }
            lodGroup.SetLODs(new[] { new LOD(Mathf.Clamp(cullHeight, 0.006f, 0.12f), renderers) });
            lodGroup.RecalculateBounds();
        }

        private static void AddBoundsCollider(GameObject root)
        {
            if (!TryGetRendererBounds(root.transform, out Bounds bounds))
            {
                return;
            }

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = root.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size;
        }

        private static void NormalizeHeight(Transform root, float targetHeight)
        {
            if (targetHeight <= 0f || !TryGetRendererBounds(root, out Bounds bounds) || bounds.size.y <= 0.001f)
            {
                return;
            }

            float scale = targetHeight / bounds.size.y;
            if (root.childCount > 0)
            {
                root.GetChild(0).localScale *= scale;
            }
        }

        private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bounds = new Bounds(root.position, Vector3.zero);
            bool found = false;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return found;
        }

        private static void MarkStatic(GameObject root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            }
        }

        private static void EnsureFolder(string parent, string folder)
        {
            string path = parent + "/" + folder;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
