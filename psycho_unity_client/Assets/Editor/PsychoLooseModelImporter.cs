using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Psycho.Cache;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoLooseModelImporter
    {
        private const int LooseModelIdBase = 1_000_000;
        private const float NormalSmoothingTolerance = 0.00075f;
        private const string GeneratedRoot = "Assets/Generated/LooseModels";
        private const string ReportPath = GeneratedRoot + "/loose_model_import_report.json";
        private const string PreviewScenePath = GeneratedRoot + "/PsychoLooseModelPreview.unity";

        [MenuItem("Psycho/Cache/Import Loose Model Pack")]
        public static void ImportLooseModelPack()
        {
            LooseImportResult result = ImportLooseModels(true);
            Debug.Log($"Psycho loose model import complete: {result.imported} imported, {result.failed} failed. Report: {ReportPath}");
        }

        public static void ImportLooseModelPackBatch()
        {
            ImportLooseModelPack();
        }

        [MenuItem("Psycho/Cache/Render Loose Model Preview")]
        public static void RenderLooseModelPreview()
        {
            if (!File.Exists(ToFullPath(PreviewScenePath)))
            {
                ImportLooseModels(true);
            }

            EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("Loose model preview scene does not contain a camera.");
            }

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-loose-model-preview.png"));
            RenderCameraToPng(camera, outputPath, 1800, 1000);
        }

        public static void RenderLooseModelPreviewBatch()
        {
            RenderLooseModelPreview();
        }

        public static string DefaultLooseModelPath
        {
            get
            {
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
                return Path.Combine(projectRoot, "necrotic_client-item_attributes", "these");
            }
        }

        private static LooseImportResult ImportLooseModels(bool buildPreviewScene)
        {
            EnsureGeneratedFolders();
            string modelRoot = DefaultLooseModelPath;
            LooseImportResult result = new LooseImportResult
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                sourceRoot = modelRoot
            };

            if (!Directory.Exists(modelRoot))
            {
                result.entries = new[]
                {
                    LooseImportReportEntry.Failed(string.Empty, "source folder missing")
                };
                WriteReport(result);
                return result;
            }

            string[] files = Directory.GetFiles(modelRoot, "*.dat", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            result.scanned = files.Length;

            Material material = PsychoCacheMeshImporter.LoadOrCreateVertexColorMaterial();
            List<ImportedLooseModel> imported = new List<ImportedLooseModel>();
            List<LooseImportReportEntry> entries = new List<LooseImportReportEntry>();

            for (int i = 0; i < files.Length; i++)
            {
                string filePath = files[i];
                string relativePath = GetRelativePath(modelRoot, filePath);
                int modelId = LooseModelIdBase + i;
                string assetName = $"loose_{i:D4}_{SanitizeFileName(Path.GetFileNameWithoutExtension(filePath))}.asset";
                string assetPath = $"{GeneratedRoot}/{assetName}";

                try
                {
                    byte[] bytes = File.ReadAllBytes(filePath);
                    PsychoCacheModel decoded = PsychoModelDecoder.Decode(bytes, modelId);
                    Mesh mesh = PsychoModelMeshBuilder.BuildMesh(decoded);
                    ApplySoftNormals(mesh);
                    Mesh assetMesh = CreateOrUpdateMeshAsset(mesh, assetPath);
                    imported.Add(new ImportedLooseModel
                    {
                        displayName = Path.GetFileNameWithoutExtension(filePath),
                        relativePath = relativePath,
                        mesh = assetMesh
                    });

                    result.imported++;
                    entries.Add(LooseImportReportEntry.Imported(relativePath, assetPath, decoded.Format.ToString(), decoded.VertexCount, decoded.FaceCount));
                }
                catch (Exception ex)
                {
                    result.failed++;
                    entries.Add(LooseImportReportEntry.Failed(relativePath, ex.Message));
                    Debug.LogWarning($"Failed to decode loose model '{relativePath}': {ex.Message}");
                }
            }

            result.entries = entries.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            WriteReport(result);

            if (buildPreviewScene)
            {
                BuildPreviewScene(imported, material);
            }

            return result;
        }

        private static void BuildPreviewScene(List<ImportedLooseModel> imported, Material material)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Psycho Loose Model Preview");

            GameObject lightObject = new GameObject("Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.26f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -30f, 0f);

            GameObject fillObject = new GameObject("Fill Light");
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.46f;
            fill.color = new Color(0.70f, 0.82f, 1f, 1f);
            fillObject.transform.rotation = Quaternion.Euler(22f, 128f, 0f);

            GameObject cameraObject = new GameObject("Preview Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 45f;
            camera.farClipPlane = 500f;
            camera.transform.position = new Vector3(15.5f, 9.2f, -18.5f);
            camera.transform.rotation = Quaternion.Euler(32f, -38f, 0f);

            int columns = 12;
            float spacing = 3.2f;
            for (int i = 0; i < imported.Count; i++)
            {
                ImportedLooseModel model = imported[i];
                GameObject objectRoot = new GameObject(SanitizeGameObjectName(model.displayName));
                objectRoot.transform.SetParent(root.transform, false);
                objectRoot.transform.localPosition = new Vector3((i % columns) * spacing, 0f, (i / columns) * spacing);

                MeshFilter filter = objectRoot.AddComponent<MeshFilter>();
                filter.sharedMesh = model.mesh;
                MeshRenderer renderer = objectRoot.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;

                Bounds bounds = model.mesh.bounds;
                float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (maxSize > 0.001f)
                {
                    objectRoot.transform.localScale = Vector3.one * Mathf.Min(1.75f, 1.65f / maxSize);
                }

                objectRoot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            root.transform.position = new Vector3(-(columns - 1) * spacing * 0.5f, 0f, -5.5f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.64f, 0.72f, 0.82f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.46f, 0.50f, 0.46f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.18f, 0.15f, 1f);
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/PsychoSkyboxPrototype.mat");
            EditorSceneManager.SaveScene(scene, PreviewScenePath);
        }

        private static Mesh CreateOrUpdateMeshAsset(Mesh mesh, string assetPath)
        {
            mesh.name = Path.GetFileNameWithoutExtension(assetPath);
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, assetPath);
                return mesh;
            }

            EditorUtility.CopySerialized(mesh, existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static void ApplySoftNormals(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            if (vertices == null || vertices.Length == 0 || triangles == null || triangles.Length < 3)
            {
                return;
            }

            Dictionary<VertexNormalKey, Vector3> normalSums = new Dictionary<VertexNormalKey, Vector3>();
            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];
                if (!IsValidVertex(vertices, a) || !IsValidVertex(vertices, b) || !IsValidVertex(vertices, c))
                {
                    continue;
                }

                Vector3 faceNormal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                if (faceNormal.sqrMagnitude < 0.000001f)
                {
                    continue;
                }

                faceNormal.Normalize();
                AddNormal(normalSums, vertices[a], faceNormal);
                AddNormal(normalSums, vertices[b], faceNormal);
                AddNormal(normalSums, vertices[c], faceNormal);
            }

            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                VertexNormalKey key = new VertexNormalKey(vertices[i], NormalSmoothingTolerance);
                if (!normalSums.TryGetValue(key, out Vector3 normal) || normal.sqrMagnitude < 0.000001f)
                {
                    normal = Vector3.up;
                }

                normal.Normalize();
                normals[i] = normal;
            }

            mesh.normals = normals;
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
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
            Debug.Log($"Rendered loose model preview to {outputPath}");
        }

        private static void AddNormal(Dictionary<VertexNormalKey, Vector3> normalSums, Vector3 vertex, Vector3 normal)
        {
            VertexNormalKey key = new VertexNormalKey(vertex, NormalSmoothingTolerance);
            if (normalSums.TryGetValue(key, out Vector3 existing))
            {
                normalSums[key] = existing + normal;
            }
            else
            {
                normalSums.Add(key, normal);
            }
        }

        private static bool IsValidVertex(Vector3[] vertices, int index)
        {
            return index >= 0 && index < vertices.Length;
        }

        private static string GetRelativePath(string root, string path)
        {
            Uri rootUri = new Uri(AppendDirectorySeparatorChar(Path.GetFullPath(root)));
            Uri pathUri = new Uri(Path.GetFullPath(path));
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return path;
            }

            return path + Path.DirectorySeparatorChar;
        }

        private static string SanitizeFileName(string value)
        {
            StringBuilder builder = new StringBuilder(value.Length);
            foreach (char character in value.ToLowerInvariant())
            {
                if ((character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'))
                {
                    builder.Append(character);
                }
                else if (builder.Length == 0 || builder[builder.Length - 1] != '_')
                {
                    builder.Append('_');
                }
            }

            string sanitized = builder.ToString().Trim('_');
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "model";
            }

            return sanitized.Length > 64 ? sanitized.Substring(0, 64) : sanitized;
        }

        private static string SanitizeGameObjectName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Loose Model" : value.Trim();
        }

        private static void EnsureGeneratedFolders()
        {
            Directory.CreateDirectory(ToFullPath("Assets/Generated"));
            Directory.CreateDirectory(ToFullPath(GeneratedRoot));
        }

        private static void WriteReport(LooseImportResult result)
        {
            File.WriteAllText(ToFullPath(ReportPath), JsonUtility.ToJson(result, true));
            AssetDatabase.ImportAsset(ReportPath);
        }

        private static string ToFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private sealed class ImportedLooseModel
        {
            public string displayName;
            public string relativePath;
            public Mesh mesh;
        }

        [Serializable]
        private sealed class LooseImportResult
        {
            public string generatedAtUtc;
            public string sourceRoot;
            public int scanned;
            public int imported;
            public int failed;
            public LooseImportReportEntry[] entries = Array.Empty<LooseImportReportEntry>();
        }

        [Serializable]
        private sealed class LooseImportReportEntry
        {
            public string sourcePath;
            public string status;
            public string assetPath;
            public string format;
            public int vertices;
            public int faces;
            public string error;

            public static LooseImportReportEntry Imported(string sourcePath, string assetPath, string format, int vertices, int faces)
            {
                return new LooseImportReportEntry
                {
                    sourcePath = sourcePath,
                    status = "imported",
                    assetPath = assetPath,
                    format = format,
                    vertices = vertices,
                    faces = faces
                };
            }

            public static LooseImportReportEntry Failed(string sourcePath, string error)
            {
                return new LooseImportReportEntry
                {
                    sourcePath = sourcePath,
                    status = "failed",
                    error = error
                };
            }
        }

        private readonly struct VertexNormalKey : IEquatable<VertexNormalKey>
        {
            private readonly int x;
            private readonly int y;
            private readonly int z;

            public VertexNormalKey(Vector3 value, float tolerance)
            {
                float inverse = 1f / tolerance;
                x = Mathf.RoundToInt(value.x * inverse);
                y = Mathf.RoundToInt(value.y * inverse);
                z = Mathf.RoundToInt(value.z * inverse);
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
    }
}
