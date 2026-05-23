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
            Material body = ResolveMaterial(npc.materialClass);
            Material trim = ResolveMaterial(npc.attackable ? "Metal" : "Cloth");

            AddPrimitive(root, PrimitiveType.Cube, "Torso", new Vector3(0f, 0.94f * size, 0f), new Vector3(0.46f, 0.72f, 0.28f) * size, body);
            AddPrimitive(root, PrimitiveType.Sphere, "Head", new Vector3(0f, 1.42f * size, 0f), new Vector3(0.30f, 0.30f, 0.30f) * size, trim);
            AddPrimitive(root, PrimitiveType.Cube, "Left Arm", new Vector3(-0.36f * size, 0.94f * size, 0f), new Vector3(0.14f, 0.58f, 0.16f) * size, body);
            AddPrimitive(root, PrimitiveType.Cube, "Right Arm", new Vector3(0.36f * size, 0.94f * size, 0f), new Vector3(0.14f, 0.58f, 0.16f) * size, body);
            AddPrimitive(root, PrimitiveType.Cube, "Left Leg", new Vector3(-0.14f * size, 0.36f * size, 0f), new Vector3(0.16f, 0.70f, 0.18f) * size, trim);
            AddPrimitive(root, PrimitiveType.Cube, "Right Leg", new Vector3(0.14f * size, 0.36f * size, 0f), new Vector3(0.16f, 0.70f, 0.18f) * size, trim);
            AddPrimitive(root, PrimitiveType.Cube, "Facing Marker", new Vector3(0f, 1.04f * size, 0.18f * size), new Vector3(0.12f, 0.16f, 0.06f) * size, trim);

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
