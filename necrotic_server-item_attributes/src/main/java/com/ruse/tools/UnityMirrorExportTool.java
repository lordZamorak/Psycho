package com.ruse.tools;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.stream.JsonWriter;
import com.ruse.GameSettings;
import com.ruse.model.container.impl.Equipment;
import com.ruse.model.definitions.GameObjectDefinition;
import com.ruse.model.definitions.ItemDefinition;
import com.ruse.model.definitions.NpcDefinition;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.Reader;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.Instant;
import java.util.Arrays;
import java.util.Comparator;
import java.util.Locale;
import java.util.Map;

/**
 * Exports the Java server's authoritative definitions into Unity-friendly JSON.
 */
public final class UnityMirrorExportTool {

	private static final String OUTPUT_VERSION = "1";

	private UnityMirrorExportTool() {
	}

	public static void main(String[] args) throws Exception {
		Path output = args.length > 0
				? Paths.get(args[0]).toAbsolutePath().normalize()
				: Paths.get("..", "psycho_unity_client", "Assets", "StreamingAssets", "PsychoMirror").toAbsolutePath().normalize();
		Files.createDirectories(output);

		ItemDefinition.init();
		NpcDefinition.parseNpcs().load();
		GameObjectDefinition.init();

		int itemCount = exportItems(output.resolve("items.json"));
		int npcCount = exportNpcs(output.resolve("npcs.json"));
		int spawnCount = exportNpcSpawns(output.resolve("npc_spawns.json"));
		int objectCount = exportObjects(output.resolve("objects.json"));
		int shopCount = exportShops(output.resolve("shops.json"));
		exportManifest(output.resolve("manifest.json"), itemCount, npcCount, spawnCount, objectCount, shopCount);

		System.out.println("Exported Unity mirror data to " + output);
		System.out.println("Items=" + itemCount + ", NPCs=" + npcCount + ", spawns=" + spawnCount + ", objects=" + objectCount + ", shops=" + shopCount);
	}

	private static int exportItems(Path path) throws IOException {
		int count = 0;
		try (JsonWriter writer = writer(path)) {
			writer.beginObject();
			writer.name("items").beginArray();
			for (Map.Entry<Integer, ItemDefinition> entry : ItemDefinition.getEntries().stream()
					.sorted(Comparator.comparingInt(Map.Entry::getKey)).toArray(Map.Entry[]::new)) {
				ItemDefinition item = entry.getValue();
				writer.beginObject();
				writer.name("id").value(item.getId());
				writer.name("name").value(clean(item.getName()));
				writer.name("description").value(clean(item.getDescription()));
				writer.name("value").value(item.getValue());
				writer.name("stackable").value(item.isStackable());
				writer.name("noted").value(item.isNoted());
				writer.name("equipmentSlot").value(item.getEquipmentSlot());
				writer.name("visualClass").value(classifyItem(item));
				writer.name("materialClass").value(materialForItem(item));
				writer.name("scale").value(scaleForItem(item));
				writer.name("actions").beginArray();
				for (int i = 0; i < 5; i++) {
					writer.value(item.getAction(i));
				}
				writer.endArray();
				writer.endObject();
				count++;
			}
			writer.endArray();
			writer.endObject();
		}
		return count;
	}

	private static int exportNpcs(Path path) throws IOException {
		int count = 0;
		try (JsonWriter writer = writer(path)) {
			writer.beginObject();
			writer.name("npcs").beginArray();
			for (Map.Entry<Integer, NpcDefinition> entry : NpcDefinition.getEntries().stream()
					.sorted(Comparator.comparingInt(Map.Entry::getKey)).toArray(Map.Entry[]::new)) {
				NpcDefinition npc = entry.getValue();
				writer.beginObject();
				writer.name("id").value(npc.getId());
				writer.name("name").value(clean(npc.getName()));
				writer.name("examine").value(clean(npc.getExamine()));
				writer.name("combat").value(npc.getCombatLevel());
				writer.name("size").value(npc.getSize());
				writer.name("attackable").value(npc.isAttackable());
				writer.name("aggressive").value(npc.isAggressive());
				writer.name("retreats").value(npc.isRetreats());
				writer.name("poisonous").value(npc.isPoisonous());
				writer.name("respawn").value(npc.getRespawnTime());
				writer.name("maxHit").value(npc.getMaxHit());
				writer.name("hitpoints").value(npc.getHitpoints());
				writer.name("attackSpeed").value(npc.getAttackSpeed());
				writer.name("attackAnim").value(npc.getAttackAnimation());
				writer.name("defenceAnim").value(npc.getDefenceAnimation());
				writer.name("deathAnim").value(npc.getDeathAnimation());
				writer.name("attackBonus").value(npc.getAttackBonus());
				writer.name("defenceMelee").value(npc.getDefenceMelee());
				writer.name("defenceRange").value(npc.getDefenceRange());
				writer.name("defenceMage").value(npc.getDefenceMage());
				writer.name("slayerLevel").value(npc.getSlayerLevel());
				writer.name("visualClass").value(classifyNpc(npc));
				writer.name("materialClass").value(materialForNpc(npc));
				writer.name("scale").value(Math.max(0.85f, npc.getSize() <= 0 ? 1f : npc.getSize()));
				writer.endObject();
				count++;
			}
			writer.endArray();
			writer.endObject();
		}
		return count;
	}

	private static int exportNpcSpawns(Path path) throws IOException {
		int count = 0;
		try (JsonWriter writer = writer(path)) {
			writer.beginObject();
			writer.name("spawns").beginArray();
			count += exportNpcSpawnFile(writer, Paths.get(GameSettings.DEFINITION_DIRECTORY, "world_npcs.json"), "world_npcs.json");
			Path spawnDirectory = Paths.get(GameSettings.DEFINITION_DIRECTORY, "npc_spawns");
			if (Files.isDirectory(spawnDirectory)) {
				try (DirectoryStream<Path> files = Files.newDirectoryStream(spawnDirectory, "*.json")) {
					for (Path file : files) {
						count += exportNpcSpawnFile(writer, file, "npc_spawns/" + file.getFileName());
					}
				}
			}
			writer.endArray();
			writer.endObject();
		}
		return count;
	}

	private static int exportNpcSpawnFile(JsonWriter writer, Path file, String source) throws IOException {
		if (!Files.exists(file)) {
			return 0;
		}

		int count = 0;
		try (Reader reader = Files.newBufferedReader(file, StandardCharsets.UTF_8)) {
			JsonArray rows = new JsonParser().parse(reader).getAsJsonArray();
			for (JsonElement element : rows) {
				JsonObject row = element.getAsJsonObject();
				int npcId = row.get("npc-id").getAsInt();
				String face = row.has("face") && !row.get("face").isJsonNull() ? row.get("face").getAsString() : "";
				JsonObject walking = row.has("walking-policy") ? row.getAsJsonObject("walking-policy") : null;
				if (row.has("spawns")) {
					JsonArray positions = row.getAsJsonArray("spawns");
					for (JsonElement position : positions) {
						writeNpcSpawn(writer, source, npcId, face, walking, position.getAsJsonObject());
						count++;
					}
				} else if (row.has("position")) {
					writeNpcSpawn(writer, source, npcId, face, walking, row.getAsJsonObject("position"));
					count++;
				}
			}
		}
		return count;
	}

	private static void writeNpcSpawn(JsonWriter writer, String source, int npcId, String face, JsonObject walking, JsonObject position) throws IOException {
		NpcDefinition npc = NpcDefinition.forId(npcId);
		writer.beginObject();
		writer.name("npcId").value(npcId);
		writer.name("npcName").value(clean(npc.getName()));
		writer.name("source").value(source);
		writer.name("face").value(face);
		writer.name("x").value(position.get("x").getAsInt());
		writer.name("y").value(position.get("y").getAsInt());
		writer.name("z").value(position.has("z") ? position.get("z").getAsInt() : 0);
		writer.name("coordinateWalking").value(walking != null && walking.has("coordinate") && walking.get("coordinate").getAsBoolean());
		writer.name("walkRadius").value(walking != null && walking.has("radius") ? walking.get("radius").getAsInt() : 0);
		writer.endObject();
	}

	private static int exportObjects(Path path) throws IOException {
		int count = 0;
		try (JsonWriter writer = writer(path)) {
			writer.beginObject();
			writer.name("objects").beginArray();
			int standardCount = GameObjectDefinition.streamIndices[0].length;
			int extendedCount = GameObjectDefinition.streamIndices[1].length;
			int osrsCount = GameObjectDefinition.streamIndices[2].length;
			int maxStandardId = Math.max(standardCount, extendedCount);

			for (int id = 0; id < maxStandardId; id++) {
				GameObjectDefinition object = GameObjectDefinition.forId(id);
				if (writeObjectDefinition(writer, object, id, "standard")) {
					count++;
				}
			}
			for (int id = 0; id < osrsCount; id++) {
				int objectId = 70_000 + id;
				GameObjectDefinition object = GameObjectDefinition.forId(objectId);
				if (writeObjectDefinition(writer, object, objectId, "osrs")) {
					count++;
				}
			}

			writer.endArray();
			writer.endObject();
		}
		return count;
	}

	private static boolean writeObjectDefinition(JsonWriter writer, GameObjectDefinition object, int requestedId, String source) throws IOException {
		if (object == null || object.getName() == null || object.getName().trim().length() <= 1 || "null".equalsIgnoreCase(object.getName())) {
			return false;
		}

		writer.beginObject();
		writer.name("id").value(requestedId);
		writer.name("name").value(clean(object.getName()));
		writer.name("source").value(source);
		writer.name("sizeX").value(object.getSizeX());
		writer.name("sizeY").value(object.getSizeY());
		writer.name("interactive").value(object.interactive);
		writer.name("unwalkable").value(object.unwalkable);
		writer.name("impenetrable").value(object.impenetrable);
		writer.name("visualClass").value(classifyObject(object));
		writer.name("materialClass").value(materialForObject(object));
		writer.name("modelIds").beginArray();
		if (object.modelArray != null) {
			for (int modelId : object.modelArray) {
				writer.value(modelId);
			}
		}
		writer.endArray();
		writer.name("actions").beginArray();
		if (object.actions != null) {
			for (String action : object.actions) {
				if (action == null) {
					writer.nullValue();
				} else {
					writer.value(action);
				}
			}
		}
		writer.endArray();
		writer.endObject();
		return true;
	}

	private static int exportShops(Path path) throws IOException {
		Path source = Paths.get(GameSettings.DEFINITION_DIRECTORY, "world_shops.json");
		if (!Files.exists(source)) {
			return 0;
		}

		int count = 0;
		try (Reader reader = Files.newBufferedReader(source, StandardCharsets.UTF_8);
			 JsonWriter writer = writer(path)) {
			JsonArray shops = new JsonParser().parse(reader).getAsJsonArray();
			writer.beginObject();
			writer.name("shops").beginArray();
			for (JsonElement element : shops) {
				JsonObject shop = element.getAsJsonObject();
				writer.beginObject();
				writer.name("id").value(shop.get("id").getAsInt());
				writer.name("name").value(clean(shop.get("name").getAsString()));
				writer.name("currency").value(shop.get("currency").getAsInt());
				writer.name("items").beginArray();
				for (JsonElement itemElement : shop.getAsJsonArray("items")) {
					JsonObject item = itemElement.getAsJsonObject();
					writer.beginObject();
					writer.name("id").value(item.get("id").getAsInt());
					writer.name("amount").value(item.get("amount").getAsInt());
					writer.name("name").value(clean(ItemDefinition.forId(item.get("id").getAsInt()).getName()));
					writer.endObject();
				}
				writer.endArray();
				writer.endObject();
				count++;
			}
			writer.endArray();
			writer.endObject();
		}
		return count;
	}

	private static void exportManifest(Path path, int itemCount, int npcCount, int spawnCount, int objectCount, int shopCount) throws IOException {
		try (JsonWriter writer = writer(path)) {
			writer.beginObject();
			writer.name("version").value(OUTPUT_VERSION);
			writer.name("generatedAtUtc").value(Instant.now().toString());
			writer.name("sourceServerPath").value(Paths.get(".").toAbsolutePath().normalize().toString());
			writer.name("itemCount").value(itemCount);
			writer.name("npcCount").value(npcCount);
			writer.name("npcSpawnCount").value(spawnCount);
			writer.name("objectCount").value(objectCount);
			writer.name("shopCount").value(shopCount);
			writer.name("objectDefinitionSources").beginArray();
			writer.value("data/clipping/objects/loc.dat");
			writer.value("data/clipping/objects/667loc.dat");
			writer.value("data/clipping/objects/loc_osrs.dat");
			writer.endArray();
			writer.endObject();
		}
	}

	private static JsonWriter writer(Path path) throws IOException {
		Files.createDirectories(path.getParent());
		Writer output = new BufferedWriter(Files.newBufferedWriter(path, StandardCharsets.UTF_8));
		JsonWriter writer = new JsonWriter(output);
		writer.setIndent("  ");
		return writer;
	}

	private static String classifyItem(ItemDefinition item) {
		String name = lower(item.getName());
		int slot = item.getEquipmentSlot();
		if (item.isNoted()) return "Note";
		if (name.contains("coin") || name.contains("token") || name.contains("ticket") || name.contains("point")) return "Currency";
		if (name.contains("rune") || name.contains("spell")) return "Rune";
		if (name.contains("potion") || name.contains("flask") || name.contains("vial")) return "Potion";
		if (name.contains("shark") || name.contains("fish") || name.contains("lobster") || name.contains("cake") || name.contains("pie")) return "Food";
		if (name.contains("ore") || name.contains("bar") || name.contains("coal") || name.contains("log") || name.contains("plank")) return "Resource";
		if (name.contains("gem") || name.contains("crystal") || name.contains("diamond") || name.contains("sapphire") || name.contains("emerald") || name.contains("ruby")) return "Gem";
		if (name.contains("scroll") || name.contains("clue") || name.contains("book") || name.contains("note")) return "Scroll";
		if (name.contains("key")) return "Key";
		if (name.contains("axe") || name.contains("hatchet") || name.contains("pickaxe") || name.contains("hammer") || name.contains("knife")) return "Tool";
		if (slot == Equipment.WEAPON_SLOT) return item.isTwoHanded() ? "TwoHandedWeapon" : "Weapon";
		if (slot == Equipment.SHIELD_SLOT) return "Shield";
		if (slot >= 0) return "Armor";
		return item.isStackable() ? "Stackable" : "Item";
	}

	private static String materialForItem(ItemDefinition item) {
		String visual = classifyItem(item);
		String name = lower(item.getName());
		if ("Currency".equals(visual) || name.contains("gold")) return "Coin";
		if ("Rune".equals(visual)) return "Rune";
		if ("Resource".equals(visual) && (name.contains("log") || name.contains("plank"))) return "Wood";
		if ("Resource".equals(visual) && (name.contains("ore") || name.contains("bar"))) return "Metal";
		if ("Potion".equals(visual)) return "Water";
		if ("Food".equals(visual) || name.contains("herb") || name.contains("seed")) return "Organic";
		if ("Gem".equals(visual)) return "Crystal";
		if ("Scroll".equals(visual) || "Note".equals(visual)) return "Paper";
		if ("Armor".equals(visual) || "Weapon".equals(visual) || "TwoHandedWeapon".equals(visual) || "Shield".equals(visual) || "Tool".equals(visual)) return "Metal";
		return "Default";
	}

	private static float scaleForItem(ItemDefinition item) {
		if (item.getValue() >= 1_000_000) return 1.25f;
		if (item.getValue() >= 100_000) return 1.12f;
		return 1f;
	}

	private static String classifyNpc(NpcDefinition npc) {
		String name = lower(npc.getName());
		if (name.contains("banker")) return "Banker";
		if (name.contains("merchant") || name.contains("shop") || name.contains("trader")) return "Merchant";
		if (name.contains("guard") || name.contains("knight") || name.contains("warrior")) return "Guard";
		if (npc.getCombatLevel() >= 300 || name.contains("boss")) return "Boss";
		if (npc.isAttackable()) return "Combatant";
		return "Citizen";
	}

	private static String materialForNpc(NpcDefinition npc) {
		if (npc.getCombatLevel() >= 300) return "Metal";
		if (npc.isAttackable()) return "Leather";
		return "Cloth";
	}

	private static String classifyObject(GameObjectDefinition object) {
		String name = lower(object.getName());
		String actions = object.actions == null ? "" : lower(Arrays.toString(object.actions));
		if (name.contains("tree") || actions.contains("chop")) return "Tree";
		if (name.contains("rock") || name.contains("ore") || actions.contains("mine")) return "Rock";
		if (name.contains("water") || name.contains("fountain") || name.contains("well")) return "Water";
		if (name.contains("bank") || name.contains("booth")) return "Bank";
		if (name.contains("door") || name.contains("gate")) return "Door";
		if (name.contains("altar")) return "Altar";
		if (name.contains("furnace") || name.contains("anvil")) return "Forge";
		if (name.contains("stall") || name.contains("counter") || name.contains("shop")) return "Shop";
		if (name.contains("ladder") || name.contains("stair")) return "Traversal";
		if (name.contains("chest") || name.contains("crate")) return "Container";
		return object.interactive ? "InteractiveObject" : "Scenery";
	}

	private static String materialForObject(GameObjectDefinition object) {
		String visual = classifyObject(object);
		if ("Tree".equals(visual)) return "Leaf";
		if ("Rock".equals(visual) || "Forge".equals(visual) || "Altar".equals(visual)) return "Stone";
		if ("Water".equals(visual)) return "Water";
		if ("Door".equals(visual) || "Shop".equals(visual) || "Container".equals(visual)) return "Wood";
		if ("Bank".equals(visual)) return "Stone";
		return "Default";
	}

	private static String lower(String value) {
		return value == null ? "" : value.toLowerCase(Locale.ROOT);
	}

	private static String clean(String value) {
		return value == null ? "" : value.replace('\n', ' ').replace('\r', ' ').trim();
	}
}
