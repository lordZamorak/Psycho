package org.necrotic.client.tools;

import java.text.NumberFormat;

public class Misc {

    public static String formatNumber(double number) {
        return NumberFormat.getInstance().format(number);
    }
	
	private static String OS = null;

	public static String getOsName() {
		if (OS == null) {
			OS = System.getProperty("os.name");
		}
		return OS;
	}

	public static boolean isWindows() {
		return getOsName().startsWith("Windows");
	}

	public static String getUnderscoredNumber(int number) {
		String string = String.valueOf(number);
		for (int i = string.length() - 3; i > 0; i -= 3) {
			string = string.substring(0, i) + "_" + string.substring(i);
		}
		return string;
	}

}