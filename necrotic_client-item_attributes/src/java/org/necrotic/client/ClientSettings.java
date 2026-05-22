package org.necrotic.client;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

public class ClientSettings {

    private static final String FILE = "settings.txt";

    private Map<String, Integer> settings = new HashMap<>();
    private boolean loaded = false;

    public void set(String key, int setting) {
        settings.put(key, setting);
        if (loaded) {
            exportSettings();
        }
    }

    public void set(String key, boolean setting) {
        settings.put(key, setting ? 1 : 0);
        if (loaded) {
            exportSettings();
        }
    }

    public void flipBool(String key) {
        set(key, !getBool(key));
    }

    public int getInt(String key) {
        return settings.getOrDefault(key, -1);
    }

    public boolean getBool(String key) {
        return settings.getOrDefault(key, 0) != 0;
    }

    private String getFile() {
        return Signlink.getSettingsDirectory() + FILE;
    }

    public boolean isSettingsFilePresent() {
        return new File(getFile()).exists();
    }

    public void importSettings() {
        loaded = true;
        if (new File(getFile()).exists()) {
            try (BufferedReader reader = new BufferedReader(new FileReader(getFile()))) {
               reader.lines().forEach(line -> {
                   String[] data = line.split("=");
                   settings.put(data[0], Integer.parseInt(data[1]));
               });
            } catch (IOException exception) {
                exception.printStackTrace();
            }
        }
    }

    public void exportSettings() {
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(getFile()))) {
            settings.forEach((key, value) -> {
                try {
                    writer.write(key + "=" + value);
                    writer.newLine();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            });
        } catch (IOException exception) {
            exception.printStackTrace();
        }
    }

}
