using System.IO;
using Psycho.Mirror;
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
            Material hills = CreateMaterial("Assets/DistantHillPrototype.mat", new Color(0.26f, 0.38f, 0.22f), 0.28f);
            Material mountains = CreateMaterial("Assets/DistantMountainPrototype.mat", new Color(0.34f, 0.37f, 0.39f), 0.44f);
            Material cloud = CreateUnlitMaterial("Assets/CloudPrototype.mat", new Color(0.92f, 0.95f, 0.96f, 0.78f));
            Material npcBody = CreateMaterial("Assets/NpcBodyPrototype.mat", new Color(0.55f, 0.19f, 0.17f), 0.28f);
            Material npcTrim = CreateMaterial("Assets/NpcTrimPrototype.mat", new Color(0.78f, 0.61f, 0.34f), 0.34f);
            ConfigureTransparent(cloud);
            Material water = CreateMaterial("Assets/WaterPrototype.mat", new Color(0.07f, 0.38f, 0.58f, 0.82f), 0.78f);
            ConfigureTransparent(water);

            GameObject target = new GameObject("Camera Target");
            target.transform.position = Vector3.zero;

            CreatePlane("Ground", Vector3.zero, new Vector3(72f, 1f, 72f), grass);
            CreatePlane("Stone Path", new Vector3(0f, 0.02f, 0f), new Vector3(3.2f, 1f, 40f), stone);
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
            CreateTree(new Vector3(-18.5f, 0.05f, 11.5f), trunk, canopy);
            CreateTree(new Vector3(17.5f, 0.05f, -12.5f), trunk, canopy);

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
            camera.farClipPlane = 2400f;
            camera.depthTextureMode = DepthTextureMode.Depth;
            cameraObject.tag = "MainCamera";
            OrbitCameraRig orbit = cameraObject.AddComponent<OrbitCameraRig>();
            SerializedObject orbitObject = new SerializedObject(orbit);
            orbitObject.FindProperty("target").objectReferenceValue = target.transform;
            orbitObject.FindProperty("distance").floatValue = 31f;
            orbitObject.FindProperty("height").floatValue = 5.6f;
            orbitObject.FindProperty("pitch").floatValue = 9f;
            orbitObject.FindProperty("focusHeight").floatValue = 2.1f;
            orbitObject.ApplyModifiedPropertiesWithoutUndo();
            SetCameraPose(cameraObject.transform, target.transform.position);
            CreateDistantVista(mountains, hills, cameraObject.transform);
            CreateCloudLayer(cloud);
            CreatePrototypeNpc(new Vector3(-1.9f, 0.16f, -3.8f), npcBody, npcTrim, 5.5f, 2.8f);
            CreatePrototypeNpc(new Vector3(4.1f, 0.16f, 2.8f), npcTrim, npcBody, 4.2f, 2.5f);
            CreatePrototypeNpc(new Vector3(-7.4f, 0.16f, 4.3f), npcBody, npcTrim, 3.6f, 2.2f);
            CreatePrototypeNpc(new Vector3(8.8f, 0.16f, -8.2f), npcTrim, npcBody, 6.3f, 3.1f);

            RenderSettings.skybox = CreateSkybox();
            RenderSettings.ambientLight = new Color(0.42f, 0.48f, 0.54f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.34f, 0.46f, 0.55f);
            RenderSettings.fogDensity = 0.0032f;
            RenderSettings.reflectionIntensity = 0.34f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;

            GameObject environment = new GameObject("Dynamic Environment");
            EnvironmentLightingController environmentLighting = environment.AddComponent<EnvironmentLightingController>();
            SerializedObject environmentObject = new SerializedObject(environmentLighting);
            environmentObject.FindProperty("sun").objectReferenceValue = light;
            environmentObject.ApplyModifiedPropertiesWithoutUndo();

            GameObject protocol = new GameObject("Psycho Protocol Client");
            protocol.AddComponent<PsychoProtocolClient>();

            GameObject mirror = new GameObject("Psycho Java Mirror");
            mirror.AddComponent<PsychoMirrorLoader>();
            mirror.AddComponent<PsychoVisualFactory>();
            mirror.AddComponent<PsychoMirrorScenePopulator>();

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
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
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

        private static Material CreateUnlitMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Unlit/Transparent");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (shader != null)
            {
                material.shader = shader;
            }

            material.color = color;
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
            cameraTransform.position = targetPosition + new Vector3(-17.5f, 8.8f, -28f);
            cameraTransform.LookAt(targetPosition + Vector3.up * 2.1f);
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

        private static void CreateDistantVista(Material mountainMaterial, Material hillMaterial, Transform viewer)
        {
            GameObject vistaRoot = new GameObject("Distant Vista");
            DistantVistaParallax parallax = vistaRoot.AddComponent<DistantVistaParallax>();
            SerializedObject parallaxObject = new SerializedObject(parallax);
            parallaxObject.FindProperty("viewer").objectReferenceValue = viewer;
            parallaxObject.FindProperty("parallaxStrength").floatValue = 0.018f;
            parallaxObject.ApplyModifiedPropertiesWithoutUndo();

            for (int i = 0; i < 24; i++)
            {
                float angle = i * Mathf.PI * 2f / 24f;
                float radius = 128f + Mathf.Sin(i * 1.71f) * 11f;
                float width = 24f + (i % 5) * 5f;
                float height = 14f + Mathf.Sin(i * 0.83f) * 3.5f + (i % 4) * 2.3f;
                float depth = 18f + (i % 3) * 5f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                GameObject mountain = CreateMountain($"Distant Mountain {i + 1}", position, width, height, depth, mountainMaterial);
                mountain.transform.SetParent(vistaRoot.transform, true);
            }

            for (int i = 0; i < 18; i++)
            {
                float angle = i * Mathf.PI * 2f / 18f + 0.15f;
                float radius = 74f + Mathf.Cos(i * 1.29f) * 7f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, -1.4f, Mathf.Sin(angle) * radius);
                GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hill.name = $"Rolling Hill {i + 1}";
                hill.transform.position = position;
                hill.transform.localScale = new Vector3(19f + (i % 4) * 4f, 4.5f + (i % 3) * 1.2f, 14f + (i % 5) * 2.5f);
                hill.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                hill.GetComponent<MeshRenderer>().sharedMaterial = hillMaterial;
                hill.transform.SetParent(vistaRoot.transform, true);
            }
        }

        private static GameObject CreateMountain(string name, Vector3 position, float width, float height, float depth, Material material)
        {
            Mesh mesh = new Mesh();
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
            mountain.transform.rotation = Quaternion.LookRotation(-new Vector3(position.x, 0f, position.z).normalized);
            mountain.AddComponent<MeshFilter>().sharedMesh = mesh;
            mountain.AddComponent<MeshRenderer>().sharedMaterial = material;
            return mountain;
        }

        private static void CreateCloudLayer(Material material)
        {
            for (int i = 0; i < 18; i++)
            {
                float x = Mathf.Sin(i * 2.91f) * 62f;
                float z = Mathf.Cos(i * 1.73f) * 64f;
                float y = 25f + (i % 5) * 2.4f;
                GameObject cloud = new GameObject($"Moving Cloud {i + 1}");
                cloud.transform.position = new Vector3(x, y, z);
                cloud.transform.rotation = Quaternion.Euler(0f, i * 23f, 0f);

                int lobes = 3 + i % 4;
                for (int lobe = 0; lobe < lobes; lobe++)
                {
                    GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    puff.name = "Cloud Puff";
                    puff.transform.SetParent(cloud.transform, false);
                    float localX = (lobe - (lobes - 1) * 0.5f) * 3.8f;
                    puff.transform.localPosition = new Vector3(localX, Mathf.Sin(lobe * 1.7f) * 0.35f, Mathf.Cos(lobe * 1.1f) * 1.1f);
                    puff.transform.localScale = new Vector3(6.2f + lobe * 0.6f, 0.75f + (lobe % 2) * 0.25f, 2.8f + (lobe % 3) * 0.6f);
                    MeshRenderer renderer = puff.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = material;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }

                CloudDrift drift = cloud.AddComponent<CloudDrift>();
                SerializedObject driftObject = new SerializedObject(drift);
                driftObject.FindProperty("driftSpeed").floatValue = 0.42f + (i % 4) * 0.06f;
                driftObject.FindProperty("wrapDistance").floatValue = 135f;
                driftObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void CreatePrototypeNpc(Vector3 position, Material bodyMaterial, Material trimMaterial, float wanderRadius, float speed)
        {
            GameObject npc = new GameObject("Wandering Prototype NPC");
            npc.transform.position = position;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "NPC Body";
            body.transform.SetParent(npc.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            body.transform.localScale = new Vector3(0.46f, 0.72f, 0.46f);
            body.GetComponent<MeshRenderer>().sharedMaterial = bodyMaterial;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "NPC Head";
            head.transform.SetParent(npc.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            head.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            head.GetComponent<MeshRenderer>().sharedMaterial = trimMaterial;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "NPC Direction Marker";
            marker.transform.SetParent(npc.transform, false);
            marker.transform.localPosition = new Vector3(0f, 1.2f, 0.34f);
            marker.transform.localScale = new Vector3(0.18f, 0.18f, 0.08f);
            marker.GetComponent<MeshRenderer>().sharedMaterial = trimMaterial;

            PrototypeNpcWander wander = npc.AddComponent<PrototypeNpcWander>();
            SerializedObject wanderObject = new SerializedObject(wander);
            wanderObject.FindProperty("wanderRadius").floatValue = wanderRadius;
            wanderObject.FindProperty("speed").floatValue = speed;
            wanderObject.ApplyModifiedPropertiesWithoutUndo();
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
