using UnityEngine;

namespace Psycho.Mirror
{
    public sealed class PsychoMirrorScenePopulator : MonoBehaviour
    {
        [Header("World projection")]
        [SerializeField] private int originX = 3690;
        [SerializeField] private int originY = 2977;
        [SerializeField] private float tileScale = 0.72f;
        [SerializeField] private float visibleTileRadius = 92f;

        [Header("Preview limits")]
        [SerializeField] private int maxNpcSpawns = 160;
        [SerializeField] private int itemPreviewCount = 72;
        [SerializeField] private int objectPreviewCount = 72;

        private PsychoMirrorLoader loader;
        private PsychoVisualFactory factory;

        private void Start()
        {
            loader = GetComponent<PsychoMirrorLoader>();
            factory = GetComponent<PsychoVisualFactory>();
            if (factory == null)
            {
                factory = gameObject.AddComponent<PsychoVisualFactory>();
            }

            PsychoMirrorDatabase database = loader != null ? loader.Database : null;
            if (database == null)
            {
                database = PsychoMirrorDatabase.LoadFromStreamingAssets();
            }

            PopulateNpcSpawns(database);
            PopulateItemPreview(database);
            PopulateObjectPreview(database);
        }

        private void PopulateNpcSpawns(PsychoMirrorDatabase database)
        {
            GameObject root = new GameObject("Mirrored NPC Spawns");
            root.transform.SetParent(transform, false);

            int spawned = 0;
            foreach (PsychoMirrorNpcSpawn spawn in database.NpcSpawns)
            {
                Vector3 position = WorldToUnity(spawn.x, spawn.y, spawn.z);
                if (new Vector2(position.x, position.z).magnitude > visibleTileRadius)
                {
                    continue;
                }

                if (!database.TryGetNpc(spawn.npcId, out PsychoMirrorNpc npc))
                {
                    continue;
                }

                GameObject npcVisual = factory.CreateNpcVisual(npc);
                npcVisual.transform.SetParent(root.transform, false);
                npcVisual.transform.localPosition = position;
                npcVisual.transform.localRotation = FaceToRotation(spawn.face);
                spawned++;
                if (spawned >= maxNpcSpawns)
                {
                    break;
                }
            }
        }

        private void PopulateItemPreview(PsychoMirrorDatabase database)
        {
            GameObject root = new GameObject("Mirrored Item Visual Samples");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(-13f, 0.08f, 13f);

            int count = Mathf.Min(itemPreviewCount, database.Items.Length);
            for (int i = 0; i < count; i++)
            {
                PsychoMirrorItem item = database.Items[i * Mathf.Max(1, database.Items.Length / count)];
                GameObject itemVisual = factory.CreateItemVisual(item);
                itemVisual.transform.SetParent(root.transform, false);
                itemVisual.transform.localPosition = new Vector3((i % 12) * 1.05f, 0f, (i / 12) * 1.15f);
            }
        }

        private void PopulateObjectPreview(PsychoMirrorDatabase database)
        {
            GameObject root = new GameObject("Mirrored Object Visual Samples");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(12f, 0.05f, 12f);

            int count = Mathf.Min(objectPreviewCount, database.Objects.Length);
            for (int i = 0; i < count; i++)
            {
                PsychoMirrorObject worldObject = database.Objects[i * Mathf.Max(1, database.Objects.Length / count)];
                GameObject objectVisual = factory.CreateObjectVisual(worldObject);
                objectVisual.transform.SetParent(root.transform, false);
                objectVisual.transform.localPosition = new Vector3((i % 12) * 1.25f, 0f, (i / 12) * 1.25f);
            }
        }

        private Vector3 WorldToUnity(int x, int y, int z)
        {
            return new Vector3((x - originX) * tileScale, z * 2.8f, (y - originY) * tileScale);
        }

        private static Quaternion FaceToRotation(string face)
        {
            switch (face)
            {
                case "NORTH": return Quaternion.Euler(0f, 0f, 0f);
                case "EAST": return Quaternion.Euler(0f, 90f, 0f);
                case "SOUTH": return Quaternion.Euler(0f, 180f, 0f);
                case "WEST": return Quaternion.Euler(0f, 270f, 0f);
                default: return Quaternion.identity;
            }
        }
    }
}
