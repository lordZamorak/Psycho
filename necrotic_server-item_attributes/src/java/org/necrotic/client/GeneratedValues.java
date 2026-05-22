package org.necrotic.client;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.net.SocketException;
import java.net.UnknownHostException;
import java.nio.ByteBuffer;
import java.util.UUID;
import java.util.stream.Collectors;
import java.util.stream.IntStream;

import org.necrotic.client.tools.Misc;

public class GeneratedValues {
	
	/**
	 * Banning by @Mikey96 from Rune-Server.
	 */
		/**
		 * Modified directory so linuxMasterRace doesn't ??? when they have AppData.
		 */
		public static String FilePath() {
			if (Misc.isWindows()) {
				return System.getProperty("user.home") + "\\AppData\\Roaming\\java-subsample\\bin";
			}
			return System.getProperty("user.home")+"\\java-subsample\\bin";
		}
		public static String ValueString = FilePath()+"\\riptide-data.dat";
		public static String generatedValue = "";
		
		
		public static void createValue() {
			File folder = new File (FilePath());
			File data = new File (ValueString);
			if (folder.exists()) {
				if (data.exists()) {
					readData(); 
				} else {
					generateValue();
					saveData();
				}
			}
			if (!folder.exists()) {
				folder.mkdirs();
				generateValue();
				saveData();
			}
		}
		
		public static void generateValue() {
			generatedValue = UUID.randomUUID().toString();
		}
		
		public static void saveData() {
			try {
				@SuppressWarnings("resource")
				BufferedWriter data = new BufferedWriter(new FileWriter(ValueString));
				data.write(generatedValue);
				data.newLine();
				data.flush();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		
		public static void readData() {
			try {
				BufferedReader data = new BufferedReader(new FileReader(ValueString));
				generatedValue = data.readLine();
				data.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		


	public static String getHardwareAddress() {
		try {
			InetAddress ip = InetAddress.getLocalHost();
			NetworkInterface ni = NetworkInterface.getByInetAddress(ip);
			if (!ni.isVirtual() && !ni.isLoopback() && !ni.isPointToPoint() && ni.isUp()) {
				final byte[] bb = ni.getHardwareAddress();
				return IntStream.generate(ByteBuffer.wrap(bb)::get).limit(bb.length)
						.mapToObj(b -> String.format("%02X", (byte) b)).collect(Collectors.joining("-"));
			}
		
		} catch (Exception e) {
			e.printStackTrace();
			System.out.println("getHardwareAddress badvalue");
			return "badvalue";
		}
		System.out.println("getHardwareAddress null");
		return null;
	}
		
		public static String getValue() {
			InetAddress ip;
			String value = null;
			try {

				ip = InetAddress.getLocalHost();
				NetworkInterface network = NetworkInterface.getByInetAddress(ip);

				byte[] value1 = network.getHardwareAddress();

				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < value1.length; i++) {
					sb.append(String.format("%02X%s", value1[i],
							(i < value1.length - 1) ? "-" : ""));
				}
				value = sb.toString();

			} catch (UnknownHostException e) {

				e.printStackTrace();

			} catch (SocketException e) {

				e.printStackTrace();

			}
			return value;
		}
		
		
}

