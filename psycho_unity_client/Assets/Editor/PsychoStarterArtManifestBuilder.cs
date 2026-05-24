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
        private const string FarmAnimalFbxRoot = "Assets/PsychoArtSource/Creatures/QuaterniusFarmAnimals/FBX";
        private const string MonsterFbxRoot = "Assets/PsychoArtSource/Creatures/QuaterniusAnimatedMonsters/FBX";

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

        private static readonly int[] DogNpcIds =
        {
            98, 99, 821, 1593, 1594, 3582, 4766, 4767, 5917, 5918,
            6374, 6897, 6968, 7257, 7258, 9205, 9206, 9207, 10726, 10727,
            10728, 10729, 10730, 10731, 10732, 10733, 10734, 10735, 11562,
            11584, 11585, 12922, 12923, 12924, 12925, 12926, 12927, 12928,
            12929, 12930, 12931, 30111, 30112, 30113, 30114, 30131, 32902,
            32922, 33829, 33830, 34228, 37025, 37209, 37771, 38041
        };

        private static readonly int[] PuppyNpcIds =
        {
            6958, 6960, 6962, 6964, 6969, 7237, 7239, 7241, 7243, 7245,
            7247, 7249, 7251, 7259, 7260, 30964, 33099
        };

        private static readonly int[] TerrorDogNpcIds =
        {
            5417, 5418, 11365, 36473, 36474
        };

        private static readonly int[] CowNpcIds =
        {
            81, 397, 955, 1767, 3309, 12362, 12363, 12365, 32790, 32791,
            32793, 32795, 35842, 36340, 36401
        };

        private static readonly int[] CalfNpcIds =
        {
            1766, 1768, 2310, 12364, 12366, 32792, 32794, 32801
        };

        private static readonly int[] SheepNpcIds =
        {
            42, 43, 1271, 1272, 1529, 1762, 1763, 1764, 1765, 3310,
            3311, 3579, 5148, 5149, 5150, 5151, 5152, 5153, 5154, 5155,
            5156, 5157, 5158, 5159, 5160, 5161, 5162, 5163, 5164, 5165,
            5172, 5173, 8876, 8877, 14374, 14375, 30731, 30804, 30805,
            30806, 30807, 31178, 31299, 31300, 31301, 31302, 31303, 31304,
            31308, 31309, 32691, 32692, 32693, 32694, 32695, 32696, 32697,
            32698, 32699, 32786, 32787, 32788, 32789, 33987, 33988, 35306,
            35307, 35726, 35843, 35844, 35845, 35846
        };

        private static readonly int[] LambNpcIds =
        {
            5146, 5147, 31176, 31177
        };

        private static readonly int[] RamNpcIds =
        {
            3672, 3673, 5168, 5169, 5170, 12369, 12370, 12371, 31261,
            31262, 31263, 31264, 31265
        };

        private static readonly int[] ImpNpcIds =
        {
            708, 709, 1531, 3062, 6074, 6211, 8881, 8994, 11605, 33134,
            33355, 35007, 35008, 35728, 35738, 37020, 37881
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
            if (!BuildSourceCreaturePrefab(prefabs, FarmAnimalFbxRoot, "Pug", "Starter_Dog", 0.58f))
            {
                BuildDogPrefab(prefabs, "Starter_Dog", 0.58f, new Color(0.38f, 0.28f, 0.20f), new Color(0.74f, 0.56f, 0.42f));
            }

            if (!BuildSourceCreaturePrefab(prefabs, FarmAnimalFbxRoot, "Pug", "Starter_Puppy", 0.34f))
            {
                BuildDogPrefab(prefabs, "Starter_Puppy", 0.34f, new Color(0.52f, 0.42f, 0.32f), new Color(0.78f, 0.60f, 0.45f));
            }

            if (!BuildSourceCreaturePrefab(prefabs, FarmAnimalFbxRoot, "Pug", "Starter_TerrorDog", 0.85f))
            {
                BuildDogPrefab(prefabs, "Starter_TerrorDog", 0.85f, new Color(0.15f, 0.12f, 0.10f), new Color(0.45f, 0.22f, 0.18f), true);
            }

            if (!BuildSourceCreaturePrefab(prefabs, FarmAnimalFbxRoot, "Cow", "Starter_Cow", 0.95f))
            {
                BuildCattlePrefab(prefabs, "Starter_Cow", 0.95f, new Color(0.52f, 0.42f, 0.30f), new Color(0.78f, 0.62f, 0.46f));
            }

            if (!BuildSourceCreaturePrefab(prefabs, FarmAnimalFbxRoot, "Cow", "Starter_Calf", 0.68f))
            {
                BuildCattlePrefab(prefabs, "Starter_Calf", 0.68f, new Color(0.58f, 0.46f, 0.34f), new Color(0.82f, 0.66f, 0.50f));
            }

            if (!BuildSourceCreaturePrefab(prefabs, FarmAnimalFbxRoot, "Sheep", "Starter_Sheep", 0.58f))
            {
                BuildSheepPrefab(prefabs, "Starter_Sheep", 0.58f, new Color(0.82f, 0.78f, 0.66f), new Color(0.23f, 0.20f, 0.18f), false);
            }

            if (!BuildSourceCreaturePrefab(prefabs, FarmAnimalFbxRoot, "Sheep", "Starter_Lamb", 0.36f))
            {
                BuildSheepPrefab(prefabs, "Starter_Lamb", 0.36f, new Color(0.88f, 0.84f, 0.72f), new Color(0.26f, 0.23f, 0.20f), false);
            }

            if (!BuildSourceCreaturePrefab(prefabs, FarmAnimalFbxRoot, "Sheep", "Starter_Ram", 0.62f))
            {
                BuildSheepPrefab(prefabs, "Starter_Ram", 0.62f, new Color(0.76f, 0.72f, 0.62f), new Color(0.25f, 0.22f, 0.19f), true);
            }

            if (!BuildSourceCreaturePrefab(prefabs, MonsterFbxRoot, "Dragon", "Starter_Imp", 0.62f))
            {
                BuildImpPrefab(prefabs, "Starter_Imp", 0.62f);
            }

            BuildStaticPrefab(prefabs, FoliageFbxRoot, "CommonTree_1", "CommonTree_1", 3.4f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "CommonTree_2", "CommonTree_2", 3.7f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "CommonTree_3", "CommonTree_3", 3.2f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Pine_1", "Pine_1", 4.2f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Pine_2", "Pine_2", 4.5f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Pine_3", "Pine_3", 4.0f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "TwistedTree_1", "TwistedTree_1", 3.8f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "DeadTree_1", "DeadTree_1", 3.3f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Bush_Common", "Bush_Common", 0.74f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Bush_Common_Flowers", "Bush_Common_Flowers", 0.78f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Grass_Common_Tall", "Grass_Common_Tall", 0.58f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Grass_Wispy_Tall", "Grass_Wispy_Tall", 0.64f, true);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Rock_Medium_1", "Rock_Medium_1", 0.75f, false);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Rock_Medium_2", "Rock_Medium_2", 0.68f, false);
            BuildStaticPrefab(prefabs, FoliageFbxRoot, "Rock_Medium_3", "Rock_Medium_3", 0.82f, false);

            BuildStaticPrefab(prefabs, BuildingFbxRoot, "house", "KayKit_House", 2.9f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "market", "KayKit_Market", 2.7f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "barracks", "KayKit_Barracks", 3.1f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "castle", "KayKit_Castle", 4.4f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "wall_gate", "KayKit_WallGate", 2.7f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "wall_gate_closed", "KayKit_WallGateClosed", 2.7f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "wall_straight", "KayKit_Wall", 1.7f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "watchtower", "KayKit_Watchtower", 4.1f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "well", "KayKit_Well", 1.15f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "bridge", "KayKit_Bridge", 0.9f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "bridge_roofed", "KayKit_RoofedBridge", 1.45f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "mine", "KayKit_Mine", 2.2f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "lumbermill", "KayKit_Lumbermill", 2.8f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "watermill", "KayKit_Watermill", 3.1f, false);
            BuildStaticPrefab(prefabs, BuildingFbxRoot, "mill", "KayKit_Mill", 3.3f, false);
            BuildMedievalPropPrefabs(prefabs);

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

        [MenuItem("Psycho/Art Pipeline/Render Starter Creature Preview")]
        public static void RenderStarterCreaturePreview()
        {
            if (!File.Exists($"{PrefabRoot}/Starter_Dog.prefab"))
            {
                BuildStarterManifest();
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Creature Preview Ground";
            ground.transform.localScale = new Vector3(2.5f, 1f, 1.1f);
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            groundRenderer.sharedMaterial = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.21f, 0.25f, 0.20f)
            };

            GameObject lightRoot = new GameObject("Creature Preview Key Light");
            Light keyLight = lightRoot.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.2f;
            lightRoot.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            string[] creaturePrefabs =
            {
                "Starter_Puppy", "Starter_Dog", "Starter_TerrorDog", "Starter_Calf",
                "Starter_Cow", "Starter_Lamb", "Starter_Sheep", "Starter_Ram", "Starter_Imp"
            };

            float startX = -4.6f;
            for (int i = 0; i < creaturePrefabs.Length; i++)
            {
                string prefabName = creaturePrefabs[i];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
                if (prefab == null)
                {
                    Debug.LogWarning($"Creature preview prefab missing: {prefabName}");
                    continue;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                {
                    continue;
                }

                instance.name = prefabName;
                instance.transform.position = new Vector3(startX + i * 1.15f, 0f, 0f);
                instance.transform.rotation = Quaternion.Euler(0f, 155f, 0f);
            }

            GameObject cameraRoot = new GameObject("Creature Preview Camera");
            Camera camera = cameraRoot.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.11f);
            camera.orthographic = true;
            camera.orthographicSize = 1.65f;
            camera.transform.position = new Vector3(0.1f, 1.55f, -5.2f);
            camera.transform.LookAt(new Vector3(0.1f, 0.55f, 0f));

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-starter-creature-preview.png"));
            RenderPreviewCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderStarterCreaturePreviewBatch()
        {
            RenderStarterCreaturePreview();
        }

        [MenuItem("Psycho/Art Pipeline/Render Starter Humanoid Preview")]
        public static void RenderStarterHumanoidPreview()
        {
            if (!File.Exists($"{PrefabRoot}/Warrior_Player.prefab"))
            {
                BuildStarterManifest();
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Humanoid Preview Ground";
            ground.transform.localScale = new Vector3(2.4f, 1f, 1.0f);
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            groundRenderer.sharedMaterial = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.20f, 0.23f, 0.20f)
            };

            GameObject lightRoot = new GameObject("Humanoid Preview Key Light");
            Light keyLight = lightRoot.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.18f;
            keyLight.color = new Color(1f, 0.92f, 0.82f);
            lightRoot.transform.rotation = Quaternion.Euler(44f, -32f, 0f);

            string[] humanoidPrefabs =
            {
                "Warrior_Player", "Monk_Citizen", "Ranger_Citizen", "Wizard_Citizen",
                "Rogue_Merchant", "Cleric_Banker", "Rogue_Undead", "Rogue_Goblin", "Warrior_Dwarf"
            };

            float startX = -4.25f;
            for (int i = 0; i < humanoidPrefabs.Length; i++)
            {
                string prefabName = humanoidPrefabs[i];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
                if (prefab == null)
                {
                    Debug.LogWarning($"Humanoid preview prefab missing: {prefabName}");
                    continue;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                {
                    continue;
                }

                instance.name = prefabName;
                instance.transform.position = new Vector3(startX + i * 1.05f, 0f, 0f);
                instance.transform.rotation = Quaternion.Euler(0f, 158f, 0f);
            }

            GameObject cameraRoot = new GameObject("Humanoid Preview Camera");
            Camera camera = cameraRoot.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.075f, 0.090f, 0.10f);
            camera.orthographic = true;
            camera.orthographicSize = 1.55f;
            camera.transform.position = new Vector3(0.05f, 1.43f, -6.1f);
            camera.transform.LookAt(new Vector3(0.05f, 0.78f, 0f));

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-starter-humanoid-preview.png"));
            RenderPreviewCameraToPng(camera, outputPath, 1600, 900);
        }

        public static void RenderStarterHumanoidPreviewBatch()
        {
            RenderStarterHumanoidPreview();
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
            Add(entries, prefabs, "Starter_Dog", PsychoArtAssetKind.Npc, "Dog", ids: DogNpcIds, targetHeight: 0.58f, cullHeight: 0.036f);
            Add(entries, prefabs, "Starter_Puppy", PsychoArtAssetKind.Npc, "Puppy", ids: PuppyNpcIds, targetHeight: 0.34f, cullHeight: 0.048f);
            Add(entries, prefabs, "Starter_TerrorDog", PsychoArtAssetKind.Npc, "Terror Dog", ids: TerrorDogNpcIds, targetHeight: 0.85f, cullHeight: 0.032f);
            Add(entries, prefabs, "Starter_Cow", PsychoArtAssetKind.Npc, "Cow", ids: CowNpcIds, targetHeight: 0.95f, cullHeight: 0.030f);
            Add(entries, prefabs, "Starter_Calf", PsychoArtAssetKind.Npc, "Cow Calf", ids: CalfNpcIds, targetHeight: 0.68f, cullHeight: 0.036f);
            Add(entries, prefabs, "Starter_Sheep", PsychoArtAssetKind.Npc, "Sheep", ids: SheepNpcIds, targetHeight: 0.58f, cullHeight: 0.038f);
            Add(entries, prefabs, "Starter_Lamb", PsychoArtAssetKind.Npc, "Lamb", ids: LambNpcIds, targetHeight: 0.36f, cullHeight: 0.048f);
            Add(entries, prefabs, "Starter_Ram", PsychoArtAssetKind.Npc, "Ram", ids: RamNpcIds, targetHeight: 0.62f, cullHeight: 0.036f);
            Add(entries, prefabs, "Starter_Imp", PsychoArtAssetKind.Npc, "Imp", ids: ImpNpcIds, targetHeight: 0.62f, cullHeight: 0.034f);

            Add(entries, prefabs, "CommonTree_1", PsychoArtAssetKind.Object, "Common Tree A", visualClass: "Tree", targetHeight: 3.4f, cullHeight: 0.012f);
            Add(entries, prefabs, "CommonTree_2", PsychoArtAssetKind.Object, "Common Tree B", visualClass: "Tree", targetHeight: 3.7f, cullHeight: 0.012f);
            Add(entries, prefabs, "CommonTree_3", PsychoArtAssetKind.Object, "Common Tree C", visualClass: "Tree", targetHeight: 3.2f, cullHeight: 0.012f);
            Add(entries, prefabs, "Pine_1", PsychoArtAssetKind.Object, "Pine Tree A", visualClass: "Tree", nameFragments: new[] { "pine", "conifer", "evergreen" }, targetHeight: 4.2f, cullHeight: 0.010f);
            Add(entries, prefabs, "Pine_2", PsychoArtAssetKind.Object, "Pine Tree B", visualClass: "Tree", nameFragments: new[] { "pine", "conifer", "evergreen" }, targetHeight: 4.5f, cullHeight: 0.010f);
            Add(entries, prefabs, "Pine_3", PsychoArtAssetKind.Object, "Pine Tree C", visualClass: "Tree", nameFragments: new[] { "pine", "conifer", "evergreen" }, targetHeight: 4.0f, cullHeight: 0.010f);
            Add(entries, prefabs, "TwistedTree_1", PsychoArtAssetKind.Object, "Twisted Tree", visualClass: "Tree", nameFragments: new[] { "dead tree", "dying tree", "twisted tree" }, targetHeight: 3.8f, cullHeight: 0.010f);
            Add(entries, prefabs, "DeadTree_1", PsychoArtAssetKind.Object, "Dead Tree", visualClass: "Tree", nameFragments: new[] { "dead tree", "burnt tree", "hollow tree" }, targetHeight: 3.3f, cullHeight: 0.010f);
            Add(entries, prefabs, "Bush_Common", PsychoArtAssetKind.Object, "Common Bush", nameFragments: new[] { "bush", "shrub", "bramble", "fern", "plant" }, targetHeight: 0.74f, cullHeight: 0.018f);
            Add(entries, prefabs, "Bush_Common_Flowers", PsychoArtAssetKind.Object, "Flowering Bush", nameFragments: new[] { "flower", "flowers", "flowering", "rose", "heather" }, targetHeight: 0.78f, cullHeight: 0.018f);
            Add(entries, prefabs, "Grass_Common_Tall", PsychoArtAssetKind.Object, "Tall Grass", nameFragments: new[] { "grass", "reeds", "reed", "weeds" }, targetHeight: 0.58f, cullHeight: 0.022f);
            Add(entries, prefabs, "Grass_Wispy_Tall", PsychoArtAssetKind.Object, "Wispy Grass", nameFragments: new[] { "grass", "reed", "reeds", "rush", "rushes" }, targetHeight: 0.64f, cullHeight: 0.022f);

            Add(entries, prefabs, "Rock_Medium_1", PsychoArtAssetKind.Object, "Rock A", visualClass: "Rock", targetHeight: 0.75f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "Rock_Medium_2", PsychoArtAssetKind.Object, "Rock B", visualClass: "Rock", targetHeight: 0.68f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "Rock_Medium_3", PsychoArtAssetKind.Object, "Rock C", visualClass: "Rock", targetHeight: 0.82f, markStatic: true, cullHeight: 0.020f);

            Add(entries, prefabs, "KayKit_House", PsychoArtAssetKind.Object, "Medieval House", nameFragments: new[] { "house", "home", "hut" }, targetHeight: 2.9f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Market", PsychoArtAssetKind.Object, "Medieval Market", nameFragments: new[] { "market", "stall", "shop" }, targetHeight: 2.7f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Barracks", PsychoArtAssetKind.Object, "Barracks", nameFragments: new[] { "barracks", "guild", "hall" }, targetHeight: 3.1f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Castle", PsychoArtAssetKind.Object, "Castle", nameFragments: new[] { "castle", "keep", "fortress" }, targetHeight: 4.4f, markStatic: true, cullHeight: 0.014f);
            Add(entries, prefabs, "KayKit_WallGate", PsychoArtAssetKind.Object, "Wall Gate", nameFragments: new[] { "gate" }, targetHeight: 2.7f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_WallGateClosed", PsychoArtAssetKind.Object, "Closed Wall Gate", nameFragments: new[] { "closed gate", "large gate" }, targetHeight: 2.7f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Wall", PsychoArtAssetKind.Object, "Wall", nameFragments: new[] { "wall" }, targetHeight: 1.7f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "KayKit_Watchtower", PsychoArtAssetKind.Object, "Watchtower", nameFragments: new[] { "watchtower", "tower" }, targetHeight: 4.1f, markStatic: true, cullHeight: 0.014f);
            Add(entries, prefabs, "KayKit_Well", PsychoArtAssetKind.Object, "Well", nameFragments: new[] { "well" }, targetHeight: 1.15f, markStatic: true, cullHeight: 0.024f);
            Add(entries, prefabs, "KayKit_Bridge", PsychoArtAssetKind.Object, "Bridge", nameFragments: new[] { "bridge" }, targetHeight: 0.9f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "KayKit_RoofedBridge", PsychoArtAssetKind.Object, "Roofed Bridge", nameFragments: new[] { "roofed bridge", "covered bridge" }, targetHeight: 1.45f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Mine", PsychoArtAssetKind.Object, "Mine", nameFragments: new[] { "mine", "mining" }, targetHeight: 2.2f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Lumbermill", PsychoArtAssetKind.Object, "Lumbermill", nameFragments: new[] { "lumber", "sawmill" }, targetHeight: 2.8f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Watermill", PsychoArtAssetKind.Object, "Watermill", nameFragments: new[] { "watermill" }, targetHeight: 3.1f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "KayKit_Mill", PsychoArtAssetKind.Object, "Mill", nameFragments: new[] { "mill" }, targetHeight: 3.3f, markStatic: true, cullHeight: 0.018f);
            Add(entries, prefabs, "Starter_Crate", PsychoArtAssetKind.Object, "Supply Crate", ids: new[] { 354, 355, 356, 357, 358, 366 }, nameFragments: new[] { "crate" }, targetHeight: 0.82f, markStatic: true, cullHeight: 0.030f);
            Add(entries, prefabs, "Starter_Barrel", PsychoArtAssetKind.Object, "Oak Barrel", ids: new[] { 362, 364 }, nameFragments: new[] { "barrel", "cask" }, targetHeight: 0.92f, markStatic: true, cullHeight: 0.030f);
            Add(entries, prefabs, "Starter_Chest", PsychoArtAssetKind.Object, "Ironbound Chest", ids: new[] { 172, 375 }, nameFragments: new[] { "closed chest", "chest" }, targetHeight: 0.78f, markStatic: true, cullHeight: 0.032f);
            Add(entries, prefabs, "Starter_Cart", PsychoArtAssetKind.Object, "Wooden Cart", ids: new[] { 306, 307, 327 }, nameFragments: new[] { "cart", "cart wheel" }, targetHeight: 1.05f, markStatic: true, cullHeight: 0.028f);
            Add(entries, prefabs, "Starter_Door", PsychoArtAssetKind.Object, "Reinforced Door", ids: new[] { 73, 74, 134, 1512, 1534, 15535 }, nameFragments: new[] { "large door", "door" }, targetHeight: 1.85f, markStatic: true, cullHeight: 0.024f);
            Add(entries, prefabs, "Starter_BankBooth", PsychoArtAssetKind.Object, "Bank Booth", ids: new[] { 2213, 2215, 11402 }, nameFragments: new[] { "bank booth", "closed bank booth" }, targetHeight: 1.32f, markStatic: true, cullHeight: 0.024f);
            Add(entries, prefabs, "Starter_BankTable", PsychoArtAssetKind.Object, "Bank Table", ids: new[] { 590, 591 }, nameFragments: new[] { "bank table", "counter" }, targetHeight: 0.86f, markStatic: true, cullHeight: 0.030f);
            Add(entries, prefabs, "Starter_MarketStall", PsychoArtAssetKind.Object, "Market Stall", ids: new[] { 634, 635 }, nameFragments: new[] { "market stall", "tea stall", "stall" }, targetHeight: 1.82f, markStatic: true, cullHeight: 0.020f);
            Add(entries, prefabs, "Starter_Signpost", PsychoArtAssetKind.Object, "Hanging Signpost", ids: new[] { 961, 1076, 1085 }, nameFragments: new[] { "signpost", "notice board", "sign" }, targetHeight: 1.42f, markStatic: true, cullHeight: 0.026f);
            Add(entries, prefabs, "Starter_DefenceWall", PsychoArtAssetKind.Object, "Timber Defence", ids: new[] { 824, 848, 849, 1864 }, nameFragments: new[] { "wooden defence", "timber defence", "spear wall" }, targetHeight: 1.35f, markStatic: true, cullHeight: 0.026f);
            Add(entries, prefabs, "Starter_Anvil", PsychoArtAssetKind.Object, "Smithing Anvil", ids: new[] { 2783 }, nameFragments: new[] { "anvil" }, targetHeight: 0.68f, markStatic: true, cullHeight: 0.032f);
            Add(entries, prefabs, "Starter_Furnace", PsychoArtAssetKind.Object, "Stone Furnace", nameFragments: new[] { "furnace", "range", "forge" }, targetHeight: 1.22f, markStatic: true, cullHeight: 0.026f);

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

        private static void BuildMedievalPropPrefabs(Dictionary<string, GameObject> prefabs)
        {
            Material darkWood = LoadOrCreateStarterMaterial("Starter_Prop_DarkWood", new Color(0.24f, 0.15f, 0.08f), 0.00f, 0.30f);
            Material warmWood = LoadOrCreateStarterMaterial("Starter_Prop_WarmWood", new Color(0.42f, 0.27f, 0.14f), 0.00f, 0.34f);
            Material paleWood = LoadOrCreateStarterMaterial("Starter_Prop_PaleWood", new Color(0.58f, 0.42f, 0.25f), 0.00f, 0.32f);
            Material iron = LoadOrCreateStarterMaterial("Starter_Prop_Iron", new Color(0.25f, 0.26f, 0.27f), 0.36f, 0.50f);
            Material cloth = LoadOrCreateStarterMaterial("Starter_Prop_MarketCloth", new Color(0.45f, 0.08f, 0.08f), 0.00f, 0.42f);
            Material glass = LoadOrCreateStarterMaterial("Starter_Prop_BoothGlass", new Color(0.48f, 0.66f, 0.70f), 0.00f, 0.58f);
            Material stone = LoadOrCreateStarterMaterial("Starter_Prop_Stone", new Color(0.40f, 0.38f, 0.34f), 0.00f, 0.22f);
            Material ember = LoadOrCreateStarterMaterial("Starter_Prop_Ember", new Color(0.96f, 0.38f, 0.08f), 0.00f, 0.68f);
            Mesh stakeMesh = LoadOrCreateMeshAsset("Starter_Prop_TaperedStake", CreateTaperedCylinderYMesh(8, 0.08f));

            BuildCratePrefab(prefabs, darkWood, paleWood, iron);
            BuildBarrelPrefab(prefabs, darkWood, warmWood, iron);
            BuildChestPrefab(prefabs, darkWood, paleWood, iron);
            BuildCartPrefab(prefabs, darkWood, warmWood, iron);
            BuildDoorPrefab(prefabs, darkWood, paleWood, iron);
            BuildBankBoothPrefab(prefabs, darkWood, paleWood, iron, glass);
            BuildBankTablePrefab(prefabs, darkWood, paleWood, iron);
            BuildMarketStallPrefab(prefabs, darkWood, paleWood, cloth);
            BuildSignpostPrefab(prefabs, darkWood, paleWood, iron);
            BuildDefenceWallPrefab(prefabs, darkWood, iron, stakeMesh);
            BuildAnvilPrefab(prefabs, iron);
            BuildFurnacePrefab(prefabs, stone, iron, ember);
        }

        private static void BuildCratePrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material paleWood, Material iron)
        {
            GameObject root = new GameObject("Starter_Crate");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Crate Core", paleWood, new Vector3(0f, 0.39f, 0f), Quaternion.identity, new Vector3(0.72f, 0.72f, 0.72f));
            for (int i = -1; i <= 1; i += 2)
            {
                AddPrimitivePart(root.transform, PrimitiveType.Cube, $"Crate Front Plank {i}", darkWood, new Vector3(i * 0.22f, 0.40f, -0.372f), Quaternion.identity, new Vector3(0.045f, 0.78f, 0.045f));
                AddPrimitivePart(root.transform, PrimitiveType.Cube, $"Crate Back Plank {i}", darkWood, new Vector3(i * 0.22f, 0.40f, 0.372f), Quaternion.identity, new Vector3(0.045f, 0.78f, 0.045f));
                AddPrimitivePart(root.transform, PrimitiveType.Cube, $"Crate Side Plank {i}", darkWood, new Vector3(-0.372f, 0.40f, i * 0.22f), Quaternion.identity, new Vector3(0.045f, 0.78f, 0.045f));
                AddPrimitivePart(root.transform, PrimitiveType.Cube, $"Crate Far Side Plank {i}", darkWood, new Vector3(0.372f, 0.40f, i * 0.22f), Quaternion.identity, new Vector3(0.045f, 0.78f, 0.045f));
            }

            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Crate Iron Band Front", iron, new Vector3(0f, 0.74f, -0.392f), Quaternion.identity, new Vector3(0.76f, 0.050f, 0.030f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Crate Iron Band Back", iron, new Vector3(0f, 0.74f, 0.392f), Quaternion.identity, new Vector3(0.76f, 0.050f, 0.030f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Crate Top Grain", darkWood, new Vector3(0f, 0.775f, 0f), Quaternion.identity, new Vector3(0.68f, 0.035f, 0.68f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_Crate", root, 0.030f);
        }

        private static void BuildBarrelPrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material warmWood, Material iron)
        {
            GameObject root = new GameObject("Starter_Barrel");
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Barrel Body", warmWood, new Vector3(0f, 0.46f, 0f), Quaternion.identity, new Vector3(0.34f, 0.46f, 0.34f));
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Barrel Top Rim", iron, new Vector3(0f, 0.88f, 0f), Quaternion.identity, new Vector3(0.36f, 0.035f, 0.36f));
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Barrel Lower Rim", iron, new Vector3(0f, 0.16f, 0f), Quaternion.identity, new Vector3(0.36f, 0.035f, 0.36f));
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Barrel Mid Band", iron, new Vector3(0f, 0.51f, 0f), Quaternion.identity, new Vector3(0.365f, 0.028f, 0.365f));
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Barrel Lid", darkWood, new Vector3(0f, 0.94f, 0f), Quaternion.identity, new Vector3(0.31f, 0.018f, 0.31f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_Barrel", root, 0.030f);
        }

        private static void BuildChestPrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material paleWood, Material iron)
        {
            GameObject root = new GameObject("Starter_Chest");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Chest Base", darkWood, new Vector3(0f, 0.30f, 0f), Quaternion.identity, new Vector3(0.86f, 0.42f, 0.52f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Chest Raised Lid", paleWood, new Vector3(0f, 0.57f, 0f), Quaternion.Euler(-5f, 0f, 0f), new Vector3(0.88f, 0.20f, 0.54f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Chest Front Lock Plate", iron, new Vector3(0f, 0.42f, -0.285f), Quaternion.identity, new Vector3(0.16f, 0.18f, 0.030f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Chest Left Band", iron, new Vector3(-0.31f, 0.42f, -0.292f), Quaternion.identity, new Vector3(0.045f, 0.50f, 0.030f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Chest Right Band", iron, new Vector3(0.31f, 0.42f, -0.292f), Quaternion.identity, new Vector3(0.045f, 0.50f, 0.030f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_Chest", root, 0.032f);
        }

        private static void BuildCartPrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material warmWood, Material iron)
        {
            GameObject root = new GameObject("Starter_Cart");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Cart Bed", warmWood, new Vector3(0f, 0.50f, 0f), Quaternion.identity, new Vector3(1.18f, 0.22f, 0.82f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Cart Front Rail", darkWood, new Vector3(0f, 0.74f, -0.42f), Quaternion.identity, new Vector3(1.22f, 0.25f, 0.055f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Cart Back Rail", darkWood, new Vector3(0f, 0.74f, 0.42f), Quaternion.identity, new Vector3(1.22f, 0.25f, 0.055f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Cart Draw Beam", darkWood, new Vector3(0f, 0.42f, -0.94f), Quaternion.identity, new Vector3(0.16f, 0.16f, 1.08f));
            for (int i = -1; i <= 1; i += 2)
            {
                AddPrimitivePart(root.transform, PrimitiveType.Cylinder, $"Cart Wheel {i}", darkWood, new Vector3(i * 0.68f, 0.34f, 0.18f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.25f, 0.060f, 0.25f));
                AddPrimitivePart(root.transform, PrimitiveType.Cylinder, $"Cart Iron Hub {i}", iron, new Vector3(i * 0.69f, 0.34f, 0.18f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.11f, 0.070f, 0.11f));
            }

            SaveGeneratedStaticPrefab(prefabs, "Starter_Cart", root, 0.028f);
        }

        private static void BuildDoorPrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material paleWood, Material iron)
        {
            GameObject root = new GameObject("Starter_Door");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Door Slab", darkWood, new Vector3(0f, 0.92f, 0f), Quaternion.identity, new Vector3(0.92f, 1.76f, 0.12f));
            for (int i = -1; i <= 1; i++)
            {
                AddPrimitivePart(root.transform, PrimitiveType.Cube, $"Door Vertical Plank {i + 2}", paleWood, new Vector3(i * 0.25f, 0.92f, -0.07f), Quaternion.identity, new Vector3(0.055f, 1.70f, 0.035f));
            }

            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Door Upper Brace", iron, new Vector3(0f, 1.35f, -0.095f), Quaternion.identity, new Vector3(0.84f, 0.065f, 0.035f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Door Lower Brace", iron, new Vector3(0f, 0.48f, -0.095f), Quaternion.identity, new Vector3(0.84f, 0.065f, 0.035f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Door Diagonal Brace", iron, new Vector3(0f, 0.92f, -0.105f), Quaternion.Euler(0f, 0f, -32f), new Vector3(0.080f, 1.04f, 0.035f));
            AddPrimitivePart(root.transform, PrimitiveType.Sphere, "Door Ring Pull", iron, new Vector3(0.28f, 0.94f, -0.14f), Quaternion.identity, new Vector3(0.11f, 0.11f, 0.025f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_Door", root, 0.024f);
        }

        private static void BuildBankBoothPrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material paleWood, Material iron, Material glass)
        {
            GameObject root = new GameObject("Starter_BankBooth");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Bank Booth Counter", darkWood, new Vector3(0f, 0.42f, 0f), Quaternion.identity, new Vector3(1.22f, 0.64f, 0.56f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Bank Booth Stone Kick", iron, new Vector3(0f, 0.13f, -0.31f), Quaternion.identity, new Vector3(1.10f, 0.14f, 0.050f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Bank Booth Desk Top", paleWood, new Vector3(0f, 0.78f, 0f), Quaternion.identity, new Vector3(1.34f, 0.10f, 0.66f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Bank Booth Glass", glass, new Vector3(0f, 1.16f, -0.26f), Quaternion.identity, new Vector3(1.10f, 0.54f, 0.040f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Bank Booth Window Slot", darkWood, new Vector3(0f, 0.92f, -0.31f), Quaternion.identity, new Vector3(0.56f, 0.09f, 0.050f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_BankBooth", root, 0.024f);
        }

        private static void BuildBankTablePrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material paleWood, Material iron)
        {
            GameObject root = new GameObject("Starter_BankTable");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Bank Table Top", paleWood, new Vector3(0f, 0.58f, 0f), Quaternion.identity, new Vector3(1.20f, 0.12f, 0.72f));
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    AddPrimitivePart(root.transform, PrimitiveType.Cube, $"Bank Table Leg {x}.{z}", darkWood, new Vector3(x * 0.46f, 0.28f, z * 0.26f), Quaternion.identity, new Vector3(0.12f, 0.54f, 0.12f));
                }
            }

            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Ledger Cover", darkWood, new Vector3(-0.22f, 0.67f, -0.04f), Quaternion.Euler(0f, 18f, 0f), new Vector3(0.34f, 0.035f, 0.25f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Ledger Pages", iron, new Vector3(-0.22f, 0.70f, -0.04f), Quaternion.Euler(0f, 18f, 0f), new Vector3(0.28f, 0.025f, 0.20f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_BankTable", root, 0.030f);
        }

        private static void BuildMarketStallPrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material paleWood, Material cloth)
        {
            GameObject root = new GameObject("Starter_MarketStall");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Stall Counter", paleWood, new Vector3(0f, 0.56f, 0f), Quaternion.identity, new Vector3(1.46f, 0.24f, 0.78f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Stall Front Cloth", cloth, new Vector3(0f, 0.44f, -0.42f), Quaternion.identity, new Vector3(1.34f, 0.34f, 0.030f));
            for (int x = -1; x <= 1; x += 2)
            {
                AddPrimitivePart(root.transform, PrimitiveType.Cube, $"Stall Post Front {x}", darkWood, new Vector3(x * 0.66f, 1.02f, -0.34f), Quaternion.identity, new Vector3(0.10f, 1.56f, 0.10f));
                AddPrimitivePart(root.transform, PrimitiveType.Cube, $"Stall Post Back {x}", darkWood, new Vector3(x * 0.66f, 1.02f, 0.34f), Quaternion.identity, new Vector3(0.10f, 1.56f, 0.10f));
            }

            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Stall Canopy", cloth, new Vector3(0f, 1.74f, 0f), Quaternion.Euler(0f, 0f, 2f), new Vector3(1.62f, 0.16f, 1.10f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Stall Rear Beam", darkWood, new Vector3(0f, 1.60f, 0.42f), Quaternion.identity, new Vector3(1.56f, 0.12f, 0.10f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_MarketStall", root, 0.020f);
        }

        private static void BuildSignpostPrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material paleWood, Material iron)
        {
            GameObject root = new GameObject("Starter_Signpost");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Signpost Upright", darkWood, new Vector3(0f, 0.70f, 0f), Quaternion.identity, new Vector3(0.14f, 1.34f, 0.14f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Signpost Cross Beam", darkWood, new Vector3(0f, 1.22f, 0f), Quaternion.identity, new Vector3(0.92f, 0.10f, 0.12f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Hanging Sign Board", paleWood, new Vector3(0f, 0.98f, -0.08f), Quaternion.identity, new Vector3(0.76f, 0.36f, 0.055f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Left Chain", iron, new Vector3(-0.26f, 1.10f, -0.10f), Quaternion.identity, new Vector3(0.025f, 0.24f, 0.025f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Right Chain", iron, new Vector3(0.26f, 1.10f, -0.10f), Quaternion.identity, new Vector3(0.025f, 0.24f, 0.025f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_Signpost", root, 0.026f);
        }

        private static void BuildDefenceWallPrefab(Dictionary<string, GameObject> prefabs, Material darkWood, Material iron, Mesh stakeMesh)
        {
            GameObject root = new GameObject("Starter_DefenceWall");
            for (int i = -3; i <= 3; i++)
            {
                AddMeshPart(root.transform, $"Sharpened Stake {i + 4}", stakeMesh, darkWood, new Vector3(i * 0.16f, 0.65f, 0f), Quaternion.Euler(0f, 0f, i % 2 == 0 ? -4f : 5f), new Vector3(0.15f, 1.26f, 0.15f));
            }

            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Defence Cross Beam Low", iron, new Vector3(0f, 0.42f, -0.06f), Quaternion.identity, new Vector3(1.20f, 0.07f, 0.08f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Defence Cross Beam High", iron, new Vector3(0f, 0.82f, -0.06f), Quaternion.identity, new Vector3(1.14f, 0.07f, 0.08f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_DefenceWall", root, 0.026f);
        }

        private static void BuildAnvilPrefab(Dictionary<string, GameObject> prefabs, Material iron)
        {
            GameObject root = new GameObject("Starter_Anvil");
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Anvil Foot", iron, new Vector3(0f, 0.12f, 0f), Quaternion.identity, new Vector3(0.52f, 0.18f, 0.42f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Anvil Waist", iron, new Vector3(0f, 0.31f, 0f), Quaternion.identity, new Vector3(0.34f, 0.22f, 0.30f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Anvil Face", iron, new Vector3(0f, 0.49f, 0f), Quaternion.identity, new Vector3(0.82f, 0.20f, 0.36f));
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Anvil Horn", iron, new Vector3(0.48f, 0.49f, 0f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.18f, 0.20f, 0.18f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_Anvil", root, 0.032f);
        }

        private static void BuildFurnacePrefab(Dictionary<string, GameObject> prefabs, Material stone, Material iron, Material ember)
        {
            GameObject root = new GameObject("Starter_Furnace");
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Furnace Stone Body", stone, new Vector3(0f, 0.54f, 0f), Quaternion.identity, new Vector3(0.50f, 0.54f, 0.50f));
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Furnace Iron Rim", iron, new Vector3(0f, 1.05f, 0f), Quaternion.identity, new Vector3(0.52f, 0.045f, 0.52f));
            AddPrimitivePart(root.transform, PrimitiveType.Cube, "Furnace Fire Mouth", ember, new Vector3(0f, 0.48f, -0.48f), Quaternion.identity, new Vector3(0.42f, 0.30f, 0.055f));
            AddPrimitivePart(root.transform, PrimitiveType.Cylinder, "Furnace Ember Bed", ember, new Vector3(0f, 1.12f, 0f), Quaternion.identity, new Vector3(0.32f, 0.020f, 0.32f));
            SaveGeneratedStaticPrefab(prefabs, "Starter_Furnace", root, 0.026f);
        }

        private static GameObject AddPrimitivePart(Transform root, PrimitiveType primitiveType, string name, Material material, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(root, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            MeshRenderer renderer = part.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            RemovePrimitiveCollider(part);
            return part;
        }

        private static void RemovePrimitiveCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void SaveGeneratedStaticPrefab(Dictionary<string, GameObject> prefabs, string prefabName, GameObject root, float cullHeight)
        {
            ConfigureRenderers(root);
            AlignVisualBottomToRootGround(root.transform);
            AddCullLod(root, cullHeight);
            AddBoundsCollider(root);
            string prefabPath = $"{PrefabRoot}/{prefabName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
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

        private static void BuildDogPrefab(Dictionary<string, GameObject> prefabs, string prefabName, float targetHeight, Color furColor, Color skinColor, bool menacing = false)
        {
            Material fur = LoadOrCreateStarterMaterial($"{prefabName}_Fur", furColor, 0.01f, menacing ? 0.30f : 0.40f);
            Material skin = LoadOrCreateStarterMaterial($"{prefabName}_Skin", skinColor, 0.0f, 0.28f);
            Material eye = LoadOrCreateStarterMaterial($"{prefabName}_Eye", Color.black, 0.0f, 0.60f);

            Mesh bodyMesh = LoadOrCreateMeshAsset($"{prefabName}_Ellipsoid", CreateEllipsoidMesh(22, 12));
            Mesh legMesh = LoadOrCreateMeshAsset($"{prefabName}_Leg", CreateTaperedCylinderYMesh(10, 0.76f));
            Mesh tailMesh = LoadOrCreateMeshAsset($"{prefabName}_Tail", CreateTaperedCylinderZMesh(10, 0.48f));

            GameObject root = new GameObject(prefabName);
            float h = Mathf.Max(0.16f, targetHeight);
            AddMeshPart(root.transform, "Body", bodyMesh, fur, new Vector3(0f, h * 0.32f, -h * 0.04f), Quaternion.Euler(-3f, 0f, 0f), new Vector3(h * 0.46f, h * 0.36f, h * 0.88f));
            AddMeshPart(root.transform, "Chest", bodyMesh, fur, new Vector3(0f, h * 0.40f, h * 0.28f), Quaternion.Euler(5f, 0f, 0f), new Vector3(h * 0.40f, h * 0.40f, h * 0.44f));
            AddMeshPart(root.transform, "Head", bodyMesh, fur, new Vector3(0f, h * 0.56f, h * 0.60f), Quaternion.Euler(7f, 0f, 0f), new Vector3(h * 0.34f, h * 0.30f, h * 0.36f));
            AddMeshPart(root.transform, "Muzzle", bodyMesh, skin, new Vector3(0f, h * 0.51f, h * 0.82f), Quaternion.identity, new Vector3(h * 0.20f, h * 0.13f, h * 0.20f));
            AddMeshPart(root.transform, "Nose", bodyMesh, eye, new Vector3(0f, h * 0.54f, h * 0.95f), Quaternion.identity, Vector3.one * (h * 0.045f));
            AddMeshPart(root.transform, "Ear L", bodyMesh, fur, new Vector3(-h * 0.14f, h * 0.72f, h * 0.55f), Quaternion.Euler(12f, 0f, -18f), new Vector3(h * 0.10f, h * 0.22f, h * 0.055f));
            AddMeshPart(root.transform, "Ear R", bodyMesh, fur, new Vector3(h * 0.14f, h * 0.72f, h * 0.55f), Quaternion.Euler(12f, 0f, 18f), new Vector3(h * 0.10f, h * 0.22f, h * 0.055f));
            AddMeshPart(root.transform, "Eye L", bodyMesh, eye, new Vector3(-h * 0.10f, h * 0.60f, h * 0.77f), Quaternion.identity, Vector3.one * (h * 0.032f));
            AddMeshPart(root.transform, "Eye R", bodyMesh, eye, new Vector3(h * 0.10f, h * 0.60f, h * 0.77f), Quaternion.identity, Vector3.one * (h * 0.032f));

            AddMeshPart(root.transform, "Leg FL", legMesh, skin, new Vector3(-h * 0.18f, h * 0.15f, h * 0.26f), Quaternion.identity, new Vector3(h * 0.075f, h * 0.30f, h * 0.075f));
            AddMeshPart(root.transform, "Leg FR", legMesh, skin, new Vector3(h * 0.18f, h * 0.15f, h * 0.26f), Quaternion.identity, new Vector3(h * 0.075f, h * 0.30f, h * 0.075f));
            AddMeshPart(root.transform, "Leg BL", legMesh, skin, new Vector3(-h * 0.19f, h * 0.15f, -h * 0.36f), Quaternion.identity, new Vector3(h * 0.080f, h * 0.30f, h * 0.080f));
            AddMeshPart(root.transform, "Leg BR", legMesh, skin, new Vector3(h * 0.19f, h * 0.15f, -h * 0.36f), Quaternion.identity, new Vector3(h * 0.080f, h * 0.30f, h * 0.080f));
            AddMeshPart(root.transform, "Tail", tailMesh, fur, new Vector3(0f, h * 0.48f, -h * 0.52f), Quaternion.Euler(-24f, 0f, 0f), new Vector3(h * 0.055f, h * 0.055f, h * 0.54f));

            SaveGeneratedCreaturePrefab(prefabs, prefabName, root, menacing ? 0.030f : 0.038f);
        }

        private static void BuildCattlePrefab(Dictionary<string, GameObject> prefabs, string prefabName, float targetHeight, Color hideColor, Color muzzleColor)
        {
            Material hide = LoadOrCreateStarterMaterial($"{prefabName}_Hide", hideColor, 0.0f, 0.36f);
            Material muzzle = LoadOrCreateStarterMaterial($"{prefabName}_Muzzle", muzzleColor, 0.0f, 0.30f);
            Material horn = LoadOrCreateStarterMaterial($"{prefabName}_Horn", new Color(0.78f, 0.70f, 0.52f), 0.0f, 0.42f);
            Material eye = LoadOrCreateStarterMaterial($"{prefabName}_Eye", Color.black, 0.0f, 0.58f);

            Mesh bodyMesh = LoadOrCreateMeshAsset($"{prefabName}_Ellipsoid", CreateEllipsoidMesh(24, 12));
            Mesh legMesh = LoadOrCreateMeshAsset($"{prefabName}_Leg", CreateTaperedCylinderYMesh(10, 0.82f));
            Mesh hornMesh = LoadOrCreateMeshAsset($"{prefabName}_Horn", CreateTaperedCylinderYMesh(8, 0.12f));

            GameObject root = new GameObject(prefabName);
            float h = Mathf.Max(0.30f, targetHeight);
            AddMeshPart(root.transform, "Body", bodyMesh, hide, new Vector3(0f, h * 0.43f, -h * 0.03f), Quaternion.Euler(-2f, 0f, 0f), new Vector3(h * 0.82f, h * 0.48f, h * 1.28f));
            AddMeshPart(root.transform, "Chest", bodyMesh, hide, new Vector3(0f, h * 0.48f, h * 0.40f), Quaternion.identity, new Vector3(h * 0.62f, h * 0.50f, h * 0.48f));
            AddMeshPart(root.transform, "Head", bodyMesh, hide, new Vector3(0f, h * 0.64f, h * 0.76f), Quaternion.Euler(4f, 0f, 0f), new Vector3(h * 0.42f, h * 0.34f, h * 0.46f));
            AddMeshPart(root.transform, "Muzzle", bodyMesh, muzzle, new Vector3(0f, h * 0.57f, h * 1.03f), Quaternion.identity, new Vector3(h * 0.36f, h * 0.20f, h * 0.26f));
            AddMeshPart(root.transform, "Ear L", bodyMesh, hide, new Vector3(-h * 0.28f, h * 0.72f, h * 0.72f), Quaternion.Euler(0f, 0f, -24f), new Vector3(h * 0.16f, h * 0.09f, h * 0.06f));
            AddMeshPart(root.transform, "Ear R", bodyMesh, hide, new Vector3(h * 0.28f, h * 0.72f, h * 0.72f), Quaternion.Euler(0f, 0f, 24f), new Vector3(h * 0.16f, h * 0.09f, h * 0.06f));
            AddMeshPart(root.transform, "Horn L", hornMesh, horn, new Vector3(-h * 0.18f, h * 0.86f, h * 0.72f), Quaternion.Euler(0f, 0f, 27f), new Vector3(h * 0.045f, h * 0.18f, h * 0.045f));
            AddMeshPart(root.transform, "Horn R", hornMesh, horn, new Vector3(h * 0.18f, h * 0.86f, h * 0.72f), Quaternion.Euler(0f, 0f, -27f), new Vector3(h * 0.045f, h * 0.18f, h * 0.045f));
            AddMeshPart(root.transform, "Eye L", bodyMesh, eye, new Vector3(-h * 0.13f, h * 0.66f, h * 0.98f), Quaternion.identity, Vector3.one * (h * 0.028f));
            AddMeshPart(root.transform, "Eye R", bodyMesh, eye, new Vector3(h * 0.13f, h * 0.66f, h * 0.98f), Quaternion.identity, Vector3.one * (h * 0.028f));

            AddMeshPart(root.transform, "Leg FL", legMesh, hide, new Vector3(-h * 0.30f, h * 0.20f, h * 0.34f), Quaternion.identity, new Vector3(h * 0.095f, h * 0.40f, h * 0.095f));
            AddMeshPart(root.transform, "Leg FR", legMesh, hide, new Vector3(h * 0.30f, h * 0.20f, h * 0.34f), Quaternion.identity, new Vector3(h * 0.095f, h * 0.40f, h * 0.095f));
            AddMeshPart(root.transform, "Leg BL", legMesh, hide, new Vector3(-h * 0.30f, h * 0.20f, -h * 0.43f), Quaternion.identity, new Vector3(h * 0.105f, h * 0.40f, h * 0.105f));
            AddMeshPart(root.transform, "Leg BR", legMesh, hide, new Vector3(h * 0.30f, h * 0.20f, -h * 0.43f), Quaternion.identity, new Vector3(h * 0.105f, h * 0.40f, h * 0.105f));

            SaveGeneratedCreaturePrefab(prefabs, prefabName, root, 0.032f);
        }

        private static void BuildSheepPrefab(Dictionary<string, GameObject> prefabs, string prefabName, float targetHeight, Color woolColor, Color faceColor, bool horns)
        {
            Material wool = LoadOrCreateStarterMaterial($"{prefabName}_Wool", woolColor, 0.0f, 0.52f);
            Material face = LoadOrCreateStarterMaterial($"{prefabName}_Face", faceColor, 0.0f, 0.34f);
            Material horn = LoadOrCreateStarterMaterial($"{prefabName}_Horn", new Color(0.66f, 0.58f, 0.42f), 0.0f, 0.40f);
            Material eye = LoadOrCreateStarterMaterial($"{prefabName}_Eye", Color.black, 0.0f, 0.58f);

            Mesh bodyMesh = LoadOrCreateMeshAsset($"{prefabName}_Ellipsoid", CreateEllipsoidMesh(22, 12));
            Mesh legMesh = LoadOrCreateMeshAsset($"{prefabName}_Leg", CreateTaperedCylinderYMesh(10, 0.76f));
            Mesh hornMesh = LoadOrCreateMeshAsset($"{prefabName}_Horn", CreateTaperedCylinderYMesh(8, 0.18f));

            GameObject root = new GameObject(prefabName);
            float h = Mathf.Max(0.20f, targetHeight);
            AddMeshPart(root.transform, "Core Wool", bodyMesh, wool, new Vector3(0f, h * 0.40f, -h * 0.04f), Quaternion.identity, new Vector3(h * 0.70f, h * 0.50f, h * 0.92f));
            int puffIndex = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    AddMeshPart(root.transform, $"Wool Puff {puffIndex++}", bodyMesh, wool, new Vector3(h * 0.18f * x, h * (0.48f + 0.05f * Mathf.Abs(x)), h * (0.20f * z - 0.05f)), Quaternion.identity, Vector3.one * (h * 0.24f));
                }
            }

            AddMeshPart(root.transform, "Head", bodyMesh, face, new Vector3(0f, h * 0.48f, h * 0.55f), Quaternion.Euler(5f, 0f, 0f), new Vector3(h * 0.32f, h * 0.27f, h * 0.36f));
            AddMeshPart(root.transform, "Muzzle", bodyMesh, face, new Vector3(0f, h * 0.42f, h * 0.77f), Quaternion.identity, new Vector3(h * 0.20f, h * 0.13f, h * 0.18f));
            AddMeshPart(root.transform, "Ear L", bodyMesh, face, new Vector3(-h * 0.16f, h * 0.56f, h * 0.50f), Quaternion.Euler(0f, 0f, -18f), new Vector3(h * 0.10f, h * 0.055f, h * 0.05f));
            AddMeshPart(root.transform, "Ear R", bodyMesh, face, new Vector3(h * 0.16f, h * 0.56f, h * 0.50f), Quaternion.Euler(0f, 0f, 18f), new Vector3(h * 0.10f, h * 0.055f, h * 0.05f));
            AddMeshPart(root.transform, "Eye L", bodyMesh, eye, new Vector3(-h * 0.09f, h * 0.50f, h * 0.72f), Quaternion.identity, Vector3.one * (h * 0.026f));
            AddMeshPart(root.transform, "Eye R", bodyMesh, eye, new Vector3(h * 0.09f, h * 0.50f, h * 0.72f), Quaternion.identity, Vector3.one * (h * 0.026f));

            if (horns)
            {
                AddMeshPart(root.transform, "Horn L", hornMesh, horn, new Vector3(-h * 0.13f, h * 0.65f, h * 0.49f), Quaternion.Euler(0f, 0f, 38f), new Vector3(h * 0.04f, h * 0.16f, h * 0.04f));
                AddMeshPart(root.transform, "Horn R", hornMesh, horn, new Vector3(h * 0.13f, h * 0.65f, h * 0.49f), Quaternion.Euler(0f, 0f, -38f), new Vector3(h * 0.04f, h * 0.16f, h * 0.04f));
            }

            AddMeshPart(root.transform, "Leg FL", legMesh, face, new Vector3(-h * 0.22f, h * 0.15f, h * 0.23f), Quaternion.identity, new Vector3(h * 0.070f, h * 0.30f, h * 0.070f));
            AddMeshPart(root.transform, "Leg FR", legMesh, face, new Vector3(h * 0.22f, h * 0.15f, h * 0.23f), Quaternion.identity, new Vector3(h * 0.070f, h * 0.30f, h * 0.070f));
            AddMeshPart(root.transform, "Leg BL", legMesh, face, new Vector3(-h * 0.22f, h * 0.15f, -h * 0.35f), Quaternion.identity, new Vector3(h * 0.075f, h * 0.30f, h * 0.075f));
            AddMeshPart(root.transform, "Leg BR", legMesh, face, new Vector3(h * 0.22f, h * 0.15f, -h * 0.35f), Quaternion.identity, new Vector3(h * 0.075f, h * 0.30f, h * 0.075f));

            SaveGeneratedCreaturePrefab(prefabs, prefabName, root, 0.038f);
        }

        private static void BuildImpPrefab(Dictionary<string, GameObject> prefabs, string prefabName, float targetHeight)
        {
            Material skin = LoadOrCreateStarterMaterial($"{prefabName}_Skin", new Color(0.58f, 0.14f, 0.11f), 0.0f, 0.42f);
            Material wing = LoadOrCreateStarterMaterial($"{prefabName}_Wing", new Color(0.18f, 0.08f, 0.07f), 0.0f, 0.32f);
            Material horn = LoadOrCreateStarterMaterial($"{prefabName}_Horn", new Color(0.85f, 0.72f, 0.52f), 0.0f, 0.44f);
            Material eye = LoadOrCreateStarterMaterial($"{prefabName}_Eye", new Color(0.02f, 0.01f, 0.00f), 0.0f, 0.62f);

            Mesh bodyMesh = LoadOrCreateMeshAsset($"{prefabName}_Ellipsoid", CreateEllipsoidMesh(22, 12));
            Mesh limbMesh = LoadOrCreateMeshAsset($"{prefabName}_Limb", CreateTaperedCylinderYMesh(10, 0.70f));
            Mesh tailMesh = LoadOrCreateMeshAsset($"{prefabName}_Tail", CreateTaperedCylinderZMesh(10, 0.20f));
            Mesh hornMesh = LoadOrCreateMeshAsset($"{prefabName}_Horn", CreateTaperedCylinderYMesh(8, 0.08f));
            Mesh wingMesh = LoadOrCreateMeshAsset($"{prefabName}_Wing", CreateWingMesh());

            GameObject root = new GameObject(prefabName);
            float h = Mathf.Max(0.32f, targetHeight);
            AddMeshPart(root.transform, "Body", bodyMesh, skin, new Vector3(0f, h * 0.43f, 0f), Quaternion.identity, new Vector3(h * 0.42f, h * 0.48f, h * 0.30f));
            AddMeshPart(root.transform, "Head", bodyMesh, skin, new Vector3(0f, h * 0.78f, h * 0.02f), Quaternion.identity, new Vector3(h * 0.34f, h * 0.30f, h * 0.32f));
            AddMeshPart(root.transform, "Snout", bodyMesh, skin, new Vector3(0f, h * 0.74f, h * 0.22f), Quaternion.identity, new Vector3(h * 0.18f, h * 0.10f, h * 0.16f));
            AddMeshPart(root.transform, "Horn L", hornMesh, horn, new Vector3(-h * 0.10f, h * 0.98f, h * 0.02f), Quaternion.Euler(0f, 0f, -20f), new Vector3(h * 0.040f, h * 0.16f, h * 0.040f));
            AddMeshPart(root.transform, "Horn R", hornMesh, horn, new Vector3(h * 0.10f, h * 0.98f, h * 0.02f), Quaternion.Euler(0f, 0f, 20f), new Vector3(h * 0.040f, h * 0.16f, h * 0.040f));
            AddMeshPart(root.transform, "Eye L", bodyMesh, eye, new Vector3(-h * 0.08f, h * 0.80f, h * 0.20f), Quaternion.identity, Vector3.one * (h * 0.030f));
            AddMeshPart(root.transform, "Eye R", bodyMesh, eye, new Vector3(h * 0.08f, h * 0.80f, h * 0.20f), Quaternion.identity, Vector3.one * (h * 0.030f));

            AddMeshPart(root.transform, "Arm L", limbMesh, skin, new Vector3(-h * 0.27f, h * 0.45f, h * 0.03f), Quaternion.Euler(0f, 0f, 24f), new Vector3(h * 0.055f, h * 0.30f, h * 0.055f));
            AddMeshPart(root.transform, "Arm R", limbMesh, skin, new Vector3(h * 0.27f, h * 0.45f, h * 0.03f), Quaternion.Euler(0f, 0f, -24f), new Vector3(h * 0.055f, h * 0.30f, h * 0.055f));
            AddMeshPart(root.transform, "Leg L", limbMesh, skin, new Vector3(-h * 0.11f, h * 0.16f, h * 0.03f), Quaternion.identity, new Vector3(h * 0.070f, h * 0.32f, h * 0.070f));
            AddMeshPart(root.transform, "Leg R", limbMesh, skin, new Vector3(h * 0.11f, h * 0.16f, h * 0.03f), Quaternion.identity, new Vector3(h * 0.070f, h * 0.32f, h * 0.070f));
            AddMeshPart(root.transform, "Tail", tailMesh, skin, new Vector3(0f, h * 0.34f, -h * 0.18f), Quaternion.Euler(-12f, 0f, 0f), new Vector3(h * 0.040f, h * 0.040f, h * 0.44f));
            AddMeshPart(root.transform, "Wing L", wingMesh, wing, new Vector3(-h * 0.14f, h * 0.56f, -h * 0.18f), Quaternion.Euler(8f, -28f, 10f), new Vector3(h * 0.52f, h * 0.45f, h * 0.52f));
            AddMeshPart(root.transform, "Wing R", wingMesh, wing, new Vector3(h * 0.14f, h * 0.56f, -h * 0.18f), Quaternion.Euler(8f, 28f, -10f), new Vector3(-h * 0.52f, h * 0.45f, h * 0.52f));

            SaveGeneratedCreaturePrefab(prefabs, prefabName, root, 0.034f);
        }

        private static void SaveGeneratedCreaturePrefab(Dictionary<string, GameObject> prefabs, string prefabName, GameObject root, float cullHeight)
        {
            ConfigureRenderers(root);
            AddCullLod(root, cullHeight);
            AlignVisualBottomToRootGround(root.transform);
            string prefabPath = $"{PrefabRoot}/{prefabName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab != null)
            {
                prefabs[prefabName] = prefab;
            }
        }

        private static bool BuildSourceCreaturePrefab(Dictionary<string, GameObject> prefabs, string fbxRoot, string modelName, string prefabName, float targetHeight)
        {
            string modelPath = $"{fbxRoot}/{modelName}.fbx";
            GameObject prefab = BuildPrefab(modelPath, prefabName, targetHeight, false, false);
            if (prefab == null)
            {
                return false;
            }

            prefabs[prefabName] = prefab;
            return true;
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

        private static void RenderPreviewCameraToPng(Camera camera, string outputPath, int width, int height)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            RenderTexture target = new RenderTexture(width, height, 24)
            {
                antiAliasing = 4
            };

            camera.targetTexture = target;
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D capture = new Texture2D(width, height, TextureFormat.RGB24, false);
            capture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            capture.Apply();
            File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(capture);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            Debug.Log($"Rendered starter art preview to {outputPath}");
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

        private static Mesh CreateWingMesh()
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(new[]
            {
                new Vector3(0f, 0.10f, 0f),
                new Vector3(0.54f, 0.48f, 0f),
                new Vector3(0.30f, -0.48f, 0f),
                new Vector3(0.06f, -0.16f, 0f)
            });
            mesh.SetUVs(0, new[]
            {
                new Vector2(0.0f, 0.5f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.8f, 0.0f),
                new Vector2(0.2f, 0.2f)
            });
            mesh.SetTriangles(new[] { 0, 1, 3, 3, 1, 2, 3, 1, 0, 2, 1, 3 }, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
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
            Transform modelParent = root.transform;
            if (humanoid)
            {
                GameObject presentationRoot = new GameObject("Humanoid Presentation Root");
                presentationRoot.transform.SetParent(root.transform, false);
                modelParent = presentationRoot.transform;
            }

            GameObject model = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (model == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                Debug.LogWarning($"Could not instantiate starter art model: {modelPath}");
                return null;
            }

            model.name = "Model";
            model.transform.SetParent(modelParent, false);
            NormalizeHeight(root.transform, targetHeight);
            AlignVisualBottomToRootGround(root.transform);
            if (overrideMaterial != null)
            {
                AssignMaterial(root, overrideMaterial);
            }

            ConfigureRenderers(root);
            if (foliage)
            {
                ConfigureFoliagePresentation(root, prefabName, targetHeight, 0.010f);
            }
            else
            {
                AddCullLod(root, 0.018f);
            }

            if (addCollider)
            {
                AddBoundsCollider(root);
            }

            if (humanoid)
            {
                PsychoHumanoidPresentationRig presentationRig = model.AddComponent<PsychoHumanoidPresentationRig>();
                presentationRig.Configure(null, 0.006f, 0.018f, 1.55f, 0.85f, 1.15f);

                CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0f, targetHeight * 0.5f, 0f);
                collider.height = targetHeight;
                collider.radius = 0.32f;
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

        private static void ConfigureFoliagePresentation(GameObject root, string prefabName, float targetHeight, float cullHeight)
        {
            Renderer[] highRenderers = root.GetComponentsInChildren<Renderer>(true);
            AddFoliageWind(root, prefabName, targetHeight);
            Renderer[] lowRenderers = CreateFoliageLodProxy(root.transform, prefabName, targetHeight);
            AddFoliageLods(root, highRenderers, lowRenderers, cullHeight);
        }

        private static void AddFoliageWind(GameObject root, string prefabName, float targetHeight)
        {
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
            bool grassLike = IsGrassLikeFoliage(prefabName);
            bool bushLike = IsBushLikeFoliage(prefabName);
            bool deadTree = IsDeadTreeFoliage(prefabName);
            float amplitude = grassLike
                ? Mathf.Clamp(targetHeight * 0.055f, 0.018f, 0.052f)
                : bushLike
                    ? Mathf.Clamp(targetHeight * 0.040f, 0.020f, 0.058f)
                    : Mathf.Clamp(targetHeight * (deadTree ? 0.006f : 0.011f), 0.018f, 0.064f);
            float speed = grassLike ? 1.24f : bushLike ? 0.98f : 0.74f;
            float spatialFrequency = grassLike ? 2.15f : bushLike ? 1.62f : 0.92f;
            float gustStrength = grassLike ? 0.34f : bushLike ? 0.30f : 0.22f;
            float turbulence = grassLike ? 0.070f : bushLike ? 0.050f : 0.030f;

            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                if (filter == null || filter.sharedMesh == null)
                {
                    continue;
                }

                WindAnimatedFoliage wind = filter.GetComponent<WindAnimatedFoliage>();
                if (wind == null)
                {
                    wind = filter.gameObject.AddComponent<WindAnimatedFoliage>();
                }

                float offset = 1f + (i % 3) * 0.07f;
                wind.Configure(
                    amplitude * offset,
                    speed + i * 0.05f,
                    spatialFrequency,
                    gustStrength,
                    turbulence,
                    new Vector2(1f, 0.35f));
                EditorUtility.SetDirty(wind);
            }
        }

        private static Renderer[] CreateFoliageLodProxy(Transform root, string prefabName, float targetHeight)
        {
            List<Renderer> renderers = new List<Renderer>(4);
            GameObject proxyRoot = new GameObject("Foliage LOD Proxy");
            proxyRoot.transform.SetParent(root, false);

            Material leaf = LoadOrCreateStarterMaterial("Foliage_LOD_Leaf", new Color(0.22f, 0.40f, 0.20f), 0f, 0.20f);
            Material bark = LoadOrCreateStarterMaterial("Foliage_LOD_Bark", new Color(0.27f, 0.19f, 0.12f), 0f, 0.24f);
            Material flower = LoadOrCreateStarterMaterial("Foliage_LOD_Flower", new Color(0.70f, 0.64f, 0.38f), 0f, 0.36f);
            Mesh trunkMesh = LoadOrCreateMeshAsset("Foliage_LOD_Trunk_8", CreateTaperedCylinderYMesh(8, 0.62f));
            Mesh ellipsoidMesh = LoadOrCreateMeshAsset("Foliage_LOD_Ellipsoid_10x5", CreateEllipsoidMesh(10, 5));
            Mesh coneMesh = LoadOrCreateMeshAsset("Foliage_LOD_Cone_10", CreateTaperedCylinderYMesh(10, 0.06f));

            if (IsGrassLikeFoliage(prefabName))
            {
                renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Grass Blade A", ellipsoidMesh, leaf, new Vector3(-0.08f, targetHeight * 0.38f, 0f), Quaternion.Euler(0f, 18f, -8f), new Vector3(targetHeight * 0.20f, targetHeight * 0.78f, targetHeight * 0.055f)));
                renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Grass Blade B", ellipsoidMesh, leaf, new Vector3(0.08f, targetHeight * 0.34f, 0.02f), Quaternion.Euler(0f, -28f, 7f), new Vector3(targetHeight * 0.18f, targetHeight * 0.70f, targetHeight * 0.050f)));
                return renderers.ToArray();
            }

            if (IsBushLikeFoliage(prefabName))
            {
                renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Bush Mass", ellipsoidMesh, leaf, new Vector3(0f, targetHeight * 0.46f, 0f), Quaternion.identity, new Vector3(targetHeight * 1.20f, targetHeight * 0.78f, targetHeight * 1.04f)));
                if (prefabName.IndexOf("Flower", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Flower Flecks", ellipsoidMesh, flower, new Vector3(0.03f, targetHeight * 0.72f, 0.02f), Quaternion.Euler(0f, 31f, 0f), new Vector3(targetHeight * 0.62f, targetHeight * 0.14f, targetHeight * 0.48f)));
                }

                return renderers.ToArray();
            }

            bool pineLike = IsPineLikeFoliage(prefabName);
            bool deadTree = IsDeadTreeFoliage(prefabName);
            renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Bark Trunk", trunkMesh, bark, new Vector3(0f, targetHeight * 0.42f, 0f), Quaternion.identity, new Vector3(targetHeight * 0.080f, targetHeight * 0.84f, targetHeight * 0.080f)));

            if (deadTree)
            {
                renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Bare Branch A", trunkMesh, bark, new Vector3(-targetHeight * 0.10f, targetHeight * 0.70f, 0f), Quaternion.Euler(0f, 0f, 58f), new Vector3(targetHeight * 0.035f, targetHeight * 0.45f, targetHeight * 0.035f)));
                renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Bare Branch B", trunkMesh, bark, new Vector3(targetHeight * 0.11f, targetHeight * 0.82f, 0.03f), Quaternion.Euler(12f, 0f, -46f), new Vector3(targetHeight * 0.030f, targetHeight * 0.36f, targetHeight * 0.030f)));
                return renderers.ToArray();
            }

            if (pineLike)
            {
                renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Lower Conifer Mass", coneMesh, leaf, new Vector3(0f, targetHeight * 0.48f, 0f), Quaternion.identity, new Vector3(targetHeight * 0.46f, targetHeight * 0.72f, targetHeight * 0.46f)));
                renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Upper Conifer Mass", coneMesh, leaf, new Vector3(0f, targetHeight * 0.77f, 0f), Quaternion.Euler(0f, 32f, 0f), new Vector3(targetHeight * 0.30f, targetHeight * 0.54f, targetHeight * 0.30f)));
                return renderers.ToArray();
            }

            renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Broadleaf Mass", ellipsoidMesh, leaf, new Vector3(0f, targetHeight * 0.72f, 0f), Quaternion.identity, new Vector3(targetHeight * 0.70f, targetHeight * 0.46f, targetHeight * 0.66f)));
            renderers.Add(CreateFoliageProxyPart(proxyRoot.transform, "LOD Side Leaf Mass", ellipsoidMesh, leaf, new Vector3(targetHeight * 0.18f, targetHeight * 0.66f, -targetHeight * 0.05f), Quaternion.Euler(0f, 38f, 0f), new Vector3(targetHeight * 0.48f, targetHeight * 0.34f, targetHeight * 0.42f)));
            return renderers.ToArray();
        }

        private static Renderer CreateFoliageProxyPart(Transform root, string name, Mesh mesh, Material material, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(root, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
            return renderer;
        }

        private static void AddFoliageLods(GameObject root, Renderer[] highRenderers, Renderer[] lowRenderers, float cullHeight)
        {
            if (highRenderers == null || highRenderers.Length == 0)
            {
                AddCullLod(root, cullHeight);
                return;
            }

            LODGroup lodGroup = root.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = root.AddComponent<LODGroup>();
            }

            float nearHeight = Mathf.Clamp(cullHeight * 6.5f, 0.055f, 0.095f);
            float farHeight = Mathf.Clamp(cullHeight, 0.006f, 0.045f);
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;
            lodGroup.SetLODs(lowRenderers != null && lowRenderers.Length > 0
                ? new[] { new LOD(nearHeight, highRenderers), new LOD(farHeight, lowRenderers) }
                : new[] { new LOD(farHeight, highRenderers) });
            lodGroup.RecalculateBounds();
        }

        private static bool IsPineLikeFoliage(string prefabName)
        {
            return prefabName.IndexOf("Pine", StringComparison.OrdinalIgnoreCase) >= 0
                || prefabName.IndexOf("Conifer", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsDeadTreeFoliage(string prefabName)
        {
            return prefabName.IndexOf("Dead", StringComparison.OrdinalIgnoreCase) >= 0
                || prefabName.IndexOf("Twisted", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsBushLikeFoliage(string prefabName)
        {
            return prefabName.IndexOf("Bush", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsGrassLikeFoliage(string prefabName)
        {
            return prefabName.IndexOf("Grass", StringComparison.OrdinalIgnoreCase) >= 0;
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
