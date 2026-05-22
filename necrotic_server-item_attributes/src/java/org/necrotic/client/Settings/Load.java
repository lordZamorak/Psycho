package org.necrotic.client.Settings;

import java.io.File;
import java.io.FileReader;
import java.nio.file.Path;
import java.nio.file.Paths;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import org.necrotic.Configuration;
import org.necrotic.client.Client;
import org.necrotic.client.Signlink;

/**
 * Created by Brandon on 7/28/2017.
 */
public class Load {

    public static void settings(Client client) {
        Path path = Paths.get(Signlink.getSettingsDirectory(), "/settings.json");
        File file = path.toFile();

        //if it doesn't exist no use on loading it, wait for saving method to create it.
        if(!file.exists()) {
            return;
        }

        try (FileReader fileReader = new FileReader(file)) {

            JsonParser fileparser = new JsonParser();
            JsonObject reader = (JsonObject) fileparser.parse(fileReader);

            if(reader.has("high-detail")) {
                Configuration.HIGH_DETAIL = reader.get("high-detail").getAsBoolean();
            }

            if(reader.has("save-accounts")) {
                Configuration.SAVE_ACCOUNTS = reader.get("save-accounts").getAsBoolean();
            }
            if(reader.has("split-chat-color")) {
                client.splitChatColor = reader.get("split-chat-color").getAsInt();
            }
            if(reader.has("clan-chat-color")) {
                client.splitChatColor = reader.get("clan-chat-color").getAsInt();
            }
            if(reader.has("split-chat")) {
                client.variousSettings[502] = reader.get("split-chat").getAsInt();
            }
            if(reader.has("brightness")) {
                client.variousSettings[166] = reader.get("brightness").getAsInt();
            }
            if(reader.has("quick-prayers")) {
                String qp = reader.get("quick-prayers").getAsString();
                for (int i = 0; i < qp.length(); i++)
                    client.quickPrayers[i] = Integer.parseInt(qp.substring(i, i+1));
            }
            if(reader.has("quick-curses")) {
                String qp = reader.get("quick-curses").getAsString();
                for (int i = 0; i < qp.length(); i++)
                    client.quickCurses[i] = Integer.parseInt(qp.substring(i, i+1));
            }
        } catch (Exception e) {
            System.out.println(e);
        }

    }

}
