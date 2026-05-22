package org.necrotic.client.world;

import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

import org.necrotic.client.cache.definition.ObjectDefinition;
import org.necrotic.client.graphics.DrawingArea;
import org.necrotic.client.graphics.rsinterface.Settings;

public final class EnvironmentalEffects {

	private static final Map<Integer, Boolean> VEGETATION_OBJECTS = new HashMap<>();
	private static int sceneTick;
	private static boolean sceneEnabled = true;

	private EnvironmentalEffects() {
	}

	public static void beginScene(int tick) {
		sceneTick = tick;
		sceneEnabled = enabled();
	}

	public static boolean enabled() {
		try {
			return Settings.get(Settings.Data.DYNAMIC_ENVIRONMENT);
		} catch (Exception ignored) {
			return true;
		}
	}

	public static boolean isWaterTexture(int textureId) {
		return sceneEnabled && textureId == 1;
	}

	public static int texelPos(int defaultIndex, int mipmap, int textureId) {
		int size = 128 >> mipmap;
		int x = (defaultIndex & 127) >> mipmap;
		int y = (defaultIndex >> 7) >> mipmap;

		if (isWaterTexture(textureId)) {
			int mask = size - 1;
			int finePhase = (sceneTick * 23 + (y << 6) + (x << 2)) & 2047;
			int crossPhase = (sceneTick * 17 + (x << 5) - (y << 3)) & 2047;
			int rippleX = Model.SINE[finePhase] >> (15 + mipmap);
			int rippleY = Model.COSINE[crossPhase] >> (15 + mipmap);
			x = (x + (sceneTick >> (2 + mipmap)) + rippleX) & mask;
			y = (y + (sceneTick >> (3 + mipmap)) + rippleY) & mask;
		}

		return x + y * size;
	}

	public static int materialRgb(int rgb, int textureId) {
		if (!isWaterTexture(textureId)) {
			return rgb;
		}

		int shimmer = 12 + ((Model.SINE[(sceneTick * 11) & 2047] + 65536) >> 14);
		return blend(rgb, 0x7fc8ff, shimmer);
	}

	public static void applySceneAtmosphere(int width, int height, boolean fogEnabled) {
		if (!sceneEnabled || width <= 0 || height <= 0 || DrawingArea.pixels == null) {
			return;
		}

		int horizonHeight = Math.max(1, height / 4);
		int topAlpha = fogEnabled ? 12 : 7;
		DrawingArea.drawAlphaGradient(0, 0, width, horizonHeight, 0x9fd0e0, 0x40576a, topAlpha);
	}

	public static void applyTerrainWind(int width, int height) {
		int pixelCount = width * height;
		if (!sceneEnabled || width <= 0 || height <= 0 || DrawingArea.pixels == null || pixelCount > 2300000) {
			return;
		}

		int[] pixels = DrawingArea.pixels;
		int stride = DrawingArea.width;
		int safeHeight = Math.min(height, DrawingArea.height);
		int safeWidth = Math.min(width, stride);
		int step = pixelCount > 900000 ? 2 : 1;
		for (int y = 0; y < safeHeight; y += step) {
			int offset = y * stride;
			int phase = sceneTick * 7 + y * 19;
			for (int x = 0; x < safeWidth; x += step) {
				int index = offset + x;
				int rgb = pixels[index];
				if (isGrassPixel(rgb)) {
					int wave = Model.SINE[(phase + x * 11) & 2047] >> 14;
					pixels[index] = adjustGrassPixel(rgb, wave);
					if (step > 1) {
						adjustGrassNeighbor(pixels, index + 1, wave, x + 1 < safeWidth);
						adjustGrassNeighbor(pixels, index + stride, wave, y + 1 < safeHeight);
						adjustGrassNeighbor(pixels, index + stride + 1, wave, x + 1 < safeWidth && y + 1 < safeHeight);
					}
				}
			}
		}
	}

	public static boolean isVegetationObject(int objectId) {
		if (!sceneEnabled || objectId <= 0) {
			return false;
		}

		Boolean cached = VEGETATION_OBJECTS.get(objectId);
		if (cached != null) {
			return cached;
		}

		boolean vegetation = false;
		try {
			ObjectDefinition definition = ObjectDefinition.forID(objectId);
			String name = definition.name == null ? "" : definition.name.toLowerCase(Locale.ROOT);
			vegetation = (name.contains("tree") && !name.contains("stump"))
					|| name.contains("bush")
					|| name.contains("fern")
					|| name.contains("grass")
					|| name.contains("plant")
					|| name.contains("reed")
					|| name.contains("ivy")
					|| name.contains("palm")
					|| name.contains("jungle")
					|| name.contains("sapling")
					|| name.contains("root")
					|| name.contains("vine")
					|| name.contains("cactus");
		} catch (Exception ignored) {
			vegetation = false;
		}

		VEGETATION_OBJECTS.put(objectId, vegetation);
		return vegetation;
	}

	public static int vegetationSwayX(int objectId, int vertex, int localY, int modelHeight, int worldX, int worldZ) {
		return vegetationSway(objectId, vertex, localY, modelHeight, worldX, worldZ, 0);
	}

	public static int vegetationSwayZ(int objectId, int vertex, int localY, int modelHeight, int worldX, int worldZ) {
		return vegetationSway(objectId, vertex, localY, modelHeight, worldX, worldZ, 512);
	}

	private static int vegetationSway(int objectId, int vertex, int localY, int modelHeight, int worldX, int worldZ, int phaseOffset) {
		if (modelHeight <= 0) {
			return 0;
		}

		int heightWeight = (-localY * 256) / modelHeight;
		if (heightWeight <= 0) {
			return 0;
		}
		if (heightWeight > 256) {
			heightWeight = 256;
		}

		int phase = sceneTick * 12 + objectId * 19 + vertex * 29 + worldX / 8 + worldZ / 11 + phaseOffset;
		int gust = 5 + ((Model.SINE[(sceneTick * 5 + objectId) & 2047] + 65536) >> 15);
		int wave = (Model.SINE[phase & 2047] * gust) >> 16;
		return (wave * heightWeight) >> 8;
	}

	private static int blend(int from, int to, int alpha) {
		int inv = 256 - alpha;
		int rb = ((from & 0xff00ff) * inv + (to & 0xff00ff) * alpha) & 0xff00ff00;
		int g = ((from & 0x00ff00) * inv + (to & 0x00ff00) * alpha) & 0x00ff0000;
		return (rb | g) >>> 8;
	}

	private static boolean isGrassPixel(int rgb) {
		int r = rgb >> 16 & 0xff;
		int g = rgb >> 8 & 0xff;
		int b = rgb & 0xff;
		return g > 42 && g >= r + 5 && g >= b + 8 && r < 145 && b < 125;
	}

	private static int adjustGrassPixel(int rgb, int wave) {
		int r = clamp((rgb >> 16 & 0xff) + wave / 2);
		int g = clamp((rgb >> 8 & 0xff) + wave);
		int b = clamp((rgb & 0xff) + wave / 3);
		return r << 16 | g << 8 | b;
	}

	private static void adjustGrassNeighbor(int[] pixels, int index, int wave, boolean inBounds) {
		if (!inBounds) {
			return;
		}

		int rgb = pixels[index];
		if (isGrassPixel(rgb)) {
			pixels[index] = adjustGrassPixel(rgb, wave);
		}
	}

	private static int clamp(int value) {
		if (value < 0) {
			return 0;
		}
		if (value > 255) {
			return 255;
		}
		return value;
	}
}
