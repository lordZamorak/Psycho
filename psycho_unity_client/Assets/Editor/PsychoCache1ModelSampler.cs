using System;
using System.Collections.Generic;
using System.IO;
using Psycho.Cache;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoCache1ModelSampler
    {
        private const int MaxImportedModels = 160;
        private const int MaxFilesPerIndex = 2500;
        private const float NormalSmoothingTolerance = 0.00075f;
        private const string GeneratedRoot = "Assets/Generated/Cache1Models";
        private const string ReportPath = GeneratedRoot + "/cache1_model_probe_report.json";
        private const string PreviewScenePath = GeneratedRoot + "/PsychoCache1ModelPreview.unity";

        [MenuItem("Psycho/Cache/Probe Cache1 Model Sample")]
        public static void ProbeCache1ModelSample()
        {
            Cache1ProbeReport report = ProbeCache1Models(true);
            Debug.Log($"Psycho cache1 model probe complete: {report.imported} imported from {report.scannedFiles} scanned files. Report: {ReportPath}");
        }

        public static void ProbeCache1ModelSampleBatch()
        {
            ProbeCache1ModelSample();
        }

        public static void EnsureCache1ModelSampleAssets()
        {
            ProbeCache1Models(false);
        }

        [MenuItem("Psycho/Cache/Render Cache1 Model Preview")]
        public static void RenderCache1ModelPreview()
        {
            if (!File.Exists(ToFullPath(PreviewScenePath)))
            {
                ProbeCache1Models(true);
            }

            EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("Cache1 model preview scene does not contain a camera.");
            }

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "run-logs", "unity-cache1-model-preview.png"));
            RenderCameraToPng(camera, outputPath, 1800, 1000);
        }

        public static void RenderCache1ModelPreviewBatch()
        {
            RenderCache1ModelPreview();
        }

        private static Cache1ProbeReport ProbeCache1Models(bool buildPreviewScene)
        {
            EnsureGeneratedFolders();
            string cacheRoot = PsychoCacheStore.DefaultClientCache1Path;
            Cache1ProbeReport report = new Cache1ProbeReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                sourceRoot = cacheRoot
            };

            if (!Directory.Exists(cacheRoot))
            {
                report.entries = new[] { Cache1ProbeEntry.Failed(0, 0, "cache1 folder missing") };
                WriteReport(report);
                return report;
            }

            Material material = PsychoCacheMeshImporter.LoadOrCreateVertexColorMaterial();
            List<ImportedCache1Model> imported = new List<ImportedCache1Model>();
            List<Cache1ProbeEntry> entries = new List<Cache1ProbeEntry>();
            int[] cacheIndexes = BuildCacheIndexOrder(cacheRoot);

            using (PsychoCacheStore store = new PsychoCacheStore(cacheRoot))
            {
                for (int indexOrder = 0; indexOrder < cacheIndexes.Length && report.imported < MaxImportedModels; indexOrder++)
                {
                    int cacheIndex = cacheIndexes[indexOrder];
                    int fileCount;
                    try
                    {
                        fileCount = store.GetFileCount(cacheIndex);
                    }
                    catch (Exception ex)
                    {
                        entries.Add(Cache1ProbeEntry.Failed(cacheIndex, 0, ex.Message));
                        continue;
                    }

                    int maxFileId = Math.Min(fileCount, MaxFilesPerIndex);
                    for (int fileId = 0; fileId < maxFileId && report.imported < MaxImportedModels; fileId++)
                    {
                        report.scannedFiles++;
                        byte[] bytes = store.ReadContainerFile(cacheIndex, fileId);
                        if (bytes == null)
                        {
                            continue;
                        }

                        try
                        {
                            int modelId = cacheIndex * 1_000_000 + fileId;
                            PsychoCacheModel decoded = PsychoModelDecoder.Decode(bytes, modelId);
                            if (decoded.VertexCount <= 0 || decoded.FaceCount <= 0)
                            {
                                continue;
                            }

                            Mesh mesh = PsychoModelMeshBuilder.BuildMesh(decoded);
                            ApplySoftNormals(mesh);
                            string assetPath = $"{GeneratedRoot}/cache1_idx{cacheIndex:D2}_file{fileId:D5}.asset";
                            Mesh assetMesh = CreateOrUpdateMeshAsset(mesh, assetPath);
                            imported.Add(new ImportedCache1Model
                            {
                                cacheIndex = cacheIndex,
                                fileId = fileId,
                                mesh = assetMesh
                            });

                            report.imported++;
                            entries.Add(Cache1ProbeEntry.Imported(cacheIndex, fileId, assetPath, decoded.Format.ToString(), decoded.VertexCount, decoded.FaceCount));
                        }
                        catch (Exception ex)
                        {
                            if (entries.Count < 400)
                            {
                                entries.Add(Cache1ProbeEntry.Failed(cacheIndex, fileId, ex.Message));
                            }
                        }
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

        private static int[] BuildCacheIndexOrder(string cacheRoot)
        {
            HashSet<int> seen = new HashSet<int>();
            List<int> indexes = new List<int>();
            AddIndexIfPresent(indexes, seen, cacheRoot, 7);
            AddIndexIfPresent(indexes, seen, cacheRoot, 1);
            AddIndexIfPresent(indexes, seen, cacheRoot, 2);
            AddIndexIfPresent(indexes, seen, cacheRoot, 0);

            string[] files = Directory.GetFiles(cacheRoot, "main_file_cache.idx*", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string idText = name.Substring("main_file_cache.idx".Length);
                if (int.TryParse(idText, out int cacheIndex))
                {
                    AddIndexIfPresent(indexes, seen, cacheRoot, cacheIndex);
                }
            }

            return indexes.ToArray();
        }

        private static void AddIndexIfPresent(List<int> indexes, HashSet<int> seen, string cacheRoot, int cacheIndex)
        {
            if (cacheIndex < 0 || !seen.Add(cacheIndex))
            {
                return;
            }

            if (File.Exists(Path.Combine(cacheRoot, $"main_file_cache.idx{cacheIndex}")))
            {
                indexes.Add(cacheIndex);
            }
        }

        private static void BuildPreviewScene(List<ImportedCache1Model> imported, Material material)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Psycho Cache1 Model Preview");

            GameObject lightObject = new GameObject("Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.18f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -30f, 0f);

            GameObject cameraObject = new GameObject("Preview Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 45f;
            camera.farClipPlane = 500f;
            camera.transform.position = new Vector3(18f, 10f, -21f);
            camera.transform.rotation = Quaternion.Euler(31f, -39f, 0f);

            int columns = 16;
            float spacing = 2.7f;
            for (int i = 0; i < imported.Count; i++)
            {
                ImportedCache1Model model = imported[i];
                GameObject objectRoot = new GameObject($"idx{model.cacheIndex}_file{model.fileId}");
                objectRoot.transform.SetParent(root.transform, false);
                objectRoot.transform.localPosition = new Vector3((i % columns) * spacing, 0f, (i / columns) * spacing);
                objectRoot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                MeshFilter filter = objectRoot.AddComponent<MeshFilter>();
                filter.sharedMesh = model.mesh;
                MeshRenderer renderer = objectRoot.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;

                Bounds bounds = model.mesh.bounds;
                float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (maxSize > 0.001f)
                {
                    objectRoot.transform.localScale = Vector3.one * Mathf.Min(1.55f, 1.45f / maxSize);
                }
            }

            root.transform.position = new Vector3(-(columns - 1) * spacing * 0.5f, 0f, -4.5f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.70f, 0.80f, 1f);
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
            Debug.Log($"Rendered cache1 model preview to {outputPath}");
        }

        private static void EnsureGeneratedFolders()
        {
            Directory.CreateDirectory(ToFullPath("Assets/Generated"));
            Directory.CreateDirectory(ToFullPath(GeneratedRoot));
        }

        private static void WriteReport(Cache1ProbeReport report)
        {
            File.WriteAllText(ToFullPath(ReportPath), JsonUtility.ToJson(report, true));
            AssetDatabase.ImportAsset(ReportPath);
        }

        private static string ToFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private sealed class ImportedCache1Model
        {
            public int cacheIndex;
            public int fileId;
            public Mesh mesh;
        }

        [Serializable]
        private sealed class Cache1ProbeReport
        {
            public string generatedAtUtc;
            public string sourceRoot;
            public int scannedFiles;
            public int imported;
            public Cache1ProbeEntry[] entries = Array.Empty<Cache1ProbeEntry>();
        }

        [Serializable]
        private sealed class Cache1ProbeEntry
        {
            public int cacheIndex;
            public int fileId;
            public string status;
            public string assetPath;
            public string format;
            public int vertices;
            public int faces;
            public string error;

            public static Cache1ProbeEntry Imported(int cacheIndex, int fileId, string assetPath, string format, int vertices, int faces)
            {
                return new Cache1ProbeEntry
                {
                    cacheIndex = cacheIndex,
                    fileId = fileId,
                    status = "imported",
                    assetPath = assetPath,
                    format = format,
                    vertices = vertices,
                    faces = faces
                };
            }

            public static Cache1ProbeEntry Failed(int cacheIndex, int fileId, string error)
            {
                return new Cache1ProbeEntry
                {
                    cacheIndex = cacheIndex,
                    fileId = fileId,
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
