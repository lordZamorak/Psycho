using System.IO;
using Psycho.Networking;
using Psycho.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
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

            QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 4);
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadows = ShadowQuality.All;

            Material grass = CreateMaterial("Assets/GrassWindPrototype.mat", new Color(0.24f, 0.48f, 0.14f), 0.18f);
            Material dirt = CreateMaterial("Assets/DarkSoilPrototype.mat", new Color(0.28f, 0.22f, 0.15f), 0.12f);
            Material stone = CreateMaterial("Assets/StonePathPrototype.mat", new Color(0.46f, 0.46f, 0.42f), 0.24f);
            Material trunk = CreateMaterial("Assets/TreeTrunkPrototype.mat", new Color(0.34f, 0.20f, 0.11f), 0.18f);
            Material canopy = CreateMaterial("Assets/TreeCanopyPrototype.mat", new Color(0.20f, 0.43f, 0.16f), 0.20f);
            Material reeds = CreateMaterial("Assets/ReedPrototype.mat", new Color(0.36f, 0.49f, 0.17f), 0.17f);
            Material wildflowers = CreateMaterial("Assets/WildflowerPrototype.mat", new Color(0.88f, 0.74f, 0.34f), 0.22f);
            Material water = CreateMaterial("Assets/WaterPrototype.mat", new Color(0.07f, 0.38f, 0.58f, 0.82f), 0.78f);
            ConfigureTransparent(water);

            GameObject target = new GameObject("Camera Target");
            target.transform.position = Vector3.zero;

            CreatePlane("Ground", Vector3.zero, new Vector3(22f, 1f, 22f), grass);
            CreatePlane("Stone Path", new Vector3(0f, 0.02f, 0f), new Vector3(3.2f, 1f, 20f), stone);
            CreatePlane("Soil Patch", new Vector3(-5.4f, 0.03f, 1.8f), new Vector3(4.2f, 1f, 3.4f), dirt);

            GameObject waterPlane = CreateSubdividedPlane("Ripple Water", new Vector3(5.2f, 0.05f, -4.7f), 5f, 4f, 28, water);
            waterPlane.AddComponent<ProceduralWater>();

            for (int i = 0; i < 185; i++)
            {
                float x = Mathf.Sin(i * 12.989f) * 9.4f;
                float z = Mathf.Cos(i * 5.713f) * 9.4f;
                if (Mathf.Abs(x) < 1.9f)
                {
                    x += x < 0f ? -2.2f : 2.2f;
                }

                Material material = i % 23 == 0 ? wildflowers : grass;
                CreateGrassBlade(new Vector3(x, 0.06f, z), 0.48f + (i % 7) * 0.05f, material);
            }

            for (int i = 0; i < 36; i++)
            {
                float angle = i * Mathf.PI * 2f / 36f;
                float radiusX = 2.95f + Mathf.Sin(i * 1.37f) * 0.2f;
                float radiusZ = 2.45f + Mathf.Cos(i * 0.91f) * 0.15f;
                Vector3 position = new Vector3(5.2f + Mathf.Cos(angle) * radiusX, 0.08f, -4.7f + Mathf.Sin(angle) * radiusZ);
                CreateReed(position, 0.85f + (i % 5) * 0.08f, reeds);
            }

            for (int i = 0; i < 30; i++)
            {
                float x = Mathf.Sin(i * 6.317f) * 8.7f;
                float z = Mathf.Cos(i * 4.791f) * 8.2f;
                if (Mathf.Abs(x) < 1.65f)
                {
                    z += 2.1f;
                }

                CreatePebble(new Vector3(x, 0.10f, z), 0.16f + (i % 4) * 0.035f, stone);
            }

            CreateTree(new Vector3(-7.2f, 0.05f, -5.4f), trunk, canopy);
            CreateTree(new Vector3(7.4f, 0.05f, 4.9f), trunk, canopy);

            GameObject sun = new GameObject("Sun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.92f, 0.80f);
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(44f, -36f, 0f);

            GameObject fill = new GameObject("Soft Blue Fill");
            Light fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.18f;
            fillLight.color = new Color(0.45f, 0.58f, 0.72f);
            fill.transform.rotation = Quaternion.Euler(22f, 138f, 0f);

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
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 160f;
            camera.depthTextureMode = DepthTextureMode.Depth;
            cameraObject.tag = "MainCamera";
            OrbitCameraRig orbit = cameraObject.AddComponent<OrbitCameraRig>();
            SerializedObject orbitObject = new SerializedObject(orbit);
            orbitObject.FindProperty("target").objectReferenceValue = target.transform;
            orbitObject.FindProperty("distance").floatValue = 13.5f;
            orbitObject.FindProperty("height").floatValue = 3.2f;
            orbitObject.FindProperty("pitch").floatValue = 24f;
            orbitObject.FindProperty("focusHeight").floatValue = 1.25f;
            orbitObject.ApplyModifiedPropertiesWithoutUndo();
            SetCameraPose(cameraObject.transform, target.transform.position);

            RenderSettings.skybox = CreateSkybox();
            RenderSettings.ambientLight = new Color(0.42f, 0.48f, 0.54f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.34f, 0.46f, 0.55f);
            RenderSettings.fogDensity = 0.008f;
            RenderSettings.reflectionIntensity = 0.34f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;

            GameObject environment = new GameObject("Dynamic Environment");
            EnvironmentLightingController environmentLighting = environment.AddComponent<EnvironmentLightingController>();
            SerializedObject environmentObject = new SerializedObject(environmentLighting);
            environmentObject.FindProperty("sun").objectReferenceValue = light;
            environmentObject.ApplyModifiedPropertiesWithoutUndo();

            GameObject protocol = new GameObject("Psycho Protocol Client");
            protocol.AddComponent<PsychoProtocolClient>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Built {ScenePath}");
        }

        [MenuItem("Psycho/Render Prototype Preview")]
        public static void RenderPrototypePreview()
        {
            if (!File.Exists(ScenePath))
            {
                BuildPrototypeScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Camera camera = UnityEngine.Object.FindObjectOfType<Camera>();
            if (camera == null)
            {
                throw new InvalidDataException("Prototype scene does not contain a camera.");
            }

            RenderTexture target = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture previousCameraTarget = camera.targetTexture;

            camera.targetTexture = target;
            RenderTexture.active = target;
            camera.Render();
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();

            camera.targetTexture = previousCameraTarget;
            RenderTexture.active = previous;

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string repoRoot = Directory.GetParent(projectRoot).FullName;
            string outputDirectory = Path.Combine(repoRoot, "run-logs");
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(outputDirectory, "unity-prototype-preview-pass2.png");
            File.WriteAllBytes(outputPath, image.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
            Debug.Log($"Rendered prototype preview to {outputPath}");
        }

        private static Material CreateMaterial(string path, Color color, float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            material.SetFloat("_Glossiness", smoothness);
            material.SetFloat("_Metallic", 0f);
            material.enableInstancing = true;
            return material;
        }

        private static void ConfigureTransparent(Material material)
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static void SetCameraPose(Transform cameraTransform, Vector3 targetPosition)
        {
            cameraTransform.position = targetPosition + new Vector3(-7.2f, 7.6f, -9.8f);
            cameraTransform.LookAt(targetPosition + Vector3.up * 1.25f);
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
            plane.transform.localScale = new Vector3(scale.x / 10f, 1f, scale.z / 10f);
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

            GameObject clump = new GameObject("Wind Grass");
            clump.transform.position = position;
            clump.transform.rotation = Quaternion.Euler(0f, position.x * 37f + position.z * 17f, 0f);
            clump.AddComponent<MeshFilter>().sharedMesh = mesh;
            clump.AddComponent<MeshRenderer>().sharedMaterial = material;
            AddWind(clump, 0.13f, 1.95f, 0.38f);
        }

        private static void CreateReed(Vector3 position, float height, Material material)
        {
            Mesh mesh = new Mesh();
            float width = height * 0.065f;
            mesh.vertices = new[]
            {
                new Vector3(-width, 0f, 0f),
                new Vector3(width, 0f, 0f),
                new Vector3(-width * 0.25f, height, 0f),
                new Vector3(width * 0.25f, height, 0f)
            };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateNormals();

            GameObject reed = new GameObject("Water Reed");
            reed.transform.position = position;
            reed.transform.rotation = Quaternion.Euler(0f, position.x * 51f + position.z * 29f, 0f);
            reed.AddComponent<MeshFilter>().sharedMesh = mesh;
            reed.AddComponent<MeshRenderer>().sharedMaterial = material;
            AddWind(reed, 0.20f, 1.55f, 0.55f);
        }

        private static void CreatePebble(Vector3 position, float size, Material material)
        {
            GameObject pebble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pebble.name = "Ground Pebble";
            pebble.transform.position = position;
            pebble.transform.rotation = Quaternion.Euler(position.z * 13f, position.x * 29f, position.z * 41f);
            pebble.transform.localScale = new Vector3(size * 1.55f, size * 0.42f, size);
            pebble.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateTree(Vector3 position, Material trunkMaterial, Material canopyMaterial)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Wind Tree Trunk";
            trunk.transform.position = position + Vector3.up * 0.85f;
            trunk.transform.localScale = new Vector3(0.28f, 0.85f, 0.28f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = trunkMaterial;

            CreateBranch(position + new Vector3(0.05f, 1.35f, 0f), new Vector3(0.68f, 0.48f, 0.18f), trunkMaterial);
            CreateBranch(position + new Vector3(-0.05f, 1.25f, 0.02f), new Vector3(-0.48f, 0.34f, -0.34f), trunkMaterial);

            CreateCanopyLobe(position + new Vector3(0f, 1.95f, 0f), new Vector3(1.35f, 0.82f, 1.15f), canopyMaterial);
            CreateCanopyLobe(position + new Vector3(0.62f, 1.82f, 0.18f), new Vector3(0.82f, 0.58f, 0.76f), canopyMaterial);
            CreateCanopyLobe(position + new Vector3(-0.48f, 1.73f, -0.28f), new Vector3(0.72f, 0.54f, 0.68f), canopyMaterial);
        }

        private static void CreateBranch(Vector3 origin, Vector3 direction, Material material)
        {
            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch.name = "Tree Branch";
            branch.transform.position = origin + direction * 0.5f;
            branch.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            branch.transform.localScale = new Vector3(0.10f, direction.magnitude * 0.5f, 0.10f);
            branch.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateCanopyLobe(Vector3 position, Vector3 scale, Material material)
        {
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Wind Tree Canopy";
            canopy.transform.position = position;
            canopy.transform.localScale = scale;
            canopy.GetComponent<MeshRenderer>().sharedMaterial = material;
            AddWind(canopy, 0.06f, 1.18f, 0.26f);
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
    }
}
