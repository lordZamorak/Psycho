package org.necrotic.client.cache.definition;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Objects;
import java.util.stream.IntStream;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonObject;
import org.necrotic.Configuration;
import org.necrotic.client.List;
import org.necrotic.client.Signlink;
import org.necrotic.client.cache.Archive;
import org.necrotic.client.cache.DataType;
import org.necrotic.client.graphics.DrawingArea;
import org.necrotic.client.graphics.Sprite;
import org.necrotic.client.io.ByteBuffer;
import org.necrotic.client.tools.FieldGenerator;
import org.necrotic.client.tools.TempWriter;
import org.necrotic.client.world.Model;
import org.necrotic.client.world.Rasterizer;

public final class ItemDefinition {

	private static ByteBuffer[] buffer = new ByteBuffer[2];
    private static int[][] streamIndices = new int[2][];
    public static int[] totalItems = new int[2];

	private static ItemDefinition[] cache;
	private static int cacheIndex;
	public static boolean isMembers = true;
	public static List mruNodes1 = new List(200);
	public static List mruNodes2 = new List(100);


    public static void unpackConfig(Archive streamLoader) {
        int index = 0;
        buffer[index] = new ByteBuffer(streamLoader.get("obj.dat"));
        ByteBuffer stream = new ByteBuffer(streamLoader.get("obj.idx"));
        totalItems[index] = stream.readUnsignedShort();
        streamIndices[index] = new int[totalItems[0]];
        int i = 2;
        for (int j = 0; j < totalItems[index]; j++) {
            streamIndices[index][j] = i;
            i += stream.readUnsignedShort();
        }

        index = 1;
		buffer[index] = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/obj.dat"));
		stream = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/obj.idx"));
        totalItems[index] = stream.readUnsignedShort();
        streamIndices[index] = new int[totalItems[index]];
        i = 2;
        for (int j = 0; j < totalItems[index]; j++) {
            streamIndices[index][j] = i;
            i += stream.readUnsignedShort();
        }

        cache = new ItemDefinition[10];
        for (int k = 0; k < 10; k++) {
            cache[k] = new ItemDefinition();
        }

        System.out.println("667 items: " + totalItems[0]);
        System.out.println("OSRS items: " + totalItems[1]);

        if (Configuration.developerMode) {
			TempWriter writer = new TempWriter("item_ids");
			IntStream.range(0, 100_000).forEach(id -> {
				try {
					ItemDefinition def = ItemDefinition.get(id);
					if (!def.name.equalsIgnoreCase("dwarf remains")) {
						writer.writeLine(def.id + " " + def.name);
					}
				} catch (Exception e) {}
			});
			writer.close();

			TempWriter writer2 = new TempWriter("item_fields");
			FieldGenerator generator = new FieldGenerator(writer2::writeLine);
			IntStream.range(0, 100_000).forEach(id -> {
				try {
					ItemDefinition definition = ItemDefinition.get(id);
					generator.add(definition.name + (definition.certTemplateID != -1 ? " noted" : ""), id, definition.dataType == DataType.OSRS);
				} catch (Exception e) {}
			});
			writer2.close();

			Gson gson = new GsonBuilder().setPrettyPrinting().create();
			ArrayList<JsonObject> jsonObjectList = new ArrayList<>();
			IntStream.range(0, 100_000).forEach(id -> {
				try {
					ItemDefinition definition = ItemDefinition.get(id);
					if (definition != null && definition.id > 0 && definition.id != 30_000 && definition.actions != null
							&& !Arrays.stream(definition.actions).allMatch(Objects::isNull)) {
						JsonObject jsonObject = new JsonObject();
						jsonObject.addProperty("id", id);
						jsonObject.add("actions", gson.toJsonTree(definition.actions));
						jsonObjectList.add(jsonObject);
					}
				} catch (Exception e) {
				}
			});

			try {
				BufferedWriter writer3 = new BufferedWriter(new FileWriter("./temp/item_actions.json"));
				writer3.write(gson.toJson(jsonObjectList));
				writer3.close();
			} catch (IOException e) {
				e.printStackTrace();
			}


		}
    }

	public static ItemDefinition get(int id) {
		for (int i = 0; i < 10; i++) {
			if (cache[i].id == id) {
				return cache[i];
			}
		}

		cacheIndex = (cacheIndex + 1) % 10;
		ItemDefinition itemDef = cache[cacheIndex];

		int index = 0;
		if (id >= 30_000) {
		    index = 1;
		    id -= 30_000;
		    itemDef.dataType = DataType.OSRS;
        } else {
			itemDef.dataType = DataType.STANDARD;
		}

		if (id >= totalItems[index]) {
		    id = 0;
        }

		buffer[index].position = streamIndices[index][id];
		itemDef.id = id + (index == 1 ? 30_000 : 0);
		itemDef.setDefaults();
		itemDef.doReadValues(buffer[index]);
		if (index == 0) {
            ItemDefinitionCustom.do667(itemDef);
        } else if (index == 1) {
			ItemDefinitionCustom.doOsrs(itemDef);
		}

		if (index == 1) {
		    if (itemDef.certTemplateID != -1)
		        itemDef.certTemplateID += 30_000;
            if (itemDef.certID != -1)
                itemDef.certID += 30_000;
        }

		if (itemDef.certTemplateID != -1)
			itemDef.toNote();
		if (itemDef.lendTemplateID != -1)
			itemDef.toLend();
		return itemDef;
	}

    @Override
    public String toString() {
        return "ItemDefinition{" +
                "actions=" + Arrays.toString(actions) +
                ", id=" + id +
                ", name='" + name + '\'' +
                '}';
    }

    public static Sprite getSprite(int i, int j, int k) {
		if (k == 0) {
			Sprite sprite = (Sprite) mruNodes1.insertFromCache(i);

			if (sprite != null && sprite.maxHeight != j && sprite.maxHeight != -1) {
				sprite.unlink();
				sprite = null;
			}

			if (sprite != null) {
				return sprite;
			}
		}

		ItemDefinition definition = get(i);

		if (definition.stackIDs == null) {
			j = -1;
		}

		if (j > 1) {
			int i1 = -1;

			for (int j1 = 0; j1 < 10; j1++) {
				if (j >= definition.stackAmounts[j1] && definition.stackAmounts[j1] != 0) {
					i1 = definition.stackIDs[j1];
				}
			}

			if (i1 != -1) {
				definition = get(i1);
			}
		}

		Model model = definition.getInventoryModel(1);

		if (model == null) {
			return null;
		}

		Sprite sprite = null;

		if (definition.certTemplateID != -1) {
			sprite = getSprite(definition.certID, 10, -1);

			if (sprite == null) {
				return null;
			}
		}

		if (definition.lendTemplateID != -1) {
			sprite = getSprite(definition.lendID, 50, 0);

			if (sprite == null) {
				return null;
			}
		}

		Sprite sprite2 = new Sprite(32, 32);
		int k1 = Rasterizer.centerX;
		int l1 = Rasterizer.centerY;
		int ai[] = Rasterizer.lineOffsets;
		int ai1[] = DrawingArea.pixels;
		int i2 = DrawingArea.width;
		int j2 = DrawingArea.height;
		int k2 = DrawingArea.topX;
		int l2 = DrawingArea.bottomX;
		int i3 = DrawingArea.topY;
		int j3 = DrawingArea.bottomY;
		Rasterizer.notTextured = false;
		DrawingArea.initDrawingArea(32, 32, sprite2.myPixels);
		DrawingArea.drawPixels(32, 0, 0, 0, 32);
		Rasterizer.method364();
		int k3 = definition.modelZoom;

		if (k == -1) {
			k3 = (int) (k3 * 1.5D);
		}

		if (k > 0) {
			k3 = (int) (k3 * 1.04D);
		}

		int l3 = Rasterizer.SINE[definition.modelRotation1] * k3 >> 16;
		int i4 = Rasterizer.COSINE[definition.modelRotation1] * k3 >> 16;
		model.renderSingle(definition.modelRotation2, definition.modelOffsetX, definition.modelRotation1, definition.modelOffset1, l3 + model.modelHeight / 2 + definition.modelOffsetY, i4 + definition.modelOffsetY);

		for (int i5 = 31; i5 >= 0; i5--) {
			for (int j4 = 31; j4 >= 0; j4--) {
				if (sprite2.myPixels[i5 + j4 * 32] == 0) {
					if (i5 > 0 && sprite2.myPixels[i5 - 1 + j4 * 32] > 1) {
						sprite2.myPixels[i5 + j4 * 32] = 1;
					} else if (j4 > 0 && sprite2.myPixels[i5 + (j4 - 1) * 32] > 1) {
						sprite2.myPixels[i5 + j4 * 32] = 1;
					} else if (i5 < 31 && sprite2.myPixels[i5 + 1 + j4 * 32] > 1) {
						sprite2.myPixels[i5 + j4 * 32] = 1;
					} else if (j4 < 31 && sprite2.myPixels[i5 + (j4 + 1) * 32] > 1) {
						sprite2.myPixels[i5 + j4 * 32] = 1;
					}
				}
			}
		}

		if (k > 0) {
			for (int j5 = 31; j5 >= 0; j5--) {
				for (int k4 = 31; k4 >= 0; k4--) {
					if (sprite2.myPixels[j5 + k4 * 32] == 0) {
						if (j5 > 0 && sprite2.myPixels[j5 - 1 + k4 * 32] == 1) {
							sprite2.myPixels[j5 + k4 * 32] = k;
						} else if (k4 > 0 && sprite2.myPixels[j5 + (k4 - 1) * 32] == 1) {
							sprite2.myPixels[j5 + k4 * 32] = k;
						} else if (j5 < 31 && sprite2.myPixels[j5 + 1 + k4 * 32] == 1) {
							sprite2.myPixels[j5 + k4 * 32] = k;
						} else if (k4 < 31 && sprite2.myPixels[j5 + (k4 + 1) * 32] == 1) {
							sprite2.myPixels[j5 + k4 * 32] = k;
						}
					}
				}
			}
		} else if (k == 0) {
			for (int k5 = 31; k5 >= 0; k5--) {
				for (int l4 = 31; l4 >= 0; l4--) {
					if (sprite2.myPixels[k5 + l4 * 32] == 0 && k5 > 0 && l4 > 0 && sprite2.myPixels[k5 - 1 + (l4 - 1) * 32] > 0) {
						sprite2.myPixels[k5 + l4 * 32] = 0x302020;
					}
				}
			}
		}

		if (definition.certTemplateID != -1) {
			int l5 = sprite.maxWidth;
			int j6 = sprite.maxHeight;
			sprite.maxWidth = 32;
			sprite.maxHeight = 32;
			sprite.drawSprite(0, 0);
			sprite.maxWidth = l5;
			sprite.maxHeight = j6;
		}

		if (definition.lendTemplateID != -1) {
			int l5 = sprite.maxWidth;
			int j6 = sprite.maxHeight;
			sprite.maxWidth = 32;
			sprite.maxHeight = 32;
			sprite.drawSprite(0, 0);
			sprite.maxWidth = l5;
			sprite.maxHeight = j6;
		}

		if (k == 0) {
			mruNodes1.removeFromCache(sprite2, i);
		}

		DrawingArea.initDrawingArea(j2, i2, ai1);
		DrawingArea.setBounds(k2, i3, l2, j3);
		Rasterizer.centerX = k1;
		Rasterizer.centerY = l1;
		Rasterizer.lineOffsets = ai;
		Rasterizer.notTextured = true;

		if (definition.stackable) {
			sprite2.maxWidth = 33;
		} else {
			sprite2.maxWidth = 32;
		}

		sprite2.maxHeight = j;
		return sprite2;
	}

	public static Sprite getSprite(int i, int j, int k, int zoom) {
		if (k == 0 && zoom != -1) {
			Sprite sprite = (Sprite) mruNodes1.insertFromCache(i);

			if (sprite != null && sprite.maxHeight != j && sprite.maxHeight != -1) {
				sprite.unlink();
				sprite = null;
			}

			if (sprite != null) {
				return sprite;
			}
		}

		ItemDefinition definition = get(i);

		if (definition.stackIDs == null) {
			j = -1;
		}

		if (j > 1) {
			int i1 = -1;

			for (int j1 = 0; j1 < 10; j1++) {
				if (j >= definition.stackAmounts[j1] && definition.stackAmounts[j1] != 0) {
					i1 = definition.stackIDs[j1];
				}
			}

			if (i1 != -1) {
				definition = get(i1);
			}
		}

		Model model = definition.getInventoryModel(1);

		if (model == null) {
			return null;
		}

		Sprite sprite = null;

		if (definition.certTemplateID != -1) {
			sprite = getSprite(definition.certID, 10, -1);

			if (sprite == null) {
				return null;
			}
		}

		if (definition.lendTemplateID != -1) {
			sprite = getSprite(definition.lendID, 50, 0);

			if (sprite == null) {
				return null;
			}
		}

		Sprite sprite2 = new Sprite(32, 32);
		int k1 = Rasterizer.centerX;
		int l1 = Rasterizer.centerY;
		int ai[] = Rasterizer.lineOffsets;
		int ai1[] = DrawingArea.pixels;
		int i2 = DrawingArea.width;
		int j2 = DrawingArea.height;
		int k2 = DrawingArea.topX;
		int l2 = DrawingArea.bottomX;
		int i3 = DrawingArea.topY;
		int j3 = DrawingArea.bottomY;
		Rasterizer.notTextured = false;
		DrawingArea.initDrawingArea(32, 32, sprite2.myPixels);
		DrawingArea.drawPixels(32, 0, 0, 0, 32);
		Rasterizer.method364();
		int k3 = definition.modelZoom;
		if (zoom != -1 && zoom != 0)
			k3 = (definition.modelZoom * 100) / zoom;
		if (k == -1) {
			k3 = (int) (k3 * 1.5D);
		}

		if (k > 0) {
			k3 = (int) (k3 * 1.04D);
		}

		int l3 = Rasterizer.SINE[definition.modelRotation1] * k3 >> 16;
		int i4 = Rasterizer.COSINE[definition.modelRotation1] * k3 >> 16;
		model.renderSingle(definition.modelRotation2, definition.modelOffsetX, definition.modelRotation1, definition.modelOffset1, l3 + model.modelHeight / 2 + definition.modelOffsetY, i4 + definition.modelOffsetY);

		for (int i5 = 31; i5 >= 0; i5--) {
			for (int j4 = 31; j4 >= 0; j4--) {
				if (sprite2.myPixels[i5 + j4 * 32] == 0) {
					if (i5 > 0 && sprite2.myPixels[i5 - 1 + j4 * 32] > 1) {
						sprite2.myPixels[i5 + j4 * 32] = 1;
					} else if (j4 > 0 && sprite2.myPixels[i5 + (j4 - 1) * 32] > 1) {
						sprite2.myPixels[i5 + j4 * 32] = 1;
					} else if (i5 < 31 && sprite2.myPixels[i5 + 1 + j4 * 32] > 1) {
						sprite2.myPixels[i5 + j4 * 32] = 1;
					} else if (j4 < 31 && sprite2.myPixels[i5 + (j4 + 1) * 32] > 1) {
						sprite2.myPixels[i5 + j4 * 32] = 1;
					}
				}
			}
		}

		if (k > 0) {
			for (int j5 = 31; j5 >= 0; j5--) {
				for (int k4 = 31; k4 >= 0; k4--) {
					if (sprite2.myPixels[j5 + k4 * 32] == 0) {
						if (j5 > 0 && sprite2.myPixels[j5 - 1 + k4 * 32] == 1) {
							sprite2.myPixels[j5 + k4 * 32] = k;
						} else if (k4 > 0 && sprite2.myPixels[j5 + (k4 - 1) * 32] == 1) {
							sprite2.myPixels[j5 + k4 * 32] = k;
						} else if (j5 < 31 && sprite2.myPixels[j5 + 1 + k4 * 32] == 1) {
							sprite2.myPixels[j5 + k4 * 32] = k;
						} else if (k4 < 31 && sprite2.myPixels[j5 + (k4 + 1) * 32] == 1) {
							sprite2.myPixels[j5 + k4 * 32] = k;
						}
					}
				}
			}
		} else if (k == 0) {
			for (int k5 = 31; k5 >= 0; k5--) {
				for (int l4 = 31; l4 >= 0; l4--) {
					if (sprite2.myPixels[k5 + l4 * 32] == 0 && k5 > 0 && l4 > 0 && sprite2.myPixels[k5 - 1 + (l4 - 1) * 32] > 0) {
						sprite2.myPixels[k5 + l4 * 32] = 0x302020;
					}
				}
			}
		}

		if (definition.certTemplateID != -1) {
			int l5 = sprite.maxWidth;
			int j6 = sprite.maxHeight;
			sprite.maxWidth = 32;
			sprite.maxHeight = 32;
			sprite.drawSprite(0, 0);
			sprite.maxWidth = l5;
			sprite.maxHeight = j6;
		}

		if (definition.lendTemplateID != -1) {
			int l5 = sprite.maxWidth;
			int j6 = sprite.maxHeight;
			sprite.maxWidth = 32;
			sprite.maxHeight = 32;
			sprite.drawSprite(0, 0);
			sprite.maxWidth = l5;
			sprite.maxHeight = j6;
		}

		if (k == 0) {
			mruNodes1.removeFromCache(sprite2, i);
		}

		DrawingArea.initDrawingArea(j2, i2, ai1);
		DrawingArea.setBounds(k2, i3, l2, j3);
		Rasterizer.centerX = k1;
		Rasterizer.centerY = l1;
		Rasterizer.lineOffsets = ai;
		Rasterizer.notTextured = true;

		if (definition.stackable) {
			sprite2.maxWidth = 33;
		} else {
			sprite2.maxWidth = 32;
		}

		sprite2.maxHeight = j;
		return sprite2;
	}

	public static void nullify() {
		mruNodes2 = null;
		mruNodes1 = null;
		streamIndices = null;
		cache = null;
		buffer = null;
	}

	public String[] actions;
	private int anInt162;
    public int anInt164;
	public int maleWearId;
	private int anInt166;
	private int anInt167;
	private int anInt173;
	public int maleDialogue;
	private int anInt184;
	private int anInt185;
    public int anInt188; //male arms
	private int anInt191;
	private int anInt192;
	private int anInt196;
    public int femaleDialogue;
	public int femaleWearId;
	public int modelOffsetX;
	public int certID;
	public int certTemplateID;
	public byte[] description;
	public byte femaleWieldX;
	public byte femaleWieldY;
	public byte femaleWieldZ;
	public String[] groundActions;
	public int id;
	public int lendID;
	private int lendTemplateID;
	public byte maleWieldX;
	public byte maleWieldY;
	public byte maleWieldZ;
	public boolean membersObject;
	public int modelID;
    public int modelOffset1;
    public int modelOffsetY;
	public int modelRotation1;
	public int modelRotation2;
	public int modelZoom;
    public int[] modifiedModelColors;
    public int[] originalModelColors;
	public int[] modifiedTexture;
	public int[] originalTexture;
	public String name;
	public boolean stackable;
	private int[] stackAmounts;
	private int[] stackIDs;
	public int team;
	public int value;
	public DataType dataType;

	public ItemDefinition() {
		id = -1;
	}

	public Model getInventoryModel(int amount) {
		if (stackIDs != null && amount > 1) {
			int id = -1;

			for (int i = 0; i < 10; i++) {
				if (amount >= stackAmounts[i] && stackAmounts[i] != 0) {
					id = stackIDs[i];
				}
			}

			if (id != -1) {
				return get(id).getInventoryModel(1);
			}
		}

		Model model = (Model) mruNodes2.insertFromCache(id);

		if (model != null) {
			return model;
		}

		model = Model.fetchModel(modelID, dataType);

		if (model == null) {
			return null;
		}

        ItemDefinitionCustom.textureOsrs(model, id);

		if (anInt167 != 128 || anInt192 != 128 || anInt191 != 128) {
			model.scaleT(anInt167, anInt191, anInt192);
		}

		if (modifiedModelColors != null) {
			for (int l = 0; l < modifiedModelColors.length; l++) {
				model.method476(modifiedModelColors[l], originalModelColors[l]);
			}
		}

		model.light(64 + anInt196, 768 + anInt184, -50, -10, -50, true);
		model.renders_inside_tile = true;
		mruNodes2.removeFromCache(model, id);
		return model;
	}

	public boolean dialogueModelFetched(int j) {
		int k = maleDialogue;
		int l = anInt166;

		if (j == 1) {
			k = femaleDialogue;
			l = anInt173;
		}

		if (k == -1) {
			return true;
		}

		boolean flag = true;

		if (!Model.method463(k, dataType)) {
			flag = false;
		}

		if (l != -1 && !Model.method463(l, dataType)) {
			flag = false;
		}

		return flag;
	}

	public Model method194(int j) {
		int k = maleDialogue;
		int l = anInt166;

		if (j == 1) {
			k = femaleDialogue;
			l = anInt173;
		}

		if (k == -1) {
			return null;
		}

		Model model = Model.fetchModel(k, dataType);

		if (l != -1) {
			Model model_1 = Model.fetchModel(l, dataType);
			Model models[] = { model, model_1 };
			model = new Model(2, models);
		}

        ItemDefinitionCustom.textureOsrs(model, id);

		if (modifiedModelColors != null) {
			for (int i1 = 0; i1 < modifiedModelColors.length; i1++) {
				model.method476(modifiedModelColors[i1], originalModelColors[i1]);
			}
		}

		return model;
	}

	public boolean method195(int j) {
		int k = maleWearId;
		int l = anInt188;
		int i1 = anInt185;

		if (j == 1) {
			k = femaleWearId;
			l = anInt164;
			i1 = anInt162;
		}

		if (k == -1) {
			return true;
		}

		boolean flag = true;

		if (!Model.method463(k, dataType)) {
			flag = false;
		}

		if (l != -1 && !Model.method463(l, dataType)) {
			flag = false;
		}

		if (i1 != -1 && !Model.method463(i1, dataType)) {
			flag = false;
		}

		return flag;
	}

	public Model method196(int i) {
		int j = maleWearId;
		int k = anInt188;
		int l = anInt185;

		if (i == 1) {
			j = femaleWearId;
			k = anInt164;
			l = anInt162;
		}

		if (j == -1) {
			return null;
		}

		Model model = Model.fetchModel(j, dataType);

		if (k != -1) {
			if (l != -1) {
				Model model_1 = Model.fetchModel(k, dataType);
				Model model_3 = Model.fetchModel(l, dataType);
				Model model_1s[] = { model, model_1, model_3 };
				model = new Model(3, model_1s);
			} else {
				Model model_2 = Model.fetchModel(k, dataType);
				Model models[] = { model, model_2 };
				model = new Model(2, models);
			}
		}

        ItemDefinitionCustom.textureOsrs(model, id);

		if (i == 0 && (maleWieldX != 0 || maleWieldY != 0 || maleWieldZ != 0)) {
			model.translate(maleWieldX, maleWieldY, maleWieldZ);
		}

		if (i == 1 && (femaleWieldX != 0 || femaleWieldY != 0 || femaleWieldZ != 0)) {
			model.translate(femaleWieldX, femaleWieldY, femaleWieldZ);
		}

		if (modifiedModelColors != null) {
			for (int i1 = 0; i1 < modifiedModelColors.length; i1++) {
				model.method476(modifiedModelColors[i1], originalModelColors[i1]);
			}
		}

		return model;
	}

	public Model method202(int i) {
		if (stackIDs != null && i > 1) {
			int j = -1;

			for (int k = 0; k < 10; k++) {
				if (i >= stackAmounts[k] && stackAmounts[k] != 0) {
					j = stackIDs[k];
				}
			}

			if (j != -1) {
				return get(j).method202(1);
			}
		}

		Model model = Model.fetchModel(modelID, dataType);

		if (model == null) {
			return null;
		}

		ItemDefinitionCustom.textureOsrs(model, id);

		if (modifiedModelColors != null) {
			for (int l = 0; l < modifiedModelColors.length; l++) {
				model.method476(modifiedModelColors[l], originalModelColors[l]);
			}
		}

		return model;
	}

	private void doReadValues(ByteBuffer buffer) {
	    if (dataType == DataType.STANDARD) {
	        readValues(buffer);
        } else {
	        readValuesOsrs(buffer);
        }
    }

	private void readValues(ByteBuffer buffer) {
		do {
			int opcode = buffer.readUnsignedByte();

			if (opcode == 0) {
				return;
			} else if (opcode == 1) {
				modelID = buffer.readUnsignedShort();
			} else if (opcode == 2) {
				name = buffer.readString();
			} else if (opcode == 3) {
				description = buffer.readBytes();
			} else if (opcode == 4) {
				modelZoom = buffer.readUnsignedShort();
			} else if (opcode == 5) {
				modelRotation1 = buffer.readUnsignedShort();
			} else if (opcode == 6) {
				modelRotation2 = buffer.readUnsignedShort();
			} else if (opcode == 7) {
				modelOffset1 = buffer.readUnsignedShort();

				if (modelOffset1 > 32767) {
					modelOffset1 -= 0x10000;
				}
			} else if (opcode == 8) {
				modelOffsetY = buffer.readUnsignedShort();

				if (modelOffsetY > 32767) {
					modelOffsetY -= 0x10000;
				}
			} else if (opcode == 10) {
				buffer.readUnsignedShort();
			} else if (opcode == 11) {
				stackable = true;
			} else if (opcode == 12) {
				value = buffer.readIntLittleEndian();
			} else if (opcode == 16) {
				membersObject = true;
			} else if (opcode == 23) {
				maleWearId = buffer.readUnsignedShort();
				maleWieldY = buffer.readtSignedByte();
			} else if (opcode == 24) {
				anInt188 = buffer.readUnsignedShort();
			} else if (opcode == 25) {
				femaleWearId = buffer.readUnsignedShort();
				femaleWieldY = buffer.readtSignedByte();
			} else if (opcode == 26) {
				anInt164 = buffer.readUnsignedShort();
			} else if (opcode >= 30 && opcode < 35) {
				if (groundActions == null) {
					groundActions = new String[5];
				}

				groundActions[opcode - 30] = buffer.readString();

				if (groundActions[opcode - 30].equalsIgnoreCase("hidden")) {
					groundActions[opcode - 30] = null;
				}
			} else if (opcode >= 35 && opcode < 40) {
				if (actions == null) {
					actions = new String[5];
				}

				actions[opcode - 35] = buffer.readString();
			} else if (opcode == 40) {
				int size = buffer.readUnsignedByte();
				modifiedModelColors = new int[size];
				originalModelColors = new int[size];

				for (int k = 0; k < size; k++) {
					modifiedModelColors[k] = buffer.readUnsignedShort();
					originalModelColors[k] = buffer.readUnsignedShort();
				}
			} else if (opcode == 78) {
				anInt185 = buffer.readUnsignedShort();
			} else if (opcode == 79) {
				anInt162 = buffer.readUnsignedShort();
			} else if (opcode == 90) {
				maleDialogue = buffer.readUnsignedShort();
			} else if (opcode == 91) {
				femaleDialogue = buffer.readUnsignedShort();
			} else if (opcode == 92) {
				anInt166 = buffer.readUnsignedShort();
			} else if (opcode == 93) {
				anInt173 = buffer.readUnsignedShort();
			} else if (opcode == 95) {
				modelOffsetX = buffer.readUnsignedShort();
			} else if (opcode == 97) {
				certID = buffer.readUnsignedShort();
			} else if (opcode == 98) {
				certTemplateID = buffer.readUnsignedShort();
			} else if (opcode >= 100 && opcode < 110) {
				if (stackIDs == null) {
					stackIDs = new int[10];
					stackAmounts = new int[10];
				}

				stackIDs[opcode - 100] = buffer.readUnsignedShort();
				stackAmounts[opcode - 100] = buffer.readUnsignedShort();
			} else if (opcode == 110) {
				anInt167 = buffer.readUnsignedShort();
			} else if (opcode == 111) {
				anInt192 = buffer.readUnsignedShort();
			} else if (opcode == 112) {
				anInt191 = buffer.readUnsignedShort();
			} else if (opcode == 113) {
				anInt196 = buffer.readtSignedByte();
			} else if (opcode == 114) {
				anInt184 = buffer.readtSignedByte() * 5;
			} else if (opcode == 115) {
				team = buffer.readUnsignedByte();
			} else if (opcode == 121) {
				lendID = buffer.readUnsignedShort();
			} else if (opcode == 122) {
				lendTemplateID = buffer.readUnsignedShort();
			}
		} while (true);
	}

    private void readValuesOsrs(ByteBuffer buffer) {
        do {
            int opcode = buffer.readUnsignedByte();

            if (opcode == 0) {
                return;
            } else if (opcode == 1) {
                modelID = buffer.readUnsignedShort();
            } else if (opcode == 2) {
                name = buffer.readString();
            } else if (opcode == 3) {
                //description = buffer.getBytes();
            } else if (opcode == 4) {
                modelZoom = buffer.readUnsignedShort();
            } else if (opcode == 5) {
                modelRotation1 = buffer.readUnsignedShort();
            } else if (opcode == 6) {
                modelRotation2 = buffer.readUnsignedShort();
            } else if (opcode == 7) {
                modelOffset1 = buffer.readUnsignedShort();

                if (modelOffset1 > 32767) {
                    modelOffset1 -= 0x10000;
                }
            } else if (opcode == 8) {
                modelOffsetY = buffer.readUnsignedShort();

                if (modelOffsetY > 32767) {
                    modelOffsetY -= 0x10000;
                }
            } else if (opcode == 10) {
                //buffer.getUnsignedShort();
            } else if (opcode == 11) {
                stackable = true;
            } else if (opcode == 12) {
                value = buffer.readInt();
            } else if (opcode == 16) {
                membersObject = true;
            } else if (opcode == 23) {
                maleWearId = buffer.readUnsignedShort();
                maleWieldY = buffer.readtSignedByte();
            } else if (opcode == 24) {
                anInt188 = buffer.readUnsignedShort();
            } else if (opcode == 25) {
                femaleWearId = buffer.readUnsignedShort();
                femaleWieldY = buffer.readtSignedByte();
            } else if (opcode == 26) {
                anInt164 = buffer.readUnsignedShort();
            } else if (opcode >= 30 && opcode < 35) {
                if (groundActions == null) {
                    groundActions = new String[5];
                }

                groundActions[opcode - 30] = buffer.readString();

                if (groundActions[opcode - 30].equalsIgnoreCase("hidden")) {
                    groundActions[opcode - 30] = null;
                }
            } else if (opcode >= 35 && opcode < 40) {
                if (actions == null) {
                    actions = new String[5];
                }

                actions[opcode - 35] = buffer.readString();
            } else if (opcode == 40) {
                int size = buffer.readUnsignedByte();
                modifiedModelColors = new int[size];
                originalModelColors = new int[size];

                for (int k = 0; k < size; k++) {
					originalModelColors[k] = buffer.readUnsignedShort();
					modifiedModelColors[k] = buffer.readUnsignedShort();
                }
            } else if (opcode == 41) {
                int size = buffer.readUnsignedByte();
				modifiedTexture = new int[size];
				originalTexture = new int[size];

				for (int k = 0; k < size; k++) {
					modifiedTexture[k] = buffer.readUnsignedShort();
					originalTexture[k] = buffer.readUnsignedShort();
				}
            } else if (opcode == 42) {
                buffer.readUnsignedByte();
            } else if (opcode == 65) {
            } else if (opcode == 78) {
                anInt185 = buffer.readUnsignedShort();
            } else if (opcode == 79) {
                anInt162 = buffer.readUnsignedShort();
            } else if (opcode == 90) {
                maleDialogue = buffer.readUnsignedShort();
            } else if (opcode == 91) {
                femaleDialogue = buffer.readUnsignedShort();
            } else if (opcode == 92) {
                anInt166 = buffer.readUnsignedShort();
            } else if (opcode == 93) {
                anInt173 = buffer.readUnsignedShort();
            } else if (opcode == 95) {
                modelOffsetX = buffer.readUnsignedShort();
            } else if (opcode == 97) {
                certID = buffer.readUnsignedShort();
            } else if (opcode == 98) {
                certTemplateID = buffer.readUnsignedShort();
            } else if (opcode >= 100 && opcode < 110) {
                if (stackIDs == null) {
                    stackIDs = new int[10];
                    stackAmounts = new int[10];
                }

                stackIDs[opcode - 100] = buffer.readUnsignedShort();
                stackAmounts[opcode - 100] = buffer.readUnsignedShort();
            } else if (opcode == 110) {
                anInt167 = buffer.readUnsignedShort();
            } else if (opcode == 111) {
                anInt192 = buffer.readUnsignedShort();
            } else if (opcode == 112) {
                anInt191 = buffer.readUnsignedShort();
            } else if (opcode == 113) {
                anInt196 = buffer.readtSignedByte();
            } else if (opcode == 114) {
                anInt184 = buffer.readtSignedByte() * 5;
            } else if (opcode == 115) {
                team = buffer.readUnsignedByte();
            } else if (opcode == 139) {
                buffer.readUnsignedShort();
            } else if (opcode == 140) {
                buffer.readUnsignedShort();
            } else if (opcode == 148) {
                buffer.readUnsignedShort();
            } else if (opcode == 149) {
                buffer.readUnsignedShort();
            } else if (opcode == 249) {
                int length = buffer.readUnsignedByte();
                for (int i = 0; i < length; i++) {
                    boolean isString = buffer.readUnsignedByte() == 1;
                    buffer.read24BitInt();
                    if (isString) {
                       buffer.readString();
                    } else {
                        buffer.readInt();
                    }
                }
            } else {
                System.out.println("Unknown item opcode: " + opcode);
            }
        } while (true);
    }

	private void setDefaults() {
		modelID = 0;
		name = null;
		description = null;
		originalModelColors = null;
		modifiedModelColors = null;
		originalTexture = null;
		modifiedTexture = null;
		modelZoom = 2000;
		modelRotation1 = 0;
		modelRotation2 = 0;
		modelOffsetX = 0;
		modelOffset1 = 0;
		modelOffsetY = 0;
		stackable = false;
		value = 1;
		membersObject = false;
		groundActions = null;
		actions = null;
		lendID = -1;
		lendTemplateID = -1;
		maleWearId = -1;
		anInt188 = -1;
		femaleWearId = -1;
		anInt164 = -1;
		anInt185 = -1;
		anInt162 = -1;
		maleDialogue = -1;
		anInt166 = -1;
		femaleDialogue = -1;
		anInt173 = -1;
		stackIDs = null;
		stackAmounts = null;
		certID = -1;
		certTemplateID = -1;
		anInt167 = 128;
		anInt192 = 128;
		anInt191 = 128;
		anInt196 = 0;
		anInt184 = 0;
		team = 0;
		femaleWieldY = 0;
		femaleWieldX = 0;
		femaleWieldZ = 0;
		maleWieldX = 0;
		maleWieldZ = 0;
		maleWieldY = 0;
	}

	private void toLend() {
		ItemDefinition itemDef = get(lendTemplateID);
		actions = new String[5];
		modelID = itemDef.modelID;
		modelOffset1 = itemDef.modelOffset1;
		modelRotation2 = itemDef.modelRotation2;
		modelOffsetY = itemDef.modelOffsetY;
		modelZoom = itemDef.modelZoom;
		modelRotation1 = itemDef.modelRotation1;
		modelOffsetX = itemDef.modelOffsetX;
		value = 0;
		ItemDefinition definition = get(lendID);
		anInt166 = definition.anInt166;
		originalModelColors = definition.originalModelColors;
		anInt185 = definition.anInt185;
		femaleWearId = definition.femaleWearId;
		anInt173 = definition.anInt173;
		maleDialogue = definition.maleDialogue;
		groundActions = definition.groundActions;
		maleWearId = definition.maleWearId;
		name = definition.name;
		anInt188 = definition.anInt188;
		membersObject = definition.membersObject;
		femaleDialogue = definition.femaleDialogue;
		anInt164 = definition.anInt164;
		anInt162 = definition.anInt162;
		modifiedModelColors = definition.modifiedModelColors;
		team = definition.team;

		if (definition.actions != null) {
			for (int i = 0; i < 4; i++) {
				actions[i] = definition.actions[i];
			}
		}

		actions[4] = "Discard";
	}

	private void toNote() {
		ItemDefinition definition = get(certTemplateID);
		modelID = definition.modelID;
		modelZoom = definition.modelZoom;
		modelRotation1 = definition.modelRotation1;
		modelRotation2 = definition.modelRotation2;
		modelOffsetX = definition.modelOffsetX;
		modelOffset1 = definition.modelOffset1;
		modelOffsetY = definition.modelOffsetY;
		modifiedModelColors = definition.modifiedModelColors;
		originalModelColors = definition.originalModelColors;
		definition = get(certID);
		name = definition.name;
		membersObject = definition.membersObject;
		value = definition.value;
		String s = "a";
		char c = definition.name.charAt(0);

		if (c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U') {
			s = "an";
		}

		description = ("Swap this note at any bank for " + s + " " + definition.name + ".").getBytes();
		stackable = true;
	}
	
	public static void printDefinitionsForId(int itemId) {
		/*Print out Grain*/
		ItemDefinition dumpitem = ItemDefinition.get(itemId);
		if (dumpitem.name != null) {
			System.out.println("Dumping: "+dumpitem.name);
		} else {
			System.out.println("ItemDefinition.get("+itemId+").name == null");
		}
		System.out.println("itemId: "+dumpitem.id);
		System.out.println("modelId: "+dumpitem.modelID);
		System.out.println("maleWearId: "+dumpitem.maleWearId);
		System.out.println("femaleWearId: "+dumpitem.femaleWearId);
		System.out.println("modelOffset1: "+dumpitem.modelOffset1);
		System.out.println("modelOffSetX: "+dumpitem.modelOffsetX);
		System.out.println("modelOffSetY: "+dumpitem.modelOffsetY);
		System.out.println("modelRotation1: "+dumpitem.modelRotation1);
		System.out.println("modelRotation2: "+dumpitem.modelRotation2);
		System.out.println("modelZoom: "+dumpitem.modelZoom);
		//System.out.println("def "+dumpitem);
		if (dumpitem.modifiedModelColors != null) {
			for (int i = 0; i < dumpitem.modifiedModelColors.length; i++) {
				System.out.println("modifiedModelColors[" + i + "]: " + dumpitem.modifiedModelColors[i]);
			}
		}
		if (dumpitem.originalModelColors != null) {
			for (int i = 0; i < dumpitem.originalModelColors.length; i++) {
				System.out.println("originalModelColors[" + i + "]: " + dumpitem.originalModelColors[i]);
			}
		}
		if (dumpitem.actions != null) {
			for (int i = 0; i < dumpitem.actions.length; i++) {
				System.out.println("Action[" + i + "]: " + dumpitem.actions[i]);
			}
		}
		if (dumpitem.groundActions != null) {
			for (int i = 0; i < dumpitem.groundActions.length; i++) {
				System.out.println("groundAction[" + i + "]: " + dumpitem.groundActions[i]);
			}
		}
	}

}