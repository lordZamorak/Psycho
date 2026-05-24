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
        private const string MeshRoot = "Assets/Resources/PsychoArt/Meshes/Starter";
        private const string CharacterFbxRoot = "Assets/PsychoArtSource/Characters/QuaterniusRpgCharacters/FBX";
        private const string CharacterTextureRoot = "Assets/PsychoArtSource/Characters/QuaterniusRpgCharacters/Textures";
        private const string FoliageFbxRoot = "Assets/PsychoArtSource/Foliage/QuaterniusStylizedNature/FBX";
        private const string BuildingFbxRoot = "Assets/PsychoArtSource/Environment/KayKitMedievalBuilder/FBX";

        private static readonly int[] RegularRatNpcIds =
        {
            47, 2682, 2980, 2981, 3007, 3008, 3009, 3010, 3011, 3012,
            3013, 3014, 3015, 3016, 3017, 3018, 4396, 4415, 7202, 7204,
            7461, 8209, 31020, 31021, 31022, 32492, 32513, 32854, 32855,
            34593, 34594, 34610, 34611, 34612, 34613, 34614, 34615, 34616,
            34617, 34618
        };

        private static readonly int[] GiantRatNpcIds =
        {
            86, 87, 88, 224, 446, 950, 978, 2032, 2033, 2982,
            3382, 3647, 3662, 3707, 4395, 4920, 4921, 4922, 4923, 4924,
            4925, 4926, 4927, 4928, 4929, 4936, 4937, 4938, 4939, 4940,
            4941, 4942, 4943, 4944, 4945, 6088, 6089, 6090, 6847, 6848,
            6899, 7920, 8828, 8829, 9472, 10480, 10481, 10482, 10483, 10484,
            10485, 10486, 12348, 12349, 12350, 12351, 12914, 12915, 12916,
            12917, 12918, 12919, 12920, 31062, 31679, 31680, 31681, 31682,
            32510, 32511, 32512, 32856, 32857, 32858, 32859, 32860, 32861,
            32862, 32863, 32864, 32865, 32866, 32867, 33313, 33314, 33315,
            33607, 33608, 33609, 33969, 33970, 33971, 34501, 34534, 34535,
            34595, 34689, 34690, 34809, 36793, 39026, 39040
        };

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
            EnsureFolder("Assets/Resources/PsychoArt", "Meshes");
            EnsureFolder("Assets/Resources/PsychoArt/Meshes", "Starter");

            Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            BuildCharacterPrefab(prefabs, "Warrior", "Warrior_Player", 1.85f);
            BuildCharacterPrefab(prefabs, "Monk", "Monk_Citizen", 1.72f);
            BuildCharacterPrefab(prefabs, "Rogue", "Rogue_Merchant", 1.72f);
            BuildCharacterPrefab(prefabs, "Cleric", "Cleric_Banker", 1.72f);
            BuildCharacterPrefab(prefabs, "Ranger", "Ranger_Citizen", 1.72f);
            BuildCharacterPrefab(prefabs, "Wizard", "Wizard_Citizen", 1.72f);
            BuildCharacterPrefab(prefabs, "Rogue", "Rogue_Undead", 1.66f, new Color(0.60f, 0.68f, 0.56f), "Rogue_Undead");
            BuildCharacterPrefab(prefabs, "Rogue", "Rogue_Goblin", 1.34f, new Color(0.50f, 0.68f, 0.38f), "Rogue_Goblin");
            BuildCharacterPrefab(prefabs, "Warrior", "Warrior_Dwarf", 1.36f, new Color(0.82f, 0.66f, 0.48f), "Warrior_Dwarf");
            BuildRatPrefab(prefabs, "Starter_Rat", 0.18f, new Color(0.34f, 0.31f, 0.24f), new Color(0.63f, 0.45f, 0.38f));
            BuildRatPrefab(prefabs, "Starter_GiantRat", 0.36f, new Color(0.27f, 0.25f, 0.22f), new Color(0.52f, 0.36f, 0.32f));

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

            Add(entries, prefabs, "Warrior_Player", PsychoArtAssetKind.Player, "Psycho Warrior Player", targetHeight: 1.85f, matchAny: true);

            Add(entries, prefabs, "Monk_Citizen", PsychoArtAssetKind.Npc, "Citizen Monk", visualClass: "Citizen", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Ranger_Citizen", PsychoArtAssetKind.Npc, "Citizen Ranger", visualClass: "Citizen", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Wizard_Citizen", PsychoArtAssetKind.Npc, "Citizen Wizard", visualClass: "Citizen", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Rogue_Merchant", PsychoArtAssetKind.Npc, "Merchant Rogue", visualClass: "Merchant", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Cleric_Banker", PsychoArtAssetKind.Npc, "Banker Cleric", visualClass: "Banker", targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Warrior_Player", PsychoArtAssetKind.Npc, "Guard Warrior", visualClass: "Guard", targetHeight: 1.78f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Monk_Citizen", PsychoArtAssetKind.Npc, "Town Citizen Monk", nameFragments: new[] { "man", "woman", "citizen", "villager", "farmer", "cook", "guide", "master", "trainer" }, targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Ranger_Citizen", PsychoArtAssetKind.Npc, "Ranger Archer", nameFragments: new[] { "archer", "ranger", "bowman", "hunter", "fletcher" }, targetHeight: 1.74f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Wizard_Citizen", PsychoArtAssetKind.Npc, "Wizard Mage", nameFragments: new[] { "wizard", "mage", "magician", "sorcerer", "witch", "enchanter" }, targetHeight: 1.74f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Rogue_Merchant", PsychoArtAssetKind.Npc, "Shopkeeper Rogue", nameFragments: new[] { "shop", "merchant", "trader", "seller", "buyer", "rogue", "thief" }, targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Cleric_Banker", PsychoArtAssetKind.Npc, "Banking Cleric", nameFragments: new[] { "banker", "clerk", "exchange", "account" }, targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Warrior_Player", PsychoArtAssetKind.Npc, "Armored Warrior", nameFragments: new[] { "guard", "warrior", "knight", "soldier", "champion", "fighter", "barbarian", "paladin" }, targetHeight: 1.80f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Cleric_Banker", PsychoArtAssetKind.Npc, "Priest Cleric", nameFragments: new[] { "monk", "priest", "cleric", "brother", "druid", "sage" }, targetHeight: 1.72f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Rogue_Undead", PsychoArtAssetKind.Npc, "Undead Rogue", nameFragments: new[] { "zombie", "skeleton", "undead", "revenant", "shade", "wight" }, targetHeight: 1.66f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Rogue_Goblin", PsychoArtAssetKind.Npc, "Goblin Rogue", nameFragments: new[] { "goblin", "hobgoblin" }, targetHeight: 1.34f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Warrior_Dwarf", PsychoArtAssetKind.Npc, "Dwarf Warrior", nameFragments: new[] { "dwarf" }, targetHeight: 1.36f, localEuler: new Vector3(0f, 180f, 0f));
            Add(entries, prefabs, "Starter_Rat", PsychoArtAssetKind.Npc, "Small Rat", ids: RegularRatNpcIds, targetHeight: 0.18f, cullHeight: 0.050f);
            Add(entries, prefabs, "Starter_GiantRat", PsychoArtAssetKind.Npc, "Giant Rat", ids: GiantRatNpcIds, targetHeight: 0.36f, cullHeight: 0.044f);

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

        private static void BuildCharacterPrefab(Dictionary<string, GameObject> prefabs, string modelName, string prefabName, float targetHeight, Color? materialTint = null, string materialKey = null)
        {
            string modelPath = $"{CharacterFbxRoot}/{modelName}.fbx";
            Material overrideMaterial = LoadOrCreateCharacterMaterial(modelName, materialKey ?? modelName, materialTint ?? Color.white);
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

        private static void BuildRatPrefab(Dictionary<string, GameObject> prefabs, string prefabName, float targetHeight, Color furColor, Color skinColor)
        {
            Material fur = LoadOrCreateStarterMaterial($"{prefabName}_Fur", furColor, 0.01f, 0.38f);
            Material skin = LoadOrCreateStarterMaterial($"{prefabName}_Skin", skinColor, 0.0f, 0.30f);
            Material eye = LoadOrCreateStarterMaterial($"{prefabName}_Eye", new Color(0.025f, 0.022f, 0.018f), 0.0f, 0.64f);

            Mesh bodyMesh = LoadOrCreateMeshAsset($"{prefabName}_Ellipsoid", CreateEllipsoidMesh(20, 12));
            Mesh legMesh = LoadOrCreateMeshAsset($"{prefabName}_Leg", CreateTaperedCylinderYMesh(10, 0.70f));
            Mesh tailMesh = LoadOrCreateMeshAsset($"{prefabName}_Tail", CreateTaperedCylinderZMesh(12, 0.30f));

            GameObject root = new GameObject(prefabName);
            float h = Mathf.Max(0.08f, targetHeight);
            AddMeshPart(root.transform, "Body", bodyMesh, fur, new Vector3(0f, h * 0.27f, -h * 0.03f), Quaternion.Euler(-4f, 0f, 0f), new Vector3(h * 0.74f, h * 0.34f, h * 1.32f));
            AddMeshPart(root.transform, "Head", bodyMesh, fur, new Vector3(0f, h * 0.35f, h * 0.55f), Quaternion.Euler(8f, 0f, 0f), new Vector3(h * 0.39f, h * 0.25f, h * 0.47f));
            AddMeshPart(root.transform, "Nose", bodyMesh, skin, new Vector3(0f, h * 0.32f, h * 0.83f), Quaternion.identity, new Vector3(h * 0.14f, h * 0.085f, h * 0.17f));
            AddMeshPart(root.transform, "Ear L", bodyMesh, fur, new Vector3(-h * 0.13f, h * 0.49f, h * 0.48f), Quaternion.Euler(0f, 0f, -19f), new Vector3(h * 0.095f, h * 0.13f, h * 0.052f));
            AddMeshPart(root.transform, "Ear R", bodyMesh, fur, new Vector3(h * 0.13f, h * 0.49f, h * 0.48f), Quaternion.Euler(0f, 0f, 19f), new Vector3(h * 0.095f, h * 0.13f, h * 0.052f));
            AddMeshPart(root.transform, "Eye L", bodyMesh, eye, new Vector3(-h * 0.11f, h * 0.39f, h * 0.75f), Quaternion.identity, Vector3.one * (h * 0.032f));
            AddMeshPart(root.transform, "Eye R", bodyMesh, eye, new Vector3(h * 0.11f, h * 0.39f, h * 0.75f), Quaternion.identity, Vector3.one * (h * 0.032f));

            AddMeshPart(root.transform, "Leg FL", legMesh, skin, new Vector3(-h * 0.20f, h * 0.09f, h * 0.25f), Quaternion.identity, new Vector3(h * 0.070f, h * 0.17f, h * 0.070f));
            AddMeshPart(root.transform, "Leg FR", legMesh, skin, new Vector3(h * 0.20f, h * 0.09f, h * 0.25f), Quaternion.identity, new Vector3(h * 0.070f, h * 0.17f, h * 0.070f));
            AddMeshPart(root.transform, "Leg BL", legMesh, skin, new Vector3(-h * 0.22f, h * 0.09f, -h * 0.36f), Quaternion.identity, new Vector3(h * 0.075f, h * 0.18f, h * 0.075f));
            AddMeshPart(root.transform, "Leg BR", legMesh, skin, new Vector3(h * 0.22f, h * 0.09f, -h * 0.36f), Quaternion.identity, new Vector3(h * 0.075f, h * 0.18f, h * 0.075f));
            AddMeshPart(root.transform, "Paw FL", bodyMesh, skin, new Vector3(-h * 0.20f, h * 0.02f, h * 0.31f), Quaternion.identity, new Vector3(h * 0.12f, h * 0.04f, h * 0.17f));
            AddMeshPart(root.transform, "Paw FR", bodyMesh, skin, new Vector3(h * 0.20f, h * 0.02f, h * 0.31f), Quaternion.identity, new Vector3(h * 0.12f, h * 0.04f, h * 0.17f));
            AddMeshPart(root.transform, "Paw BL", bodyMesh, skin, new Vector3(-h * 0.22f, h * 0.02f, -h * 0.31f), Quaternion.identity, new Vector3(h * 0.13f, h * 0.04f, h * 0.18f));
            AddMeshPart(root.transform, "Paw BR", bodyMesh, skin, new Vector3(h * 0.22f, h * 0.02f, -h * 0.31f), Quaternion.identity, new Vector3(h * 0.13f, h * 0.04f, h * 0.18f));
            AddMeshPart(root.transform, "Tail", tailMesh, skin, new Vector3(0f, h * 0.25f, -h * 0.64f), Quaternion.Euler(-7f, 0f, 0f), new Vector3(h * 0.050f, h * 0.050f, h * 0.94f));

            ConfigureRenderers(root);
            AddCullLod(root, 0.050f);
            AlignVisualBottomToRootGround(root.transform);
            string prefabPath = $"{PrefabRoot}/{prefabName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab != null)
            {
                prefabs[prefabName] = prefab;
            }
        }

        private static Material LoadOrCreateStarterMaterial(string materialName, Color color, float metallic, float smoothness)
        {
            string materialPath = $"{MaterialRoot}/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.name = materialName;
            material.color = color;
            material.enableInstancing = true;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void AddMeshPart(Transform root, string name, Mesh mesh, Material material, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(root, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            MeshFilter filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static Mesh LoadOrCreateMeshAsset(string meshName, Mesh generated)
        {
            string meshPath = $"{MeshRoot}/{meshName}.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            generated.name = meshName;
            generated.RecalculateBounds();
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, meshPath);
                return generated;
            }

            EditorUtility.CopySerialized(generated, existing);
            existing.name = meshName;
            EditorUtility.SetDirty(existing);
            UnityEngine.Object.DestroyImmediate(generated);
            return existing;
        }

        private static Mesh CreateEllipsoidMesh(int segments, int rings)
        {
            List<Vector3> vertices = new List<Vector3>((segments + 1) * (rings + 1));
            List<Vector3> normals = new List<Vector3>(vertices.Capacity);
            List<Vector2> uvs = new List<Vector2>(vertices.Capacity);
            List<int> triangles = new List<int>(segments * rings * 6);

            for (int ring = 0; ring <= rings; ring++)
            {
                float v = ring / (float)rings;
                float phi = Mathf.PI * v;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);
                for (int segment = 0; segment <= segments; segment++)
                {
                    float u = segment / (float)segments;
                    float theta = Mathf.PI * 2f * u;
                    Vector3 normal = new Vector3(Mathf.Cos(theta) * sinPhi, cosPhi, Mathf.Sin(theta) * sinPhi);
                    vertices.Add(normal * 0.5f);
                    normals.Add(normal.normalized);
                    uvs.Add(new Vector2(u, v));
                }
            }

            int stride = segments + 1;
            for (int ring = 0; ring < rings; ring++)
            {
                for (int segment = 0; segment < segments; segment++)
                {
                    int a = ring * stride + segment;
                    int b = a + stride;
                    int c = b + 1;
                    int d = a + 1;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(d);
                    triangles.Add(d);
                    triangles.Add(b);
                    triangles.Add(c);
                }
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            return mesh;
        }

        private static Mesh CreateTaperedCylinderYMesh(int segments, float topRadiusMultiplier)
        {
            List<Vector3> vertices = new List<Vector3>((segments + 1) * 2 + 2);
            List<Vector3> normals = new List<Vector3>(vertices.Capacity);
            List<int> triangles = new List<int>(segments * 12);

            for (int y = 0; y <= 1; y++)
            {
                float radius = y == 0 ? 0.5f : 0.5f * topRadiusMultiplier;
                float height = y == 0 ? -0.5f : 0.5f;
                for (int segment = 0; segment <= segments; segment++)
                {
                    float theta = Mathf.PI * 2f * (segment / (float)segments);
                    Vector3 normal = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized;
                    vertices.Add(new Vector3(normal.x * radius, height, normal.z * radius));
                    normals.Add(normal);
                }
            }

            int bottomCenter = vertices.Count;
            vertices.Add(new Vector3(0f, -0.5f, 0f));
            normals.Add(Vector3.down);
            int topCenter = vertices.Count;
            vertices.Add(new Vector3(0f, 0.5f, 0f));
            normals.Add(Vector3.up);

            int stride = segments + 1;
            for (int segment = 0; segment < segments; segment++)
            {
                int a = segment;
                int b = stride + segment;
                int c = stride + segment + 1;
                int d = segment + 1;
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(d);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(bottomCenter);
                triangles.Add(d);
                triangles.Add(a);
                triangles.Add(topCenter);
                triangles.Add(b);
                triangles.Add(c);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            return mesh;
        }

        private static Mesh CreateTaperedCylinderZMesh(int segments, float tipRadiusMultiplier)
        {
            List<Vector3> vertices = new List<Vector3>((segments + 1) * 2 + 2);
            List<Vector3> normals = new List<Vector3>(vertices.Capacity);
            List<int> triangles = new List<int>(segments * 12);

            for (int z = 0; z <= 1; z++)
            {
                float radius = z == 0 ? 0.5f : 0.5f * tipRadiusMultiplier;
                float depth = z == 0 ? 0f : -1f;
                for (int segment = 0; segment <= segments; segment++)
                {
                    float theta = Mathf.PI * 2f * (segment / (float)segments);
                    Vector3 normal = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f).normalized;
                    vertices.Add(new Vector3(normal.x * radius, normal.y * radius, depth));
                    normals.Add(normal);
                }
            }

            int baseCenter = vertices.Count;
            vertices.Add(Vector3.zero);
            normals.Add(Vector3.forward);
            int tipCenter = vertices.Count;
            vertices.Add(new Vector3(0f, 0f, -1f));
            normals.Add(Vector3.back);

            int stride = segments + 1;
            for (int segment = 0; segment < segments; segment++)
            {
                int a = segment;
                int b = stride + segment;
                int c = stride + segment + 1;
                int d = segment + 1;
                triangles.Add(a);
                triangles.Add(d);
                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(baseCenter);
                triangles.Add(a);
                triangles.Add(d);
                triangles.Add(tipCenter);
                triangles.Add(c);
                triangles.Add(b);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            return mesh;
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

        private static Material LoadOrCreateCharacterMaterial(string modelName, string materialKey, Color tint)
        {
            string texturePath = $"{CharacterTextureRoot}/{modelName}_Texture.png";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                Debug.LogWarning($"Character texture missing for starter art model {modelName}: {texturePath}");
                return null;
            }

            string materialPath = $"{MaterialRoot}/{materialKey}_Textured.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.name = $"{materialKey}_Textured";
            material.mainTexture = texture;
            material.color = tint;
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
