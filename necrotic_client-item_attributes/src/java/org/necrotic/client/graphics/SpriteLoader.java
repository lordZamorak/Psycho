package org.necrotic.client.graphics;

import java.io.ByteArrayInputStream;
import java.io.DataInputStream;
import java.io.File;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.nio.channels.FileChannel;
import java.util.zip.GZIPInputStream;

import org.necrotic.Configuration;
import org.necrotic.client.Signlink;
import org.necrotic.client.io.ByteBuffer;
import org.necrotic.client.tools.FileUtilities;

public class SpriteLoader {

	public static SpriteLoader[] cache;
	public static Sprite[] sprites = null;

	/**
	 * Creates a sprite out of the spriteData.
	 *
	 * @param sprite
	 */
	private static void createSprite(SpriteLoader sprite) {
		sprites[sprite.id] = new Sprite(sprite.spriteData);
		sprites[sprite.id].drawOffsetX = sprite.drawOffsetX;
		sprites[sprite.id].drawOffsetY = sprite.drawOffsetY;
	}

	/**
	 * Dump sprites.
	 */
	public static void main(String...args) {
		Configuration.developerMode = true;
		loadSprites();
		System.out.println("Dumping sprites..");
		File directory = new File(Signlink.getCacheDirectory() + "/sprites/");
		if(!directory.exists()) {
			directory.mkdir();
		}

		for (SpriteLoader sprite : cache) {
			String file = directory.getPath() + "/" + sprite.id + ".png";
			FileUtilities.WriteFile(file, sprite.spriteData);
			System.out.println("Writing image: " + file);
		}
	}

	/**
	 * Loads the sprite data and index files from the cache location. This can
	 * be edited to use an archive such as config or media to load from the
	 * cache.
	 */

	public static void loadSprites() {
		try {
			ByteBuffer index = new ByteBuffer(readFile(Signlink.getCacheDirectory() + "/" + "sprites.idx"));
			ByteBuffer data = new ByteBuffer(readFile(Signlink.getCacheDirectory() + "/" + "sprites.dat"));
			DataInputStream indexFile = new DataInputStream(new GZIPInputStream(new ByteArrayInputStream(index.buffer)));
			DataInputStream dataFile = new DataInputStream(new GZIPInputStream(new ByteArrayInputStream(data.buffer)));
			int totalSprites = indexFile.readInt();

			if (cache == null) {
				cache = new SpriteLoader[totalSprites];
				sprites = new Sprite[totalSprites];
			}

			for (int i = 0; i < totalSprites; i++) {
				int id = indexFile.readInt();
				if (cache[id] == null) {
					cache[id] = new SpriteLoader();
				}

				cache[id].readValues(indexFile, dataFile);
				createSprite(cache[id]);
			}

			indexFile.close();
			dataFile.close();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	private static byte[] readFile(String name) {
		try {
			RandomAccessFile raf = new RandomAccessFile(name, "r");
			java.nio.ByteBuffer buf = raf.getChannel().map(FileChannel.MapMode.READ_ONLY, 0, raf.length());

			try {
				if (buf.hasArray()) {
					return buf.array();
				} else {
					byte[] array = new byte[buf.remaining()];
					buf.get(array);
					return array;
				}
			} finally {
				raf.close();
			}
		} catch (Exception e) {
			e.printStackTrace();
		}

		return null;
	}

	private int id;
	private int drawOffsetX;
	private int drawOffsetY;
	public byte[] spriteData;

	/**
	 * Sets the default values.
	 */
	public SpriteLoader() {
		id = -1;
		drawOffsetX = 0;
		drawOffsetY = 0;
		spriteData = null;
	}

	/**
	 * Reads the information from the index and data files.
	 *
	 * @param index holds the sprite indices
	 * @param data holds the sprite data per index
	 * @throws IOException
	 */
	private void readValues(DataInputStream index, DataInputStream data) throws IOException {
		do {
			int opcode = data.readByte();

			if (opcode == 0) {
				break;
			}
			if (opcode == 1) {
				id = data.readShort();
			} else if (opcode == 2) {
				data.readUTF();
			} else if (opcode == 3) {
				drawOffsetX = data.readShort();
			} else if (opcode == 4) {
				drawOffsetY = data.readShort();
			} else if (opcode == 5) {
				int indexLength = index.readInt();
				byte[] dataread = new byte[indexLength];
				data.readFully(dataread);
				spriteData = dataread;
			}
		} while (true);
	}

}