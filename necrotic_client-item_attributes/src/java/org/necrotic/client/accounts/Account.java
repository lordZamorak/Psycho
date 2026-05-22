package org.necrotic.client.accounts;

/**
 * Represents an account save 
 * @author Tedi
 */
public class Account {
	
	private String username;
	private String password;
	private int box;
	private int helmet;

	public Account(String username, String password, int helmet) {
		this.username = username;
		this.password = password;
		this.helmet = helmet;
	}

	public String getUsername() {
		return username;
	}

	public String getPassword() {
		return password;
	}
	
	public void setPassword(String password) {
		this.password = password;
	}

	public int getBox() {
		return box;
	}

	public int getHelmet() {
		return helmet;
	}

	public void setBox(int box) {
		this.box = box;
	}
	
	public void setHelmet(int helmet) {
		this.helmet = helmet;
	}

	@Override
	public String toString() {
		StringBuilder sb = new StringBuilder();
		sb.append(" username : ");
		sb.append(getUsername());
		sb.append(" password : ");
		sb.append(getPassword());
		return sb.toString();
	}
}
