package org.necrotic;

/**
 * The client's features can easily be toggled/changed here.
 * @author Gabriel Hannason
 */
public class Configuration {

	public static boolean developerMode = false;

	public static final boolean PRINT_EMPTY_INTERFACE_SECTIONS = false;
	public static final boolean FORCE_CACHE_UPDATE = false;
	public static final boolean STOP_CACHE_UPDATES = false; //overrides the above
	
	/** LOADS CACHE FROM ./ IF TRUE, OTHERWISE USER.HOME FOLDER**/
	public static final boolean DROPBOX_MODE = false;
	
	/** MAIN CONSTANTS **/
	public final static String CLIENT_NAME = "Psycho";
	public final static String WEBSITE = "playpsycho.local";
	public final static String TRAY_ICON = "https://playpsycho.local/downloads/client/icon-32x32.png";

	public final static String CACHE_DIRECTORY_NAME = CLIENT_NAME + "Cache";
	public static final String SETTINGS_DIRECTORY_NAME = CLIENT_NAME + "Settings";
	public final static int CLIENT_VERSION = 55;

	public final static boolean JAGCACHED_ENABLED = false;
	public final static String JAGCACHED_HOST  = "";

	public final static int SERVER_PORT = 13377;
	public final static boolean DISPLAY_GAMEWORLD_ON_LOGIN = false;
	
	/** UPDATING **/
	public final static int NPC_BITS = 18;

	
	/** FEATURES **/
	public static boolean SAVE_ACCOUNTS = true;
	public static boolean MONEY_POUCH_ENABLED = false;


	/**
	 * The client will run in high memory with textures rendering
	 */
	public static boolean HIGH_DETAIL = false;
	public static boolean hdTexturing = false;

	public final static String SERVER_HOST() {
		return "localhost";
	}

}
