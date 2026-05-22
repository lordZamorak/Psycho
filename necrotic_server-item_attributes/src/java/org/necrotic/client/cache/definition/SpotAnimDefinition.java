package org.necrotic.client.cache.definition;

import org.necrotic.client.List;
import org.necrotic.client.Signlink;
import org.necrotic.client.cache.Archive;
import org.necrotic.client.cache.DataType;
import org.necrotic.client.io.ByteBuffer;
import org.necrotic.client.world.Model;

public final class SpotAnimDefinition {

	public static List list = new List(30);
	public static SpotAnimDefinition[] cache;

	public static void load(Archive archive) {
		ByteBuffer stream1 = new ByteBuffer(archive.get("spotanim.dat"));
		ByteBuffer stream2 = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/spotanim.dat"));

		int length1 = stream1.readUnsignedShort();
		int length2 = stream2.readUnsignedShort();

		if (cache == null) {
			cache = new SpotAnimDefinition[length1 + length2 + 30_0000];
		}

		for (int j = 0; j < length1; j++) {
			if (cache[j] == null)
				cache[j] = new SpotAnimDefinition();
			cache[j].id = j;
			cache[j].dataType = DataType.STANDARD;
			cache[j].readValues(stream1);
			loadCustom(j);
		}

		for (int j = 0; j < length2; j++) {
			int index = j + 30_000;
			if (cache[index] == null)
				cache[index] = new SpotAnimDefinition();
			cache[index].id = index;
			cache[index].dataType = DataType.OSRS;
			cache[index].readValues(stream2);
		}

		custom();
		System.out.println("Loaded " + length1 + " 667 gfx.");
		System.out.println("Loaded " + length2 + " OSRS gfx.");
	}

	private static void loadCustom(int j) {
		switch (j) {
			case 2959:
				cache[2959].modelId = cache[2114].modelId;
				cache[2959].animationId = cache[2114].animationId;
				cache[2959].animation = cache[2114].animation;
				cache[2959].originalColours = new int[] {127, 49874, -1, -1, -1, -1};
				cache[2959].destColours = new int[] {0, 100, -1, -1, -1, -1};
				//cache[2959].modelId = cache[2114].modelId;
				break;
		}
	}

	private static void custom() {
		cache[2274].modelId = cache[2281].modelId;
		cache[2274].animationId = cache[2281].animationId;
		cache[2274].rotation = 90;
		cache[2274].animation = cache[2281].animation;
	}

	public Animation animation;
	private int id;
	public int modelId;
	private int animationId;
	public int sizeXY;
	public int sizeZ;
	public int rotation;
	public int shadow;
	public int lightness;
	private int[] originalColours;
	private  int[] destColours;
	private DataType dataType = DataType.STANDARD;

	private SpotAnimDefinition() {
		animationId = -1;
		originalColours = new int[6];
		destColours = new int[6];
		sizeXY = 128;
		sizeZ = 128;
	}

	public Model getModel() {
		Model model = (Model) list.insertFromCache(id);

		if (model != null) {
			return model;
		}

		model = Model.fetchModel(modelId, dataType);

		if (model == null) {
			return null;
		}

		for (int i = 0; i < 6; i++) {
			if (originalColours[0] != 0) {
				model.method476(originalColours[i], destColours[i]);
			}
		}

		list.removeFromCache(model, id);
		return model;
	}

	private void readValues(ByteBuffer stream) {
        do {
            int i = stream.readUnsignedByte();
            if (i == 0)
                return;
            if (i == 1) {
                modelId = stream.readUnsignedShort();
            }
            else if (i == 2) {
                animationId = stream.readUnsignedShort() + (dataType == DataType.OSRS ? 30_000 : 0);
                if (Animation.cache != null) {
					animation = Animation.cache[animationId];
				}
            } else if (i == 4)
                sizeXY = stream.readUnsignedShort();
            else if (i == 5)
                sizeZ = stream.readUnsignedShort();
            else if (i == 6)
                rotation = stream.readUnsignedShort();
            else if (i == 7)
                shadow = stream.readUnsignedByte();
            else if (i == 8)
                lightness = stream.readUnsignedByte();
            else if (i == 40) {
                int j = stream.readUnsignedByte();
                for (int k = 0; k < j; k++) {
                    originalColours[k] = stream.readUnsignedShort();
                    destColours[k] = stream.readUnsignedShort();
                }
            } else
                System.out.println("Error unrecognised spotanim config code: " + i);
        } while (true);
    }
}