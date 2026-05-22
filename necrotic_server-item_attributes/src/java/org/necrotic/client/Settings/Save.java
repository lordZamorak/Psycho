package org.necrotic.client.Settings;

import java.io.File;
import java.io.FileWriter;
import java.nio.file.Path;
import java.nio.file.Paths;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonObject;
import org.necrotic.Configuration;
import org.necrotic.client.Client;
import org.necrotic.client.Signlink;

/**
 * Created by Brandon on 7/28/2017.
 */
public class Save {

    public static void settings(Client client) {
        Path path = Paths.get(Signlink.getSettingsDirectory(), "/settings.json");
        File file = path.toFile();
        file.getParentFile().setWritable(true);

        if(!file.exists()) {
            try {
                file.getParentFile().mkdirs();
                System.out.println("Making settings directory.");
            } catch (SecurityException e) {
                System.out.println("Unable to create directory for settings!");
            }
        }

        try (FileWriter writer = new FileWriter(file)) {
            Gson builder = new GsonBuilder().setPrettyPrinting().create();
            JsonObject object = new JsonObject();
            object.addProperty("save-accounts", Configuration.SAVE_ACCOUNTS);
            object.addProperty("high-detail", Configuration.HIGH_DETAIL);
            object.addProperty("split-chat-color", new Integer(client.splitChatColor));
            object.addProperty("clan-chat-color", new Integer(client.clanChatColor));
            object.addProperty("split-chat", new Integer(client.variousSettings[502]));
            object.addProperty("brightness", new Integer(client.variousSettings[166]));

            StringBuilder stringSave = new StringBuilder();
            for(int i = 0; i < client.quickPrayers.length; i++)
                stringSave.append(client.quickPrayers[i]);
            object.add("quick-prayers", builder.toJsonTree(stringSave.toString()));

            stringSave = new StringBuilder();
            for(int i = 0; i < client.quickCurses.length; i++)
                stringSave.append(client.quickCurses[i]);
            object.add("quick-curses", builder.toJsonTree(stringSave.toString()));

            writer.write(builder.toJson(object));
        } catch (Exception e) {
            System.out.println(e);
        }

    }
}

