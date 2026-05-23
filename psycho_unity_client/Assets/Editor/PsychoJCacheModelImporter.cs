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
    public static class PsychoJCacheModelImporter
    {
        private const int MaxImportedModels = 96;
        private const int MaxScannedCandidates = 2400;
        private const int JCacheModelIdBase = 2_000_000;
        private const float NormalSmoothingTolerance = 0.00075f;
        private const float MinModelDimension = 0.08f;
        private const float MinLargestModelDimension = 0.35f;
        private const float MaxLargestModelDimension = 96f;
        private const float MaxModelAspectRatio = 36f;
        private const float MinUsedVertexRatio = 0.18f;
        private const float MinHealthyTriangleRatio = 0.36f;
        private const float MinSurfaceToBoundsAreaRatio = 0.08f;
        private const string GeneratedRoot = "Assets/Generated/JCacheModels";
        private const string ReportPath = GeneratedRoot + "/jcache_model_import_report.json";
        private const string PreviewScenePath = GeneratedRoot + "/PsychoJCacheModelPreview.unity";

        [MenuItem("Psycho/Cache/Import JS5 JCache Model Candidates")]
        public static void ImportJCacheModelPack()
        {
            JCacheImportReport report = ImportJCacheModels(true);
            Debug.Log($"Psycho JS5 jcache import complete: {report.imported} imported, {report.failed} failed, {report.skipped} skipped from {report.scanned} candidates. Report: {ReportPath}");
        }

        public static void ImportJCacheModelPackBatch()
        {
            ImportJCacheModelPack();
        }

        public static void EnsureJCacheModelAssets()
        {
            ImportJCacheModels(false);
        }

        [MenuItem("Psycho/Cache/Render JS5 JCache Model Preview")]
        public static void RenderJCacheModelPreview()
        {
            if (!File.Exists(ToFullPath(PreviewScenePath)))
            {
                ImportJCacheModels(true);
            }

            EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("JCache model preview scene does not contain a camera.");
            }

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-jcache-model-preview.png"));
            RenderCameraToPng(camera, outputPath, 1800, 1000);
        }

        public static void RenderJCacheModelPreviewBatch()
        {
            RenderJCacheModelPreview();
        }

        public static string DefaultJCacheModelExportPath
        {
            get
            {
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
                string overridePath = Environment.GetEnvironmentVariable("PSYCHO_JCACHE_MODEL_EXPORT_PATH");
                if (!string.IsNullOrWhiteSpace(overridePath))
                {
                    return Path.GetFullPath(overridePath);
                }

                return Path.Combine(projectRoot, "necrotic_client-item_attributes", "jcache_exports", "js5-8", "models");
            }
        }

        private static JCacheImportReport ImportJCacheModels(bool buildPreviewScene)
        {
            EnsureGeneratedFolders();
            string modelRoot = DefaultJCacheModelExportPath;
            JCacheImportReport report = new JCacheImportReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                sourceRoot = modelRoot
            };

            if (!Directory.Exists(modelRoot))
            {
                report.entries = new[]
                {
                    JCacheImportEntry.Failed(string.Empty, "source folder missing; run scripts/export-js5-jcache-models.py first")
                };
                WriteReport(report);
                return report;
            }

            string[] files = Directory.GetFiles(modelRoot, "*.dat", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            report.scanned = Math.Min(files.Length, MaxScannedCandidates);

            Material material = PsychoCacheMeshImporter.LoadOrCreateVertexColorMaterial();
            List<ImportedJCacheModel> imported = new List<ImportedJCacheModel>();
            List<JCacheImportEntry> entries = new List<JCacheImportEntry>();

            for (int i = 0; i < files.Length && i < MaxScannedCandidates && report.imported < MaxImportedModels; i++)
            {
                string filePath = files[i];
                string relativePath = GetRelativePath(modelRoot, filePath);
                int modelId = JCacheModelIdBase + ExtractRankOrIndex(filePath, i);

                try
                {
                    byte[] bytes = File.ReadAllBytes(filePath);
                    if (!LooksLikePlausibleModel(bytes, out string guessedFormat, out int guessedVertices, out int guessedFaces, out string skipReason))
                    {
                        report.skipped++;
                        if (entries.Count < 500)
                        {
                            entries.Add(JCacheImportEntry.Skipped(relativePath, skipReason, guessedFormat, guessedVertices, guessedFaces));
                        }

                        continue;
                    }

                    PsychoCacheModel decoded = PsychoModelDecoder.Decode(bytes, modelId);
                    if (!LooksLikePlayableDecodedModel(decoded, out skipReason))
                    {
                        report.skipped++;
                        if (entries.Count < 500)
                        {
                            entries.Add(JCacheImportEntry.Skipped(relativePath, skipReason, decoded.Format.ToString(), decoded.VertexCount, decoded.FaceCount));
                        }

                        continue;
                    }

                    Mesh mesh = PsychoModelMeshBuilder.BuildMesh(decoded);
                    if (!LooksLikePlayableMesh(mesh, out skipReason))
                    {
                        report.skipped++;
                        if (entries.Count < 500)
                        {
                            entries.Add(JCacheImportEntry.Skipped(relativePath, skipReason, decoded.Format.ToString(), decoded.VertexCount, decoded.FaceCount));
                        }

                        continue;
                    }

                    ApplySoftNormals(mesh);
                    string assetPath = $"{GeneratedRoot}/{SanitizeFileName(Path.GetFileNameWithoutExtension(filePath))}.asset";
                    Mesh assetMesh = CreateOrUpdateMeshAsset(mesh, assetPath);
                    imported.Add(new ImportedJCacheModel
                    {
                        displayName = Path.GetFileNameWithoutExtension(filePath),
                        relativePath = relativePath,
                        mesh = assetMesh,
                        vertices = decoded.VertexCount,
                        faces = decoded.FaceCount
                    });

                    report.imported++;
                    entries.Add(JCacheImportEntry.Imported(relativePath, assetPath, decoded.Format.ToString(), decoded.VertexCount, decoded.FaceCount, mesh.bounds.size));
                }
                catch (Exception ex)
                {
                    report.failed++;
                    if (entries.Count < 500)
                    {
                        entries.Add(JCacheImportEntry.Failed(relativePath, ex.Message));
                    }
                }
            }

            report.entries = entries.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            WriteReport(report);

            if (buildPreviewScene)
            {
                BuildPreviewScene(imported, material);
            }

            return report;
        }

        private static bool LooksLikePlausibleModel(byte[] bytes, out string modelFormat, out int vertices, out int faces, out string reason)
        {
            modelFormat = "unknown";
            vertices = 0;
            faces = 0;
            reason = string.Empty;

            if (bytes == null || bytes.Length < 20)
            {
                reason = "candidate file too short";
                return false;
            }

            if (bytes[bytes.Length - 1] == 0xff && bytes[bytes.Length - 2] == 0xff && bytes.Length >= 23)
            {
                modelFormat = "new";
                vertices = ReadUnsignedShort(bytes, bytes.Length - 23);
                faces = ReadUnsignedShort(bytes, bytes.Length - 21);
            }
            else
            {
                modelFormat = "old";
                vertices = ReadUnsignedShort(bytes, bytes.Length - 18);
                faces = ReadUnsignedShort(bytes, bytes.Length - 16);
            }

            if (vertices < 3 || vertices > 35000)
            {
                reason = $"implausible vertex count {vertices}";
                return false;
            }

            if (faces < 1 || faces > 70000)
            {
                reason = $"implausible face count {faces}";
                return false;
            }

            return true;
        }

        private static bool LooksLikePlayableDecodedModel(PsychoCacheModel model, out string reason)
        {
            reason = string.Empty;
            if (model.VertexCount < 3 || model.FaceCount < 1)
            {
                reason = "decoded model is empty";
                return false;
            }

            if (model.VertexCount > 35000 || model.FaceCount > 70000)
            {
                reason = $"decoded model is too large: {model.VertexCount} vertices, {model.FaceCount} faces";
                return false;
            }

            if (model.FaceCount > model.VertexCount * 8)
            {
                reason = $"decoded face/vertex ratio is suspicious: {model.FaceCount} faces for {model.VertexCount} vertices";
                return false;
            }

            return true;
        }

        private static bool LooksLikePlayableMesh(Mesh mesh, out string reason)
        {
            reason = string.Empty;
            if (mesh == null || mesh.vertexCount < 3 || mesh.triangles.Length < 3)
            {
                reason = "mesh is empty after decode";
                return false;
            }

            Bounds bounds = mesh.bounds;
            float largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            float smallest = Mathf.Min(bounds.size.x, Mathf.Min(bounds.size.y, bounds.size.z));
            if (!IsFinite(bounds.size) || !IsFinite(bounds.center))
            {
                reason = $"mesh bounds are not finite: {bounds.size}";
                return false;
            }

            if (smallest < MinModelDimension)
            {
                reason = $"mesh is too flat/thin for a replacement model: {bounds.size}";
                return false;
            }

            if (largest < MinLargestModelDimension || largest > MaxLargestModelDimension)
            {
                reason = $"mesh bounds are outside playable range: {bounds.size}";
                return false;
            }

            float aspectRatio = largest / Mathf.Max(smallest, 0.0001f);
            if (aspectRatio > MaxModelAspectRatio)
            {
                reason = $"mesh aspect ratio is suspicious ({aspectRatio:0.0}): {bounds.size}";
                return false;
            }

            return HasHealthyTriangleDistribution(mesh, bounds, out reason);
        }

        private static bool HasHealthyTriangleDistribution(Mesh mesh, Bounds bounds, out string reason)
        {
            reason = string.Empty;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            int triangleCount = triangles.Length / 3;
            if (triangleCount <= 0)
            {
                reason = "mesh has no triangles";
                return false;
            }

            bool[] usedVertices = new bool[vertices.Length];
            int usedVertexCount = 0;
            int healthyTriangles = 0;
            float surfaceArea = 0f;
            float diagonal = Mathf.Max(bounds.size.magnitude, 0.001f);
            float minimumAreaMagnitude = diagonal * diagonal * 0.00000035f;

            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];
                if (!IsValidVertex(vertices, a) || !IsValidVertex(vertices, b) || !IsValidVertex(vertices, c))
                {
                    continue;
                }

                MarkUsedVertex(usedVertices, a, ref usedVertexCount);
                MarkUsedVertex(usedVertices, b, ref usedVertexCount);
                MarkUsedVertex(usedVertices, c, ref usedVertexCount);

                Vector3 va = vertices[a];
                Vector3 vb = vertices[b];
                Vector3 vc = vertices[c];
                if (!IsFinite(va) || !IsFinite(vb) || !IsFinite(vc))
                {
                    continue;
                }

                float areaMagnitude = Vector3.Cross(vb - va, vc - va).magnitude;
                if (areaMagnitude > minimumAreaMagnitude)
                {
                    healthyTriangles++;
                    surfaceArea += areaMagnitude * 0.5f;
                }
            }

            float usedVertexRatio = vertices.Length == 0 ? 0f : usedVertexCount / (float)vertices.Length;
            if (usedVertexRatio < MinUsedVertexRatio)
            {
                reason = $"mesh uses too few decoded vertices ({usedVertexRatio:P0})";
                return false;
            }

            float healthyTriangleRatio = triangleCount == 0 ? 0f : healthyTriangles / (float)triangleCount;
            if (healthyTriangleRatio < MinHealthyTriangleRatio)
            {
                reason = $"mesh has too many degenerate triangles ({healthyTriangleRatio:P0} healthy)";
                return false;
            }

            float boundsSurfaceArea = 2f * (
                bounds.size.x * bounds.size.y
                + bounds.size.x * bounds.size.z
                + bounds.size.y * bounds.size.z);
            float surfaceToBoundsRatio = surfaceArea / Mathf.Max(boundsSurfaceArea, 0.0001f);
            if (surfaceToBoundsRatio < MinSurfaceToBoundsAreaRatio)
            {
                reason = $"mesh surface area is too sparse for its bounds ({surfaceToBoundsRatio:P0})";
                return false;
            }

            return true;
        }

        private static void BuildPreviewScene(List<ImportedJCacheModel> imported, Material material)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Psycho JS5-8 JCache Model Preview");

            GameObject keyLightObject = new GameObject("Key Light");
            Light keyLight = keyLightObject.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.24f;
            keyLightObject.transform.rotation = Quaternion.Euler(50f, -32f, 0f);

            GameObject fillLightObject = new GameObject("Fill Light");
            Light fillLight = fillLightObject.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.34f;
            fillLight.color = new Color(0.72f, 0.82f, 1f, 1f);
            fillLightObject.transform.rotation = Quaternion.Euler(24f, 130f, 0f);

            GameObject cameraObject = new GameObject("Preview Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 42f;
            camera.farClipPlane = 800f;
            camera.transform.position = new Vector3(18f, 11f, -23f);
            camera.transform.rotation = Quaternion.Euler(31f, -39f, 0f);

            int columns = 12;
            float spacing = 3.15f;
            int rows = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(1, imported.Count) / (float)columns));
            int visibleColumns = Mathf.Max(1, Mathf.Min(columns, Mathf.Max(1, imported.Count)));

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Preview Floor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = new Vector3((visibleColumns - 1) * spacing * 0.5f, -0.035f, (rows - 1) * spacing * 0.5f);
            floor.transform.localScale = new Vector3(Mathf.Max(1.4f, visibleColumns * spacing * 0.12f), 1f, Mathf.Max(1.2f, rows * spacing * 0.14f));
            MeshRenderer floorRenderer = floor.GetComponent<MeshRenderer>();
            Material floorMaterial = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.22f, 0.24f, 0.21f, 1f)
            };
            floorMaterial.SetFloat("_Glossiness", 0.08f);
            floorRenderer.sharedMaterial = floorMaterial;

            for (int i = 0; i < imported.Count; i++)
            {
                ImportedJCacheModel model = imported[i];
                GameObject objectRoot = new GameObject(SanitizeGameObjectName(model.displayName));
                objectRoot.transform.SetParent(root.transform, false);
                objectRoot.transform.localPosition = new Vector3((i % columns) * spacing, 0f, (i / columns) * spacing);
                objectRoot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                Bounds bounds = model.mesh.bounds;
                float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                float previewScale = maxSize > 0.001f ? Mathf.Min(1.75f, 1.65f / maxSize) : 1f;
                objectRoot.transform.localScale = Vector3.one * previewScale;

                GameObject meshObject = new GameObject("Mesh");
                meshObject.transform.SetParent(objectRoot.transform, false);
                meshObject.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);

                MeshFilter filter = meshObject.AddComponent<MeshFilter>();
                filter.sharedMesh = model.mesh;
                MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            root.transform.position = new Vector3(-(columns - 1) * spacing * 0.5f, 0f, -5.5f);
            AimCameraAtPreview(camera, imported.Count, columns, spacing);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.64f, 0.72f, 0.82f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.46f, 0.50f, 0.46f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.18f, 0.15f, 1f);
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/PsychoSkyboxPrototype.mat");
            EditorSceneManager.SaveScene(scene, PreviewScenePath);
        }

        private static void AimCameraAtPreview(Camera camera, int itemCount, int columns, float spacing)
        {
            int count = Math.Max(1, itemCount);
            int rows = Mathf.CeilToInt(count / (float)columns);
            int visibleColumns = Math.Min(columns, count);
            float width = Math.Max(1f, (visibleColumns - 1) * spacing);
            float depth = Math.Max(1f, (rows - 1) * spacing);
            float span = Math.Max(width, depth);
            Vector3 center = new Vector3(0f, 0.88f, -5.5f + depth * 0.5f);
            Vector3 offset = new Vector3(span * 0.35f + 4.2f, span * 0.22f + 4.8f, -(span * 0.72f + 7.5f));
            camera.transform.position = center + offset;
            camera.transform.LookAt(center);
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

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x)
                && !float.IsNaN(value.y) && !float.IsInfinity(value.y)
                && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static void MarkUsedVertex(bool[] usedVertices, int index, ref int usedVertexCount)
        {
            if (usedVertices[index])
            {
                return;
            }

            usedVertices[index] = true;
            usedVertexCount++;
        }

        private static int ReadUnsignedShort(byte[] bytes, int offset)
        {
            return (bytes[offset] << 8) | bytes[offset + 1];
        }

        private static int ExtractRankOrIndex(string path, int fallback)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            const string rankMarker = "rank_";
            int rankIndex = name.IndexOf(rankMarker, StringComparison.OrdinalIgnoreCase);
            if (rankIndex >= 0)
            {
                int start = rankIndex + rankMarker.Length;
                int end = start;
                while (end < name.Length && char.IsDigit(name[end]))
                {
                    end++;
                }

                if (end > start && int.TryParse(name.Substring(start, end - start), out int rank))
                {
                    return rank;
                }
            }

            return fallback;
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
            Debug.Log($"Rendered JS5 jcache model preview to {outputPath}");
        }

        private static string GetRelativePath(string root, string path)
        {
            Uri rootUri = new Uri(AppendDirectorySeparatorChar(Path.GetFullPath(root)));
            Uri pathUri = new Uri(Path.GetFullPath(path));
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path
                : path + Path.DirectorySeparatorChar;
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
                sanitized = "jcache_model";
            }

            return sanitized.Length > 96 ? sanitized.Substring(0, 96) : sanitized;
        }

        private static string SanitizeGameObjectName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "JCache Model" : value.Trim();
        }

        private static void EnsureGeneratedFolders()
        {
            Directory.CreateDirectory(ToFullPath("Assets/Generated"));
            Directory.CreateDirectory(ToFullPath(GeneratedRoot));
        }

        private static void WriteReport(JCacheImportReport report)
        {
            File.WriteAllText(ToFullPath(ReportPath), JsonUtility.ToJson(report, true));
            AssetDatabase.ImportAsset(ReportPath);
        }

        private static string ToFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private sealed class ImportedJCacheModel
        {
            public string displayName;
            public string relativePath;
            public Mesh mesh;
            public int vertices;
            public int faces;
        }

        [Serializable]
        private sealed class JCacheImportReport
        {
            public string generatedAtUtc;
            public string sourceRoot;
            public int scanned;
            public int imported;
            public int skipped;
            public int failed;
            public JCacheImportEntry[] entries = Array.Empty<JCacheImportEntry>();
        }

        [Serializable]
        private sealed class JCacheImportEntry
        {
            public string sourcePath;
            public string status;
            public string assetPath;
            public string format;
            public int vertices;
            public int faces;
            public Vector3 bounds;
            public string error;

            public static JCacheImportEntry Imported(string sourcePath, string assetPath, string format, int vertices, int faces, Vector3 bounds)
            {
                return new JCacheImportEntry
                {
                    sourcePath = sourcePath,
                    status = "imported",
                    assetPath = assetPath,
                    format = format,
                    vertices = vertices,
                    faces = faces,
                    bounds = bounds
                };
            }

            public static JCacheImportEntry Skipped(string sourcePath, string reason, string format, int vertices, int faces)
            {
                return new JCacheImportEntry
                {
                    sourcePath = sourcePath,
                    status = "skipped",
                    format = format,
                    vertices = vertices,
                    faces = faces,
                    error = reason
                };
            }

            public static JCacheImportEntry Failed(string sourcePath, string error)
            {
                return new JCacheImportEntry
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
                x = Mathf.RoundToInt(value.x / tolerance);
                y = Mathf.RoundToInt(value.y / tolerance);
                z = Mathf.RoundToInt(value.z / tolerance);
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
