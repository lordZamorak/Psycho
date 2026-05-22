package org.necrotic.client.cache.definition;

import org.necrotic.client.cache.Archive;
import org.necrotic.client.io.ByteBuffer;

public final class Varp {

	private static Varp[] cache;
	private static int anInt702;
	private static int[] anIntArray703;

	public static void load(Archive archive) {
		ByteBuffer buffer = new ByteBuffer(archive.get("varp.dat"));
		int size = buffer.readUnsignedShort();
		anInt702 = 0;

		if (getCache() == null) {
			setCache(new Varp[size]);
		}

		if (anIntArray703 == null) {
			anIntArray703 = new int[size];
		}

		for (int j = 0; j < size; j++) {
			if (getCache()[j] == null) {
				getCache()[j] = new Varp();
			}

			getCache()[j].readValues(buffer, j);
		}

		if (buffer.position != buffer.buffer.length) {
			System.out.println("varptype load mismatch");
		}
	}

	private int anInt709;

	private Varp() {
	}

	private void readValues(ByteBuffer stream, int i) {
		do {
			int opcode = stream.readUnsignedByte();

			if (opcode == 0) {
				return;
			}
			if (opcode == 1) {
				stream.readUnsignedByte();
			} else if (opcode == 2) {
				stream.readUnsignedByte();
			} else if (opcode == 3) {
				anIntArray703[anInt702++] = i;
			} else if (opcode == 5) {
				setAnInt709(stream.readUnsignedShort());
			} else if (opcode == 7) {
				stream.readIntLittleEndian();
			} else if (opcode == 8) {
			} else if (opcode == 10) {
				stream.readString();
			} else if (opcode == 11) {
			} else if (opcode == 12) {
				stream.readIntLittleEndian();
			} else {
				System.out.println("Error unrecognised config code: " + opcode);
			}
		} while (true);
	}

	public static Varp[] getCache() {
		return cache;
	}

	public static void setCache(Varp[] cache) {
		Varp.cache = cache;
	}

	public int getAnInt709() {
		return anInt709;
	}

	public void setAnInt709(int anInt709) {
		this.anInt709 = anInt709;
	}

}