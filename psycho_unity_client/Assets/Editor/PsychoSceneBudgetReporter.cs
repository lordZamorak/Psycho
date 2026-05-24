using System;
using System.Collections.Generic;
using System.IO;
using Psycho.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoSceneBudgetReporter
    {
        private const string HostedScenePath = "Assets/Scenes/PsychoHostedTestWorld.unity";
        private const string ReportPath = "Assets/Generated/Hosted/hosted_scene_budget_report.json";

        [MenuItem("Psycho/Profiling/Write Hosted Scene Budget Report")]
        public static void WriteHostedSceneBudgetReport()
        {
            EditorSceneManager.OpenScene(HostedScenePath, OpenSceneMode.Single);
            BudgetReport report = BuildReport();
            string json = JsonUtility.ToJson(report, true);
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ReportPath));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, json);
            AssetDatabase.Refresh();
            Debug.Log($"Hosted scene budget report written to {ReportPath}: {report.renderers} renderers, {report.triangles} triangles, {report.lodGroups} LOD groups.");
        }

        public static void WriteHostedSceneBudgetReportBatch()
        {
            WriteHostedSceneBudgetReport();
        }

        private static BudgetReport BuildReport()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            HashSet<Mesh> uniqueMeshes = new HashSet<Mesh>();
            HashSet<Material> uniqueMaterials = new HashSet<Material>();
            BudgetReport report = new BudgetReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                scenePath = scene.path,
                rootObjects = roots.Length
            };

            foreach (GameObject root in roots)
            {
                CountTransformTree(root.transform, report);
            }

            foreach (MeshFilter meshFilter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                CountMesh(meshFilter.sharedMesh, uniqueMeshes, report);
            }

            foreach (SkinnedMeshRenderer skinned in UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                CountMesh(skinned.sharedMesh, uniqueMeshes, report);
            }

            foreach (Renderer renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                report.renderers++;
                if (HasAncestorName(renderer.transform, "Art Replacement")
                    || HasAncestorName(renderer.transform, "Psycho Hero Art Prefab"))
                {
                    report.artReplacementRenderers++;
                }

                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        uniqueMaterials.Add(materials[i]);
                    }
                }
            }

            report.uniqueMeshes = uniqueMeshes.Count;
            report.uniqueMaterials = uniqueMaterials.Count;
            report.lodGroups = UnityEngine.Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            report.colliders = UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            report.windAnimatedFoliage = UnityEngine.Object.FindObjectsByType<WindAnimatedFoliage>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            report.lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            report.cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            return report;
        }

        private static void CountTransformTree(Transform transform, BudgetReport report)
        {
            report.gameObjects++;
            if (transform.gameObject.isStatic)
            {
                report.staticGameObjects++;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                CountTransformTree(transform.GetChild(i), report);
            }
        }

        private static void CountMesh(Mesh mesh, HashSet<Mesh> uniqueMeshes, BudgetReport report)
        {
            if (mesh == null || !uniqueMeshes.Add(mesh))
            {
                return;
            }

            report.vertices += mesh.vertexCount;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                report.triangles += mesh.GetIndexCount(i) / 3;
            }
        }

        private static bool HasAncestorName(Transform transform, string fragment)
        {
            while (transform != null)
            {
                if (transform.name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                transform = transform.parent;
            }

            return false;
        }

        [Serializable]
        private sealed class BudgetReport
        {
            public string generatedAtUtc;
            public string scenePath;
            public int rootObjects;
            public int gameObjects;
            public int staticGameObjects;
            public int renderers;
            public int artReplacementRenderers;
            public int uniqueMeshes;
            public int uniqueMaterials;
            public long vertices;
            public long triangles;
            public int lodGroups;
            public int colliders;
            public int windAnimatedFoliage;
            public int lights;
            public int cameras;
        }
    }
}
