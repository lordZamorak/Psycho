using System.IO;
using UnityEditor;
using UnityEngine;

namespace Psycho.Editor
{
    public static class PsychoHighResolutionMaterialAuthoring
    {
        private const int TextureSize = 2048;
        private const string ResourceDirectory = "Assets/Resources/PsychoMaterials";

        [MenuItem("Psycho/Generate 2K Visual Materials")]
        public static void Generate2KMaterials()
        {
            Directory.CreateDirectory(ResourceDirectory);

            MaterialSpec[] specs =
            {
                new MaterialSpec("Default", new Color32(116, 111, 96, 255), new Color32(166, 158, 132, 255), Pattern.Stone, 0.22f, 0f, false),
                new MaterialSpec("Grass", new Color32(44, 112, 38, 255), new Color32(118, 166, 64, 255), Pattern.Fibers, 0.18f, 0f, false),
                new MaterialSpec("Leaf", new Color32(38, 104, 38, 255), new Color32(92, 156, 60, 255), Pattern.Fibers, 0.22f, 0f, false),
                new MaterialSpec("Wood", new Color32(92, 54, 25, 255), new Color32(158, 104, 48, 255), Pattern.Wood, 0.20f, 0f, false),
                new MaterialSpec("Stone", new Color32(88, 90, 84, 255), new Color32(146, 145, 134, 255), Pattern.Stone, 0.28f, 0f, false),
                new MaterialSpec("Water", new Color32(20, 95, 140, 220), new Color32(76, 168, 210, 220), Pattern.Waves, 0.82f, 0f, true),
                new MaterialSpec("Metal", new Color32(122, 124, 124, 255), new Color32(210, 206, 190, 255), Pattern.Metal, 0.64f, 0.15f, false),
                new MaterialSpec("Coin", new Color32(196, 129, 32, 255), new Color32(255, 210, 82, 255), Pattern.Metal, 0.58f, 0.25f, false),
                new MaterialSpec("Rune", new Color32(52, 96, 176, 255), new Color32(116, 196, 255, 255), Pattern.Crystal, 0.46f, 0f, false),
                new MaterialSpec("Cloth", new Color32(118, 46, 42, 255), new Color32(176, 93, 78, 255), Pattern.Fibers, 0.34f, 0f, false),
                new MaterialSpec("Leather", new Color32(92, 48, 24, 255), new Color32(150, 92, 46, 255), Pattern.Leather, 0.30f, 0f, false),
                new MaterialSpec("Crystal", new Color32(52, 172, 192, 230), new Color32(184, 250, 255, 230), Pattern.Crystal, 0.72f, 0f, true),
                new MaterialSpec("Organic", new Color32(104, 126, 48, 255), new Color32(180, 160, 72, 255), Pattern.Organic, 0.24f, 0f, false),
                new MaterialSpec("Paper", new Color32(176, 160, 112, 255), new Color32(226, 214, 164, 255), Pattern.Fibers, 0.18f, 0f, false),
                new MaterialSpec("Mountain", new Color32(92, 96, 96, 255), new Color32(156, 166, 166, 255), Pattern.Stone, 0.40f, 0f, false),
                new MaterialSpec("Cloud", new Color32(205, 218, 226, 200), new Color32(255, 255, 255, 200), Pattern.Cloud, 0.70f, 0f, true)
            };

            foreach (MaterialSpec spec in specs)
            {
                WriteTexture(AlbedoPath(spec.Key), spec.Dark, spec.Light, spec.Pattern, false);
                WriteTexture(NormalPath(spec.Key), new Color32(128, 128, 255, 255), new Color32(146, 146, 255, 255), spec.Pattern, true);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (MaterialSpec spec in specs)
            {
                ConfigureTexture(AlbedoPath(spec.Key), false);
                ConfigureTexture(NormalPath(spec.Key), true);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (MaterialSpec spec in specs)
            {
                CreateMaterialAsset(spec);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated Psycho 2K material library in {ResourceDirectory}");
        }

        private static void CreateMaterialAsset(MaterialSpec spec)
        {
            string materialPath = $"{ResourceDirectory}/Psycho_{spec.Key}_2K.mat";

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.shader = Shader.Find("Standard");
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath(spec.Key)));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath(spec.Key)));
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Glossiness", spec.Smoothness);
            material.SetFloat("_Metallic", spec.Metallic);
            material.enableInstancing = true;

            if (spec.Transparent)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }

        private static void WriteTexture(string path, Color32 dark, Color32 light, Pattern pattern, bool normal)
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, true, normal);
            Color32[] pixels = new Color32[TextureSize * TextureSize];

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = (float)x / TextureSize;
                    float v = (float)y / TextureSize;
                    float n = PatternValue(u, v, pattern);
                    pixels[y * TextureSize + x] = normal ? NormalColor(u, v, pattern, n) : Lerp(dark, light, n);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void ConfigureTexture(string path, bool normal)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.maxTextureSize = TextureSize;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 8;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            AssetDatabase.WriteImportSettingsIfDirty(path);
        }

        private static string AlbedoPath(string key) => $"{ResourceDirectory}/Psycho_{key}_Albedo_2K.png";

        private static string NormalPath(string key) => $"{ResourceDirectory}/Psycho_{key}_Normal_2K.png";

        private static Color32 NormalColor(float u, float v, Pattern pattern, float center)
        {
            float right = PatternValue(u + 1f / TextureSize, v, pattern);
            float up = PatternValue(u, v + 1f / TextureSize, pattern);
            float dx = (center - right) * 9f;
            float dy = (center - up) * 9f;
            Vector3 normal = new Vector3(dx, dy, 1f).normalized;
            return new Color32((byte)((normal.x * 0.5f + 0.5f) * 255f), (byte)((normal.y * 0.5f + 0.5f) * 255f), (byte)((normal.z * 0.5f + 0.5f) * 255f), 255);
        }

        private static float PatternValue(float u, float v, Pattern pattern)
        {
            float baseNoise = FractalNoise(u * 16f, v * 16f);
            switch (pattern)
            {
                case Pattern.Wood:
                    return Mathf.Clamp01(baseNoise * 0.45f + Mathf.Abs(Mathf.Sin((u * 18f + baseNoise * 4f) * Mathf.PI)) * 0.45f);
                case Pattern.Fibers:
                    return Mathf.Clamp01(baseNoise * 0.55f + Mathf.Abs(Mathf.Sin((v * 60f + u * 8f) * Mathf.PI)) * 0.25f);
                case Pattern.Waves:
                    return Mathf.Clamp01(baseNoise * 0.35f + Mathf.Sin((u * 9f + v * 5f + baseNoise) * Mathf.PI * 2f) * 0.22f + 0.52f);
                case Pattern.Metal:
                    return Mathf.Clamp01(baseNoise * 0.34f + Mathf.Pow(Mathf.Abs(Mathf.Sin((u + v) * 24f)), 8f) * 0.55f);
                case Pattern.Crystal:
                    return Mathf.Clamp01(baseNoise * 0.40f + Mathf.Pow(Mathf.Abs(Mathf.Sin((u * 7f - v * 9f) * Mathf.PI)), 5f) * 0.52f);
                case Pattern.Leather:
                    return Mathf.Clamp01(baseNoise * 0.75f + FractalNoise(u * 48f, v * 48f) * 0.20f);
                case Pattern.Organic:
                    return Mathf.Clamp01(baseNoise * 0.62f + Mathf.Sin((u * 13f + v * 11f) * Mathf.PI) * 0.16f + 0.18f);
                case Pattern.Cloud:
                    return Mathf.Clamp01(Mathf.Pow(baseNoise, 0.62f));
                default:
                    return Mathf.Clamp01(baseNoise * 0.72f + FractalNoise(u * 42f, v * 42f) * 0.18f);
            }
        }

        private static float FractalNoise(float x, float y)
        {
            float value = 0f;
            float amplitude = 0.5f;
            float frequency = 1f;
            for (int i = 0; i < 5; i++)
            {
                value += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
                frequency *= 2.03f;
                amplitude *= 0.5f;
            }

            return Mathf.Clamp01(value);
        }

        private static Color32 Lerp(Color32 a, Color32 b, float t)
        {
            return new Color32(
                (byte)Mathf.Lerp(a.r, b.r, t),
                (byte)Mathf.Lerp(a.g, b.g, t),
                (byte)Mathf.Lerp(a.b, b.b, t),
                (byte)Mathf.Lerp(a.a, b.a, t));
        }

        private enum Pattern
        {
            Stone,
            Fibers,
            Wood,
            Waves,
            Metal,
            Crystal,
            Leather,
            Organic,
            Cloud
        }

        private readonly struct MaterialSpec
        {
            public MaterialSpec(string key, Color32 dark, Color32 light, Pattern pattern, float smoothness, float metallic, bool transparent)
            {
                Key = key;
                Dark = dark;
                Light = light;
                Pattern = pattern;
                Smoothness = smoothness;
                Metallic = metallic;
                Transparent = transparent;
            }

            public readonly string Key;
            public readonly Color32 Dark;
            public readonly Color32 Light;
            public readonly Pattern Pattern;
            public readonly float Smoothness;
            public readonly float Metallic;
            public readonly bool Transparent;
        }
    }
}
