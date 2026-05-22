package org.necrotic.client.cache;

import java.io.BufferedInputStream;
import java.io.DataInputStream;
import java.io.File;
import java.io.FileInputStream;

import org.necrotic.client.bzip.BZIPDecompressor;
import org.necrotic.client.io.ByteBuffer;

public final class Archive {

	private final byte[] payload;
	private final int[] anIntArray730;
	private final int[] indiceTable;
	private final int dataSize;
	private final int[] hashTable;
	private final boolean isCompressed;
	private final int[] sizeTable;

	public Archive(byte[] data) {
		ByteBuffer buffer = new ByteBuffer(data);
		int decompressed = buffer.readTribyte();
		int compressed = buffer.readTribyte();
		isCompressed = decompressed != compressed;

		if (isCompressed) {
			byte[] tmp = new byte[decompressed];
			BZIPDecompressor.decompress(tmp, decompressed, data, compressed, 6);
			payload = tmp;
			buffer = new ByteBuffer(payload);
		} else {
			payload = data;
		}

		dataSize = buffer.readUnsignedShort();
		hashTable = new int[dataSize];
		sizeTable = new int[dataSize];
		anIntArray730 = new int[dataSize];
		indiceTable = new int[dataSize];
		int position = buffer.position + dataSize * 10;

		for (int i = 0; i < dataSize; i++) {
			hashTable[i] = buffer.readIntLittleEndian();
			sizeTable[i] = buffer.readTribyte();
			anIntArray730[i] = buffer.readTribyte();
			indiceTable[i] = position;
			position += anIntArray730[i];
		}
	}

	public static final byte[] ReadFile(String s) {
		try  {
			File file = new File(s);
			int i = (int)file.length();
			byte abyte0[] = new byte[i];
			DataInputStream datainputstream = new DataInputStream(new BufferedInputStream(new FileInputStream(s)));
			datainputstream.readFully(abyte0, 0, i);
			datainputstream.close();
			//TotalRead++;
			return abyte0;
		} catch(Exception exception) {
		}
		return null;
	}

	public byte[] get(String name) {
		byte[] data = null;
		int hash = getHash(name);
		for (int i = 0; i < dataSize; i++) {
			if (hashTable[i] == hash) {
				if (data == null) {
					data = new byte[sizeTable[i]];
				}

				if (!isCompressed) {
					BZIPDecompressor.decompress(data, sizeTable[i], payload, anIntArray730[i], indiceTable[i]);
				} else {
					System.arraycopy(payload, indiceTable[i], data, 0, sizeTable[i]);
				}

				return data;
			}
		}

		return null;
	}

	private int getHash(String name) {
		int hash = 0;
		name = name.toUpperCase();

		for (int i = 0; i < name.length(); i++) {
			hash = hash * 61 + name.charAt(i) - 32;
		}

		return hash;
	}

}