using System.IO;
using Psycho.Rendering;
using UnityEditor;
using UnityEngine;

namespace Psycho.Editor
{
    public static class PsychoArtPipelineBootstrap
    {
        private const string ArtSourceRoot = "Assets/PsychoArtSource";
        private const string ResourceRoot = "Assets/Resources/PsychoArt";
        private const string ManifestPath = ResourceRoot + "/PsychoArtAssetManifest.asset";

        [MenuItem("Psycho/Art Pipeline/Create Default Manifest")]
        public static void CreateDefaultManifest()
        {
            EnsureFolder("Assets", "PsychoArtSource");
            EnsureFolder(ArtSourceRoot, "Characters");
            EnsureFolder(ArtSourceRoot, "NPCs");
            EnsureFolder(ArtSourceRoot, "Environment");
            EnsureFolder(ArtSourceRoot, "Foliage");
            EnsureFolder(ArtSourceRoot, "Props");
            EnsureFolder(ArtSourceRoot, "Materials");
            EnsureFolder("Assets/Resources", "PsychoArt");

            if (AssetDatabase.LoadAssetAtPath<PsychoArtAssetManifest>(ManifestPath) == null)
            {
                PsychoArtAssetManifest manifest = ScriptableObject.CreateInstance<PsychoArtAssetManifest>();
                AssetDatabase.CreateAsset(manifest, ManifestPath);
                EditorUtility.SetDirty(manifest);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Psycho art pipeline ready. Assign real prefabs in {ManifestPath}.");
        }

        public static void CreateDefaultManifestBatch()
        {
            CreateDefaultManifest();
        }

        private static void EnsureFolder(string parent, string folder)
        {
            string path = parent + "/" + folder;
            if (Directory.Exists(path))
            {
                return;
            }

            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
