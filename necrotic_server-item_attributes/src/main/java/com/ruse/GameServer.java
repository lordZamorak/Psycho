package com.ruse;

import java.io.File;
import java.io.IOException;
import java.lang.reflect.Field;
import java.util.logging.Level;
import java.util.logging.Logger;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.dataformat.yaml.YAMLFactory;
import com.ruse.util.ShutdownHook;
import sun.misc.Unsafe;

/**
 * The starting point of Ruse.
 * @author Gabriel
 * @author Samy
 */ 
public class GameServer {

	private static final Logger logger = Logger.getLogger("Ruse");

	private static GameLoader loader;
	private static boolean updating;
	private static GameConfiguration configuration;

	public static void main(String[] params) {
		Runtime.getRuntime().addShutdownHook(new ShutdownHook());
		try {
			disableWarning();
			loadConfiguration();
			loader = new GameLoader(GameServer.getConfiguration().getPort());
			logger.info("Initializing the loader...");
			loader.init();
			loader.finish();
			logger.info("The loader has finished loading utility tasks.");
			logger.info(GameSettings.RSPS_NAME+" is now online on port " + configuration.getPort() + "!");
		} catch (Exception ex) {
			logger.log(Level.SEVERE, "Could not start "+GameSettings.RSPS_NAME+"! Program terminated.", ex);
			System.exit(1);
		}
	}

	/**
	 * Load yaml configuration.
	 * @throws IOException if an error occurs while reading the file.
	 */
	private static void loadConfiguration() throws IOException {
		File configFile = resolveConfigurationFile();
		ObjectMapper mapper = new ObjectMapper(new YAMLFactory());

		if (!configFile.exists()) {
			File parent = configFile.getParentFile();
			if (parent != null && !parent.exists() && !parent.mkdirs()) {
				throw new IOException("Could not create configuration directory: " + parent.getAbsolutePath());
			}
			mapper.writeValue(configFile, GameConfiguration.getDefault());
			logger.info("Created default configuration file at " + configFile.getAbsolutePath() + ".");
		}

		mapper.findAndRegisterModules();
		configuration = mapper.readValue(configFile, GameConfiguration.class);
		logger.info("Loaded configuration from " + configFile.getAbsolutePath() + ".");
	}

	private static File resolveConfigurationFile() {
		String path = System.getProperty("psycho.config");
		if (path == null || path.trim().isEmpty()) {
			path = System.getenv("PSYCHO_CONFIG");
		}
		if (path == null || path.trim().isEmpty()) {
			path = System.getProperty("kandarin.config");
		}
		if (path == null || path.trim().isEmpty()) {
			path = System.getenv("KANDARIN_CONFIG");
		}
		if (path == null || path.trim().isEmpty()) {
			path = GameSettings.GAME_CONFIGURATION_FILE;
		}
		return new File(path);
	}

	/**
	 * Disables the "WARNING: An illegal reflective access operation has occurred"
	 */
	public static void disableWarning() {
		try {
			Field theUnsafe = Unsafe.class.getDeclaredField("theUnsafe");
			theUnsafe.setAccessible(true);
			Unsafe u = (Unsafe) theUnsafe.get(null);

			Class cls = Class.forName("jdk.internal.module.IllegalAccessLogger");
			Field logger = cls.getDeclaredField("logger");
			u.putObjectVolatile(cls, u.staticFieldOffset(logger), null);
		} catch (Exception e) {
		}
	}

	public static GameLoader getLoader() {
		return loader;
	}

	public static Logger getLogger() {
		return logger;
	}

	public static void setUpdating(boolean updating) {
		GameServer.updating = updating;
	}

	public static boolean isUpdating() {
		return GameServer.updating;
	}

	/**
	 * Gets the {@link GameConfiguration}
	 * @return the {@link GameConfiguration}
	 */
	public static GameConfiguration getConfiguration() {
		return configuration;
	}
}
