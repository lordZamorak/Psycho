using System;
using System.IO;
using System.Linq;
using Psycho.Networking;
using Psycho.UI;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoLoginSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/PsychoLogin.unity";
        private const string HostedScenePath = "Assets/Scenes/PsychoHostedTestWorld.unity";
        private const string LoginBackgroundPath = "Assets/Resources/PsychoLogin/login_smithing_bg.png";
        private const string WindowsBuildPath = "Builds/PsychoUnityClient/Psycho.exe";

        [MenuItem("Psycho/Build Login Scene")]
        public static void BuildLoginScene()
        {
            EnsureLoginTextureImportSettings();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = BuildCamera();
            PsychoProtocolClient protocolClient = BuildNetworkClient();
            BuildEventSystem();
            BuildLoginRuntime(protocolClient);

            EditorSceneManager.SaveScene(scene, ScenePath);
            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Psycho login scene built: {ScenePath}");
        }

        public static void BuildLoginSceneBatch()
        {
            BuildLoginScene();
        }

        [MenuItem("Psycho/Render Login Scene Preview")]
        public static void RenderLoginScenePreview()
        {
            if (!File.Exists(ToFullPath(ScenePath)))
            {
                BuildLoginScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PsychoLoginScreen loginScreen = UnityEngine.Object.FindAnyObjectByType<PsychoLoginScreen>();
            if (loginScreen != null)
            {
                loginScreen.RebuildUi();
            }

            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("Psycho login scene does not contain a camera.");
            }

            Canvas.ForceUpdateCanvases();
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-login-scene-preview.png"));
            RenderCameraToPng(camera, outputPath, 1920, 1080);
        }

        public static void RenderLoginScenePreviewBatch()
        {
            RenderLoginScenePreview();
        }

        [MenuItem("Psycho/Build Windows Login Playable")]
        public static void BuildWindowsLoginPlayable()
        {
            BuildLoginScene();
            if (!File.Exists(ToFullPath(HostedScenePath)))
            {
                PsychoHostedWorldSceneBuilder.BuildHostedTestWorldScene();
            }

            string outputPath = ToProjectFullPath(WindowsBuildPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath, HostedScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport buildReport = BuildPipeline.BuildPlayer(options);
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Windows login playable build failed: {buildReport.summary.result}");
            }

            Debug.Log($"Built Windows login playable to {outputPath} ({buildReport.summary.totalSize} bytes).");
        }

        public static void BuildWindowsLoginPlayableBatch()
        {
            BuildWindowsLoginPlayable();
        }

        private static Camera BuildCamera()
        {
            GameObject cameraObject = new GameObject("Login Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static PsychoProtocolClient BuildNetworkClient()
        {
            GameObject networkObject = new GameObject("Psycho Network");
            PsychoProtocolClient protocolClient = networkObject.AddComponent<PsychoProtocolClient>();
            networkObject.AddComponent<PsychoNetworkBootstrap>();
            return protocolClient;
        }

        private static void BuildEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static void BuildLoginRuntime(PsychoProtocolClient protocolClient)
        {
            GameObject loginObject = new GameObject("Psycho Login Screen");
            PsychoLoginScreen loginScreen = loginObject.AddComponent<PsychoLoginScreen>();
            SerializedObject serialized = new SerializedObject(loginScreen);
            serialized.FindProperty("protocolClient").objectReferenceValue = protocolClient;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            loginScreen.RebuildUi();
        }

        private static void EnsureLoginTextureImportSettings()
        {
            TextureImporter importer = AssetImporter.GetAtPath(LoginBackgroundPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.maxTextureSize = 4096;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void ConfigureBuildScenes()
        {
            string[] desiredScenes = File.Exists(ToFullPath(HostedScenePath))
                ? new[] { ScenePath, HostedScenePath }
                : new[] { ScenePath };

            EditorBuildSettings.scenes = desiredScenes
                .Concat(EditorBuildSettings.scenes.Select(scene => scene.path))
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();
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
            Debug.Log($"Rendered Psycho login scene preview to {outputPath}");
        }

        private static string ToFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private static string ToProjectFullPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }
    }
}
