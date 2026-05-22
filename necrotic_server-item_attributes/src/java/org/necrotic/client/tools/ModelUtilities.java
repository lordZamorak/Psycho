package org.necrotic.client.tools;

import java.io.File;
import java.io.IOException;

import org.necrotic.client.Signlink;
import org.necrotic.client.cache.DataType;
import org.necrotic.client.world.Model;

import static java.lang.Integer.parseInt;

/**
 * Static utilities used for assisting with models.
 *
 * @author Ryley Kimmel <ryley.kimmel@live.com>
 */
public final class ModelUtilities {

	/**
	 * The path to where external models are located.
	 */
	private static final File ROOT = new File(Signlink.getCacheDirectory(), "models");

	/**
	 * Loads all of the models within the specified {@link #PATH}.
	 *
	 * @throws IOException If some I/O exception occurs.
	 */
	public static void loadModels() throws IOException {
		for (File file : ROOT.listFiles()) {
			Model.method460(FileUtilities.getBytesFromFile(file), parseInt(file.getName()), DataType.STANDARD);
			System.out.println("Loaded model " + file);
		}
	}

}