using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Psycho.Mirror
{
    [Serializable]
    public sealed class PsychoMirrorManifest
    {
        public string version;
        public string generatedAtUtc;
        public string sourceServerPath;
        public int itemCount;
        public int npcCount;
        public int npcSpawnCount;
        public int objectCount;
        public int shopCount;
        public int mapRegionCount;
        public string[] objectDefinitionSources;
    }

    [Serializable]
    public sealed class PsychoMirrorItem
    {
        public int id;
        public string name;
        public string description;
        public int value;
        public bool stackable;
        public bool noted;
        public int equipmentSlot;
        public string visualClass;
        public string materialClass;
        public float scale;
        public string[] actions;
    }

    [Serializable]
    public sealed class PsychoMirrorNpc
    {
        public int id;
        public string name;
        public string examine;
        public int combat;
        public int size;
        public bool attackable;
        public bool aggressive;
        public bool retreats;
        public bool poisonous;
        public int respawn;
        public int maxHit;
        public int hitpoints;
        public int attackSpeed;
        public int attackAnim;
        public int defenceAnim;
        public int deathAnim;
        public int attackBonus;
        public int defenceMelee;
        public int defenceRange;
        public int defenceMage;
        public int slayerLevel;
        public string visualClass;
        public string materialClass;
        public float scale;
    }

    [Serializable]
    public sealed class PsychoMirrorNpcModel
    {
        public int id;
        public string name;
        public int size;
        public int standAnimation;
        public int walkAnimation;
        public bool osrs;
        public int[] modelIds;
    }

    [Serializable]
    public sealed class PsychoMirrorNpcSpawn
    {
        public int npcId;
        public string npcName;
        public string source;
        public string face;
        public int x;
        public int y;
        public int z;
        public bool coordinateWalking;
        public int walkRadius;
    }

    [Serializable]
    public sealed class PsychoMirrorObject
    {
        public int id;
        public string name;
        public string source;
        public int sizeX;
        public int sizeY;
        public bool interactive;
        public bool unwalkable;
        public bool impenetrable;
        public string visualClass;
        public string materialClass;
        public int[] modelIds;
        public string[] actions;
    }

    [Serializable]
    public sealed class PsychoMirrorShop
    {
        public int id;
        public string name;
        public int currency;
        public PsychoMirrorShopItem[] items;
    }

    [Serializable]
    public sealed class PsychoMirrorShopItem
    {
        public int id;
        public int amount;
        public string name;
    }

    [Serializable]
    public sealed class PsychoMirrorMapRegion
    {
        public int id;
        public int regionX;
        public int regionY;
        public int landscapeFile;
        public int objectFile;
        public bool osrs;
    }

    [Serializable]
    internal sealed class PsychoMirrorItemsFile
    {
        public PsychoMirrorItem[] items;
    }

    [Serializable]
    internal sealed class PsychoMirrorNpcsFile
    {
        public PsychoMirrorNpc[] npcs;
    }

    [Serializable]
    internal sealed class PsychoMirrorNpcSpawnsFile
    {
        public PsychoMirrorNpcSpawn[] spawns;
    }

    [Serializable]
    internal sealed class PsychoMirrorNpcModelsFile
    {
        public PsychoMirrorNpcModel[] models;
    }

    [Serializable]
    internal sealed class PsychoMirrorObjectsFile
    {
        public PsychoMirrorObject[] objects;
    }

    [Serializable]
    internal sealed class PsychoMirrorShopsFile
    {
        public PsychoMirrorShop[] shops;
    }

    [Serializable]
    internal sealed class PsychoMirrorMapRegionsFile
    {
        public PsychoMirrorMapRegion[] regions;
    }

    public sealed class PsychoMirrorDatabase
    {
        public PsychoMirrorManifest Manifest { get; private set; }
        public PsychoMirrorItem[] Items { get; private set; } = new PsychoMirrorItem[0];
        public PsychoMirrorNpc[] Npcs { get; private set; } = new PsychoMirrorNpc[0];
        public PsychoMirrorNpcModel[] NpcModels { get; private set; } = new PsychoMirrorNpcModel[0];
        public PsychoMirrorNpcSpawn[] NpcSpawns { get; private set; } = new PsychoMirrorNpcSpawn[0];
        public PsychoMirrorObject[] Objects { get; private set; } = new PsychoMirrorObject[0];
        public PsychoMirrorShop[] Shops { get; private set; } = new PsychoMirrorShop[0];
        public PsychoMirrorMapRegion[] MapRegions { get; private set; } = new PsychoMirrorMapRegion[0];

        private readonly Dictionary<int, PsychoMirrorItem> itemsById = new Dictionary<int, PsychoMirrorItem>();
        private readonly Dictionary<int, PsychoMirrorNpc> npcsById = new Dictionary<int, PsychoMirrorNpc>();
        private readonly Dictionary<int, PsychoMirrorNpcModel> npcModelsById = new Dictionary<int, PsychoMirrorNpcModel>();
        private readonly Dictionary<int, PsychoMirrorObject> objectsById = new Dictionary<int, PsychoMirrorObject>();
        private readonly Dictionary<int, PsychoMirrorShop> shopsById = new Dictionary<int, PsychoMirrorShop>();
        private readonly Dictionary<int, PsychoMirrorMapRegion> mapRegionsById = new Dictionary<int, PsychoMirrorMapRegion>();

        public static string DefaultRoot => Path.Combine(Application.streamingAssetsPath, "PsychoMirror");

        public static PsychoMirrorDatabase LoadFromStreamingAssets(string root = null)
        {
            string resolvedRoot = string.IsNullOrWhiteSpace(root) ? DefaultRoot : root;
            PsychoMirrorDatabase database = new PsychoMirrorDatabase();
            database.Manifest = ReadJson<PsychoMirrorManifest>(Path.Combine(resolvedRoot, "manifest.json"));
            database.Items = ReadJson<PsychoMirrorItemsFile>(Path.Combine(resolvedRoot, "items.json"))?.items ?? new PsychoMirrorItem[0];
            database.Npcs = ReadJson<PsychoMirrorNpcsFile>(Path.Combine(resolvedRoot, "npcs.json"))?.npcs ?? new PsychoMirrorNpc[0];
            database.NpcModels = ReadJson<PsychoMirrorNpcModelsFile>(Path.Combine(resolvedRoot, "npc_models.json"))?.models ?? new PsychoMirrorNpcModel[0];
            database.NpcSpawns = ReadJson<PsychoMirrorNpcSpawnsFile>(Path.Combine(resolvedRoot, "npc_spawns.json"))?.spawns ?? new PsychoMirrorNpcSpawn[0];
            database.Objects = ReadJson<PsychoMirrorObjectsFile>(Path.Combine(resolvedRoot, "objects.json"))?.objects ?? new PsychoMirrorObject[0];
            database.Shops = ReadJson<PsychoMirrorShopsFile>(Path.Combine(resolvedRoot, "shops.json"))?.shops ?? new PsychoMirrorShop[0];
            database.MapRegions = ReadJson<PsychoMirrorMapRegionsFile>(Path.Combine(resolvedRoot, "map_regions.json"))?.regions ?? new PsychoMirrorMapRegion[0];
            database.RebuildIndexes();
            return database;
        }

        public bool TryGetItem(int id, out PsychoMirrorItem item)
        {
            return itemsById.TryGetValue(id, out item);
        }

        public bool TryGetNpc(int id, out PsychoMirrorNpc npc)
        {
            return npcsById.TryGetValue(id, out npc);
        }

        public bool TryGetNpcModel(int id, out PsychoMirrorNpcModel model)
        {
            return npcModelsById.TryGetValue(id, out model);
        }

        public bool TryGetObject(int id, out PsychoMirrorObject worldObject)
        {
            return objectsById.TryGetValue(id, out worldObject);
        }

        public bool TryGetShop(int id, out PsychoMirrorShop shop)
        {
            return shopsById.TryGetValue(id, out shop);
        }

        public bool TryGetMapRegion(int id, out PsychoMirrorMapRegion region)
        {
            return mapRegionsById.TryGetValue(id, out region);
        }

        public string GetSummary()
        {
            string generated = Manifest == null || string.IsNullOrEmpty(Manifest.generatedAtUtc) ? "unknown" : Manifest.generatedAtUtc;
            return $"Psycho mirror loaded: {Items.Length} items, {Npcs.Length} NPCs, {NpcModels.Length} NPC model definitions, {NpcSpawns.Length} NPC spawns, {Objects.Length} objects, {Shops.Length} shops, {MapRegions.Length} map regions. Generated {generated}.";
        }

        private void RebuildIndexes()
        {
            itemsById.Clear();
            npcsById.Clear();
            npcModelsById.Clear();
            objectsById.Clear();
            shopsById.Clear();
            mapRegionsById.Clear();

            foreach (PsychoMirrorItem item in Items)
            {
                itemsById[item.id] = item;
            }

            foreach (PsychoMirrorNpc npc in Npcs)
            {
                npcsById[npc.id] = npc;
            }

            foreach (PsychoMirrorNpcModel model in NpcModels)
            {
                npcModelsById[model.id] = model;
            }

            foreach (PsychoMirrorObject worldObject in Objects)
            {
                objectsById[worldObject.id] = worldObject;
            }

            foreach (PsychoMirrorShop shop in Shops)
            {
                shopsById[shop.id] = shop;
            }

            foreach (PsychoMirrorMapRegion region in MapRegions)
            {
                mapRegionsById[region.id] = region;
            }
        }

        private static T ReadJson<T>(string path) where T : class
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Psycho mirror data missing: {path}");
                return null;
            }

            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
    }
}
