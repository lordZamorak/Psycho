package org.necrotic.client.graphics.rsinterface;

import java.util.Arrays;

import org.necrotic.client.Client;
import org.necrotic.client.RSInterface;
import org.necrotic.client.graphics.fonts.TextDrawingArea;

public class Settings extends CustomInterfaces {

	private static Setting[][] settings;

	public enum Data {
		NEW_FUNCTION_KEYS("new_fkeys", "New F-keys"),
		GROUND_ITEM_OVERLAY("ground_text", "Ground item overlay"),
		NEW_HITMARKERS("new_hitmarkers", "New hitmarkers"),
		NEW_CURSORS("new_cursors", "New cursors"),
		CONSTITUTION("constitution", "Constitution"),
		HIGHLIGHT_USERNAME("highlight_username", "Highlight username when mentioned"),
		EMOTICONS("emoticons", "Display emoticons"),
		REMOVE_COLOR_BANDING("remove_color_banding", "Remove tile color banding"),


		NEW_FOV("new_fov", "Widen field of view in resizeable"),
		HP_ABOVE_HEAD("hp_above_head", "Show hitpoints above head"),
		USERNAMES_ABOVE_HEAD("username_above_head", "Show usernames above head"),
		NEW_HEALTH_BARS("new_health_bars", "New health bars"),
		TWEENING("tweening", "Tweening (smoother animations)"),
		ROOFS_OFF("roofs_off", "Hide roofs"),
		FOG("fog", "Depth buffer (fog)"),
		HD_MINIMAP("hd_minimap", "High definition minimap"),
		OLD_GAMEFRAME("old_gameframe", "Old Gameframe"),

		;

		private final String key;
		private final String format;

		Data(String key, String format) {
			this.key = key;
			this.format = format;
		}

		public String getKey() {
			return key;
		}

		public String getFormat() {
			return format;
		}
	}

	public static class Setting {
		private final int config;
		private final Data data;
		private int buttonId;

		public Setting(int config, Data data) {
			this.config = config;
			this.data = data;
		}
	}

	static {
		settings = new Setting[2][];
		settings[0] = new Setting[Data.values().length / 2 + (Data.values().length % 2)];
		settings[1] = new Setting[Data.values().length - settings[0].length];
		int index0 = 0, index1 = 0;
		int config = 650;

		for (Data data : Data.values()) {
			Setting setting = new Setting(config++, data);
			if (index0 < settings[0].length) {
				settings[0][index0++] = setting;
			} else {
				settings[1][index1++] = setting;
			}
		}
	}

	public static void setDefault() {
		set(Data.NEW_FUNCTION_KEYS, true);
		set(Data.GROUND_ITEM_OVERLAY, true);
		set(Data.NEW_HITMARKERS, true);
		set(Data.CONSTITUTION, true);
		set(Data.NEW_FOV, true);
		set(Data.NEW_HEALTH_BARS, true);
		set(Data.TWEENING, true);
		set(Data.HD_MINIMAP, true);
		set(Data.ROOFS_OFF, true);
		Client.instance.variousSettings[166] = 3;
	}

	public static void set(Data data, boolean bool) {
		Client.instance.clientSettings.set(data.key, bool);
		update(data);
		System.out.println("Set: " + data + " to " + bool);

		switch (data) {
			case ROOFS_OFF:
			case HD_MINIMAP:
				if (Client.instance.loggedIn) {
					Client.instance.loadRegion();
				}
				break;
		}
	}

	public static boolean get(Data data) {
		return Client.instance.clientSettings.getBool(data.key);
	}

	public static boolean buttonClick(int button) {
		for (Setting[] settingArray : settings) {
			for (Setting setting : settingArray) {
				if (setting.buttonId == button) {
					System.out.println("Settings button: " + button);
					set(setting.data, !Client.instance.clientSettings.getBool(setting.data.key));
					return true;
				}
			}
		}
		return false;
	}

	public static void update(Data data) {
		for (int index = 0; index < settings.length; index++) {
			for (int setting = 0; setting < settings[index].length; setting++) {
				if (settings[index][setting].data == data) {
					Client.instance.variousSettings[settings[index][setting].config] = Client.instance.clientSettings.getBool(data.key) ? 1 : 0;
					return;
				}
			}
		}

		throw new IllegalArgumentException("No setting: " + data);
	}

	public static void updateSettingsInterface() {
		Arrays.stream(Data.values()).forEach(Settings::update);
	}

	public Settings(TextDrawingArea[] tda) {
		super(tda);
	}

	private int getSettingsContainer() {
		RSInterface rsinterface = addInterface(26064);
		rsinterface.width = 424;
		rsinterface.height = 107;
		rsinterface.scrollMax = 108 + ((settings[0].length - 6) * 18);

		RSInterface.setChildren(settings[0].length * 4 + settings[1].length * 4, rsinterface);
		int current = 26065;
		int i = 0;

		for (int index = 0; index < settings.length; index++) {
			int x = index == 0 ? 2 : 210;
			int y = 0;
			for (int setting = 0; setting < settings[index].length; setting++) {
				int textX = x;
				int buttonX = x;
				if (index == 0) {
					// across bar
					setBounds(26005, x, y + 16, i++, rsinterface);
					// divider
					setBounds(26011, x + 18, y, i++, rsinterface);
				} else {
					// divider
					setBounds(26011, x - 2, y, i++, rsinterface);
					// divider
					setBounds(26011, x + 17, y, i++, rsinterface);
					buttonX -= 1;
				}

				addText(current, settings[index][setting].data.format, 0xe4a146, false, true, 52, 0);
				addButton(current + 1, 4, -1, 484, 485, 15, 15, "Toggle", settings[index][setting].config, 1);
				setBounds(current, textX + 24, y + 2, i++, rsinterface);
				setBounds(current + 1, buttonX + 2, y, i++, rsinterface);
				settings[index][setting].buttonId = current + 1;

				current += 2;
				y += 18;
			}
		}

		return rsinterface.id;
	}

	public void load() {
		RSInterface rsinterface = addInterface(26000);
		int x = 30;
		int y = 49;
		addSpriteLoader(26001, 1104);
		addText(26002, "Settings", 0xe4a146, true, true, 52, 2);
		// addInAreaHover(26003, 427, 428, 16, 16, "Close", 250, 3);
		addButton(26003, 4, -1, 427, 427, 15, 15, "Close", 427, 1);
		addText(26004, "Use New Function Keys.", 0xe4a146, false, true, 52, 0);
		addSprite(26005, 494);
		addSprite(26011, 495);
		addText(26006, "Display New Health Bars.", 0xe4a146, false, true, 52, 0);
		addText(26009, "Enable New Cursors.", 0xe4a146, false, true, 52, 0);
		addText(26015, "Display New Hitmarks.", 0xe4a146, false, true, 52, 0);
		addText(26028, "Display Hitpoints Above \\nHead.", 0xe4a146, false, true, 52, 0);
		addText(26025, "Display Usernames Above \\nHead.", 0xe4a146, false, true, 52, 0);
		addText(26030, "Enable Constitution.\\n(x10 damage)", 0xe4a146, false, true, 52, 0);
		addText(26032, "New Gameframe.", 0xe4a146, false, true, 52, 0);
		addText(26034, "Enable highlighting \\nyour username\\nwhen mentioned-\\nin chat.", 0xe4a146, false, true, 52, 0);
		addButton(26007, 4, -1, 484, 485, 15, 15, "Toggle function keys", 650, 1);
		addButton(26008, 4, -1, 484, 485, 15, 15, "Toggle health bars", 651, 1);
		addButton(26010, 4, -1, 484, 485, 15, 15, "Toggle cursors", 652, 1);
		addButton(26014, 4, -1, 484, 485, 15, 15, "Toggle hitmarks", 654, 1);
		addButton(26026, 4, -1, 484, 485, 15, 15, "Toggle hitpoints above head", 655, 1);
		addButton(26027, 4, -1, 484, 485, 15, 15, "Toggle usernames above head", 656, 1);
		addButton(26029, 4, -1, 484, 485, 15, 15, "Toggle constitution", 657, 1);
		addButton(26031, 4, -1, 484, 485, 15, 15, "Toggle gameframe", 658, 1);
		addButton(26033, 4, -1, 484, 485, 15, 15, "Toggle push-notifications", 659, 1);

		addCheckmarkHover(26054, 4, 26055, 576, 577, 57, 35, 531, 1, "Low Detail", 26056, 577, 577, 26057, "", "", 12, 20);
		addCheckmarkHover(26058, 4, 26059, 578, 579, 57, 35, 531, 2, "High Detail", 26060, 579, 579, 26061, "", "", 12, 20);

		//addText(26050, "If the game runs slowly on your computer, try reducing these settings.", tda, 1, 0xeb981f, true, true);
		addText(26051, "@yel@Resizable", tda, 0, 0xeb981f, false, true);
		addText(26052, "@yel@Fullscreen", tda, 0, 0xeb981f, false, true);
		addText(26053, "@yel@Fixed", tda, 0, 0xeb981f, false, true);

		addText(26062, "@yel@Low Detail", tda, 0, 0xeb981f, false, true);
		addText(26063, "@yel@High Detail", tda, 0, 0xeb981f, false, true);

		/**
		 * Fixed buttons
		 */
		addHoverButton(26016, 486, 50, 39, "Fixed Mode", -1, 26017, 1);
		addHoveredButton(26017, 487, 50, 39, 26018);
		/**
		 * Resizable buttons
		 */
		addHoverButton(26019, 488, 50, 39, "Resizable Mode", -1, 26020, 1);
		addHoveredButton(26020, 489, 50, 39, 26021);
		/**
		 * Fullscreen buttons
		 */
		addHoverButton(26022, 490, 50, 39, "Fullscreen Mode", -1, 26023, 1);
		addHoveredButton(26023, 491, 50, 39, 26024);

		setChildren(54 - 18 - 18 + 1, rsinterface);
		int i = 0;
		// background
		setBounds(26001, x + 0, y + 0, i, rsinterface);
		i++;
		// title
		setBounds(26002, x + 230, y + 3, i, rsinterface);
		i++;
		// close button
		setBounds(26003, x + 427, y + 3, i, rsinterface);
		i++;

		setBounds(getSettingsContainer(), x + 4, y + 84, i, rsinterface);
		i++;

		/**
		 * Fixed
		 */
		setBounds(26016, x + 30, y + 38, i, rsinterface);
		i++;
		setBounds(26017, x + 30, y + 38, i, rsinterface);
		i++;
		/**
		 * Resizable
		 */
		setBounds(26019, x + 110, y + 38, i, rsinterface);
		i++;
		setBounds(26020, x + 110, y + 38, i, rsinterface);
		i++;
		/**
		 * Fullscreen
		 */
		setBounds(26022, x + 190, y + 38, i, rsinterface);
		i++;
		setBounds(26023, x + 190, y + 38, i, rsinterface);
		i++;
		setBounds(26051, 142, 74, i, rsinterface);
		i++;
		setBounds(26052, 220, 74, i, rsinterface);
		i++;
		setBounds(26053, 70, 74, i, rsinterface);
		i++;
		setBounds(26054, 314, 88, i, rsinterface);
		i++;
		setBounds(26055, 314, 88, i, rsinterface);
		i++;
		setBounds(26058, 384, 88, i, rsinterface);
		i++;
		setBounds(26059, 384, 88, i, rsinterface);
		i++;
		setBounds(26062, 316, 75, i, rsinterface);
		i++;
		setBounds(26063, 385, 75, i, rsinterface);
	}

}