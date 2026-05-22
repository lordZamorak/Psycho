package org.necrotic.client.cache.definition;

import java.util.HashSet;
import java.util.stream.IntStream;

import com.sun.org.apache.bcel.internal.generic.FieldGen;
import org.apache.commons.lang3.math.NumberUtils;
import org.necrotic.Configuration;
import org.necrotic.client.Client;
import org.necrotic.client.FrameReader;
import org.necrotic.client.List;
import org.necrotic.client.Signlink;
import org.necrotic.client.cache.Archive;
import org.necrotic.client.cache.DataType;
import org.necrotic.client.graphics.rsinterface.Settings;
import org.necrotic.client.io.ByteBuffer;
import org.necrotic.client.tools.FieldGenerator;
import org.necrotic.client.tools.Misc;
import org.necrotic.client.tools.TempWriter;
import org.necrotic.client.world.Model;

public final class MobDefinition {

	private static MobDefinition[] cache;
	private static int cacheIndex;
	public static Client clientInstance;
	public static List mruNodes = new List(80);
	private static ByteBuffer[] buffer = new ByteBuffer[2];
	private static int[][] streamIndices = new int[2][];

	public static void load(Archive archive) {
		int index = 0;
		buffer[index] = new ByteBuffer(archive.get("npc.dat"));
		ByteBuffer stream = new ByteBuffer(archive.get("npc.idx"));
		int totalNPCs = stream.readUnsignedShort();
		streamIndices[index] = new int[totalNPCs];
		int position = 2;
		for (int i = 0; i < totalNPCs; i++) {
			streamIndices[index][i] = position;
			position += stream.readUnsignedShort();
		}
		System.out.println("Loaded " + totalNPCs + " 667 npcs.");

		index = 1;
		buffer[index] = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/npc.dat"));
		stream = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/npc.idx"));
		totalNPCs = stream.readUnsignedShort();
		streamIndices[index] = new int[totalNPCs];
		position = 2;
		for (int i = 0; i < totalNPCs; i++) {
			streamIndices[index][i] = position;
			position += stream.readUnsignedShort();
		}

		cache = new MobDefinition[20];
		for (int i = 0; i < 20; i++) {
			cache[i] = new MobDefinition();
		}
		System.out.println("Loaded " + totalNPCs + " OSRS npcs.");

		if (Configuration.developerMode) {
			TempWriter writer = new TempWriter("npc_ids");
			IntStream.range(0, 100_000).forEach(id -> {
				try {
					MobDefinition def = MobDefinition.get(id);
					writer.writeLine(def.id + " " + def.name);
				} catch (Exception e) {}
			});
			writer.close();

			TempWriter writer2 = new TempWriter("npc_fields");
			FieldGenerator generator = new FieldGenerator(writer2::writeLine);
			IntStream.range(0, 100_000).forEach(id -> {
				try {
					MobDefinition definition = MobDefinition.get(id);
					generator.add(definition.name, id, definition.dataType == DataType.OSRS);
				} catch (Exception e) {}
			});
			writer2.close();
		}
	}

	public static MobDefinition get(int id) {
		for (int i = 0; i < 20; i++) {
			if (cache[i].id == id) {
				return cache[i];
			}
		}

		cacheIndex = (cacheIndex + 1) % 20;
		MobDefinition definition = cache[cacheIndex] = new MobDefinition();

		int index = 0;
		if (id >= 30_000) {
			id -= 30_000;
			index = 1;
			definition.dataType = DataType.OSRS;
		} else {
			definition.dataType = DataType.STANDARD;
		}

		buffer[index].position = streamIndices[index][id];
		definition.id = id + (index == 1 ? 30_000 : 0);
		definition.doReadValues(buffer[index]);

		if (definition.dataType == DataType.STANDARD) {
			MobDefinitionCustom.do667(definition);
		} else {
			if (definition.standAnimation != -1)
				definition.standAnimation += 30_000;
			if (definition.walkAnimation != -1)
				definition.walkAnimation += 30_000;
			if (definition.walkingBackwardsAnimation != -1)
				definition.walkingBackwardsAnimation += 30_000;
			if (definition.walkLeftAnimation != -1)
				definition.walkLeftAnimation += 30_000;
			if (definition.walkRightAnimation != -1)
				definition.walkRightAnimation += 30_000;
		}

		return definition;
	}

	public void copy(int id) {
		MobDefinition other = get(id);
		changedModelColours = other.changedModelColours.clone();
		childrenIDs = other.childrenIDs.clone();
		combatLevel = other.combatLevel;
		configChild = other.configChild;
		degreesToTurn = other.degreesToTurn;
		description = other.description;
		dialogueModels = other.dialogueModels;
		disableRightClick = false;
		drawYellowDotOnMap = other.drawYellowDotOnMap;
		headIcon = other.headIcon;
		modelLightning = other.modelLightning;
		modelShadowing = other.modelShadowing;
		npcModels = other.npcModels.clone();
		originalModelColours = other.originalModelColours.clone();
		standAnimation = other.standAnimation;
		varBitChild = other.varBitChild;
		visibilityOrRendering = other.visibilityOrRendering;
		walkAnimation = other.walkAnimation;
		walkingBackwardsAnimation = other.walkingBackwardsAnimation;
		walkLeftAnimation = other.walkLeftAnimation;
		walkRightAnimation = other.walkRightAnimation;
	}

	public static void nullify() {
		mruNodes = null;
		streamIndices = null;
		cache = null;
		buffer = null;
	}

	public String[] actions;
	int adjustVertextPointsXOrY;
	int adjustVertextPointZ;
	int[] changedModelColours;
	public int[] childrenIDs;
	public int combatLevel;
	int configChild;
	public int degreesToTurn;
	public byte[] description;
	private int[] dialogueModels;
	public boolean disableRightClick;
	public boolean drawYellowDotOnMap;
	public int headIcon;
	int modelLightning;
	int modelShadowing;
	public String name;
	public int[] npcModels;
	public byte npcSizeInSquares;
	int[] originalModelColours;
	public int standAnimation;
	public int id;
	int varBitChild;
	public boolean visibilityOrRendering;
	public int walkAnimation;
	public int walkingBackwardsAnimation;
	public int walkLeftAnimation;
	public int walkRightAnimation;
	public DataType dataType;

	private MobDefinition() {
		walkRightAnimation = -1;
		varBitChild = -1;
		walkingBackwardsAnimation = -1;
		configChild = -1;
		combatLevel = -1;
		walkAnimation = -1;
		npcSizeInSquares = 1;
		headIcon = -1;
		standAnimation = -1;
		id = -1;
		degreesToTurn = 32;
		walkLeftAnimation = -1;
		disableRightClick = true;
		adjustVertextPointZ = 128;
		drawYellowDotOnMap = true;
		adjustVertextPointsXOrY = 128;
		visibilityOrRendering = false;
	}

	public Model method160() {
		if (childrenIDs != null) {
			MobDefinition definition = method161();

			if (definition == null) {
				return null;
			} else {
				return definition.method160();
			}
		}

		if (dialogueModels == null) {
			return null;
		}

		boolean flag1 = false;

		for (int i = 0; i < dialogueModels.length; i++) {
			if (!Model.method463(dialogueModels[i], dataType)) {
				flag1 = true;
			}
		}

		if (flag1) {
			return null;
		}

		Model aclass30_sub2_sub4_sub6s[] = new Model[dialogueModels.length];

		for (int j = 0; j < dialogueModels.length; j++) {
			aclass30_sub2_sub4_sub6s[j] = Model.fetchModel(dialogueModels[j], dataType);
		}

		Model model;

		if (aclass30_sub2_sub4_sub6s.length == 1) {
			model = aclass30_sub2_sub4_sub6s[0];
		} else {
			model = new Model(aclass30_sub2_sub4_sub6s.length, aclass30_sub2_sub4_sub6s);
		}

		if (originalModelColours != null) {
			for (int k = 0; k < originalModelColours.length; k++) {
				model.method476(originalModelColours[k], changedModelColours[k]);
			}
		}

		return model;
	}

	public MobDefinition method161() {
		int j = -1;

		try {
			if (varBitChild != -1) {
				VarBit varBit = VarBit.getCache(dataType)[varBitChild];
				int k = varBit.configId;
				int l = varBit.lsb;
				int i1 = varBit.msb;
				int j1 = Client.anIntArray1232[i1 - l];
				// System.out.println("k: " + k + " l: " + l);
				j = clientInstance.variousSettings[k] >> l & j1;
			} else if (configChild != -1) {
				j = clientInstance.variousSettings[configChild];
			}
		} catch (Exception e) {
			e.printStackTrace();
		}

		if (j < 0 || j >= childrenIDs.length || childrenIDs[j] == -1) {
			return null;
		} else {
			return get(childrenIDs[j]);
		}
	}

	public Model method164(int j, int frame, int ai[], int nextFrame, int cycle1, int cycle2) {
		if (childrenIDs != null) {
			MobDefinition entityDef = method161();

			if (entityDef == null) {
				return null;
			} else {
				return entityDef.method164(j, frame, ai, nextFrame, cycle1, cycle2);
			}
		}

		Model model = (Model) mruNodes.insertFromCache(id);

		if (model == null) {
			boolean flag = false;

			for (int i1 = 0; i1 < npcModels.length; i1++) {
				if (!Model.method463(npcModels[i1], dataType)) {
					flag = true;
				}
			}

			if (flag) {
				return null;
			}

			Model aclass30_sub2_sub4_sub6s[] = new Model[npcModels.length];

			for (int j1 = 0; j1 < npcModels.length; j1++) {
				aclass30_sub2_sub4_sub6s[j1] = Model.fetchModel(npcModels[j1], dataType);
			}

			if (aclass30_sub2_sub4_sub6s.length == 1) {
				model = aclass30_sub2_sub4_sub6s[0];
			} else {
				model = new Model(aclass30_sub2_sub4_sub6s.length, aclass30_sub2_sub4_sub6s);
			}

			if (originalModelColours != null) {
				for (int k1 = 0; k1 < originalModelColours.length; k1++) {
					model.method476(originalModelColours[k1], changedModelColours[k1]);
				}
			}

			model.createBones();
			model.light(84 + modelLightning, 1000 + modelShadowing, -90, -580, -90, true);
			mruNodes.removeFromCache(model, id);
		}

		Model model_1 = Model.EMPTY_MODEL;
		model_1.method464(model, FrameReader.isNullFrame(frame) & FrameReader.isNullFrame(j));
/*
		if (frame != -1 && j != -1) {
			model_1.method471(ai, j, frame);
		} else if (frame != -1 && !Configuration.TWEENING_ENABLED) {
			model_1.applyTransform(frame);
		} else if (frame != -1 && nextFrame != -1 && Configuration.TWEENING_ENABLED) {
			model_1.interpolateFrames(frame, nextFrame, cycle1, cycle2);
		}*/
		
		if (frame != -1 && j != -1)
			model_1.method471(ai, j, frame);
		else if (frame != -1 && nextFrame != -1 && Settings.get(Settings.Data.TWEENING))
			model_1.interpolateFrames(frame, nextFrame, cycle1, cycle2);
		else if (frame != -1)
			model_1.applyTransform(frame);
		
		if (adjustVertextPointsXOrY != 128 || adjustVertextPointZ != 128) {
			model_1.scaleT(adjustVertextPointsXOrY, adjustVertextPointsXOrY, adjustVertextPointZ);
		}

		model_1.method466();
		model_1.face_skin = null;
		model_1.vertex_skin = null;

		if (npcSizeInSquares == 1) {
			model_1.renders_inside_tile = true;
		}

		return model_1;
	}

	private void doReadValues(ByteBuffer buffer) {
		if (dataType == DataType.OSRS) {
			readValuesOsrs(buffer);
		} else {
			readValues(buffer);
		}
	}

	private void readValues(ByteBuffer buffer) {
		do {
			final int opcode = buffer.readUnsignedByte();

			if (opcode == 0) {
				return;
			}

			if (opcode == 1) {
				int j = buffer.readUnsignedByte();
				npcModels = new int[j];

				for (int j1 = 0; j1 < j; j1++) {
					npcModels[j1] = buffer.readUnsignedShort();
				}
			} else if (opcode == 2) {
				name = buffer.readString();
			} else if (opcode == 3) {
				description = buffer.readBytes();
			} else if (opcode == 12) {
				npcSizeInSquares = buffer.readtSignedByte();
			} else if (opcode == 13) {
				standAnimation = buffer.readUnsignedShort();
			} else if (opcode == 14) {
				walkAnimation = buffer.readUnsignedShort();
			} else if (opcode == 17) {
				walkAnimation = buffer.readUnsignedShort();
				walkingBackwardsAnimation = buffer.readUnsignedShort();
				walkLeftAnimation = buffer.readUnsignedShort();
				walkRightAnimation = buffer.readUnsignedShort();

				if (walkAnimation == 65535) {
					walkAnimation = -1;
				}

				if (walkingBackwardsAnimation == 65535) {
					walkingBackwardsAnimation = -1;
				}

				if (walkLeftAnimation == 65535) {
					walkLeftAnimation = -1;
				}

				if (walkRightAnimation == 65535) {
					walkRightAnimation = -1;
				}
			} else if (opcode >= 30 && opcode < 40) {
				if (actions == null) {
					actions = new String[5];
				}

				actions[opcode - 30] = buffer.readString();

				if (actions[opcode - 30].equalsIgnoreCase("hidden")) {
					actions[opcode - 30] = null;
				}
			} else if (opcode == 40) {
				int length = buffer.readUnsignedByte();
				changedModelColours = new int[length];
				originalModelColours = new int[length];

				for (int i = 0; i < length; i++) {
					originalModelColours[i] = buffer.readUnsignedShort();
					changedModelColours[i] = buffer.readUnsignedShort();
				}
			} else if (opcode == 60) {
				int length = buffer.readUnsignedByte();
				dialogueModels = new int[length];

				for (int i = 0; i < length; i++) {
					dialogueModels[i] = buffer.readUnsignedShort();
				}
			} else if (opcode == 90) {
				buffer.readUnsignedShort();
			} else if (opcode == 91) {
				buffer.readUnsignedShort();
			} else if (opcode == 92) {
				buffer.readUnsignedShort();
			} else if (opcode == 93) {
				drawYellowDotOnMap = false;
			} else if (opcode == 95) {
				combatLevel = buffer.readUnsignedShort();
			} else if (opcode == 97) {
				adjustVertextPointsXOrY = buffer.readUnsignedShort();
			} else if (opcode == 98) {
				adjustVertextPointZ = buffer.readUnsignedShort();
			} else if (opcode == 99) {
				visibilityOrRendering = true;
			} else if (opcode == 100) {
				modelLightning = buffer.readtSignedByte();
			} else if (opcode == 101) {
				modelShadowing = buffer.readtSignedByte() * 5;
			} else if (opcode == 102) {
				headIcon = buffer.readUnsignedShort();
			} else if (opcode == 103) {
				degreesToTurn = buffer.readUnsignedShort();
			} else if (opcode == 106) {
				varBitChild = buffer.readUnsignedShort();

				if (varBitChild == 65535) {
					varBitChild = -1;
				}

				configChild = buffer.readUnsignedShort();

				if (configChild == 65535) {
					configChild = -1;
				}

				int length = buffer.readUnsignedByte();
				childrenIDs = new int[length + 1];

				for (int i = 0; i <= length; i++) {
					childrenIDs[i] = buffer.readUnsignedShort();

					if (childrenIDs[i] == 65535) {
						childrenIDs[i] = -1;
					}
				}
			} else if (opcode == 107) {
				disableRightClick = false;
			}
		} while (true);
	}

	private void readValuesOsrs(ByteBuffer buffer) {
		do {
			final int opcode = buffer.readUnsignedByte();

			if (opcode == 0) {
				return;
			}

			if (opcode == 1) {
				int j = buffer.readUnsignedByte();
				npcModels = new int[j];

				for (int j1 = 0; j1 < j; j1++) {
					npcModels[j1] = buffer.readUnsignedShort();
				}
			} else if (opcode == 2) {
				name = buffer.readString();
			} else if (opcode == 3) {
				description = buffer.readBytes();
			} else if (opcode == 12) {
				npcSizeInSquares = buffer.readtSignedByte();
			} else if (opcode == 13) {
				standAnimation = buffer.readUnsignedShort();
			} else if (opcode == 14) {
				walkAnimation = buffer.readUnsignedShort();
			} else if (opcode == 17) {
				walkAnimation = buffer.readUnsignedShort();
				walkingBackwardsAnimation = buffer.readUnsignedShort();
				walkLeftAnimation = buffer.readUnsignedShort();
				walkRightAnimation = buffer.readUnsignedShort();

				if (walkAnimation == 65535) {
					walkAnimation = -1;
				}

				if (walkingBackwardsAnimation == 65535) {
					walkingBackwardsAnimation = -1;
				}

				if (walkLeftAnimation == 65535) {
					walkLeftAnimation = -1;
				}

				if (walkRightAnimation == 65535) {
					walkRightAnimation = -1;
				}
			} else if (opcode >= 30 && opcode < 40) {
				if (actions == null) {
					actions = new String[5];
				}

				actions[opcode - 30] = buffer.readString();

				if (actions[opcode - 30].equalsIgnoreCase("hidden")) {
					actions[opcode - 30] = null;
				}
			} else if (opcode == 40) {
				int length = buffer.readUnsignedByte();
				changedModelColours = new int[length];
				originalModelColours = new int[length];

				for (int i = 0; i < length; i++) {
					originalModelColours[i] = buffer.readUnsignedShort();
					changedModelColours[i] = buffer.readUnsignedShort();
				}
			} else if (opcode == 60) {
				int length = buffer.readUnsignedByte();
				dialogueModels = new int[length];

				for (int i = 0; i < length; i++) {
					dialogueModels[i] = buffer.readUnsignedShort();
				}
			} else if (opcode == 90) {
				buffer.readUnsignedShort();
			} else if (opcode == 91) {
				buffer.readUnsignedShort();
			} else if (opcode == 92) {
				buffer.readUnsignedShort();
			} else if (opcode == 93) {
				drawYellowDotOnMap = false;
			} else if (opcode == 95) {
				combatLevel = buffer.readUnsignedShort();
			} else if (opcode == 97) {
				adjustVertextPointsXOrY = buffer.readUnsignedShort();
			} else if (opcode == 98) {
				adjustVertextPointZ = buffer.readUnsignedShort();
			} else if (opcode == 99) {
				visibilityOrRendering = true;
			} else if (opcode == 100) {
				modelLightning = buffer.readtSignedByte();
			} else if (opcode == 101) {
				modelShadowing = buffer.readtSignedByte() * 5;
			} else if (opcode == 102) {
				headIcon = buffer.readUnsignedShort();
			} else if (opcode == 103) {
				degreesToTurn = buffer.readUnsignedShort();
			} else if (opcode == 106) {
				varBitChild = buffer.readUnsignedShort();

				if (varBitChild == 65535) {
					varBitChild = -1;
				}

				configChild = buffer.readUnsignedShort();

				if (configChild == 65535) {
					configChild = -1;
				}

				int length = buffer.readUnsignedByte();
				childrenIDs = new int[length + 1];

				for (int i = 0; i <= length; i++) {
					childrenIDs[i] = buffer.readUnsignedShort();

					if (childrenIDs[i] == 65535) {
						childrenIDs[i] = -1;
					}
				}
			} else if (opcode == 107) {
				disableRightClick = false;
			} else if (opcode == 118) {
				buffer.readUnsignedShort();
				buffer.readUnsignedShort();
				buffer.readUnsignedShort();
				int length = buffer.readUnsignedByte();
				for (int index = 0; index <= length; ++index)
				{
					buffer.readUnsignedShort();
				}
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
				//System.err.println("Unknown npc opcode: " + opcode);
			}
		} while (true);
	}
	
	public static void printDefinitionsForId(int mobId) {
		/*Print out Grain*/
		MobDefinition dump = MobDefinition.get(mobId);
		if (dump.name != null) {
			System.out.println("Dumping: "+dump.name);
		} else {
			System.out.println("MobDefinition.get("+mobId+").name == null");
		}
		System.out.println("combatlevel: "+dump.combatLevel);
		System.out.println("id: "+dump.id);
		if (dump.npcModels != null) {
			for (int i = 0; i < dump.npcModels.length; i++) {
				System.out.println("npcModels[" + i + "]: " + dump.npcModels[i]);
			}
		}
		if (dump.actions != null) {
			for (int i = 0; i < dump.actions.length; i++) {
				System.out.println("Action[" + i + "]: " + dump.actions[i]);
			}
		}
		System.out.println("degreesToTurn: "+dump.degreesToTurn);
		System.out.println("headIcon: "+dump.headIcon);
		System.out.println("npcSizeInSquares: "+dump.npcSizeInSquares);
		System.out.println("standAnimation: "+dump.standAnimation);
		System.out.println("walkAnimation: "+dump.walkAnimation);
		System.out.println("walkingBackwardsAnimation: "+dump.walkingBackwardsAnimation);
		System.out.println("walkLeftAnimation: "+dump.walkLeftAnimation);
		System.out.println("walkRightAnimation: "+dump.walkRightAnimation);
		if (dump.originalModelColours != null) {
			for (int i = 0; i < dump.originalModelColours.length; i++) {
				System.out.println("originalModelColours[" + i + "]: " + dump.originalModelColours[i]);
			}
		}
		if (dump.changedModelColours != null) {
			for (int i = 0; i < dump.changedModelColours.length; i++) {
				System.out.println("changedModelColours[" + i + "]: " + dump.changedModelColours[i]);
			}
		}
		if (dump.childrenIDs != null) {
			for (int i = 0; i < dump.childrenIDs.length; i++) {
				System.out.println("childrenIDs[" + i + "]: " + dump.changedModelColours[i]);
			}
		}
		System.out.println("configChild: "+dump.configChild);
		System.out.println("varBitChild: "+dump.varBitChild);
		System.out.println("modelLightning: "+dump.modelLightning);
		System.out.println("modelShadowing: "+dump.modelShadowing);
		System.out.println("drawYellowDotOnMap: "+dump.drawYellowDotOnMap);
		System.out.println("disableRightClick: "+dump.disableRightClick);
		System.out.println("visibilityOrRendering: "+dump.visibilityOrRendering);
		
		
	}

}