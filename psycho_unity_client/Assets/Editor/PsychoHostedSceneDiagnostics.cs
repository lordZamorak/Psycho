using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Psycho.Editor
{
    public static class PsychoHostedSceneDiagnostics
    {
        private const string HostedScenePath = "Assets/Scenes/PsychoHostedTestWorld.unity";
        private const string ReportPath = "Assets/Generated/Hosted/hosted_player_grounding_report.json";

        [MenuItem("Psycho/Diagnostics/Write Hosted Player Grounding Report")]
        public static void WriteHostedPlayerGroundingReport()
        {
            EditorSceneManager.OpenScene(HostedScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Playable Adventurer");
            GroundingReport report = new GroundingReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                scenePath = SceneManager.GetActiveScene().path,
                foundPlayer = player != null
            };

            if (player != null)
            {
                report.playerPosition = player.transform.position;
                report.playerChildren = player.transform.childCount;
                report.rendererBounds = BoundsInfo.FromTryGet(TryGetRendererBounds(player.transform, out Bounds rendererBounds), rendererBounds);
                report.colliderBounds = BoundsInfo.FromTryGet(TryGetColliderBounds(player.transform, out Bounds colliderBounds), colliderBounds);
                report.terrainHit = TryGetTerrainHit(player.transform.position + Vector3.up * 8f, out RaycastHit hit);
                if (report.terrainHit)
                {
                    report.terrainPoint = hit.point;
                    report.terrainCollider = hit.collider == null ? string.Empty : hit.collider.name;
                    report.playerRootFeetDelta = player.transform.position.y - hit.point.y;
                    report.rendererMinDelta = report.rendererBounds.min.y - hit.point.y;
                    report.colliderMinDelta = report.colliderBounds.min.y - hit.point.y;
                }
            }

            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ReportPath));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, JsonUtility.ToJson(report, true));
            AssetDatabase.Refresh();
            Debug.Log($"Hosted player grounding report written to {ReportPath}.");
        }

        public static void WriteHostedPlayerGroundingReportBatch()
        {
            WriteHostedPlayerGroundingReport();
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

        private static bool TryGetColliderBounds(Transform root, out Bounds bounds)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            bounds = new Bounds(root.position, Vector3.zero);
            bool found = false;
            foreach (Collider collider in colliders)
            {
                if (collider == null || collider.isTrigger)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = collider.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }

            return found;
        }

        private static bool TryGetTerrainHit(Vector3 origin, out RaycastHit bestHit)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 24f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            bestHit = default;
            float bestDistance = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || !hit.collider.name.Contains("Terrain"))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestHit = hit;
                }
            }

            return bestDistance < float.MaxValue;
        }

        [Serializable]
        private sealed class GroundingReport
        {
            public string generatedAtUtc;
            public string scenePath;
            public bool foundPlayer;
            public Vector3 playerPosition;
            public int playerChildren;
            public BoundsInfo rendererBounds;
            public BoundsInfo colliderBounds;
            public bool terrainHit;
            public Vector3 terrainPoint;
            public string terrainCollider;
            public float playerRootFeetDelta;
            public float rendererMinDelta;
            public float colliderMinDelta;
        }

        [Serializable]
        private struct BoundsInfo
        {
            public bool found;
            public Vector3 min;
            public Vector3 max;
            public Vector3 center;
            public Vector3 size;

            public static BoundsInfo FromTryGet(bool found, Bounds bounds)
            {
                return new BoundsInfo
                {
                    found = found,
                    min = bounds.min,
                    max = bounds.max,
                    center = bounds.center,
                    size = bounds.size
                };
            }
        }
    }
}
