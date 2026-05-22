package org.necrotic.client.cache;

import java.util.Arrays;
import java.util.Optional;

/**
 * Stores information about the cache archives.
 */
public enum CacheArchive {
    MODELS_STANDARD(1),
    ANIMATIONS_STANDARD(2),
    MUSIC_STANDARD(3),
    MAPS_STANDARD(4),
    TEXTURES(5),
    MODELS_OSRS(6),
    ANIMATIONS_OSRS(7),
    MUSIC_OSRS(8),
    MAPS_OSRS(9),
    ;

    public static CacheArchive forFileIndex(int index) {
        Optional<CacheArchive> optional = Arrays.stream(CacheArchive.values()).filter(archive -> archive.getFileIndex() == index).findFirst();
        if (!optional.isPresent()) {
            throw new IllegalArgumentException("No archive for file index: " + index);
        } else {
            return optional.get();
        }
    }

    public static CacheArchive forArchiveIndex(int index) {
        Optional<CacheArchive> optional = Arrays.stream(CacheArchive.values()).filter(archive -> archive.index == index).findFirst();
        if (!optional.isPresent()) {
            throw new IllegalArgumentException("No archive for index: " + index);
        } else {
            return optional.get();
        }
    }

    private final int index;

    CacheArchive(int index) {
        this.index = index;
    }

    public int getIndex() {
        return index;
    }

    /**
     * Gets the index that the file extension uses (the same index minus one).
     */
    public int getFileIndex() {
        return index - 1;
    }

    @Override
    public String toString() {
        return super.toString() + "{" + "index=" + index + '}';
    }
}
