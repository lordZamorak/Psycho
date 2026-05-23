using Psycho.Rendering;
using UnityEngine;

namespace Psycho.Mirror
{
    public sealed class PsychoVisualFactory : MonoBehaviour
    {
        private const float DogSizedImpFallbackScale = 0.40f;

        [SerializeField] private bool animateFoliage = true;
        [SerializeField] private bool animateNpcPreviews;

        public GameObject CreateItemVisual(PsychoMirrorItem item)
        {
            GameObject root = new GameObject($"Item {item.id} - {item.name}");
            float scale = Mathf.Max(0.45f, item.scale <= 0f ? 1f : item.scale);
            Material material = ResolveMaterial(item.materialClass);

            switch (item.visualClass)
            {
                case "Currency":
                    AddPrimitive(root, PrimitiveType.Cylinder, "Coin", new Vector3(0f, 0.08f, 0f), new Vector3(0.46f, 0.08f, 0.46f) * scale, material);
                    break;
                case "Rune":
                    AddPrimitive(root, PrimitiveType.Cylinder, "Rune Stone", new Vector3(0f, 0.08f, 0f), new Vector3(0.42f, 0.10f, 0.42f) * scale, material);
                    AddPrimitive(root, PrimitiveType.Cube, "Rune Mark", new Vector3(0f, 0.18f, 0f), new Vector3(0.08f, 0.03f, 0.48f) * scale, ResolveMaterial("Crystal"));
                    break;
                case "Weapon":
                case "TwoHandedWeapon":
                    AddPrimitive(root, PrimitiveType.Cylinder, "Grip", new Vector3(0f, 0.42f, 0f), new Vector3(0.08f, 0.42f, 0.08f) * scale, ResolveMaterial("Leather"));
                    AddPrimitive(root, PrimitiveType.Cube, "Blade", new Vector3(0f, 0.98f, 0f), new Vector3(0.16f, item.visualClass == "TwoHandedWeapon" ? 1.10f : 0.72f, 0.05f) * scale, material);
                    break;
                case "Shield":
                    AddPrimitive(root, PrimitiveType.Cylinder, "Shield Face", new Vector3(0f, 0.55f, 0f), new Vector3(0.52f, 0.10f, 0.52f) * scale, material).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    break;
                case "Armor":
                    AddPrimitive(root, PrimitiveType.Capsule, "Armor Form", new Vector3(0f, 0.62f, 0f), new Vector3(0.44f, 0.62f, 0.30f) * scale, material);
                    break;
                case "Potion":
                    AddPrimitive(root, PrimitiveType.Cylinder, "Bottle", new Vector3(0f, 0.30f, 0f), new Vector3(0.24f, 0.34f, 0.24f) * scale, material);
                    AddPrimitive(root, PrimitiveType.Cylinder, "Cork", new Vector3(0f, 0.70f, 0f), new Vector3(0.12f, 0.10f, 0.12f) * scale, ResolveMaterial("Wood"));
                    break;
                case "Food":
                case "Gem":
                case "Resource":
                    AddPrimitive(root, PrimitiveType.Sphere, item.visualClass, new Vector3(0f, 0.28f, 0f), new Vector3(0.42f, 0.34f, 0.42f) * scale, material);
                    break;
                case "Scroll":
                case "Note":
                    AddPrimitive(root, PrimitiveType.Cube, "Folded Paper", new Vector3(0f, 0.05f, 0f), new Vector3(0.54f, 0.04f, 0.34f) * scale, material);
                    break;
                case "Tool":
                    AddPrimitive(root, PrimitiveType.Cylinder, "Handle", new Vector3(0f, 0.42f, 0f), new Vector3(0.07f, 0.48f, 0.07f) * scale, ResolveMaterial("Wood"));
                    AddPrimitive(root, PrimitiveType.Cube, "Tool Head", new Vector3(0.18f, 0.82f, 0f), new Vector3(0.34f, 0.18f, 0.10f) * scale, material);
                    break;
                default:
                    AddPrimitive(root, PrimitiveType.Cube, "Item Form", new Vector3(0f, 0.24f, 0f), Vector3.one * 0.42f * scale, material);
                    break;
            }

            return root;
        }

        public GameObject CreateNpcVisual(PsychoMirrorNpc npc)
        {
            GameObject root = new GameObject($"NPC {npc.id} - {npc.name}");
            float size = ShouldUseDogSizedImpScale(npc) ? DogSizedImpFallbackScale : Mathf.Max(0.85f, npc.scale <= 0f ? 1f : npc.scale);
            Material body = CreateNpcClothingMaterial(npc, false);
            Material trim = CreateNpcClothingMaterial(npc, true);
            Material leather = CreateRuntimeMaterial("NPC Leather", new Color(0.30f, 0.18f, 0.10f), 0.26f, "Leather");
            Material skin = CreateRuntimeMaterial("NPC Skin", new Color(0.78f, 0.58f, 0.42f), 0.22f);
            Material hair = CreateRuntimeMaterial("NPC Hair", new Color(0.17f, 0.11f, 0.06f), 0.25f, "Leather");

            AddPrimitive(root, PrimitiveType.Capsule, "Clothed Ribcage Volume", new Vector3(0f, 0.97f * size, 0.035f * size), new Vector3(0.225f, 0.380f, 0.165f) * size, body);
            AddTaperedPrism(root, "Layered Humanoid Torso Front", new Vector3(0f, 0.94f * size, -0.140f * size), 0.44f * size, 0.34f * size, 0.58f * size, 0.042f * size, body);
            AddTaperedPrism(root, "Layered Humanoid Coat Back", new Vector3(0f, 0.95f * size, 0.155f * size), 0.34f * size, 0.26f * size, 0.44f * size, 0.036f * size, body);
            AddPrimitive(root, PrimitiveType.Cube, "Waist Belt", new Vector3(0f, 0.61f * size, -0.01f * size), new Vector3(0.44f, 0.07f, 0.27f) * size, leather);
            AddPrimitive(root, PrimitiveType.Cube, "Belt Buckle", new Vector3(0f, 0.61f * size, -0.175f * size), new Vector3(0.075f, 0.076f, 0.028f) * size, trim);
            AddPrimitive(root, PrimitiveType.Cube, "Chest Strap", new Vector3(-0.07f * size, 0.96f * size, -0.15f * size), new Vector3(0.045f, 0.50f, 0.025f) * size, leather).transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
            AddPrimitive(root, PrimitiveType.Cube, "Counter Chest Strap", new Vector3(0.09f * size, 0.94f * size, -0.152f * size), new Vector3(0.036f, 0.44f, 0.023f) * size, leather).transform.localRotation = Quaternion.Euler(0f, 0f, 19f);
            AddPrimitive(root, PrimitiveType.Cube, "Collar Trim", new Vector3(0f, 1.25f * size, -0.04f * size), new Vector3(0.36f, 0.055f, 0.20f) * size, trim);
            AddPrimitive(root, PrimitiveType.Sphere, "Neck", new Vector3(0f, 1.295f * size, 0f), new Vector3(0.095f, 0.085f, 0.085f) * size, skin);
            AddPrimitive(root, PrimitiveType.Sphere, "Head", new Vector3(0f, 1.44f * size, 0f), new Vector3(0.23f, 0.27f, 0.21f) * size, skin);
            AddPrimitive(root, PrimitiveType.Sphere, "Hair Cap", new Vector3(0f, 1.58f * size, 0.015f * size), new Vector3(0.22f, 0.09f, 0.20f) * size, hair);
            AddPrimitive(root, PrimitiveType.Cube, "Brow", new Vector3(0f, 1.48f * size, -0.20f * size), new Vector3(0.14f, 0.018f, 0.014f) * size, hair);
            AddPrimitive(root, PrimitiveType.Cube, "Nose", new Vector3(0f, 1.42f * size, -0.21f * size), new Vector3(0.030f, 0.055f, 0.046f) * size, skin);
            AddPrimitive(root, PrimitiveType.Sphere, "Left Eye Shadow", new Vector3(-0.062f * size, 1.47f * size, -0.205f * size), new Vector3(0.020f, 0.012f, 0.010f) * size, hair);
            AddPrimitive(root, PrimitiveType.Sphere, "Right Eye Shadow", new Vector3(0.062f * size, 1.47f * size, -0.205f * size), new Vector3(0.020f, 0.012f, 0.010f) * size, hair);
            AddPrimitive(root, PrimitiveType.Cube, "Tunic Split Front", new Vector3(0f, 0.48f * size, -0.115f * size), new Vector3(0.055f, 0.235f, 0.032f) * size, trim);
            AddPrimitive(root, PrimitiveType.Cube, "Left Tunic Hem", new Vector3(-0.135f * size, 0.50f * size, -0.095f * size), new Vector3(0.145f, 0.205f, 0.038f) * size, body).transform.localRotation = Quaternion.Euler(0f, 0f, 4f);
            AddPrimitive(root, PrimitiveType.Cube, "Right Tunic Hem", new Vector3(0.135f * size, 0.50f * size, -0.095f * size), new Vector3(0.145f, 0.205f, 0.038f) * size, body).transform.localRotation = Quaternion.Euler(0f, 0f, -4f);
            AddPrimitive(root, PrimitiveType.Sphere, "Left Shoulder Pad", new Vector3(-0.31f * size, 1.20f * size, -0.01f * size), new Vector3(0.125f, 0.070f, 0.115f) * size, trim);
            AddPrimitive(root, PrimitiveType.Sphere, "Right Shoulder Pad", new Vector3(0.31f * size, 1.20f * size, -0.01f * size), new Vector3(0.125f, 0.070f, 0.115f) * size, trim);
            AddNpcLimb(root, true, size, body, trim, skin);
            AddNpcLimb(root, false, size, body, trim, skin);
            AddNpcLeg(root, true, size, trim, leather);
            AddNpcLeg(root, false, size, trim, leather);

            if (animateNpcPreviews)
            {
                PrototypeNpcWander wander = root.AddComponent<PrototypeNpcWander>();
                SetPrivateFloat(wander, "wanderRadius", Mathf.Lerp(2.4f, 7.5f, Mathf.Clamp01(npc.combat / 350f)));
                SetPrivateFloat(wander, "speed", npc.attackable ? 2.6f : 1.7f);
            }

            return root;
        }

        private static bool ShouldUseDogSizedImpScale(PsychoMirrorNpc npc)
        {
            if (npc == null || string.IsNullOrWhiteSpace(npc.name))
            {
                return false;
            }

            string name = npc.name.Trim().ToLowerInvariant();
            if (name.Contains("impling") || name.Contains("chimp") || name.Contains("snow imp"))
            {
                return false;
            }

            return name == "imp"
                || name == "imp champion"
                || name == "imp defender"
                || name == "booth imp"
                || name == "reanimated imp"
                || name == "revenant imp";
        }

        private static Material CreateNpcClothingMaterial(PsychoMirrorNpc npc, bool trim)
        {
            string visualClass = npc?.visualClass == null ? string.Empty : npc.visualClass.ToLowerInvariant();
            int seed = npc == null ? 0 : npc.id * 37 + (trim ? 19 : 7);
            float tint = Hash01(seed);
            Color color;
            if (visualClass == "banker")
            {
                color = trim ? new Color(0.22f, 0.20f, 0.14f) : Color.Lerp(new Color(0.18f, 0.25f, 0.34f), new Color(0.30f, 0.31f, 0.25f), tint);
            }
            else if (visualClass == "merchant")
            {
                color = trim ? new Color(0.26f, 0.17f, 0.10f) : Color.Lerp(new Color(0.28f, 0.30f, 0.20f), new Color(0.37f, 0.25f, 0.17f), tint);
            }
            else if (visualClass == "guard" || (npc != null && npc.attackable))
            {
                color = trim ? new Color(0.38f, 0.38f, 0.35f) : Color.Lerp(new Color(0.25f, 0.30f, 0.24f), new Color(0.30f, 0.23f, 0.18f), tint);
            }
            else
            {
                color = trim ? new Color(0.22f, 0.16f, 0.10f) : Color.Lerp(new Color(0.28f, 0.34f, 0.24f), new Color(0.34f, 0.29f, 0.22f), tint);
            }

            return CreateRuntimeMaterial(trim ? "NPC Trim" : "NPC Tunic", color, trim ? 0.30f : 0.22f, trim ? "Leather" : "Cloth");
        }

        private static Material CreateRuntimeMaterial(string name, Color color, float smoothness, string textureClass = null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.name = name;
            material.color = string.IsNullOrWhiteSpace(textureClass)
                ? color
                : new Color(
                    Mathf.Clamp01(color.r * 1.18f + 0.06f),
                    Mathf.Clamp01(color.g * 1.18f + 0.06f),
                    Mathf.Clamp01(color.b * 1.18f + 0.06f),
                    color.a);
            material.enableInstancing = true;
            if (!string.IsNullOrWhiteSpace(textureClass))
            {
                Texture2D albedo = Resources.Load<Texture2D>($"PsychoMaterials/Psycho_{textureClass}_Albedo_2K");
                Texture2D normal = Resources.Load<Texture2D>($"PsychoMaterials/Psycho_{textureClass}_Normal_2K");
                if (albedo != null && material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", albedo);
                    material.SetTextureScale("_MainTex", new Vector2(0.62f, 0.62f));
                }

                if (normal != null && material.HasProperty("_BumpMap"))
                {
                    material.SetTexture("_BumpMap", normal);
                    material.EnableKeyword("_NORMALMAP");
                    material.SetFloat("_BumpScale", 0.42f);
                }
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            return material;
        }

        private static float Hash01(int seed)
        {
            unchecked
            {
                int hash = seed * 1103515245 + 12345;
                hash ^= hash >> 11;
                hash *= 1274126177;
                hash ^= hash >> 16;
                return (hash & 0xffff) / 65535f;
            }
        }

        private static void AddNpcLimb(GameObject root, bool left, float size, Material sleeve, Material trim, Material skin)
        {
            float side = left ? -1f : 1f;
            GameObject shoulder = AddPrimitive(root, PrimitiveType.Sphere, left ? "Left Shoulder" : "Right Shoulder", new Vector3(side * 0.33f * size, 1.17f * size, 0f), new Vector3(0.105f, 0.075f, 0.105f) * size, trim);
            shoulder.transform.localRotation = Quaternion.Euler(0f, 0f, side * 8f);
            GameObject upper = AddPrimitive(root, PrimitiveType.Capsule, left ? "Left Upper Arm" : "Right Upper Arm", new Vector3(side * 0.37f * size, 0.93f * size, 0.015f * size), new Vector3(0.064f, 0.205f, 0.061f) * size, sleeve);
            upper.transform.localRotation = Quaternion.Euler(0f, 0f, side * 7f);
            AddPrimitive(root, PrimitiveType.Sphere, left ? "Left Elbow Wrap" : "Right Elbow Wrap", new Vector3(side * 0.39f * size, 0.77f * size, 0.010f * size), new Vector3(0.065f, 0.045f, 0.058f) * size, trim);
            GameObject forearm = AddPrimitive(root, PrimitiveType.Capsule, left ? "Left Forearm" : "Right Forearm", new Vector3(side * 0.39f * size, 0.65f * size, 0.015f * size), new Vector3(0.056f, 0.160f, 0.055f) * size, sleeve);
            forearm.transform.localRotation = Quaternion.Euler(0f, 0f, side * 4f);
            AddPrimitive(root, PrimitiveType.Cube, left ? "Left Bracer" : "Right Bracer", new Vector3(side * 0.40f * size, 0.61f * size, -0.030f * size), new Vector3(0.090f, 0.120f, 0.035f) * size, trim).transform.localRotation = Quaternion.Euler(0f, 0f, side * 4f);
            AddPrimitive(root, PrimitiveType.Sphere, left ? "Left Hand" : "Right Hand", new Vector3(side * 0.39f * size, 0.47f * size, 0.018f * size), new Vector3(0.066f, 0.060f, 0.060f) * size, skin);
        }

        private static void AddNpcLeg(GameObject root, bool left, float size, Material trouser, Material boot)
        {
            float side = left ? -1f : 1f;
            GameObject thigh = AddPrimitive(root, PrimitiveType.Capsule, left ? "Left Thigh" : "Right Thigh", new Vector3(side * 0.13f * size, 0.39f * size, 0.012f * size), new Vector3(0.080f, 0.220f, 0.082f) * size, trouser);
            thigh.transform.localRotation = Quaternion.Euler(0f, 0f, side * 1.5f);
            AddPrimitive(root, PrimitiveType.Cube, left ? "Left Knee Wrap" : "Right Knee Wrap", new Vector3(side * 0.13f * size, 0.30f * size, -0.050f * size), new Vector3(0.120f, 0.060f, 0.038f) * size, boot);
            GameObject shin = AddPrimitive(root, PrimitiveType.Capsule, left ? "Left Shin" : "Right Shin", new Vector3(side * 0.13f * size, 0.18f * size, 0.015f * size), new Vector3(0.064f, 0.175f, 0.070f) * size, trouser);
            shin.transform.localRotation = Quaternion.Euler(0f, 0f, side * 1.0f);
            AddPrimitive(root, PrimitiveType.Cube, left ? "Left Boot" : "Right Boot", new Vector3(side * 0.13f * size, 0.062f * size, -0.052f * size), new Vector3(0.150f, 0.082f, 0.185f) * size, boot);
            AddPrimitive(root, PrimitiveType.Cube, left ? "Left Boot Toe" : "Right Boot Toe", new Vector3(side * 0.13f * size, 0.035f * size, -0.145f * size), new Vector3(0.138f, 0.048f, 0.090f) * size, boot);
        }

        public void SetNpcPreviewAnimation(bool enabled)
        {
            animateNpcPreviews = enabled;
        }

        public GameObject CreateObjectVisual(PsychoMirrorObject worldObject)
        {
            GameObject root = new GameObject($"Object {worldObject.id} - {worldObject.name}");
            Material material = ResolveMaterial(worldObject.materialClass);
            float width = Mathf.Max(0.8f, worldObject.sizeX);
            float depth = Mathf.Max(0.8f, worldObject.sizeY);

            switch (worldObject.visualClass)
            {
                case "Tree":
                    AddPrimitive(root, PrimitiveType.Cylinder, "Trunk", new Vector3(0f, 0.7f, 0f), new Vector3(0.24f, 0.70f, 0.24f), ResolveMaterial("Wood"));
                    GameObject canopy = AddPrimitive(root, PrimitiveType.Sphere, "Canopy", new Vector3(0f, 1.72f, 0f), new Vector3(width, 0.86f, depth), material);
                    AddWind(canopy, 0.05f, 1.2f, 0.25f);
                    break;
                case "Rock":
                    AddPrimitive(root, PrimitiveType.Sphere, "Rock Mass", new Vector3(0f, 0.35f, 0f), new Vector3(width, 0.55f, depth), material);
                    break;
                case "Water":
                    AddPrimitive(root, PrimitiveType.Cube, "Water Surface", new Vector3(0f, 0.03f, 0f), new Vector3(width, 0.04f, depth), material);
                    break;
                case "Door":
                    AddPrimitive(root, PrimitiveType.Cube, "Door Slab", new Vector3(0f, 0.85f, 0f), new Vector3(width * 0.28f, 1.7f, depth), material);
                    break;
                case "Bank":
                case "Shop":
                    AddPrimitive(root, PrimitiveType.Cube, "Counter", new Vector3(0f, 0.45f, 0f), new Vector3(width, 0.65f, depth), material);
                    AddPrimitive(root, PrimitiveType.Cube, "Top", new Vector3(0f, 0.85f, 0f), new Vector3(width * 1.08f, 0.15f, depth * 1.08f), ResolveMaterial("Stone"));
                    break;
                case "Altar":
                    AddPrimitive(root, PrimitiveType.Cylinder, "Altar Base", new Vector3(0f, 0.35f, 0f), new Vector3(width * 0.55f, 0.35f, depth * 0.55f), material);
                    AddPrimitive(root, PrimitiveType.Cube, "Altar Cap", new Vector3(0f, 0.78f, 0f), new Vector3(width, 0.22f, depth), material);
                    break;
                default:
                    AddPrimitive(root, PrimitiveType.Cube, "Scenery Form", new Vector3(0f, 0.45f, 0f), new Vector3(width, 0.9f, depth), material);
                    break;
            }

            return root;
        }

        private Material ResolveMaterial(string materialClass)
        {
            string key = string.IsNullOrWhiteSpace(materialClass) ? "Default" : materialClass;
            Material loaded = Resources.Load<Material>($"PsychoMaterials/Psycho_{key}_2K");
            if (loaded != null)
            {
                return loaded;
            }

            return FallbackMaterial(key);
        }

        private static Material FallbackMaterial(string key)
        {
            Color color;
            switch (key)
            {
                case "Coin": color = new Color(0.94f, 0.66f, 0.20f); break;
                case "Rune": color = new Color(0.36f, 0.56f, 0.92f); break;
                case "Metal": color = new Color(0.62f, 0.62f, 0.60f); break;
                case "Wood": color = new Color(0.36f, 0.22f, 0.12f); break;
                case "Leaf": color = new Color(0.22f, 0.46f, 0.16f); break;
                case "Stone": color = new Color(0.42f, 0.43f, 0.40f); break;
                case "Water": color = new Color(0.08f, 0.36f, 0.58f, 0.82f); break;
                case "Skin": color = new Color(0.78f, 0.58f, 0.42f); break;
                case "Cloth": color = new Color(0.55f, 0.24f, 0.20f); break;
                case "Leather": color = new Color(0.38f, 0.20f, 0.12f); break;
                case "Crystal": color = new Color(0.34f, 0.74f, 0.88f); break;
                case "Organic": color = new Color(0.42f, 0.52f, 0.20f); break;
                case "Paper": color = new Color(0.82f, 0.76f, 0.58f); break;
                default: color = new Color(0.48f, 0.46f, 0.40f); break;
            }

            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.enableInstancing = true;
            return material;
        }

        private static GameObject AddPrimitive(GameObject root, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject child = GameObject.CreatePrimitive(type);
            child.name = name;
            child.transform.SetParent(root.transform, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = localScale;
            child.GetComponent<MeshRenderer>().sharedMaterial = material;
            return child;
        }

        private static GameObject AddTaperedPrism(GameObject root, string name, Vector3 localPosition, float topWidth, float bottomWidth, float height, float depth, Material material)
        {
            float top = topWidth * 0.5f;
            float bottom = bottomWidth * 0.5f;
            float halfHeight = height * 0.5f;
            float halfDepth = depth * 0.5f;
            Mesh mesh = new Mesh { name = name + " Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-top, halfHeight, -halfDepth),
                new Vector3(top, halfHeight, -halfDepth),
                new Vector3(bottom, -halfHeight, -halfDepth),
                new Vector3(-bottom, -halfHeight, -halfDepth),
                new Vector3(-top, halfHeight, halfDepth),
                new Vector3(top, halfHeight, halfDepth),
                new Vector3(bottom, -halfHeight, halfDepth),
                new Vector3(-bottom, -halfHeight, halfDepth)
            };
            mesh.triangles = new[]
            {
                0, 1, 2,
                0, 2, 3,
                4, 6, 5,
                4, 7, 6,
                4, 5, 1,
                4, 1, 0,
                3, 2, 6,
                3, 6, 7,
                1, 5, 6,
                1, 6, 2,
                4, 0, 3,
                4, 3, 7
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject child = new GameObject(name);
            child.transform.SetParent(root.transform, false);
            child.transform.localPosition = localPosition;
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            child.AddComponent<MeshRenderer>().sharedMaterial = material;
            return child;
        }

        private void AddWind(GameObject target, float amplitude, float speed, float gustStrength)
        {
            if (!animateFoliage)
            {
                return;
            }

            WindAnimatedFoliage wind = target.AddComponent<WindAnimatedFoliage>();
            SetPrivateFloat(wind, "amplitude", amplitude);
            SetPrivateFloat(wind, "speed", speed);
            SetPrivateFloat(wind, "gustStrength", gustStrength);
        }

        private static void SetPrivateFloat(Component component, string fieldName, float value)
        {
            System.Reflection.FieldInfo field = component.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(component, value);
        }
    }
}
