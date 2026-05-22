package org.necrotic.client.cache.definition;

import org.necrotic.client.Signlink;
import org.necrotic.client.cache.Archive;
import org.necrotic.client.cache.DataType;
import org.necrotic.client.io.ByteBuffer;

public final class VarBit {

	private static VarBit[][] cache;

	public static void unpackConfig(Archive archive) {
		cache = new VarBit[2][];

		ByteBuffer buffer = new ByteBuffer(archive.get("varbit.dat"));
		int size = 5133;
		cache[0] = new VarBit[size];

		for (int i = 0; i < size; i++) {
			if (cache[0][i] == null) {
				cache[0][i] = new VarBit();
			}

			cache[0][i].readValues(buffer);
		}

		if (buffer.position != buffer.buffer.length) {
			System.out.println("varbit load mismatch (667)");
		}

		for(int i = 643; i <= 656; i++) {
			cache[0][i].configId = 286;
			cache[0][i].lsb = 14 + (i - 643);
			cache[0][i].msb = 14 + (i - 643);//try
		}

		buffer = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/varbit.dat"));
		int length = buffer.readUnsignedShort();
		cache[1] = new VarBit[length];
		for (int i = 0; i < length; i++) {
			if (cache[1][i] == null) {
				cache[1][i] = new VarBit();
			}

			cache[1][i].readValuesOsrs(buffer);
		}

		if (buffer.position != buffer.buffer.length) {
			System.out.println("varbit load mismatch (osrs)");
		}
	}

	public static VarBit[] getCache(DataType dataType) {
		return dataType == DataType.OSRS ? cache[1] : cache[0];
	}

	public int configId;
	public int lsb;
	public int msb;

	private VarBit() {
	}

	private void readValuesOsrs(ByteBuffer buffer) {
		configId = buffer.readUnsignedShort();
		lsb = buffer.readUnsignedByte();
		msb = buffer.readUnsignedByte();
	}

	private void readValues(ByteBuffer buffer) {
		do {
			int opcode = buffer.readUnsignedByte();

			if (opcode == 0) {
				return;
			}

			if (opcode == 1) {
				configId = buffer.readUnsignedShort();
				lsb = buffer.readUnsignedByte();
				msb = buffer.readUnsignedByte();
			} else if (opcode == 10) {
				buffer.readString();
			} else if (opcode == 3) {
				buffer.readIntLittleEndian();
			} else if (opcode == 4) {
				buffer.readIntLittleEndian();
			} else if (opcode != 2) {
				System.out.println("Error unrecognised config code: " + opcode);
			}
		} while (true);
	}
}