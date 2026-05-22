package org.necrotic.Discord;

import club.minnced.discord.rpc.DiscordEventHandlers;
import club.minnced.discord.rpc.DiscordRPC;
import club.minnced.discord.rpc.DiscordRichPresence;

/**
 * @author Arham 4
 */
public class RichPresence {

    private final String CLIENT_ID = "652638363366457344";

    private DiscordRPC lib;
    private DiscordRichPresence presence;
    private boolean enabled;

    public void initiate() {
        try {
            lib = DiscordRPC.INSTANCE;
            DiscordEventHandlers handlers = new DiscordEventHandlers();
            lib.Discord_Initialize(CLIENT_ID, handlers, true, "");
            presence = new DiscordRichPresence();
            presence.startTimestamp = System.currentTimeMillis() / 1000;
            presence.largeImageKey = "k3";
            presence.smallImageKey = "k3small";
            enabled = true;
            updatePresence();
            Thread callbackThread = new Thread(() -> {
                while (!Thread.currentThread().isInterrupted()) {
                    lib.Discord_RunCallbacks();
                    try {
                        Thread.sleep(2000);
                    } catch (InterruptedException ignored) {
                        Thread.currentThread().interrupt();
                    }
                }
            }, "RPC-Callback-Handler");
            callbackThread.setDaemon(true);
            callbackThread.start();
        } catch (Throwable throwable) {
            enabled = false;
            lib = null;
            presence = null;
            System.err.println("Discord rich presence disabled: " + throwable.getMessage());
        }
    }

    public boolean presenceIsNull() {
        return presence == null;
    }

    public void updateDetails(String details) {
        if (!isReady()) {
            return;
        }
        presence.details = details;
        updatePresence();
    }

    public void updateState(String state) {
        if (!isReady()) {
            return;
        }
        presence.state = state;
        updatePresence();
    }

    public void updateSmallImageKey(String key) {
        if (!isReady()) {
            return;
        }
        presence.smallImageKey = key;
        updatePresence();
    }

    private void updatePresence() {
        if (!isReady()) {
            return;
        }
        lib.Discord_UpdatePresence(presence);
    }

    private boolean isReady() {
        return enabled && lib != null && presence != null;
    }
}
