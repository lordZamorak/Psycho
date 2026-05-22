package org.necrotic.client.cache.definition;

import org.necrotic.client.Client;
import org.necrotic.client.Signlink;
import org.necrotic.client.cache.Archive;
import org.necrotic.client.io.ByteBuffer;

public final class Flo {

	private static final boolean SNOW_ENABLED = false;

	public static void unpackConfig(Archive streamLoader) {
		underlay = new Flo[2][];
		overlay = new Flo[2][];

		// Underlay 667
		ByteBuffer stream = new ByteBuffer(streamLoader.get("flo.dat"));
		int cacheSize = stream.readUnsignedShort();
		underlay[0] = new Flo[cacheSize];
		for (int j = 0; j < cacheSize; j++) {
			if (underlay[0][j] == null) {
				underlay[0][j] = new Flo();
			}
			underlay[0][j].readValues(stream);
		}

		// Overlay 667
		java.nio.ByteBuffer bb = java.nio.ByteBuffer.wrap(streamLoader.get("flo2.dat"));
		int count = bb.getShort();
		overlay[0] = new Flo[count];
		for (int i = 0; i < count; i++) {
			if (overlay[0][i] == null) {
				overlay[0][i] = new Flo();
			}
			overlay[0][i].readValuesOverlay667(bb);
		}

		// Osrs
		stream = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/flo.dat"));
		int underlayAmount = stream.readShort();
		System.out.println("Underlay Floors Loaded: "+underlayAmount);
		underlay[1] = new Flo[underlayAmount];
		for (int i = 0; i < underlayAmount; i++) {
			if (underlay[1][i] == null) {
				underlay[1][i] = new Flo();
			}
			underlay[1][i].readValuesUnderlayOsrs(stream);
			underlay[1][i].generateHsl();
		}
		int overlayAmount = stream.readShort();
		System.out.println("Overlay Floors Loaded: "+overlayAmount);
		overlay[1] = new Flo[overlayAmount];
		for (int i = 0; i < overlayAmount; i++) {
			if (overlay[1][i] == null) {
				overlay[1][i] = new Flo();
			}
			overlay[1][i].readValuesOverlayOsrs(stream);
			overlay[1][i].generateHsl();
		}
	}

	private static Flo[][] underlay;
	private static Flo[][] overlay;

	public int rgb;
	public int anotherRgb;
	public int texture;
	public boolean occlude;
	public int hue;
	public int saturation;
	public int lumiance;
	public int blendHue;
	public int blendHueMultiplier;
	public int hslToRgb;
	public int anotherHue;
	public int anotherSaturation;
	public int anotherLuminance;

	public static void nullLoader() {
		underlay = null;
		overlay = null;
	}

	private void generateHsl() {
		if (anotherRgb != -1) {
			rgbToHsl(anotherRgb);
			anotherHue = hue;
			anotherSaturation = saturation;
			anotherLuminance = lumiance;
		}
		rgbToHsl(rgb);
	}

	public static Flo[] getCurrentCache() {
		return underlay[Client.osrsRegion ? 1 : 0];
	}

	public static Flo[] getOverlay() {
		return overlay[Client.osrsRegion ? 1 : 0];
	}

	public Flo() {
		texture = -1;
		occlude = true;
	}

	private void rgbToHsl(int i) {
		double d = (i >> 16 & 0xff) / 256D;
		double d1 = (i >> 8 & 0xff) / 256D;
		double d2 = (i & 0xff) / 256D;
		double d3 = d;
		if (d1 < d3) {
			d3 = d1;
		}
		if (d2 < d3) {
			d3 = d2;
		}
		double d4 = d;
		if (d1 > d4) {
			d4 = d1;
		}
		if (d2 > d4) {
			d4 = d2;
		}
		double d5 = 0.0D;
		double d6 = 0.0D;
		double d7 = (d3 + d4) / 2D;
		if (d3 != d4) {
			if (d7 < 0.5D) {
				d6 = (d4 - d3) / (d4 + d3);
			}
			if (d7 >= 0.5D) {
				d6 = (d4 - d3) / (2D - d4 - d3);
			}
			if (d == d4) {
				d5 = (d1 - d2) / (d4 - d3);
			} else if (d1 == d4) {
				d5 = 2D + (d2 - d) / (d4 - d3);
			} else if (d2 == d4) {
				d5 = 4D + (d - d1) / (d4 - d3);
			}
		}
		d5 /= 6D;
		hue = (int) (d5 * 256D);
		saturation = (int) (d6 * 256D);
		lumiance = (int) (d7 * 256D);
		if (saturation < 0) {
			saturation = 0;
		} else if (saturation > 255) {
			saturation = 255;
		}
		if (lumiance < 0) {
			lumiance = 0;
		} else if (lumiance > (SNOW_ENABLED ? -255 : 255)) {
			lumiance = 255;
		}
		if (d7 > 0.5D) {
			blendHueMultiplier = (int) ((1.0D - d7) * d6 * 512D);
		} else {
			blendHueMultiplier = (int) (d7 * d6 * 512D);
		}
		if (blendHueMultiplier < 1) {
			blendHueMultiplier = 1;
		}
		blendHue = (int) (d5 * blendHueMultiplier);
		hslToRgb = hslToRgb(hue, saturation, lumiance);
	}

	private int hslToRgb(int i, int j, int k) {
		if (k > 179) {
			j /= 2;
		}
		if (k > 192) {
			j /= 2;
		}
		if (k > 217) {
			j /= 2;
		}
		if (k > 243) {
			j /= 2;
		}
		return (i / 4 << 10) + (j / 32 << 7) + k / 2;
	}

	private void readValues(ByteBuffer stream) {
		do {
			int i = stream.readUnsignedByte();
			if (i == 0) {
				return;
			} else if (i == 1) {
				rgb = stream.readTribyte();
				//if(rgb == 0x35720A || rgb == 0x50680B || rgb == 0x78680B || rgb == 0x6CAC10 || rgb == 0x819531) {//snow
					//rgb = 0x4d4a4c; // 0xffffff = snow/christmas
				//}
				rgbToHsl(rgb);
			} else if (i == 2) {
				texture = stream.readUnsignedByte();
			} else if (i == 3) {

			} else if (i == 4) {

			} else if (i == 5) {
				occlude = false;
			} else if (i == 6) {
				stream.readString();
			} else if (i == 7) {
				int j = hue;
				int k = saturation;
				int l = lumiance;
				int i1 = blendHue;
				int j1 = stream.readTribyte();
				rgbToHsl(j1);
				hue = j;
				saturation = k;
				lumiance = l;
				blendHue = i1;
				blendHueMultiplier = i1;
			} else {
				System.out.println("Error unrecognised config code: " + i);
			}
		} while (true);
	}

	private void readValuesOverlay667(java.nio.ByteBuffer byteBuffer) {
		for (;;) {
			int attributeId = byteBuffer.get();
			if (attributeId == 0) {
				break;
			} else if (attributeId == 1) {
				rgb = ((byteBuffer.get() & 0xff) << 16) + ((byteBuffer.get() & 0xff) << 8) + (byteBuffer.get() & 0xff);
				rgbToHsl(rgb);
			} else if (attributeId == 2) {
				texture = byteBuffer.get() & 0xff;
			} else if (attributeId == 3) {
				texture = byteBuffer.getShort() & 0xffff;
				if (texture == 65535) {
					texture = -1;
				}
			} else if (attributeId == 4) {

			} else if (attributeId == 5) {
				occlude = false;
			} else if (attributeId == 6) {
			} else if (attributeId == 7) {
				int anotherRgb = ((byteBuffer.get() & 0xff) << 16) + ((byteBuffer.get() & 0xff) << 8) + (byteBuffer.get() & 0xff);
			} else if (attributeId == 8) {

			} else if (attributeId == 9) {
				int int_9 = byteBuffer.getShort() & 0xffff;
			} else if (attributeId == 10) {
			} else if (attributeId == 11) {
				int int_11 = byteBuffer.get() & 0xff;
			} else if (attributeId == 12) {
			} else if (attributeId == 13) {
				int int_13 = ((byteBuffer.get() & 0xff) << 16) + ((byteBuffer.get() & 0xff) << 8) + (byteBuffer.get() & 0xff);
			} else if (attributeId == 14) {
				int int_14 = byteBuffer.get() & 0xff;
			} else if (attributeId == 15) {
				int int_15 = byteBuffer.getShort() & 0xffff;
				if (int_15 == 65535) {
					int_15 = -1;
				}
			} else if (attributeId == 16) {
				int int_16 = byteBuffer.get() & 0xff;
			} else {
				System.err.println("[OverlayFloor] Missing AttributeId: " + attributeId);
			}
		}
	}

	private void readValuesUnderlayOsrs(ByteBuffer buffer) {
		for (;;) {
			int opcode = buffer.readUnsignedByte();
			if (opcode == 0) {
				break;
			} else if (opcode == 1) {
				rgb = ((buffer.readUnsignedByte() & 0xff) << 16) + ((buffer.readUnsignedByte() & 0xff) << 8) + (buffer.readUnsignedByte() & 0xff);
			} else {
				System.out.println("Error unrecognised underlay code: " + opcode);
			}
		}
	}

	private void readValuesOverlayOsrs(ByteBuffer buffer) {
		for (;;) {
			int opcode = buffer.readUnsignedByte();
			if (opcode == 0) {
				break;
			} else if (opcode == 1) {
				rgb = ((buffer.readUnsignedByte() & 0xff) << 16) + ((buffer.readUnsignedByte() & 0xff) << 8) + (buffer.readUnsignedByte() & 0xff);
			} else if (opcode == 2) {
				texture = buffer.readUnsignedByte() & 0xff;
			} else if (opcode == 5) {
				occlude = false;
			} else if (opcode == 7) {
				anotherRgb = ((buffer.readUnsignedByte() & 0xff) << 16) + ((buffer.readUnsignedByte() & 0xff) << 8) + (buffer.readUnsignedByte() & 0xff);
			} else {
				System.out.println("Error unrecognised overlay code: " + opcode);
			}
		}
	}

}
