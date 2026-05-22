package org.necrotic.client;

import org.necrotic.client.io.ByteBuffer;

public final class SkinList {

	public SkinList(ByteBuffer stream) {
		int amt = stream.readUnsignedShort();
		opcodes = new int[amt];
		skinList = new int[amt][];
		for (int j = 0; j < amt; j++)
			opcodes[j] = stream.readUnsignedShort();

		for (int j = 0; j < amt; j++)
			skinList[j] = new int[stream.readUnsignedShort()];

		for (int j = 0; j < amt; j++)
			for (int l = 0; l < skinList[j].length; l++)
				skinList[j][l] = stream.readUnsignedShort();
	}

	public final int[] opcodes;
	public final int[][] skinList;
}
