using System.IO;
using Psycho.Networking;
using Psycho.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoPrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/PsychoPrototype.unity";

        [MenuItem("Psycho/Build Prototype Scene")]
        public static void BuildPrototypeScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material grass = CreateMaterial("Assets/GrassWindPrototype.mat", new Color(0.22f, 0.44f, 0.10f));
            Material dirt = CreateMaterial("Assets/DarkSoilPrototype.mat", new Color(0.26f, 0.21f, 0.13f));
            Material stone = CreateMaterial("Assets/StonePathPrototype.mat", new Color(0.43f, 0.43f, 0.40f));
            Material trunk = CreateMaterial("Assets/TreeTrunkPrototype.mat", new Color(0.36f, 0.20f, 0.09f));
            Material canopy = CreateMaterial("Assets/TreeCanopyPrototype.mat", new Color(0.22f, 0.42f, 0.13f));
            Material water = CreateMaterial("Assets/WaterPrototype.mat", new Color(0.07f, 0.38f, 0.58f, 0.82f));
            water.SetFloat("_Glossiness", 0.72f);

            GameObject target = new GameObject("Camera Target");
            target.transform.position = Vector3.zero;

            CreatePlane("Ground", Vector3.zero, new Vector3(18f, 1f, 18f), grass);
            CreatePlane("Stone Path", new Vector3(0f, 0.02f, 0f), new Vector3(3.2f, 1f, 18f), stone);
            CreatePlane("Soil Patch", new Vector3(-5.4f, 0.03f, 1.8f), new Vector3(4.2f, 1f, 3.4f), dirt);

            GameObject waterPlane = CreateSubdividedPlane("Ripple Water", new Vector3(5.2f, 0.05f, -4.7f), 5f, 4f, 28, water);
            waterPlane.AddComponent<ProceduralWater>();

            for (int i = 0; i < 75; i++)
            {
                float x = Mathf.Sin(i * 12.989f) * 7.4f;
                float z = Mathf.Cos(i * 5.713f) * 7.4f;
                if (Mathf.Abs(x) < 1.9f)
                {
                    x += x < 0f ? -2.2f : 2.2f;
                }

                CreateGrassBlade(new Vector3(x, 0.06f, z), 0.55f + (i % 7) * 0.045f, grass);
            }

            CreateTree(new Vector3(-7.2f, 0.05f, -5.4f), trunk, canopy);
            CreateTree(new Vector3(7.4f, 0.05f, 4.9f), trunk, canopy);

            GameObject sun = new GameObject("Sun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.92f, 0.80f);
            sun.transform.rotation = Quaternion.Euler(44f, -36f, 0f);

            GameObject wind = new GameObject("Wind");
            WindZone windZone = wind.AddComponent<WindZone>();
            windZone.mode = WindZoneMode.Directional;
            windZone.windMain = 0.55f;
            windZone.windPulseMagnitude = 0.42f;
            windZone.windPulseFrequency = 0.7f;

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 46f;
            cameraObject.tag = "MainCamera";
            OrbitCameraRig orbit = cameraObject.AddComponent<OrbitCameraRig>();
            SerializedObject orbitObject = new SerializedObject(orbit);
            orbitObject.FindProperty("target").objectReferenceValue = target.transform;
            orbitObject.ApplyModifiedPropertiesWithoutUndo();

            RenderSettings.skybox = CreateSkybox();
            RenderSettings.ambientLight = new Color(0.42f, 0.48f, 0.54f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.34f, 0.46f, 0.55f);
            RenderSettings.fogDensity = 0.008f;

            GameObject protocol = new GameObject("Psycho Protocol Client");
            protocol.AddComponent<PsychoProtocolClient>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Built {ScenePath}");
        }

        private static Material CreateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            return material;
        }

        private static Material CreateSkybox()
        {
            Material skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/PsychoSkyboxPrototype.mat");
            if (skybox == null)
            {
                skybox = new Material(Shader.Find("Skybox/Procedural"));
                AssetDatabase.CreateAsset(skybox, "Assets/PsychoSkyboxPrototype.mat");
            }

            skybox.SetColor("_SkyTint", new Color(0.36f, 0.53f, 0.68f));
            skybox.SetFloat("_AtmosphereThickness", 0.82f);
            skybox.SetFloat("_Exposure", 1.05f);
            return skybox;
        }

        private static GameObject CreatePlane(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = name;
            plane.transform.position = position;
            plane.transform.localScale = scale;
            plane.GetComponent<MeshRenderer>().sharedMaterial = material;
            return plane;
        }

        private static GameObject CreateSubdividedPlane(string name, Vector3 position, float width, float depth, int segments, Material material)
        {
            Mesh mesh = new Mesh();
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

        private static void CreateGrassBlade(Vector3 position, float height, Material material)
        {
            Mesh mesh = new Mesh();
            float width = height * 0.18f;
            mesh.vertices = new[]
            {
                new Vector3(-width, 0f, 0f),
                new Vector3(width, 0f, 0f),
                new Vector3(-width * 0.32f, height, 0f),
                new Vector3(width * 0.32f, height, 0f)
            };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateNormals();

            GameObject blade = new GameObject("Wind Grass");
            blade.transform.position = position;
            blade.transform.rotation = Quaternion.Euler(0f, position.x * 37f + position.z * 17f, 0f);
            blade.AddComponent<MeshFilter>().sharedMesh = mesh;
            blade.AddComponent<MeshRenderer>().sharedMaterial = material;
            blade.AddComponent<WindAnimatedFoliage>();
        }

        private static void CreateTree(Vector3 position, Material trunkMaterial, Material canopyMaterial)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Wind Tree Trunk";
            trunk.transform.position = position + Vector3.up * 0.85f;
            trunk.transform.localScale = new Vector3(0.28f, 0.85f, 0.28f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = trunkMaterial;

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Wind Tree Canopy";
            canopy.transform.position = position + Vector3.up * 1.9f;
            canopy.transform.localScale = new Vector3(1.35f, 0.82f, 1.15f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial = canopyMaterial;
            canopy.AddComponent<WindAnimatedFoliage>();
        }
    }
}
