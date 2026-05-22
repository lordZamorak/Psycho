package org.necrotic.client.accounts;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;

import org.necrotic.client.Signlink;

/**
 * Manages loading/saving/adding/removing of saved accounts
 * @author Tedi / Gabbe
 */
public class AccountManager {

	private final String FILE_NAME = "accounts.dat";
	private Account[] accounts = new Account[5];

	/**
	 * Constructor for the account manager.
	 * Loads all the saved accounts into the array list
	 */
	public AccountManager() {
		if(saveExists()) {
			load();
		}
	}

	/**
	 * Adds an account to the save list
	 * @param account The account to add
	 */
	public void addAccount(Account account, boolean save) {
		int box = getEmptyBox();
		if(box == -1) {
			return;
		}
		account.setBox(box);
		accounts[account.getBox()] = account;
		if(save) {
			save();
		}
	}

	/**
	 * Removes an account from the save list
	 * @param account The account to remove
	 **/
	public void removeAccount(Account account, boolean save) {
		accounts[account.getBox()] = null;
		if(save) {
			save();
		}
	}

	private boolean saveExists() {
		return new File(Signlink.getSettingsDirectory() + "/" + FILE_NAME).exists();
	}

	private void load() {
		try {
			DataInputStream stream = new DataInputStream(new FileInputStream(Signlink.getSettingsDirectory() + "/" + FILE_NAME));
			for (int acc = 0; acc < 5; acc++) {
				if (stream.read() == 1) {
					int helmet = stream.readInt();
					StringBuilder username = new StringBuilder();
					StringBuilder password = new StringBuilder();

					int length = stream.readInt();
					for (int i = 0; i < length; i++)
						username.append(stream.readChar());

					length = stream.readInt();
					for (int i = 0; i < length; i++)
						password.append(stream.readChar());

					accounts[acc] = new Account(username.toString(), password.toString(), helmet);
					System.out.println("Loaded account: " + username.toString());
				}
			}
			stream.close();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	public void save() {
		try {
			DataOutputStream stream = new DataOutputStream(new FileOutputStream(Signlink.getSettingsDirectory() + "/" + FILE_NAME));
			for (int i = 0; i < 5; i++) {
				if (accounts[i] == null) {
					stream.write(0);
				} else {
					stream.write(1);

					Account account = accounts[i];
					stream.writeInt(account.getHelmet());

					stream.writeInt(account.getUsername().length());
					for (char c : account.getUsername().toCharArray()) {
						stream.writeChar(c);
					}

					stream.writeInt(account.getPassword().length());
					for (char c : account.getPassword().toCharArray()) {
						stream.writeChar(c);
					}
				}
			}
			stream.close();
		} catch (Exception ex) {
			ex.printStackTrace();
		}
	}
	
	public Account getAccount(String username) {
		for(Account acc : accounts) {
			if(acc == null)
				continue;
			if(acc.getUsername().equals(username)) {
				return acc;
			}
		}
		return null;
	}

	public int getEmptyBox() {
		int index = -1;
		for(int i = 0; i < accounts.length; i++) {
			if(accounts[i] == null) {
				index = i;
				break;
			}
		}
		return index;
	}

	public Account[] getAccounts() {
		return accounts;
	}
}
