package org.necrotic.client.tools;

import org.necrotic.client.Decompressor;
import org.necrotic.client.cache.Archive;
import org.necrotic.client.cache.definition.MobDefinition;

import java.io.BufferedWriter;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.Instant;

public final class UnityNpcModelExportTool {

    private static final int ConfigArchiveId = 2;
    private static final int MaxNpcId = 100_000;

    private UnityNpcModelExportTool() {
    }

    public static void main(String[] args) throws Exception {
        Path outputRoot = args.length > 0
                ? Paths.get(args[0]).toAbsolutePath().normalize()
                : Paths.get("..", "psycho_unity_client", "Assets", "StreamingAssets", "PsychoMirror").toAbsolutePath().normalize();
        Path cacheRoot = args.length > 1
                ? Paths.get(args[1]).toAbsolutePath().normalize()
                : Paths.get("cache").toAbsolutePath().normalize();

        Files.createDirectories(outputRoot);
        System.setProperty("psycho.cache.dir", cacheRoot.toString());

        try (RandomAccessFile data = new RandomAccessFile(cacheRoot.resolve("main_file_cache.dat").toFile(), "r");
             RandomAccessFile index = new RandomAccessFile(cacheRoot.resolve("main_file_cache.idx0").toFile(), "r")) {
            Decompressor decompressor = new Decompressor(data, index, 1);
            byte[] configArchive = decompressor.decompress(ConfigArchiveId);
            if (configArchive == null) {
                throw new IOException("Could not read config archive " + ConfigArchiveId + " from " + cacheRoot);
            }

            MobDefinition.load(new Archive(configArchive));
        }

        Path output = outputRoot.resolve("npc_models.json");
        int exported = exportNpcModels(output);
        System.out.println("Exported " + exported + " Unity NPC model definitions to " + output);
    }

    private static int exportNpcModels(Path output) throws IOException {
        int count = 0;
        try (BufferedWriter writer = Files.newBufferedWriter(output, StandardCharsets.UTF_8)) {
            writer.write("{\n");
            writer.write("  \"generatedAtUtc\": \"" + Instant.now() + "\",\n");
            writer.write("  \"models\": [\n");

            boolean first = true;
            for (int id = 0; id < MaxNpcId; id++) {
                MobDefinition definition;
                try {
                    definition = MobDefinition.get(id);
                } catch (Exception ignored) {
                    continue;
                }

                if (definition == null || definition.npcModels == null || definition.npcModels.length == 0) {
                    continue;
                }

                if (definition.name == null || "null".equalsIgnoreCase(definition.name)) {
                    continue;
                }

                if (!first) {
                    writer.write(",\n");
                }

                writer.write("    {\n");
                writer.write("      \"id\": " + definition.id + ",\n");
                writer.write("      \"name\": \"" + escape(definition.name) + "\",\n");
                writer.write("      \"size\": " + (definition.npcSizeInSquares & 0xff) + ",\n");
                writer.write("      \"standAnimation\": " + definition.standAnimation + ",\n");
                writer.write("      \"walkAnimation\": " + definition.walkAnimation + ",\n");
                writer.write("      \"osrs\": " + (definition.id >= 30_000) + ",\n");
                writer.write("      \"modelIds\": [");
                for (int i = 0; i < definition.npcModels.length; i++) {
                    if (i > 0) {
                        writer.write(", ");
                    }
                    writer.write(Integer.toString(definition.npcModels[i]));
                }
                writer.write("]\n");
                writer.write("    }");
                first = false;
                count++;
            }

            writer.write("\n  ]\n");
            writer.write("}\n");
        }

        return count;
    }

    private static String escape(String value) {
        return value
                .replace("\\", "\\\\")
                .replace("\"", "\\\"")
                .replace("\r", "\\r")
                .replace("\n", "\\n")
                .replace("\t", "\\t");
    }
}
