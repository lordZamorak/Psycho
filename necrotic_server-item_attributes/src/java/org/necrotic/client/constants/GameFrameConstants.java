package org.necrotic.client.constants;

import java.awt.Toolkit;

import org.necrotic.client.graphics.rsinterface.Settings;

public class GameFrameConstants {

	public static final int smallTabs = 1000;

	public static final int minWidth = 800;

	public static final int minHeight = 600;

	public static boolean isOldGameframe() {
		return Settings.get(Settings.Data.OLD_GAMEFRAME);
	}

	public static int getMaxWidth() {
		return (int) Toolkit.getDefaultToolkit().getScreenSize().getWidth();
	}

	public static int getMaxHeight() {
		return (int) Toolkit.getDefaultToolkit().getScreenSize().getHeight();
	}

}